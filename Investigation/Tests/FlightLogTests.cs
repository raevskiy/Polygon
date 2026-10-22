using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Path = System.IO.Path;
using Story = Ink.Runtime.Story;

namespace KopliSoft.Investigation.Tests
{
    /// <summary>
    /// The Flying Club log book (flight_log.ink, ported from the Fungus flowchart "Flight log").
    /// </summary>
    public class FlightLogTests
    {
        private static readonly Regex Entry = new Regex(@"^\d\d\.\d\d\.\d{4} \d\d:\d\d \|.*$");

        private string chapter;
        private Story story;
        private DialogueSession session;

        [SetUp]
        public void SetUp()
        {
            chapter = StoryFiles.ChapterFolders().Single(c => c.EndsWith("Chapter1"));
            var definition = StoryFiles.LoadCase(chapter);
            story = StoryFiles.CompileChapter(chapter);
            InkCaseBindings.Bind(story, new CaseFile(definition), message => Assert.Fail(message));
            session = new DialogueSession(story, definition);
        }

        private List<DialogueLine> Play(params string[] choices)
        {
            var lines = new List<DialogueLine>();
            var queue = new Queue<string>(choices);
            for (var step = session.Next(); step.Kind != DialogueStepKind.End; step = session.Next())
            {
                if (step.Kind == DialogueStepKind.Line)
                {
                    lines.Add(step.Line);
                    continue;
                }
                if (queue.Count == 0)
                {
                    return lines;
                }
                var wanted = queue.Dequeue();
                var index = step.Choices.ToList().IndexOf(wanted);
                Assert.GreaterOrEqual(index, 0, "Choice '" + wanted + "' not offered. Offered: " + string.Join(" | ", step.Choices));
                session.Choose(index);
            }
            return lines;
        }

        [Test]
        public void WithoutPermissionAnnaOnlyLooksAtTheBook()
        {
            session.Begin("flight_log");
            var lines = Play();
            Assert.AreEqual(1, lines.Count);
            Assert.That(lines[0].Text, Does.Contain("permission"));
            Assert.IsTrue((bool)story.variablesState["flight_log_discovered"]);
            Assert.AreEqual(DialogueStepKind.End, session.Next().Kind);
        }

        [Test]
        public void WithPermissionThePagesCanBeReadAndWrittenDown()
        {
            story.variablesState["flight_log_read_permission"] = true;
            session.Begin("flight_log");
            var page = Play("Summer 1936", "August 1936").Last();
            Assert.AreEqual("log_1936_08", page.NoteId);
            Assert.That(page.Text, Does.Contain("\n29.08.1936 10:41 | CIV-810-13522 | Departure"));

            session.Begin("flight_log");
            page = Play("Summer 1937", "July 1937").Last();
            Assert.AreEqual("log_1937_07", page.NoteId);
        }

        /// <summary>
        /// Every log_* note must quote exactly the entries of its page, so the notes cannot drift from the book again.
        /// </summary>
        [Test]
        public void NotesQuoteThePagesExactly()
        {
            var definition = StoryFiles.LoadCase(chapter);
            // "|" is escaped as \| in ink source.
            var ink = File.ReadAllText(Path.Combine(chapter, "flight_log.ink")).Replace("\\|", "|");
            var logNotes = definition.notes.Where(n => n.id.StartsWith("log_")).ToList();
            Assert.IsNotEmpty(logNotes);

            foreach (var note in logNotes)
            {
                var pageLine = ink.Split('\n').SingleOrDefault(l => l.Contains("#note:" + note.id));
                Assert.IsNotNull(pageLine, "No page in flight_log.ink offers " + note.id);
                var pageEntries = pageLine.Substring(0, pageLine.IndexOf('#')).Split(new[] { DialogueLineParser.LineBreak }, System.StringSplitOptions.None)
                    .Select(e => e.Trim()).Where(e => Entry.IsMatch(e));
                var noteEntries = note.text.Split('\n').Select(e => e.Trim()).Where(e => Entry.IsMatch(e));
                CollectionAssert.AreEqual(pageEntries.ToList(), noteEntries.ToList(), note.id + " differs from its page");
            }
        }
    }
}
