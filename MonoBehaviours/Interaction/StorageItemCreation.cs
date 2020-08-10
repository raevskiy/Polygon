using KopliSoft.Inventory;
using UnityEngine;

namespace KopliSoft.Interaction
{
    public class StorageItemCreation : MonoBehaviour
    {
        private CharacterSwitch characterSwitch;

        // Start is called before the first frame update
        void Start()
        {
            characterSwitch = FindObjectOfType<CharacterSwitch>();
        }

        public void Create(int itemId, int itemAmount)
        {
            StorageInventory destinationStorage = characterSwitch.GetCurrentCharacter().GetComponentInChildren<StorageInventory>();
            destinationStorage.AddItemToStorage(itemId, itemAmount);

        }
    }
}
