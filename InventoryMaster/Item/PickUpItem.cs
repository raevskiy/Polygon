using UnityEngine;
using KopliSoft.Interaction;

namespace KopliSoft.Inventory
{
    public class PickUpItem : BaseInteractable
    {
        public Item item;
        private Inventory inventory;
        private GameObject player;

        public override bool CanInteract()
        {
            return m_Interactor != null && m_Interactor.GetComponentInChildren<StorageInventory>() != null;
        }

        public override void Interact()
        {
            StorageInventory storage = m_Interactor.GetComponentInChildren<StorageInventory>();
            storage.AddItemToStorage(item.itemID, item.itemValue);
            Destroy(this.gameObject);
        }
    }
}
