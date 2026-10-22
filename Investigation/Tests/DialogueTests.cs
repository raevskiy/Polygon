using System.Linq;
using NUnit.Framework;

namespace KopliSoft.Investigation.Tests
{
    public class DialogueTests
    {
        private const string CaseJson = @"{
            ""id"": ""test"",
            ""characters"": [ { ""id"": ""FlyingClubMaster"", ""name"": ""Yuri Solomko"" }, { ""id"": ""Protagonist"", ""name"": ""Anna"" } ],
            ""notes"": [ { ""id"": ""strike"", ""title"": ""Strike"", ""text"": ""strike"" } ],
            ""conclusions"": [],
            ""tasks"": [ { ""id"": ""t"", ""title"": ""T"", ""text"": ""t"" } ]
        }";

        private const string Ink = @"
EXTERNAL has_note(id)
EXTERNAL start_task(id)
=== function has_note(id)
~ return false
=== function start_task(id)
~ return

=== talk ===
FlyingClubMaster: A layoff was declared. #note:strike
<Yuri grins>
Note: this is not a speaker.
* { has_note(""strike"") } [Ask more]
    FlyingClubMaster: More. -> DONE
+ [Bye]
    ~ start_task(""t"")
    Protagonist: See you.
    -> DONE
";

        private CaseDefinition definition;

        [SetUp]
        public void SetUp()
        {
            definition = CaseDefinition.FromJson(CaseJson);
        }

        [Test]
        public void ParsesSpeakerAndNoteTag()
        {
            var line = DialogueLineParser.Parse("FlyingClubMaster: Hello there.\n", new[] { "note:strike" }, definition);
            Assert.AreEqual("Yuri Solomko", line.SpeakerName);
            Assert.AreEqual("Hello there.", line.Text);
            Assert.AreEqual("strike", line.NoteId);
        }

        [Test]
        public void ViaTagAddsTheChannelToTheShownName()
        {
            var line = DialogueLineParser.Parse("FlyingClubMaster: Bonan vesperon.", new[] { "via:radio" }, definition);
            Assert.AreEqual("Yuri Solomko", line.SpeakerName);
            Assert.AreEqual("Yuri Solomko <over the radio>", line.DisplayName);
            Assert.AreEqual("Yuri Solomko <over the speaking tube>", DialogueLineParser.Parse("FlyingClubMaster: Bone.", new[] { "via:tube" }, definition).DisplayName);
            Assert.AreEqual("Yuri Solomko", DialogueLineParser.Parse("FlyingClubMaster: Hi.", null, definition).DisplayName);
        }

        [Test]
        public void LineBreakMarkerBecomesNewline()
        {
            var line = DialogueLineParser.Parse("Date | Status<br>29.08.1936 | Arrival", null, definition);
            Assert.AreEqual("Date | Status\n29.08.1936 | Arrival", line.Text);
        }

        [Test]
        public void UnknownPrefixIsNarration()
        {
            var line = DialogueLineParser.Parse("Note: careful.", null, definition);
            Assert.IsNull(line.Speaker);
            Assert.AreEqual("Note: careful.", line.Text);
            Assert.IsNull(line.NoteId);
        }

        [Test]
        public void SessionPlaysKnotWithCaseFunctions()
        {
            var story = StoryFiles.Compile(Ink);
            var caseFile = new CaseFile(definition);
            InkCaseBindings.Bind(story, caseFile, message => Assert.Fail(message));
            var session = new DialogueSession(story, definition);
            session.Begin("talk");

            var first = session.Next();
            Assert.AreEqual(DialogueStepKind.Line, first.Kind);
            Assert.AreEqual("strike", first.Line.NoteId);
            Assert.AreEqual("<Yuri grins>", session.Next().Line.Text);
            Assert.IsNull(session.Next().Line.Speaker);

            // Without the note only "Bye" is offered.
            var choices = session.Next();
            Assert.AreEqual(DialogueStepKind.Choices, choices.Kind);
            CollectionAssert.AreEqual(new[] { "Bye" }, choices.Choices);

            session.Choose(0);
            Assert.AreEqual("See you.", session.Next().Line.Text);
            Assert.AreEqual(DialogueStepKind.End, session.Next().Kind);
            Assert.IsTrue(caseFile.IsTaskActive("t"));
        }

        [Test]
        public void ChoiceAppearsOnceNoteIsWrittenDown()
        {
            var story = StoryFiles.Compile(Ink);
            var caseFile = new CaseFile(definition);
            InkCaseBindings.Bind(story, caseFile, message => Assert.Fail(message));
            caseFile.AddNote("strike");
            var session = new DialogueSession(story, definition);
            session.Begin("talk");

            var step = session.Next();
            while (step.Kind == DialogueStepKind.Line)
            {
                step = session.Next();
            }
            CollectionAssert.AreEqual(new[] { "Ask more", "Bye" }, step.Choices.ToArray());
        }
    }
}
