using KopliSoft.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KopliSoft.Interaction
{
    public class Lock : MonoBehaviour
    {
        [SerializeField]
        private int keyID;
        [SerializeField]
        private Transform door;

        private BoxCollider[] lockColliders;
        private Collider keyHolder;

        void Start()
        {
            lockColliders = transform.GetComponentsInChildren<BoxCollider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            List<Item> items = null;
            if ("Player".Equals(other.gameObject.tag))
            {
                items = GameObject.Find("/Inventories/GUI/Panel - Player Inventory").GetComponent<KopliSoft.Inventory.Inventory>().getItemList();
            }
            else if (other.GetComponentInChildren<StorageInventory>() != null)
            {
                items = other.GetComponentInChildren<StorageInventory>().storageItems;
            }

            if (items != null)
            {
                foreach (Item item in items)
                {
                    if (item.itemID == keyID)
                    {
                        setLocksEnabled(false);
                        keyHolder = other;
                        return;
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other == keyHolder)
            {
                StartCoroutine(CheckDoorZeroRotation());
                keyHolder = null;
            }
        }

        IEnumerator CheckDoorZeroRotation()
        {
            while (Vector3.Dot(door.transform.right, transform.right) < 0.998f)
            {
                yield return new WaitForSeconds(.1f);
            }
            setLocksEnabled(true);
        }

        private void setLocksEnabled(bool enabled)
        {
            foreach (BoxCollider lockCollider in lockColliders)
            {
                lockCollider.enabled = enabled;
            }
        }
    }

}
