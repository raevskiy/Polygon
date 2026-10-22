using System.Linq;
using NUnit.Framework;
using Story = Ink.Runtime.Story;

namespace KopliSoft.Investigation.Tests
{
    /// <summary>
    /// The airplanes at the Flying Club (flying_club_airplanes.ink, ported from the Fungus "* Airplane" flowcharts).
    /// </summary>
    public class FlyingClubAirplanesTests
    {
        private Story story;
        private DialogueSession session;

        [SetUp]
        public void SetUp()
        {
            var chapter = StoryFiles.ChapterFolders().Single(c => c.EndsWith("Chapter1"));
            var definition = StoryFiles.LoadCase(chapter);
            story = StoryFiles.CompileChapter(chapter);
            InkCaseBindings.Bind(story, new CaseFile(definition), message => Assert.Fail(message));
            session = new DialogueSession(story, definition);
        }

        [TestCase("airplane_trimotor", "plane_trimotor", "CIV-804-84103")]
        [TestCase("airplane_british", "plane_british", "CIV-804-485")]
        [TestCase("airplane_kit", "plane_unregistered", "no registration")]
        public void ExaminingOffersTheNoteAndRemembersIt(string knot, string note, string text)
        {
            session.Begin(knot);
            var menu = session.Next();
            CollectionAssert.AreEqual(new[] { "Examine the airplane", "Go away" }, menu.Choices);
            session.Choose(0);

            var line = session.Next().Line;
            Assert.AreEqual("Anna", line.SpeakerName);
            Assert.AreEqual(note, line.NoteId);
            Assert.That(line.Text, Does.Contain(text));
            Assert.AreEqual(DialogueStepKind.End, session.Next().Kind);
            Assert.IsTrue((bool)story.variablesState[knot + "_examined"]);
        }

        [Test]
        public void GoingAwayChangesNothing()
        {
            session.Begin("airplane_trimotor");
            session.Next();
            session.Choose(1);
            Assert.AreEqual(DialogueStepKind.End, session.Next().Kind);
            Assert.IsFalse((bool)story.variablesState["airplane_trimotor_examined"]);
        }
    }
}
