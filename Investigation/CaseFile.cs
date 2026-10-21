using System;
using System.Collections.Generic;
using System.Linq;

namespace KopliSoft.Investigation
{
    public enum CombineOutcome
    {
        // A new conclusion was reached.
        NewConclusion,
        // The combination is right, but this conclusion was reached before.
        AlreadyKnown,
        // The selection is a part of a correct combination, something is missing.
        Incomplete,
        // The selected cards do not combine this way.
        NoMatch,
        // Fewer than two cards, or cards that Anna does not have.
        InvalidSelection
    }

    public sealed class CombineResult
    {
        public CombineOutcome Outcome { get; private set; }
        public ConclusionDefinition Conclusion { get; private set; }

        public CombineResult(CombineOutcome outcome, ConclusionDefinition conclusion = null)
        {
            Outcome = outcome;
            Conclusion = conclusion;
        }
    }

    /// <summary>
    /// Serializable progress of a case, stored in the save game.
    /// </summary>
    public class CaseFileState
    {
        public string caseId;
        public List<string> notes = new List<string>();
        public List<string> conclusions = new List<string>();
        public List<string> superseded = new List<string>();
        public List<string> activeTasks = new List<string>();
        public List<string> completedTasks = new List<string>();
    }

    /// <summary>
    /// Anna's progress in a case: notes written down, conclusions reached and her tasks.
    /// Notes and conclusions are both "cards" that can be combined into further conclusions.
    /// </summary>
    public class CaseFile
    {
        public event Action<NoteDefinition> NoteAdded;
        public event Action<ConclusionDefinition> ConclusionReached;
        public event Action<TaskDefinition> TaskStarted;
        public event Action<TaskDefinition> TaskCompleted;
        // Any change, including a restored state.
        public event Action Changed;

        public CaseDefinition Definition { get; private set; }

        private readonly List<string> notes = new List<string>();
        private readonly List<string> conclusions = new List<string>();
        private readonly HashSet<string> superseded = new HashSet<string>();
        private readonly List<string> activeTasks = new List<string>();
        private readonly List<string> completedTasks = new List<string>();

        public CaseFile(CaseDefinition definition)
        {
            Definition = definition;
            foreach (var taskId in definition.initialTasks)
            {
                if (definition.FindTask(taskId) != null && !activeTasks.Contains(taskId))
                {
                    activeTasks.Add(taskId);
                }
            }
        }

        public IReadOnlyList<string> Notes { get { return notes; } }
        public IReadOnlyList<string> Conclusions { get { return conclusions; } }
        public IReadOnlyList<string> ActiveTasks { get { return activeTasks; } }
        public IReadOnlyList<string> CompletedTasks { get { return completedTasks; } }

        /// <summary>
        /// Everything that can be selected for combining: notes and conclusions that are not superseded.
        /// </summary>
        public IEnumerable<string> Cards
        {
            get { return notes.Concat(conclusions.Where(c => !superseded.Contains(c))); }
        }

        public bool HasNote(string id) { return notes.Contains(id); }
        public bool HasConclusion(string id) { return conclusions.Contains(id); }
        public bool IsSuperseded(string id) { return superseded.Contains(id); }
        public bool IsTaskActive(string id) { return activeTasks.Contains(id); }
        public bool IsTaskDone(string id) { return completedTasks.Contains(id); }

        public bool HasCard(string id)
        {
            return HasNote(id) || (HasConclusion(id) && !IsSuperseded(id));
        }

        /// <summary>
        /// Returns false if the note is unknown or already written down.
        /// </summary>
        public bool AddNote(string id)
        {
            var note = Definition.FindNote(id);
            if (note == null || notes.Contains(id))
            {
                return false;
            }
            notes.Add(id);
            Raise(NoteAdded, note);
            RaiseChanged();
            return true;
        }

        public bool StartTask(string id)
        {
            var task = Definition.FindTask(id);
            if (task == null || activeTasks.Contains(id) || completedTasks.Contains(id))
            {
                return false;
            }
            activeTasks.Add(id);
            Raise(TaskStarted, task);
            RaiseChanged();
            return true;
        }

        /// <summary>
        /// Completes a task, active or not (a task can be solved before Anna sets it for herself).
        /// </summary>
        public bool CompleteTask(string id)
        {
            var task = Definition.FindTask(id);
            if (task == null || completedTasks.Contains(id))
            {
                return false;
            }
            activeTasks.Remove(id);
            completedTasks.Add(id);
            Raise(TaskCompleted, task);
            RaiseChanged();
            return true;
        }

        public CombineResult Combine(IEnumerable<string> cardIds, Relation relation)
        {
            var selection = new HashSet<string>(cardIds);
            if (selection.Count < 2 || !selection.All(HasCard))
            {
                return new CombineResult(CombineOutcome.InvalidSelection);
            }

            var candidates = Definition.conclusions.Where(c => c.relation == relation).ToList();
            var exact = candidates.FirstOrDefault(c => selection.SetEquals(c.requires));
            if (exact != null)
            {
                if (HasConclusion(exact.id))
                {
                    return new CombineResult(CombineOutcome.AlreadyKnown, exact);
                }
                Apply(exact);
                return new CombineResult(CombineOutcome.NewConclusion, exact);
            }

            var partOfSomething = candidates.Any(c => !HasConclusion(c.id) && selection.IsProperSubsetOf(c.requires));
            return new CombineResult(partOfSomething ? CombineOutcome.Incomplete : CombineOutcome.NoMatch);
        }

        private void Apply(ConclusionDefinition conclusion)
        {
            conclusions.Add(conclusion.id);
            foreach (var id in conclusion.supersedes)
            {
                superseded.Add(id);
            }
            Raise(ConclusionReached, conclusion);
            foreach (var id in conclusion.completesTasks)
            {
                CompleteTask(id);
            }
            foreach (var id in conclusion.startsTasks)
            {
                StartTask(id);
            }
            RaiseChanged();
        }

        public CaseFileState CaptureState()
        {
            return new CaseFileState
            {
                caseId = Definition.id,
                notes = new List<string>(notes),
                conclusions = new List<string>(conclusions),
                superseded = new List<string>(superseded),
                activeTasks = new List<string>(activeTasks),
                completedTasks = new List<string>(completedTasks)
            };
        }

        /// <summary>
        /// Replaces the progress with a saved one. Ids that no longer exist in the definition are dropped.
        /// </summary>
        public void RestoreState(CaseFileState state)
        {
            notes.Clear();
            conclusions.Clear();
            superseded.Clear();
            activeTasks.Clear();
            completedTasks.Clear();

            notes.AddRange(state.notes.Where(id => Definition.FindNote(id) != null).Distinct());
            conclusions.AddRange(state.conclusions.Where(id => Definition.FindConclusion(id) != null).Distinct());
            superseded.UnionWith(state.superseded);
            completedTasks.AddRange(state.completedTasks.Where(id => Definition.FindTask(id) != null).Distinct());
            activeTasks.AddRange(state.activeTasks.Where(id => Definition.FindTask(id) != null && !completedTasks.Contains(id)).Distinct());
            RaiseChanged();
        }

        private static void Raise<T>(Action<T> handler, T value)
        {
            if (handler != null)
            {
                handler(value);
            }
        }

        private void RaiseChanged()
        {
            if (Changed != null)
            {
                Changed();
            }
        }
    }
}
