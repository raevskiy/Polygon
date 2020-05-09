using UnityEngine;
using KopliSoft.Inventory;
using KopliSoft.Behaviour;

namespace KopliSoft.Interaction
{
    public class StorageInteractable : BaseInteractable
    {
        [SerializeField]
        private int lockLevel = 0;
        [SerializeField]
        private int pickpocketLevel = 0;
        [SerializeField]
        private StorageInventory storageInventory;

        void Start()
        {
            if (storageInventory == null)
            {
                storageInventory = GetComponent<StorageInventory>();
            }
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
                && (pickpocketLevel == 0 && lockLevel == 0 || IsInCriminalMode());
        }

        public override void Interact()
        {
            if (storageInventory != null)
            {
                if (pickpocketLevel > 0)
                {
                    PickPocket();
                }
                else if (lockLevel > 0)
                {
                    OpenLocker();
                }
                else
                {
                    OpenStorage();
                }
            }
        }

        private void OpenLocker()
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
                UnlockForever();
                OpenStorage();
            }
            else
            {
                Fungus.Flowchart flowchart = GameObject.Find("/Story/Flowcharts/Messages/lockpick_failed").GetComponent<Fungus.Flowchart>();
                flowchart.ExecuteBlock("Main");
            }

        }

        private void PickPocket()
        {
            int pickpocket = Random.Range(1, 9);
            if (pickpocket >= pickpocketLevel)
            {
                OpenStorage();
            }
            else
            {
                ShowPickPocketFailedMessage();
                AttackThief();
            }
        }

        private void ShowPickPocketFailedMessage()
        {
            Fungus.Flowchart flowchart = GameObject.Find("/Story/Flowcharts/Messages/pickpocket_failed").GetComponent<Fungus.Flowchart>();
            flowchart.ExecuteBlock("Main");
        }

        private void AttackThief()
        {
            PatrolController patrolController = GetComponentInParent<PatrolController>();
            patrolController.TrackPlayer();
            patrolController.CheckAlarm(m_InteractorGameObject);
        }

        private void OpenStorage()
        {
            storageInventory.ToggleStorage();
            m_Interactor.GetComponentInChildren<StorageInventory>().ToggleStorage();
        }

        public void UnlockForever()
        {
            lockLevel = 0;
            pickpocketLevel = 0;
        }
    }
}
