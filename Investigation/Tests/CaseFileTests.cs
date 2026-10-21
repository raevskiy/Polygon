using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace KopliSoft.Investigation.Tests
{
    public class CaseFileTests
    {
        private const string Json = @"{
            ""id"": ""test"",
            ""initialTasks"": [""main""],
            ""notes"": [
                { ""id"": ""a"", ""title"": ""A"", ""text"": ""a"" },
                { ""id"": ""b"", ""title"": ""B"", ""text"": ""b"" },
                { ""id"": ""c"", ""title"": ""C"", ""text"": ""c"" },
                { ""id"": ""d"", ""title"": ""D"", ""text"": ""d"" }
            ],
            ""conclusions"": [
                { ""id"": ""ab_weak"", ""title"": ""AB"", ""text"": ""ab"", ""requires"": [""a"", ""b""], ""weak"": true },
                { ""id"": ""abc"", ""title"": ""ABC"", ""text"": ""abc"", ""requires"": [""a"", ""b"", ""c""],
                  ""supersedes"": [""ab_weak""], ""startsTasks"": [""follow_up""], ""completesTasks"": [""main""] },
                { ""id"": ""cd_lie"", ""title"": ""CD"", ""text"": ""cd"", ""relation"": ""contradiction"", ""requires"": [""c"", ""d""] },
                { ""id"": ""chain"", ""title"": ""Chain"", ""text"": ""chain"", ""requires"": [""abc"", ""cd_lie""] }
            ],
            ""tasks"": [
                { ""id"": ""main"", ""title"": ""Main"", ""text"": ""main"" },
                { ""id"": ""follow_up"", ""title"": ""Follow up"", ""text"": ""follow up"" }
            ]
        }";

        private CaseFile caseFile;

        [SetUp]
        public void SetUp()
        {
            caseFile = new CaseFile(CaseDefinition.FromJson(Json));
        }

        private void Collect(params string[] ids)
        {
            foreach (var id in ids)
            {
                Assert.IsTrue(caseFile.AddNote(id), id);
            }
        }

        [Test]
        public void InitialTasksAreActive()
        {
            CollectionAssert.AreEqual(new[] { "main" }, caseFile.ActiveTasks);
        }

        [Test]
        public void AddNoteRejectsUnknownAndDuplicates()
        {
            Assert.IsTrue(caseFile.AddNote("a"));
            Assert.IsFalse(caseFile.AddNote("a"));
            Assert.IsFalse(caseFile.AddNote("nope"));
            CollectionAssert.AreEqual(new[] { "a" }, caseFile.Notes);
        }

        [Test]
        public void CombineIgnoresOrder()
        {
            Collect("a", "b");
            var result = caseFile.Combine(new[] { "b", "a" }, Relation.Match);
            Assert.AreEqual(CombineOutcome.NewConclusion, result.Outcome);
            Assert.AreEqual("ab_weak", result.Conclusion.id);
            Assert.AreEqual(CombineOutcome.AlreadyKnown, caseFile.Combine(new[] { "a", "b" }, Relation.Match).Outcome);
        }

        [Test]
        public void RelationMustMatch()
        {
            Collect("c", "d");
            Assert.AreEqual(CombineOutcome.NoMatch, caseFile.Combine(new[] { "c", "d" }, Relation.Match).Outcome);
            Assert.AreEqual(CombineOutcome.NewConclusion, caseFile.Combine(new[] { "c", "d" }, Relation.Contradiction).Outcome);
        }

        [Test]
        public void SelectionMustBeOwnedCards()
        {
            Collect("a");
            Assert.AreEqual(CombineOutcome.InvalidSelection, caseFile.Combine(new[] { "a", "b" }, Relation.Match).Outcome);
            Assert.AreEqual(CombineOutcome.InvalidSelection, caseFile.Combine(new[] { "a" }, Relation.Match).Outcome);
        }

        [Test]
        public void PartOfARuleIsIncomplete()
        {
            Collect("a", "c");
            Assert.AreEqual(CombineOutcome.Incomplete, caseFile.Combine(new[] { "a", "c" }, Relation.Match).Outcome);
        }

        [Test]
        public void StrongConclusionSupersedesWeakAndUpdatesTasks()
        {
            Collect("a", "b", "c");
            caseFile.Combine(new[] { "a", "b" }, Relation.Match);
            var started = new List<string>();
            caseFile.TaskStarted += t => started.Add(t.id);

            Assert.AreEqual(CombineOutcome.NewConclusion, caseFile.Combine(new[] { "a", "b", "c" }, Relation.Match).Outcome);

            Assert.IsTrue(caseFile.IsSuperseded("ab_weak"));
            Assert.IsFalse(caseFile.Cards.Contains("ab_weak"));
            Assert.IsTrue(caseFile.Cards.Contains("abc"));
            Assert.IsTrue(caseFile.IsTaskDone("main"));
            Assert.IsTrue(caseFile.IsTaskActive("follow_up"));
            CollectionAssert.AreEqual(new[] { "follow_up" }, started);
        }

        [Test]
        public void ConclusionsCombineFurther()
        {
            Collect("a", "b", "c", "d");
            caseFile.Combine(new[] { "a", "b", "c" }, Relation.Match);
            caseFile.Combine(new[] { "c", "d" }, Relation.Contradiction);
            Assert.AreEqual("chain", caseFile.Combine(new[] { "abc", "cd_lie" }, Relation.Match).Conclusion.id);
        }

        [Test]
        public void StateRoundTrips()
        {
            Collect("a", "b", "c");
            caseFile.Combine(new[] { "a", "b" }, Relation.Match);
            caseFile.Combine(new[] { "a", "b", "c" }, Relation.Match);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(caseFile.CaptureState());

            var restored = new CaseFile(caseFile.Definition);
            restored.RestoreState(Newtonsoft.Json.JsonConvert.DeserializeObject<CaseFileState>(json));

            CollectionAssert.AreEqual(caseFile.Cards.ToList(), restored.Cards.ToList());
            CollectionAssert.AreEqual(caseFile.ActiveTasks, restored.ActiveTasks);
            CollectionAssert.AreEqual(caseFile.CompletedTasks, restored.CompletedTasks);
        }

        [Test]
        public void RestoreDropsIdsRemovedFromData()
        {
            var state = new CaseFileState { notes = { "a", "removed_note" }, activeTasks = { "removed_task" } };
            caseFile.RestoreState(state);
            CollectionAssert.AreEqual(new[] { "a" }, caseFile.Notes);
            Assert.IsEmpty(caseFile.ActiveTasks);
        }
    }
}
