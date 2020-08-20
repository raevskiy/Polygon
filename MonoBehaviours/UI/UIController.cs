﻿using Fungus;
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
        private bool inventoryOpened;

        void Start()
        {
            BlockSignals.OnBlockStart += OnBlockStart;
            BlockSignals.OnBlockEnd += OnBlockEnd;
            Inventory.Inventory.InventoryOpen += InventoryOpened;
            Inventory.Inventory.InventoryClose += InventoryClosed;
            BlimpInteractable.VehicleEvent += VehicleDriven;
            ShowPanels.PanelOpenedEvent += IncreaseGuiCounter;
            ShowPanels.PanelClosedEvent += DecreaseGuiCounter;
        }

        private void OnDestroy()
        {
            BlockSignals.OnBlockStart -= OnBlockStart;
            BlockSignals.OnBlockEnd -= OnBlockEnd;
            Inventory.Inventory.InventoryOpen -= InventoryOpened;
            Inventory.Inventory.InventoryClose -= InventoryClosed;
            BlimpInteractable.VehicleEvent -= VehicleDriven;
            ShowPanels.PanelOpenedEvent -= IncreaseGuiCounter;
            ShowPanels.PanelClosedEvent -= DecreaseGuiCounter;
        }

        private void InventoryOpened()
        {
            IncreaseGuiCounter();
        }

        private void InventoryClosed(Inventory.Inventory inv)
        {
            DecreaseGuiCounter();
        }

        private void VehicleDriven(bool driven)
        {
            if (driven)
            {
                IncreaseGuiCounter();
            }
            else
            {
                DecreaseGuiCounter();
            }
        }

        private void OnBlockStart(Block block)
        {
            if (block.BlockName.Equals("Start"))
            {
                IncreaseGuiCounter();
                inventoryConroller.BlockInventoryUI();
            }
        }

        private void OnBlockEnd(Block block)
        {
            if (block.BlockName.Equals("End"))
            {
                DecreaseGuiCounter();
                inventoryConroller.UnblockInventoryUI();
            }
        }

        private void IncreaseGuiCounter()
        {
            guiCounter++;
            if (guiCounter == 1)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Opsive.ThirdPersonController.EventHandler.ExecuteEvent(characterSwitch.GetCurrentCharacter(), "OnAllowGameplayInput", false);
                characterSwitch.SetLocked(true);
            }
        }

        private void DecreaseGuiCounter()
        {
            guiCounter--;
            if (guiCounter == 0)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Opsive.ThirdPersonController.EventHandler.ExecuteEvent(characterSwitch.GetCurrentCharacter(), "OnAllowGameplayInput", true);
                Input.ResetInputAxes();
                characterSwitch.SetLocked(false);
            }
        }
    }
}
