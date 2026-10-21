using System;
using System.Collections.Generic;
using Fungus;
using Ink.Runtime;
using KopliSoft.Investigation;
using UnityEngine;

namespace KopliSoft.Dialogue
{
    /// <summary>
    /// Plays ink conversations through the existing Fungus Say and Menu dialogs, so they look like the old ones.
    /// </summary>
    public class InkDialogueRunner : MonoBehaviour
    {
        public static event Action<string> DialogueStarted;
        public static event Action<string> DialogueEnded;

        public event Action<DialogueLine> LineShown;
        public event Action LineHidden;

        [Tooltip("Leave empty to use the active Fungus Say Dialog")]
        [SerializeField]
        private SayDialog sayDialog;
        [Tooltip("Leave empty to use the active Fungus Menu Dialog")]
        [SerializeField]
        private MenuDialog menuDialog;

        public static InkDialogueRunner Instance { get; private set; }

        public bool IsRunning { get; private set; }
        public DialogueLine CurrentLine { get; private set; }

        private DialogueSession session;
        private string knot;
        private Action onEnded;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("Found multiple ink dialogue runners in the scene");
            }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Starts the conversation in the given knot. Returns false if it cannot start.
        /// </summary>
        public bool StartDialogue(string knot, string interviewer, Action onEnded)
        {
            var manager = InvestigationManager.Instance;
            if (IsRunning || manager == null)
            {
                Debug.LogWarning("Cannot start ink dialogue '" + knot + "': " + (IsRunning ? "another one is running" : "no InvestigationManager in the scene"));
                return false;
            }

            var story = manager.Story;
            if (interviewer != null && story.variablesState.GlobalVariableExistsWithName("interviewer"))
            {
                story.variablesState["interviewer"] = interviewer;
            }
            session = new DialogueSession(story, manager.CaseFile.Definition);
            try
            {
                session.Begin(knot);
            }
            catch (StoryException e)
            {
                Debug.LogError("Ink knot '" + knot + "' not found: " + e.Message);
                session = null;
                return false;
            }

            IsRunning = true;
            this.knot = knot;
            this.onEnded = onEnded;
            ModalUI.Open();
            if (DialogueStarted != null)
            {
                DialogueStarted(knot);
            }
            Advance();
            return true;
        }

        private void Advance()
        {
            DialogueStep step;
            try
            {
                step = session.Next();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                step = DialogueStep.End;
            }

            switch (step.Kind)
            {
                case DialogueStepKind.Line:
                    ShowLine(step.Line);
                    break;
                case DialogueStepKind.Choices:
                    ShowChoices(step.Choices);
                    break;
                default:
                    Finish();
                    break;
            }
        }

        private void ShowLine(DialogueLine line)
        {
            var dialog = GetSayDialog();
            dialog.SetActive(true);
            var character = FindCharacter(line.SpeakerName);
            dialog.SetCharacter(character);
            if (character == null && line.SpeakerName != null)
            {
                dialog.SetCharacterName(line.SpeakerName, Color.white);
            }

            CurrentLine = line;
            if (LineShown != null)
            {
                LineShown(line);
            }
            // Fade when done, like Fungus' Say command: the line disappears once clicked unless another line follows,
            // so a menu is shown without the previous line still on screen.
            dialog.Say(line.Text, true, true, true, true, false, null, OnLineComplete);
        }

        private void OnLineComplete()
        {
            CurrentLine = null;
            if (LineHidden != null)
            {
                LineHidden();
            }
            Advance();
        }

        private void ShowChoices(IReadOnlyList<string> choices)
        {
            var menu = GetMenuDialog();
            menu.SetActive(true);
            menu.Clear();

            var buttons = menu.CachedButtons;
            if (choices.Count > buttons.Length)
            {
                Debug.LogWarning("Ink knot '" + knot + "' offers " + choices.Count + " choices, the menu has only " + buttons.Length + " buttons");
            }
            for (int i = 0; i < choices.Count && i < buttons.Length; i++)
            {
                int index = i;
                var button = buttons[i];
                button.gameObject.SetActive(true);
                button.interactable = true;
                var textAdapter = new TextAdapter();
                textAdapter.InitFromGameObject(button.gameObject, true);
                if (textAdapter.HasTextObject())
                {
                    textAdapter.Text = choices[i];
                }
                button.onClick.AddListener(() => OnChoice(index));
            }
        }

        private void OnChoice(int index)
        {
            var menu = GetMenuDialog();
            menu.Clear();
            menu.SetActive(false);
            session.Choose(index);
            Advance();
        }

        private void Finish()
        {
            IsRunning = false;
            session = null;
            CurrentLine = null;
            GetSayDialog().FadeWhenDone = true;

            ModalUI.Close();
            if (DialogueEnded != null)
            {
                DialogueEnded(knot);
            }
            var callback = onEnded;
            onEnded = null;
            if (callback != null)
            {
                callback();
            }
        }

        private SayDialog GetSayDialog()
        {
            return sayDialog != null ? sayDialog : SayDialog.GetSayDialog();
        }

        private MenuDialog GetMenuDialog()
        {
            return menuDialog != null ? menuDialog : MenuDialog.GetMenuDialog();
        }

        // Reuses the Fungus Character with the same name for its color, portrait and sound.
        private static Character FindCharacter(string name)
        {
            if (name == null)
            {
                return null;
            }
            foreach (var character in Character.ActiveCharacters)
            {
                if (character.NameText == name)
                {
                    return character;
                }
            }
            return null;
        }
    }
}
