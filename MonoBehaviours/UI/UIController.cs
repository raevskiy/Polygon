using Fungus;
using UnityEngine;
using KopliSoft.Inventory;
using KopliSoft.Interaction;

namespace KopliSoft.UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField]
        private CharacterSwitch characterSwitch;
        [SerializeField]
        private InventoryController inventoryConroller;

        private int guiCounter = 0;
        private int[] characterInputCounter;

        void Start()
        {
            characterInputCounter = new int[characterSwitch.characters.Length];
            BlockSignals.OnBlockStart += OnBlockStart;
            BlockSignals.OnBlockEnd += OnBlockEnd;
            Inventory.Inventory.InventoryOpen += InventoryOpened;
            Inventory.Inventory.InventoryClose += InventoryClosed;
            ShowPanels.PanelOpenedEvent += IncreaseAllCounters;
            ShowPanels.PanelClosedEvent += DecreaseAllCounters;
            BlimpOnboarding.EnterVehicleEvent += IncreaseInputCounter;
            BlimpOnboarding.ExitVehicleEvent += DecreaseInputCounter;
        }

        private void OnDestroy()
        {
            BlockSignals.OnBlockStart -= OnBlockStart;
            BlockSignals.OnBlockEnd -= OnBlockEnd;
            Inventory.Inventory.InventoryOpen -= InventoryOpened;
            Inventory.Inventory.InventoryClose -= InventoryClosed;
            ShowPanels.PanelOpenedEvent -= IncreaseAllCounters;
            ShowPanels.PanelClosedEvent -= DecreaseAllCounters;
        }

        private void InventoryOpened()
        {
            IncreaseAllCounters();
        }

        private void InventoryClosed(Inventory.Inventory inv)
        {
            DecreaseAllCounters();
        }

        private void OnBlockStart(Block block)
        {
            if (block.BlockName.Equals("Start"))
            {
                IncreaseAllCounters();
                inventoryConroller.BlockInventoryUI();
            }
        }

        private void OnBlockEnd(Block block)
        {
            if (block.BlockName.Equals("End"))
            {
                DecreaseAllCounters();
                inventoryConroller.UnblockInventoryUI();
            }
        }

        private void IncreaseAllCounters()
        {
            guiCounter++;
            characterInputCounter[characterSwitch.GetCurrentCharacterIndex()]++;
            OnCountersIncreased();
        }

        private void DecreaseAllCounters()
        {
            guiCounter--;
            characterInputCounter[characterSwitch.GetCurrentCharacterIndex()]--;
            OnCountersDecreased();
        }

        private void IncreaseInputCounter()
        {
            characterInputCounter[characterSwitch.GetCurrentCharacterIndex()]++;
            OnCountersIncreased();
        }

        private void DecreaseInputCounter()
        {
            characterInputCounter[characterSwitch.GetCurrentCharacterIndex()]--;
            OnCountersDecreased();
        }

        private void OnCountersIncreased()
        {
            if (guiCounter == 1)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                characterSwitch.SetLocked(true);
            }
            if (characterInputCounter[characterSwitch.GetCurrentCharacterIndex()] == 1)
            {
                Opsive.ThirdPersonController.EventHandler.ExecuteEvent(characterSwitch.GetCurrentCharacter(), "OnAllowGameplayInput", false);
            }
        }

        private void OnCountersDecreased()
        {
            if (guiCounter == 0)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                characterSwitch.SetLocked(false);
            }
            if (characterInputCounter[characterSwitch.GetCurrentCharacterIndex()] == 0)
            {
                Opsive.ThirdPersonController.EventHandler.ExecuteEvent(characterSwitch.GetCurrentCharacter(), "OnAllowGameplayInput", true);
                Input.ResetInputAxes();
            }
        }
    }
}
