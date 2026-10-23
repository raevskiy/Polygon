using KopliSoft.Dialogue;
using UnityEngine;

namespace KopliSoft.Interaction
{
    /// <summary>
    /// Conversation written in ink: plays the given knot of the chapter story.
    /// </summary>
    public class InkDialogInteractable : ConversationInteractable
    {
        [Tooltip("Knot in the chapter ink story, e.g. \"librarian\"")]
        [SerializeField]
        private string knot;

        protected override bool HasConversation()
        {
            return !string.IsNullOrEmpty(knot) && InkDialogueRunner.Instance != null && !InkDialogueRunner.Instance.IsRunning;
        }

        protected override void StartConversation()
        {
            // The interviewer's tag tells ink which crew member is talking, as Fungus' "Interviewer" variable did.
            if (!InkDialogueRunner.Instance.StartDialogue(knot, m_InteractorGameObject.tag, OnConversationEnded))
            {
                OnConversationEnded();
            }
        }
    }
}
