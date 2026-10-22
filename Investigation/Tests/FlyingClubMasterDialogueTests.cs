using System.Collections.Generic;
using System.Linq;
using Ink.Runtime;
using NUnit.Framework;

namespace KopliSoft.Investigation.Tests
{
    /// <summary>
    /// Plays the Flying Club master's conversation (ported from the Fungus flowchart "Flying Club Master") with a fake Fungus side.
    /// </summary>
    public class FlyingClubMasterDialogueTests
    {
        private Story story;
        private CaseFile caseFile;
        private DialogueSession session;
        // Fake Fungus flowchart variables, "Flowchart.variable" -> value.
        private Dictionary<string, bool> fungus;
        private List<string> messages;
        private List<string> activated;

        [SetUp]
        public void SetUp()
        {
            var chapter = StoryFiles.ChapterFolders().Single(c => c.EndsWith("Chapter1"));
            var definition = StoryFiles.LoadCase(chapter);
            story = StoryFiles.CompileChapter(chapter);
            caseFile = new CaseFile(definition);
            InkCaseBindings.Bind(story, caseFile, message => Assert.Fail(message));

            fungus = new Dictionary<string, bool>();
            messages = new List<string>();
            activated = new List<string>();
            story.BindExternalFunction("fungus_bool", (string flowchart, string variable) =>
            {
                bool value;
                return (object)(fungus.TryGetValue(flowchart + "." + variable, out value) && value);
            }, true);
            story.BindExternalFunction("fungus_set_bool", (string flowchart, string variable, bool value) => { fungus[flowchart + "." + variable] = value; });
            story.BindExternalFunction("airship_event", (string name) => { messages.Add(name); });
            story.BindExternalFunction("set_active", (string path, bool active) => { if (active) activated.Add(path); });
            session = new DialogueSession(story, definition);
        }

        // Plays until choices or the end, picking choices by their text. Returns all lines shown.
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
            Assert.IsEmpty(queue, "The conversation ended before all choices were made");
            return lines;
        }

        private IReadOnlyList<string> CurrentChoices()
        {
            var step = session.Next();
            Assert.AreEqual(DialogueStepKind.Choices, step.Kind);
            return step.Choices;
        }

        private void FirstMeeting()
        {
            session.Begin("flying_club_master");
            Play("Improvising");
        }

        [Test]
        public void FirstMeetingIntroducesTheClubAndEnds()
        {
            session.Begin("flying_club_master");
            var lines = Play("We are here to help resolve the crisis.", "Between the capitalists and the proletariat?", "The proletariat becomes the new capitalists.");

            Assert.AreEqual("Anna", lines[0].SpeakerName);
            Assert.AreEqual("Yuri", lines[1].SpeakerName);
            Assert.AreEqual("Your inner voice", lines[2].SpeakerName);
            Assert.That(lines.Last().Text, Does.Contain("Flying Club"));
            Assert.AreEqual(2, (int)story.variablesState["flying_club_master_reputation"]);
            Assert.IsTrue((bool)story.variablesState["flying_club_master_intro_done"]);
        }

        [Test]
        public void LaterTalksGoStraightToQuestions()
        {
            FirstMeeting();
            session.Begin("flying_club_master");
            var lines = Play();
            Assert.AreEqual("Hi Anna. How may I help you?", lines.Single().Text);
            CollectionAssert.AreEqual(new[] { "No questions", "Questions about airplanes", "Questions about the current situation" }, CurrentChoices());
        }

        [Test]
        public void CurrentSituationOffersTheFourNotesAndThePermission()
        {
            FirstMeeting();
            session.Begin("flying_club_master");
            var lines = Play("Questions about the current situation", "What do you know about the strike?",
                "Has anything unusual happened here, at the airfield?", "Anything unusual in the neighbourhood?");

            CollectionAssert.AreEqual(new[] { "flying_club_master_strike", "flying_club_master_airfield_activity", "flying_club_master_foreign_visitors", "flying_club_master_army" },
                lines.Where(l => l.NoteId != null).Select(l => l.NoteId));
            Assert.IsTrue((bool)story.variablesState["flight_log_read_permission"]);
        }

        [Test]
        public void FirstGoodbyeSendsAnnaToBedOnce()
        {
            FirstMeeting();
            session.Begin("flying_club_master");
            var lines = Play("No questions");
            // "Hi Anna...", "See you.", then the captain and the narrator.
            Assert.AreEqual("See you.", lines[1].Text);
            Assert.AreEqual("Captain J. Meister", lines[2].SpeakerName);
            Assert.That(lines[3].Text, Does.StartWith("<You have a bed"));
            CollectionAssert.AreEqual(new[] { "unlock_captain" }, messages);

            session.Begin("flying_club_master");
            lines = Play("No questions");
            Assert.AreEqual("See you.", lines.Last().Text);
            Assert.AreEqual(1, messages.Count);
        }

        [Test]
        public void AirplaneTopicsDependOnWhatAnnaExamined()
        {
            FirstMeeting();
            session.Begin("flying_club_master");
            Play("Questions about airplanes");
            Assert.AreEqual(3, CurrentChoices().Count);

            story.variablesState["airplane_british_examined"] = true;
            session.Begin("flying_club_master");
            var lines = Play("Questions about airplanes", "I've seen a damaged British airplane of the Great War on the airfield");
            Assert.That(lines.Last().Text, Does.Contain("fuel line was clogged"));
            Assert.IsTrue(fungus["Flying Club Master.AirplaneCrashDiscussed"]);
        }

        [Test]
        public void ShortFlightNeedsTheConclusionAndRespectsReputation()
        {
            // Rude first meeting: reputation -1.
            session.Begin("flying_club_master");
            Play("Right now, we are the law *here*.");
            story.variablesState["flight_log_discovered"] = true;
            story.variablesState["flight_log_read_permission"] = true;

            session.Begin("flying_club_master");
            Play("Questions about the flight log");
            CollectionAssert.AreEqual(new[] { "<Ask something else>" }, CurrentChoices());

            foreach (var note in new[] { "log_1937_03", "log_1937_05", "log_1937_06", "log_1937_07" })
            {
                caseFile.AddNote(note);
            }
            Assert.AreEqual(CombineOutcome.NewConclusion, caseFile.Combine(new[] { "log_1937_03", "log_1937_05", "log_1937_06", "log_1937_07" }, Relation.Match).Outcome);

            // "Last" first: Yuri corrects Anna and keeps the short answer (reputation still -1).
            session.Begin("flying_club_master");
            var lines = Play("Questions about the flight log", "The last flight of the trimotor was a short one.");
            Assert.AreEqual("The most recent flight, not the last!", lines[1].Text);
            Assert.AreEqual("He lost the Pitot tube, but managed to return and land.", lines[2].Text);
            Assert.IsTrue(fungus["Flying Club Master.PitotTubeDiscussed"]);
            // Corrected once, the wrong wording is not offered again.
            CollectionAssert.AreEqual(new[] { "The most recent flight of the trimotor was a short one.", "<Ask something else>" }, CurrentChoices());
        }

        [Test]
        public void SayingMostRecentRightAwayEarnsTheFullAnswer()
        {
            session.Begin("flying_club_master");
            Play("Right now, we are the law *here*.");
            story.variablesState["flight_log_discovered"] = true;
            fungus["item13.InvestigateShortFlight"] = true;

            session.Begin("flying_club_master");
            var lines = Play("Questions about the flight log", "The most recent flight of the trimotor was a short one.");
            Assert.AreEqual("Yes, it went wrong.", lines[1].Text);
            Assert.AreEqual(0, (int)story.variablesState["flying_club_master_reputation"]);
        }
    }
}
