using Fungus;
using KopliSoft.Dialogue;
using KopliSoft.Inventory;
using System.Collections.Generic;
using UnityEngine;

namespace KopliSoft.UI
{
    public class WriteDown : MonoBehaviour
    {
        [SerializeField]
        private GameObject writeDownButton;
        [SerializeField]
        private string[] commands;
        [SerializeField]
        private int[] itemIds;
        [SerializeField]
        private StorageInventory storageForProtagonist;

        private Dictionary<string, int> commandToItemId = new Dictionary<string, int>();
        private string commandName;

        // Start is called before the first frame update
        void Start()
        {
            BlockSignals.OnCommandExecute += OnCommandExecute;
            for (int i = 0; i < commands.Length; i++)
            {
                commandToItemId.Add(commands[i], itemIds[i]);
            }
        }

        private void OnCommandExecute(Block block, Command command, int commandIndex, int maxCommandIndex)
        {
            writeDownButton.SetActive(false);
            commandName = block.BlockName + command.ItemId;
            if (commandToItemId.ContainsKey(commandName))
            {
                writeDownButton.SetActive(true);
            }
        }

        public void CreateNote()
        {
            int itemId;
            // The button may be shared with NoteWriteDownButton: ignore clicks during ink dialogues.
            var inkRunner = InkDialogueRunner.Instance;
            if (commandName == null || (inkRunner != null && inkRunner.IsRunning) || !commandToItemId.TryGetValue(commandName, out itemId))
            {
                return;
            }
            if (storageForProtagonist.FindItemById(itemId) == null)
            {
                storageForProtagonist.AddItemToStorage(itemId, 1);
                writeDownButton.SetActive(false);
            }

            // Bridge to the new case file until this dialog is ported to ink.
            var investigation = InvestigationManager.Instance;
            if (investigation != null)
            {
                var note = investigation.CaseFile.Definition.FindNoteByLegacyItemId(itemId);
                if (note != null)
                {
                    investigation.CaseFile.AddNote(note.id);
                }
                else
                {
                    Debug.LogWarning("No note with legacyItemId " + itemId + " in the case file");
                }
            }
        }
    }
}

