using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace KopliSoft.Investigation.Tests
{
    /// <summary>
    /// Checks the real story content in Assets/Story. Fails on any validation error, warnings are only printed.
    /// </summary>
    public class StoryDataTests
    {
        private static string[] Chapters()
        {
            return StoryFiles.ChapterFolders().ToArray();
        }

        [TestCaseSource(nameof(Chapters))]
        public void CaseDataIsValid(string chapterFolder)
        {
            var issues = CaseValidator.Validate(StoryFiles.LoadCase(chapterFolder), StoryFiles.InkFiles(chapterFolder));
            foreach (var warning in issues.Where(i => i.Severity == IssueSeverity.Warning))
            {
                TestContext.WriteLine(warning);
            }
            var errors = issues.Where(i => i.Severity == IssueSeverity.Error).Select(i => i.ToString()).ToList();
            Assert.IsEmpty(errors, string.Join("\n", errors));
        }

        [TestCaseSource(nameof(Chapters))]
        public void InkCompilesAndBindsToCase(string chapterFolder)
        {
            var definition = StoryFiles.LoadCase(chapterFolder);
            var story = StoryFiles.CompileChapter(chapterFolder);
            InkCaseBindings.Bind(story, new CaseFile(definition), message => Assert.Fail(message));
            Assert.DoesNotThrow(() => story.ContinueMaximally());
        }

        private static IEnumerable<TestCaseData> Knots()
        {
            var knot = new Regex(@"^\s*===\s*(?!function\b)([A-Za-z0-9_]+)", RegexOptions.Multiline);
            foreach (var chapter in Chapters())
            {
                foreach (var file in StoryFiles.InkFiles(chapter))
                {
                    foreach (Match match in knot.Matches(file.Source))
                    {
                        yield return new TestCaseData(chapter, match.Groups[1].Value).SetName("KnotPlaysToTheEnd(" + match.Groups[1].Value + ")");
                    }
                }
            }
        }

        /// <summary>
        /// Plays every conversation, always taking the first choice, and expects it to finish without ink errors.
        /// </summary>
        [TestCaseSource(nameof(Knots))]
        public void KnotPlaysToTheEnd(string chapterFolder, string knot)
        {
            var definition = StoryFiles.LoadCase(chapterFolder);
            var story = StoryFiles.CompileChapter(chapterFolder);
            var errors = new List<string>();
            story.onError += (message, type) => errors.Add(type + ": " + message);
            InkCaseBindings.Bind(story, new CaseFile(definition), message => errors.Add(message));
            var session = new DialogueSession(story, definition);
            session.Begin(knot);

            var lines = 0;
            for (var step = session.Next(); step.Kind != DialogueStepKind.End; step = session.Next())
            {
                Assert.Less(++lines, 1000, "The conversation does not end when always taking the first choice");
                if (step.Kind == DialogueStepKind.Choices)
                {
                    session.Choose(0);
                }
            }
            Assert.IsEmpty(errors, string.Join("\n", errors));
            Assert.Greater(lines, 0, "The knot shows nothing");
        }

        [Test]
        public void ValidatorCatchesBrokenReferences()
        {
            var definition = CaseDefinition.FromJson(@"{
                ""id"": ""broken"",
                ""notes"": [ { ""id"": ""a"", ""title"": ""A"", ""text"": ""a"" }, { ""id"": ""a"", ""title"": ""A2"", ""text"": ""a"" } ],
                ""conclusions"": [ { ""id"": ""x"", ""title"": ""X"", ""text"": ""x"", ""requires"": [""a"", ""missing""], ""startsTasks"": [""no_task""] } ],
                ""tasks"": []
            }");
            var ink = new[] { new InkSourceFile("t.ink", "Hello. #note:ghost\n{ has_conclusion(\"nothing\") }") };

            var errors = CaseValidator.Validate(definition, ink).Where(i => i.Severity == IssueSeverity.Error).Select(i => i.ToString()).ToList();

            Assert.That(errors, Has.Some.Contains("card a: is defined 2 times"));
            Assert.That(errors, Has.Some.Contains("unknown card 'missing'"));
            Assert.That(errors, Has.Some.Contains("unknown task 'no_task'"));
            Assert.That(errors, Has.Some.Contains("unknown note 'ghost'"));
            Assert.That(errors, Has.Some.Contains("unknown conclusion 'nothing'"));
            Assert.That(errors, Has.Some.Contains("conclusion x: can never be reached"));
        }

        [Test]
        public void ValidatorWarnsAboutOldRegistrations()
        {
            var definition = CaseDefinition.FromJson(@"{ ""id"": ""t"", ""notes"": [ { ""id"": ""a"", ""title"": ""A"", ""text"": ""MC485 crashed."", ""legacyItemId"": 1 } ] }");
            var ink = new[] { new InkSourceFile("t.ink", "UR84103 landed, CIV-804-84103 too.") };

            var warnings = CaseValidator.Validate(definition, ink).Where(i => i.Severity == IssueSeverity.Warning).Select(i => i.ToString()).ToList();

            Assert.That(warnings, Has.Some.Contains("'MC485' looks like an old-style aircraft registration"));
            Assert.That(warnings, Has.Some.Contains("'UR84103' looks like an old-style aircraft registration"));
            Assert.That(warnings, Has.None.Contains("CIV-804-84103"));
        }
    }
}
