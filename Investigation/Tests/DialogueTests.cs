using System.Linq;
using NUnit.Framework;

namespace KopliSoft.Investigation.Tests
{
    public class DialogueTests
    {
        private const string CaseJson = @"{
            ""id"": ""test"",
            ""characters"": [ { ""id"": ""Yuri"", ""name"": ""Yuri Solomko"" }, { ""id"": ""Anna"", ""name"": ""Anna"" } ],
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
Yuri: A layoff was declared. #note:strike
<Yuri grins>
Note: this is not a speaker.
* { has_note(""strike"") } [Ask more]
    Yuri: More. -> DONE
+ [Bye]
    ~ start_task(""t"")
    Anna: See you.
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
            var line = DialogueLineParser.Parse("Yuri: Hello there.\n", new[] { "note:strike" }, definition);
            Assert.AreEqual("Yuri Solomko", line.SpeakerName);
            Assert.AreEqual("Hello there.", line.Text);
            Assert.AreEqual("strike", line.NoteId);
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
