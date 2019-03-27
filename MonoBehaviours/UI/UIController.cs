using Fungus;
using UnityEngine;

namespace KopliSoft.UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField]
        private GameObject hero;
        private int guiCounter = 0;
        private bool inventoryOpened;

        void Start()
        {
            BlockSignals.OnBlockStart += OnBlockStart;
            BlockSignals.OnBlockEnd += OnBlockEnd;
            Inventory.Inventory.InventoryOpen += InventoryOpened;
            Inventory.Inventory.AllInventoriesClosed += AllInventoriesClosed;
        }

        private void InventoryOpened()
        {
            if (!inventoryOpened)
            {
                increaseGuiCounter();
                inventoryOpened = true;
            }
        }

        private void AllInventoriesClosed()
        {
            if (inventoryOpened)
            {
                decreaseGuiCounter();
                inventoryOpened = false;
            }
        }

        private void OnBlockStart(Block block)
        {
            if (block.BlockName.Equals("Start"))
            {
                increaseGuiCounter();
            }
        }

        private void OnBlockEnd(Block block)
        {
            if (block.BlockName.Equals("End"))
            {
                decreaseGuiCounter();
            }
        }

        private void increaseGuiCounter()
        {
            guiCounter++;
            if (guiCounter == 1)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Opsive.ThirdPersonController.EventHandler.ExecuteEvent(hero, "OnAllowGameplayInput", false);
            }
        }

        private void decreaseGuiCounter()
        {
            guiCounter--;
            if (guiCounter == 0)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Opsive.ThirdPersonController.EventHandler.ExecuteEvent(hero, "OnAllowGameplayInput", true);
                Input.ResetInputAxes();
            }
        }
    }
}
