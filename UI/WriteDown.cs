using Fungus;
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
            commandToItemId.TryGetValue(commandName, out itemId);
            if (storageForProtagonist.FindItemById(itemId) == null)
            {
                storageForProtagonist.AddItemToStorage(itemId, 1);
                writeDownButton.SetActive(false);
            }
        }
    }
}

