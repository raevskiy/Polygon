using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ink.Runtime;

namespace KopliSoft.Investigation
{
    public sealed class DialogueLine
    {
        // Null for narration and stage directions.
        public CharacterDefinition Speaker { get; private set; }
        public string Text { get; private set; }
        public IReadOnlyList<string> Tags { get; private set; }
        // Note Anna can write down while this line is shown, or null.
        public string NoteId { get; private set; }

        public DialogueLine(CharacterDefinition speaker, string text, IReadOnlyList<string> tags, string noteId)
        {
            Speaker = speaker;
            Text = text;
            Tags = tags;
            NoteId = noteId;
        }

        public string SpeakerName { get { return Speaker != null ? Speaker.name : null; } }
    }

    public enum DialogueStepKind
    {
        Line,
        Choices,
        End
    }

    public sealed class DialogueStep
    {
        public DialogueStepKind Kind { get; private set; }
        public DialogueLine Line { get; private set; }
        public IReadOnlyList<string> Choices { get; private set; }

        public static readonly DialogueStep End = new DialogueStep { Kind = DialogueStepKind.End };

        public static DialogueStep ForLine(DialogueLine line)
        {
            return new DialogueStep { Kind = DialogueStepKind.Line, Line = line };
        }

        public static DialogueStep ForChoices(IReadOnlyList<string> choices)
        {
            return new DialogueStep { Kind = DialogueStepKind.Choices, Choices = choices };
        }
    }

    /// <summary>
    /// Turns raw ink output into dialogue lines, following the conventions in Assets/Story/README.md:
    /// "Yuri: text" is spoken by the character with id Yuri; "#note:some_id" offers a note to write down.
    /// </summary>
    public static class DialogueLineParser
    {
        public const string NoteTag = "note";

        private static readonly Regex SpeakerPrefix = new Regex(@"^\s*([A-Za-z][A-Za-z0-9_]*)\s*:\s+(.*)$", RegexOptions.Singleline);

        public static DialogueLine Parse(string rawText, IList<string> tags, CaseDefinition definition)
        {
            var text = (rawText ?? string.Empty).Trim();
            CharacterDefinition speaker = null;
            var match = SpeakerPrefix.Match(text);
            if (match.Success)
            {
                speaker = definition.FindCharacter(match.Groups[1].Value);
                if (speaker != null)
                {
                    text = match.Groups[2].Value.Trim();
                }
            }

            var tagList = tags != null ? tags.Select(t => t.Trim()).ToList() : new List<string>();
            return new DialogueLine(speaker, text, tagList, TagValue(tagList, NoteTag));
        }

        /// <summary>
        /// Value of a "key:value" tag, or null.
        /// </summary>
        public static string TagValue(IEnumerable<string> tags, string key)
        {
            foreach (var tag in tags)
            {
                var separator = tag.IndexOf(':');
                if (separator > 0 && tag.Substring(0, separator).Trim() == key)
                {
                    return tag.Substring(separator + 1).Trim();
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Steps through one conversation (an ink knot) of the shared chapter story. UI-agnostic.
    /// </summary>
    public class DialogueSession
    {
        private readonly Story story;
        private readonly CaseDefinition definition;

        public DialogueSession(Story story, CaseDefinition definition)
        {
            this.story = story;
            this.definition = definition;
        }

        public void Begin(string knot)
        {
            story.ChoosePathString(knot);
        }

        public DialogueStep Next()
        {
            while (story.canContinue)
            {
                var text = story.Continue();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return DialogueStep.ForLine(DialogueLineParser.Parse(text, story.currentTags, definition));
                }
            }
            if (story.currentChoices.Count > 0)
            {
                return DialogueStep.ForChoices(story.currentChoices.Select(c => c.text.Trim()).ToList());
            }
            return DialogueStep.End;
        }

        public void Choose(int index)
        {
            story.ChooseChoiceIndex(index);
        }
    }
}
