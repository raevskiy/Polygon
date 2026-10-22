using System.Collections.Generic;
using System.Linq;
using KopliSoft.Controls;
using KopliSoft.Investigation;
using UnityEngine;

namespace KopliSoft.Dialogue
{
    /// <summary>
    /// Temporary IMGUI notebook to play and test the investigation before the real notebook UI exists:
    /// tasks, notes and conclusions, and combining selected cards.
    /// </summary>
    public class NotebookDebugWindow : MonoBehaviour
    {
        private bool open;
        private Vector2 scroll;
        private readonly HashSet<string> selected = new HashSet<string>();
        private string focusedTitle = "";
        private string focusedText = "";
        private string feedback = "";

        void Update()
        {
            var runner = InkDialogueRunner.Instance;
            if (GameInput.GetDown(GameAction.Notebook) && (runner == null || !runner.IsRunning) && InvestigationManager.Instance != null)
            {
                open = !open;
                if (open)
                {
                    ModalUI.Open();
                }
                else
                {
                    ModalUI.Close();
                }
            }
        }

        void OnGUI()
        {
            if (!open)
            {
                return;
            }
            var caseFile = InvestigationManager.Instance.CaseFile;
            var definition = caseFile.Definition;

            GUILayout.BeginArea(new Rect(20, 20, Mathf.Min(700, Screen.width - 40), Screen.height - 40), GUI.skin.box);
            scroll = GUILayout.BeginScrollView(scroll);

            GUILayout.Label("<b>Tasks</b> (click to read)");
            foreach (var task in caseFile.ActiveTasks.Select(definition.FindTask))
            {
                ReadableRow("[ ] " + task.title, task.title, task.text);
            }
            foreach (var task in caseFile.CompletedTasks.Select(definition.FindTask))
            {
                ReadableRow("[x] " + task.title, task.title, task.text);
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>Notes and conclusions</b> (tick two or more to combine, click a title to read)");
            foreach (var id in caseFile.Cards.ToList())
            {
                var note = definition.FindNote(id);
                var conclusion = definition.FindConclusion(id);
                var title = note != null ? note.title : conclusion.title;
                var label = note != null ? title + "  <i>(" + note.kind + ", " + note.source + ")</i>" : "<b>" + title + "</b>" + (conclusion.weak ? "  <i>(weak)</i>" : "");

                GUILayout.BeginHorizontal();
                // Selecting for combining and reading are separate, so the text shown never belongs to another card.
                var wasSelected = selected.Contains(id);
                if (GUILayout.Toggle(wasSelected, GUIContent.none, GUILayout.Width(20)) != wasSelected)
                {
                    // The result of the previous combination no longer applies.
                    feedback = "";
                    if (wasSelected)
                    {
                        selected.Remove(id);
                    }
                    else
                    {
                        selected.Add(id);
                    }
                }
                ReadableRow(label, title, note != null ? note.text : conclusion.text);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("They match"))
            {
                Combine(caseFile, Relation.Match);
            }
            if (GUILayout.Button("They contradict"))
            {
                Combine(caseFile, Relation.Contradiction);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label(feedback);
            if (focusedTitle.Length > 0)
            {
                GUILayout.Label("<b>" + focusedTitle + "</b>");
            }
            GUILayout.Label(focusedText);

            GUILayout.EndScrollView();
            GUILayout.Label("<i>" + GameInput.DisplayName(GameAction.Notebook) + " to close</i>");
            GUILayout.EndArea();
        }

        // A row whose title shows its text below; the one being read is marked.
        private void ReadableRow(string label, string title, string text)
        {
            var reading = focusedTitle == title && focusedText == text;
            if (GUILayout.Button((reading ? "> " : "") + label, GUI.skin.label))
            {
                feedback = "";
                focusedTitle = title;
                focusedText = text;
            }
        }

        private void Combine(CaseFile caseFile, Relation relation)
        {
            var result = caseFile.Combine(selected, relation);
            switch (result.Outcome)
            {
                case CombineOutcome.NewConclusion:
                    feedback = "<i>New conclusion:</i>";
                    focusedTitle = result.Conclusion.title;
                    focusedText = result.Conclusion.text;
                    selected.Clear();
                    break;
                case CombineOutcome.AlreadyKnown:
                    feedback = "I have already figured that out: " + result.Conclusion.title;
                    break;
                case CombineOutcome.Incomplete:
                    feedback = "There is something to it, but a piece is missing.";
                    break;
                case CombineOutcome.NoMatch:
                    feedback = relation == Relation.Match ? "These do not fit together." : "I see no contradiction here.";
                    break;
                default:
                    feedback = "Select at least two notes.";
                    break;
            }
        }
    }
}
