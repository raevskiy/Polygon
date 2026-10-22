using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Story = Ink.Runtime.Story;

namespace KopliSoft.Investigation.Tests
{
    /// <summary>
    /// The airship landing (diana_airship.ink, ported from the Fungus "Captain Approached", "Landing request", "Diana airship").
    /// </summary>
    public class DianaAirshipTests
    {
        private Story story;
        private DialogueSession session;
        private List<string> calls;

        [SetUp]
        public void SetUp()
        {
            var chapter = StoryFiles.ChapterFolders().Single(c => c.EndsWith("Chapter1"));
            var definition = StoryFiles.LoadCase(chapter);
            story = StoryFiles.CompileChapter(chapter);
            InkCaseBindings.Bind(story, new CaseFile(definition), message => Assert.Fail(message));
            calls = new List<string>();
            story.BindExternalFunction("set_active", (string path, bool active) => { calls.Add("set_active " + path + " " + active); });
            story.BindExternalFunction("play_animation", (string path, string state) => { calls.Add("play_animation " + path + " " + state); });
            story.BindExternalFunction("airship_event", (string name) => { calls.Add("airship " + name); });
            story.BindExternalFunction("key_name", (string action) => (object)(action == "Notebook" ? "N" : "?"), true);
            session = new DialogueSession(story, definition);
        }

        private List<DialogueLine> PlayAll(string knot)
        {
            session.Begin(knot);
            var lines = new List<DialogueLine>();
            for (var step = session.Next(); step.Kind != DialogueStepKind.End; step = session.Next())
            {
                Assert.AreEqual(DialogueStepKind.Line, step.Kind, "The airship scenes have no choices");
                lines.Add(step.Line);
            }
            return lines;
        }

        [Test]
        public void LandingRequestPlaysOnceThenTheTriggerIsSilent()
        {
            var lines = PlayAll("airship_landing_request");
            Assert.Greater(lines.Count, 10);
            CollectionAssert.Contains(calls, "set_active /Countryside/Flying Club/Lights True");
            Assert.AreEqual("play_animation /Vehicles/Diana airship Landing", calls.Last());

            // The trigger fires again whenever Anna passes by: nothing is shown (the runner then opens no UI).
            session.Begin("airship_landing_request");
            Assert.AreEqual(DialogueStepKind.End, session.Next().Kind);
        }

        [Test]
        public void RadioAndSpeakingTubeVoicesMatchTheFungusCharacters()
        {
            var names = PlayAll("airship_landing_request").Select(l => l.DisplayName).ToList();
            CollectionAssert.Contains(names, "Captain J. Meister <over the radio>");
            CollectionAssert.Contains(names, "Yuri <over the radio>");
            CollectionAssert.Contains(names, "Captain J. Meister <over the speaking tube>");
            CollectionAssert.Contains(names, "Whina <over the speaking tube>");
            CollectionAssert.Contains(names, "Oleksiy <over the speaking tube>");
            CollectionAssert.Contains(names, "In-game help");
        }

        [Test]
        public void HelpNamesTheNotebookKeyAndEsperantoCanBeWrittenDown()
        {
            var lines = PlayAll("airship_landing_request");
            Assert.That(lines.Select(l => l.Text), Has.Some.Contains("notebook ('N' key)"));
            Assert.AreEqual("flying_club_master_knows_esperanto", lines.Single(l => l.NoteId != null).NoteId);
        }

        [Test]
        public void LandingCompleteTellsTheAirshipState()
        {
            var lines = PlayAll("airship_landing_complete");
            Assert.AreEqual("Great. Anna, let's go.", lines.Last().Text);
            CollectionAssert.AreEqual(new[] { "airship landing_complete" }, calls);
        }
    }
}
