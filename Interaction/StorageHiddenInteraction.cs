using KopliSoft.Inventory;
using KopliSoft.SceneControl;
using UnityEngine;

namespace KopliSoft.Interaction
{
    public class StorageHiddenInteraction : MonoBehaviour
    {
        [SerializeField]
        private string sourceStoragePath;
        [SerializeField]
        private string destinationStoragePath;

        private StorageInventory sourceStorage;
        private StorageInventory destinationStorage;

        void Init()
        {
            if (sourceStorage == null || destinationStorage == null)
            {
                sourceStorage = SceneGraphSearch.Find(sourceStoragePath).GetComponentInChildren<StorageInventory>();
                destinationStorage = SceneGraphSearch.Find(destinationStoragePath).GetComponentInChildren<StorageInventory>();
            }
        }

        public void Transfer(int itemId, int itemAmount)
        {
            Init();

            if (CanTransfer(sourceStorage, itemId, itemAmount))
            {
                sourceStorage.RemoveItemFromStorage(itemId, itemAmount);
                destinationStorage.AddItemToStorage(itemId, itemAmount);
            }
        }

        public void Exchange(int itemIdForward, int itemAmountForward, int itemIdBackward, int itemAmountBackward)
        {
            Init();

            if (CanTransfer(sourceStorage, itemIdForward, itemAmountForward) && CanTransfer(destinationStorage, itemIdBackward, itemAmountBackward))
            {
                sourceStorage.RemoveItemFromStorage(itemIdForward, itemAmountForward);
                destinationStorage.AddItemToStorage(itemIdForward, itemAmountForward);

                destinationStorage.RemoveItemFromStorage(itemIdBackward, itemAmountBackward);
                sourceStorage.AddItemToStorage(itemIdBackward, itemAmountBackward);
            }
        }

        public bool CanTransfer(StorageInventory storage, int itemId, int itemAmount)
        {
            Item item = storage.FindItemById(itemId);
            return item != null && item.itemValue >= itemAmount;
        }
    }

}
