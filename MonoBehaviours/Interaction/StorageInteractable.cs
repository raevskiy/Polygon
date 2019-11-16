using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Opsive.ThirdPersonController;
using KopliSoft.Inventory;

namespace KopliSoft.Interaction
{
    public class StorageInteractable : BaseInteractable
    {
        [SerializeField]
        private int lockLevel = 0;
        [SerializeField]
        private int pickpocketLevel = 0;

        private StorageInventory storageInventory;

        void Start()
        {
            storageInventory = GetComponent<StorageInventory>();
        }

        protected override void OnTriggerExit(Collider other)
        {
            base.OnTriggerExit(other);
            if (other.transform.Equals(m_Interactor) && storageInventory != null && storageInventory.IsStorageOpened())
            {
                storageInventory.CloseStorage();
                m_Interactor.GetComponentInChildren<StorageInventory>().CloseStorage();
            }
        }

        public override bool CanInteract()
        {
            return m_Interactor != null && m_Interactor.GetComponentInChildren<StorageInventory>() != null
                && (pickpocketLevel == 0 || IsInCriminalMode());    //We can interact with storages in normal mode as well, only pickpockets require criminal mode
        }

        public override void Interact()
        {
            if (storageInventory != null)
            {
                if (pickpocketLevel > 0)
                {
                    pickPocket();
                }
                else
                {
                    openLocker();
                }
            }
        }

        private void openLocker()
        {
            if (lockLevel > 0 && !IsInCriminalMode())
            {
                Fungus.Flowchart flowchart = GameObject.Find("/Story/Flowcharts/Messages/lockpick_requires_criminal_mode").GetComponent<Fungus.Flowchart>();
                flowchart.ExecuteBlock("Main");
                return;
            }

            int lockpick = Random.Range(1, 9);
            if (lockpick >= lockLevel)
            {
                lockLevel = 0;  //Now it is inlocked forever
                storageInventory.ToggleStorage();
                m_Interactor.GetComponentInChildren<StorageInventory>().ToggleStorage();
            }
            else
            {
                Fungus.Flowchart flowchart = GameObject.Find("/Story/Flowcharts/Messages/lockpick_failed").GetComponent<Fungus.Flowchart>();
                flowchart.ExecuteBlock("Main");
            }

        }

        private void pickPocket()
        {
            int pickpocket = Random.Range(1, 9);
            if (pickpocket >= pickpocketLevel)
            {
                storageInventory.ToggleStorage();
                m_Interactor.GetComponentInChildren<StorageInventory>().ToggleStorage();
            }
            else
            {
                Fungus.Flowchart flowchart = GameObject.Find("/Story/Flowcharts/Messages/pickpocket_failed").GetComponent<Fungus.Flowchart>();
                flowchart.ExecuteBlock("Main");
            }
        }
    }
}
