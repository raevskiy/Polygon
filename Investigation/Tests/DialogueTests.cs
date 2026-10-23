using System.Linq;
using NUnit.Framework;

namespace KopliSoft.Investigation.Tests
{
    public class DialogueTests
    {
        private const string CaseJson = @"{
            ""id"": ""test"",
            ""characters"": [ { ""id"": ""Guide"", ""name"": ""Museum Guide"" }, { ""id"": ""Visitor"", ""name"": ""Visitor"" } ],
            ""notes"": [ { ""id"": ""opening_hours"", ""title"": ""Opening hours"", ""text"": ""opening hours"" } ],
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
Guide: The museum opens at nine. #note:opening_hours
<The guide smiles>
Note: this is not a speaker.
* { has_note(""opening_hours"") } [Ask more]
    Guide: More. -> DONE
+ [Bye]
    ~ start_task(""t"")
    Visitor: See you.
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
            var line = DialogueLineParser.Parse("Guide: Hello there.\n", new[] { "note:opening_hours" }, definition);
            Assert.AreEqual("Museum Guide", line.SpeakerName);
            Assert.AreEqual("Hello there.", line.Text);
            Assert.AreEqual("opening_hours", line.NoteId);
        }

        [Test]
        public void ViaTagAddsTheChannelToTheShownName()
        {
            var line = DialogueLineParser.Parse("Guide: Good evening.", new[] { "via:radio" }, definition);
            Assert.AreEqual("Museum Guide", line.SpeakerName);
            Assert.AreEqual("Museum Guide <over the radio>", line.DisplayName);
            Assert.AreEqual("Museum Guide <over the speaking tube>", DialogueLineParser.Parse("Guide: Fine.", new[] { "via:tube" }, definition).DisplayName);
            Assert.AreEqual("Museum Guide", DialogueLineParser.Parse("Guide: Hi.", null, definition).DisplayName);
        }

        [Test]
        public void LineBreakMarkerBecomesNewline()
        {
            var line = DialogueLineParser.Parse("Date | Status<br>01.01.2000 | Open", null, definition);
            Assert.AreEqual("Date | Status\n01.01.2000 | Open", line.Text);
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
            Assert.AreEqual("opening_hours", first.Line.NoteId);
            Assert.AreEqual("<The guide smiles>", session.Next().Line.Text);
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
            caseFile.AddNote("opening_hours");
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
