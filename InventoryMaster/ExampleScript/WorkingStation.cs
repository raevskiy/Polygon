using UnityEngine;
using System.Collections;

namespace KopliSoft.Inventory
{
    public class WorkingStation : MonoBehaviour
    {
        public KeyCode openInventory;
        public CraftSystem cS;
        public int distanceToOpenWorkingStation = 3;

        private bool showCraftSystem;
        private Inventory craftInventory;
        private GameObject inventoryInstance;

        void Start()
        {
            inventoryInstance = GameObject.FindWithTag("InventoryController");
            craftInventory = cS.GetComponent<Inventory>();
        }

        void Update()
        {
            float distance = Vector3.Distance(transform.position, inventoryInstance.transform.position);

            if (Input.GetKeyDown(openInventory) && distance <= distanceToOpenWorkingStation)
            {
                showCraftSystem = !showCraftSystem;
                if (showCraftSystem)
                {
                    craftInventory.OpenInventory();
                }
                else
                {
                    cS.backToInventory();
                    craftInventory.CloseInventory();
                }
            }
            if (showCraftSystem && distance > distanceToOpenWorkingStation)
            {
                cS.backToInventory();
                craftInventory.CloseInventory();
            }
        }
    }

}
