using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using KopliSoft.SceneControl;
using Fungus;

namespace KopliSoft.Inventory
{
    public class StorageInventory : MonoBehaviour, IDataPersistence
    {
        public Inventory inventory;

        [SerializeField]
        private string inventoryName;

        [SerializeField]
        private string flowchartPath;

        [SerializeField]
        private List<int> trackedIds;

        [SerializeField]
        public List<Item> storageItems = new List<Item>();

        [SerializeField]
        private ItemDataBaseList itemDatabase;

        private InputManager inputManagerDatabase;

        public int itemAmount;

        private Tooltip tooltip;

        private bool opened;

        private Flowchart flowchart;

        void Start()
        {
            if (inputManagerDatabase == null)
                inputManagerDatabase = (InputManager)Resources.Load("InputManager");

            if (flowchartPath.Length > 0)
            {
                flowchart = SceneGraphSearch.Find(flowchartPath).GetComponent<Flowchart>();
            }

            ItemDataBaseList inventoryItemList = (ItemDataBaseList)Resources.Load("ItemDatabase");

            int creatingItemsForChest = 1;

            int randomItemAmount = Random.Range(1, itemAmount);

            while (creatingItemsForChest < randomItemAmount)
            {
                int randomItemNumber = Random.Range(1, inventoryItemList.itemList.Count - 1);
                int raffle = Random.Range(1, 100);

                if (raffle <= inventoryItemList.itemList[randomItemNumber].rarity)
                {
                    int randomValue = Random.Range(1, inventoryItemList.itemList[randomItemNumber].getCopy().maxStack);
                    Item item = inventoryItemList.itemList[randomItemNumber].getCopy();
                    item.itemValue = randomValue;
                    storageItems.Add(item);
                    creatingItemsForChest++;
                }
            }

            if (GameObject.FindWithTag("Tooltip") != null)
            {
                tooltip = GameObject.FindWithTag("Tooltip").GetComponent<Tooltip>();
            }

            Inventory.InventoryClose += InventoryClosed;
        }

        public void LoadItemDatabase()
        {
            if (itemDatabase == null)
                itemDatabase = (ItemDataBaseList)Resources.Load("ItemDatabase");
        }

        public void AddItemToStorage(int id, int value)
        {
            DoAddItemToStorage(id, value);

            if (inventoryName != null && flowchart != null)
            {
                string blockName = inventoryName + id + "Added";
                InformFlowchart(blockName);
            }
        }

        private void DoAddItemToStorage(int id, int quantity)
        {
            Item item = itemDatabase.getItemByID(id);
            item.itemValue = quantity;
            storageItems.Add(item);
        }

        public void RemoveItemFromStorage(int id, int value)
        {
            Item item = FindItemById(id);
            if (item != null)
            {
                item.itemValue -= value;
                if (item.itemValue <= 0)
                {
                    storageItems.Remove(item);

                    if (inventoryName != null && flowchart != null)
                    {
                        string blockName = inventoryName + id + "Removed";
                        InformFlowchart(blockName);
                    }
                }
            }
        }

        public Item FindItemById(int id)
        {
            foreach (Item item in storageItems)
            {
                if (item.itemID == id)
                {
                    return item;
                }
            }
            return null;
        }

        public bool IsStorageOpened()
        {
            return opened;
        }

        public void ToggleStorage()
        {
            if (!IsStorageOpened())
            {
                OpenStorage();
            }
            else
            {
                CloseStorage();
            }
        }

        public void OpenStorage()
        {
            inventory.deleteAllItems();
            inventory.OpenInventory();
            AddItemsToInventory();
            opened = true;
        }

        public void CloseStorage()
        {
            inventory.CloseInventory();
            DoCloseStorage();
        }

        private void DoCloseStorage()
        {
            UpdateStorage();
            inventory.deleteAllItems();
            tooltip.deactivateTooltip();
            opened = false;
        }

        private void InventoryClosed(Inventory inv)
        {
            if (IsStorageOpened() && inventory == inv)
            {
                DoCloseStorage();
            }
        }

        private void UpdateStorage()
        {
            List<Item> oldItems = new List<Item>(storageItems);
            storageItems.Clear();
            storageItems = new List<Item>(inventory.getItemList());

            if (inventoryName != null && flowchart != null)
            {
                foreach (int id in trackedIds)
                {
                    InformFlowchart(oldItems, id);
                }
            }
        }

        private void InformFlowchart(List<Item> oldItems, int id)
        {
            Item trackedItem = new Item(id);
            string blockName = "";
            if (oldItems.Contains(trackedItem) && !storageItems.Contains(trackedItem))
            {
                blockName = inventoryName + id + "Removed";
            }
            else if (!oldItems.Contains(trackedItem) && storageItems.Contains(trackedItem))
            {
                blockName = inventoryName + id + "Added";
            }

            InformFlowchart(blockName);
        }

        private void InformFlowchart(string blockName)
        {
            if (flowchart.HasBlock(blockName))
            {
                flowchart.ExecuteBlock(blockName);
            }
        }

        private void AddItemsToInventory()
        {
            foreach (Item storageItem in storageItems)
            {
                inventory.addItemToInventory(storageItem.itemID, storageItem.itemValue);
            }
            inventory.stackableSettings();
        }

        public void LoadData(GameData data)
        {
            List<StorageItemData> storageItemData = data.storageItems.GetValueOrDefault(inventoryName, new List<StorageItemData>());
            foreach (StorageItemData storageItemDatum in storageItemData)
            {
                DoAddItemToStorage(storageItemDatum.id, storageItemDatum.quantity);
            }
        }

        public void SaveData(ref GameData data)
        {
            List<StorageItemData> storageItemData = new List<StorageItemData>();
            foreach (Item item in storageItems)
            {
                storageItemData.Add(new StorageItemData(item.itemID, item.itemValue));
            }
            data.storageItems.Add(inventoryName, storageItemData);
        }
    }

}
