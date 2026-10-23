using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KopliSoft.Investigation
{
    public enum IssueSeverity
    {
        Warning,
        Error
    }

    public sealed class ValidationIssue
    {
        public IssueSeverity Severity { get; private set; }
        public string Location { get; private set; }
        public string Message { get; private set; }

        public ValidationIssue(IssueSeverity severity, string location, string message)
        {
            Severity = severity;
            Location = location;
            Message = message;
        }

        public override string ToString()
        {
            return Severity + " " + Location + ": " + Message;
        }
    }

    public sealed class InkSourceFile
    {
        public string Path { get; private set; }
        public string Source { get; private set; }

        public InkSourceFile(string path, string source)
        {
            Path = path;
            Source = source;
        }
    }

    /// <summary>
    /// Checks a case definition and the ink files that use it for broken ids and impossible rules.
    /// Pure text analysis, so it works in the Editor, in tests and from the command line.
    /// </summary>
    public static class CaseValidator
    {
        // Aircraft registrations are "CIV-<ISO 3166 numeric country code>-<number>" (see Assets/Story/README.md).
        // Letters directly followed by digits (e.g. AB1234) is the old style.
        private static readonly Regex OldRegistration = new Regex(@"\b[A-Z]{1,4}\d{3,6}\b");

        private static readonly Regex NoteTag = new Regex(@"#\s*note\s*:\s*([A-Za-z0-9_]+)");
        private static readonly Regex FunctionCall = new Regex(@"\b(has_note|add_note|has_conclusion|task_active|task_done|start_task|complete_task)\s*\(\s*""([^""]*)""\s*\)");
        // snake_case identifiers, and whole function calls such as fungus_bool("Flowchart", "SomeVariable").
        private static readonly Regex Identifier = new Regex(@"\b\w+\s*\([^)]*\)|\b[A-Za-z0-9]+(?:_[A-Za-z0-9]+)+\b");
        private static readonly Regex SpeakerPrefix = new Regex(@"^\s*(?:[*+]\s*)*(?:\[[^\]]*\]\s*)?([A-Z][A-Za-z0-9_]*)\s*:\s");

        public static List<ValidationIssue> Validate(CaseDefinition definition, IEnumerable<InkSourceFile> inkFiles, IEnumerable<string> forbiddenWords = null)
        {
            var issues = new List<ValidationIssue>();
            // Words that must not appear in player-facing text come from the case data (forbiddenWords), not from code.
            var words = (forbiddenWords ?? definition.forbiddenWords ?? new List<string>()).ToList();
            var inkList = (inkFiles ?? Enumerable.Empty<InkSourceFile>()).ToList();

            ValidateDefinition(definition, words, issues);
            var usedNotes = ValidateInk(definition, inkList, words, issues);

            foreach (var note in definition.notes)
            {
                if (note.id != null && !usedNotes.Contains(note.id) && !note.legacyItemId.HasValue)
                {
                    issues.Add(Warning("note " + note.id, "is never offered in ink (#note or add_note) and has no legacyItemId, so it cannot be obtained"));
                }
            }
            return issues;
        }

        private static void ValidateDefinition(CaseDefinition def, List<string> words, List<ValidationIssue> issues)
        {
            // Notes and conclusions are both cards and share one id space.
            CheckIds(def.notes.Select(n => n.id).Concat(def.conclusions.Select(c => c.id)), "card", issues);
            CheckIds(def.tasks.Select(t => t.id), "task", issues);
            CheckIds(def.characters.Select(c => c.id), "character", issues);

            foreach (var group in def.notes.Where(n => n.legacyItemId.HasValue).GroupBy(n => n.legacyItemId.Value).Where(g => g.Count() > 1))
            {
                issues.Add(Error("notes " + string.Join(", ", group.Select(n => n.id)), "share legacyItemId " + group.Key));
            }

            foreach (var note in def.notes)
            {
                var where = "note " + note.id;
                RequireText(note.title, where, "title", issues);
                RequireText(note.text, where, "text", issues);
                CheckWords(where, words, issues, note.title, note.text, note.source);
            }

            foreach (var task in def.tasks)
            {
                var where = "task " + task.id;
                RequireText(task.title, where, "title", issues);
                RequireText(task.text, where, "text", issues);
                CheckWords(where, words, issues, task.title, task.text);
            }

            foreach (var id in def.initialTasks)
            {
                CheckRef(def.FindTask(id) != null, "initialTasks", "task", id, issues);
            }

            foreach (var conclusion in def.conclusions)
            {
                var where = "conclusion " + conclusion.id;
                RequireText(conclusion.title, where, "title", issues);
                RequireText(conclusion.text, where, "text", issues);
                CheckWords(where, words, issues, conclusion.title, conclusion.text);

                var requires = conclusion.requires ?? new List<string>();
                if (requires.Distinct().Count() < 2)
                {
                    issues.Add(Error(where, "requires at least two different cards"));
                }
                if (requires.Count != requires.Distinct().Count())
                {
                    issues.Add(Error(where, "lists the same card twice in requires"));
                }
                foreach (var id in requires)
                {
                    if (id == conclusion.id)
                    {
                        issues.Add(Error(where, "requires itself"));
                    }
                    else
                    {
                        CheckRef(def.FindNote(id) != null || def.FindConclusion(id) != null, where, "card", id, issues);
                    }
                }
                foreach (var id in conclusion.supersedes)
                {
                    CheckRef(def.FindConclusion(id) != null, where + " supersedes", "conclusion", id, issues);
                }
                foreach (var id in conclusion.startsTasks.Concat(conclusion.completesTasks))
                {
                    CheckRef(def.FindTask(id) != null, where, "task", id, issues);
                }
            }

            foreach (var group in def.conclusions.GroupBy(c => c.relation + ":" + string.Join(",", c.requires.OrderBy(r => r))).Where(g => g.Count() > 1))
            {
                issues.Add(Error("conclusions " + string.Join(", ", group.Select(c => c.id)), "have the same relation and the same required cards"));
            }

            foreach (var unreachable in UnreachableConclusions(def))
            {
                issues.Add(Error("conclusion " + unreachable, "can never be reached (it depends on a missing card or on a cycle)"));
            }
        }

        private static IEnumerable<string> UnreachableConclusions(CaseDefinition def)
        {
            var reachable = new HashSet<string>(def.notes.Select(n => n.id));
            bool progress = true;
            while (progress)
            {
                progress = false;
                foreach (var conclusion in def.conclusions)
                {
                    if (!reachable.Contains(conclusion.id) && conclusion.requires.All(reachable.Contains))
                    {
                        reachable.Add(conclusion.id);
                        progress = true;
                    }
                }
            }
            return def.conclusions.Select(c => c.id).Where(id => !reachable.Contains(id));
        }

        // Returns the ids of notes that ink can give to the player.
        private static HashSet<string> ValidateInk(CaseDefinition def, List<InkSourceFile> files, List<string> words, List<ValidationIssue> issues)
        {
            var offeredNotes = new HashSet<string>();
            foreach (var file in files)
            {
                var lines = file.Source.Replace("\r\n", "\n").Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = StripComment(lines[i]);
                    var where = file.Path + ":" + (i + 1);

                    foreach (Match match in NoteTag.Matches(line))
                    {
                        var id = match.Groups[1].Value;
                        offeredNotes.Add(id);
                        CheckRef(def.FindNote(id) != null, where, "note", id, issues);
                    }

                    foreach (Match match in FunctionCall.Matches(line))
                    {
                        var function = match.Groups[1].Value;
                        var id = match.Groups[2].Value;
                        switch (function)
                        {
                            case "has_note":
                                CheckRef(def.FindNote(id) != null, where, "note", id, issues);
                                break;
                            case "add_note":
                                offeredNotes.Add(id);
                                CheckRef(def.FindNote(id) != null, where, "note", id, issues);
                                break;
                            case "has_conclusion":
                                CheckRef(def.FindConclusion(id) != null, where, "conclusion", id, issues);
                                break;
                            default:
                                CheckRef(def.FindTask(id) != null, where, "task", id, issues);
                                break;
                        }
                    }

                    var speaker = SpeakerPrefix.Match(line);
                    if (speaker.Success && def.FindCharacter(speaker.Groups[1].Value) == null && !IsInkKeyword(line))
                    {
                        issues.Add(Warning(where, "'" + speaker.Groups[1].Value + ":' looks like a speaker but is not in characters, so it will be shown as narration"));
                    }

                    // Ids and snake_case identifiers (variables, functions) are never shown, only the text around them is checked.
                    CheckWords(where, words, issues, Identifier.Replace(FunctionCall.Replace(NoteTag.Replace(line, ""), ""), ""));
                }
            }
            return offeredNotes;
        }

        private static bool IsInkKeyword(string line)
        {
            var trimmed = line.TrimStart();
            return trimmed.StartsWith("INCLUDE") || trimmed.StartsWith("EXTERNAL") || trimmed.StartsWith("VAR")
                || trimmed.StartsWith("CONST") || trimmed.StartsWith("LIST") || trimmed.StartsWith("TODO");
        }

        private static string StripComment(string line)
        {
            var index = line.IndexOf("//", StringComparison.Ordinal);
            return index >= 0 ? line.Substring(0, index) : line;
        }

        private static void CheckIds(IEnumerable<string> ids, string kind, List<ValidationIssue> issues)
        {
            foreach (var group in ids.GroupBy(id => id ?? string.Empty))
            {
                if (group.Key.Length == 0)
                {
                    issues.Add(Error(kind, "has an empty id"));
                }
                else if (!Regex.IsMatch(group.Key, "^[A-Za-z0-9_]+$"))
                {
                    issues.Add(Error(kind + " " + group.Key, "id may contain only letters, digits and _"));
                }
                if (group.Count() > 1)
                {
                    issues.Add(Error(kind + " " + group.Key, "is defined " + group.Count() + " times"));
                }
            }
        }

        private static void CheckRef(bool exists, string where, string kind, string id, List<ValidationIssue> issues)
        {
            if (!exists)
            {
                issues.Add(Error(where, "unknown " + kind + " '" + id + "'"));
            }
        }

        private static void RequireText(string value, string where, string field, List<ValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                issues.Add(Error(where, "has no " + field));
            }
        }

        private static void CheckWords(string where, List<string> words, List<ValidationIssue> issues, params string[] texts)
        {
            foreach (var word in words)
            {
                if (texts.Any(t => t != null && t.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    issues.Add(Warning(where, "contains the forbidden word '" + word + "'"));
                }
            }
            foreach (var text in texts.Where(t => t != null))
            {
                var match = OldRegistration.Match(text);
                if (match.Success)
                {
                    issues.Add(Warning(where, "'" + match.Value + "' looks like an old-style aircraft registration; use CIV-<country code>-<number>"));
                }
            }
        }

        private static ValidationIssue Error(string where, string message)
        {
            return new ValidationIssue(IssueSeverity.Error, where, message);
        }

        private static ValidationIssue Warning(string where, string message)
        {
            return new ValidationIssue(IssueSeverity.Warning, where, message);
        }
    }
}
