using KopliSoft.Investigation;
using UnityEngine;
using UnityEngine.UI;

namespace KopliSoft.Dialogue
{
    /// <summary>
    /// Shows the "write down" button while an ink line with #note:id is on screen and the note is new.
    /// Can share the button with the old WriteDown component.
    /// </summary>
    public class NoteWriteDownButton : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        private InkDialogueRunner runner;
        private string noteId;

        void Start()
        {
            runner = InkDialogueRunner.Instance;
            if (runner == null)
            {
                Debug.LogError("NoteWriteDownButton needs an InkDialogueRunner in the scene");
                enabled = false;
                return;
            }
            runner.LineShown += OnLineShown;
            runner.LineHidden += OnLineHidden;
            button.onClick.AddListener(WriteDown);
        }

        void OnDestroy()
        {
            if (runner != null)
            {
                runner.LineShown -= OnLineShown;
                runner.LineHidden -= OnLineHidden;
            }
            button.onClick.RemoveListener(WriteDown);
        }

        private void OnLineShown(DialogueLine line)
        {
            noteId = line.NoteId;
            button.gameObject.SetActive(noteId != null && !InvestigationManager.Instance.CaseFile.HasNote(noteId));
        }

        private void OnLineHidden()
        {
            noteId = null;
            button.gameObject.SetActive(false);
        }

        private void WriteDown()
        {
            if (noteId != null && runner.IsRunning)
            {
                InvestigationManager.Instance.CaseFile.AddNote(noteId);
                button.gameObject.SetActive(false);
            }
        }
    }
}
