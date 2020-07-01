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

        private float breakForce;
        private BoxCollider[] lockColliders;
        private Collider keyHolder;

        void Start()
        {
            lockColliders = transform.GetComponentsInChildren<BoxCollider>();
            breakForce = door.GetComponent<HingeJoint>().breakForce;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsKeyHolderNearby(other))
            {
                SetBreakForce(Mathf.Infinity);
                SetLocksEnabled(false);
                keyHolder = other;
            }
        }

        private bool IsCharacter(Collider collider)
        {
            LayerMask layermask = LayerMask.GetMask(new string[] { "Player" });
            return layermask == (layermask | (1 << collider.gameObject.layer));
        }

        private bool IsKeyHolderNearby(Collider collider)
        {
            List<Item> items = null;
            if (collider.GetComponentInChildren<StorageInventory>() != null)
            {
                items = collider.GetComponentInChildren<StorageInventory>().storageItems;
            }

            if (items != null)
            {
                foreach (Item item in items)
                {
                    if (item.itemID == keyID)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other == keyHolder)
            {
                StartCoroutine(LockDoorWhenItIsClosed());
                keyHolder = null;
            }
        }

        IEnumerator LockDoorWhenItIsClosed()
        {
            while (Vector3.Dot(door.transform.right, transform.right) < 0.998f)
            {
                yield return new WaitForSeconds(.1f);
            }
            SetBreakForce(breakForce);
            SetLocksEnabled(true);
        }

        private void SetLocksEnabled(bool enabled)
        {
            foreach (BoxCollider lockCollider in lockColliders)
            {
                lockCollider.enabled = enabled;
            }
        }

        private void SetBreakForce(float breakForce)
        {
            HingeJoint joint = door.GetComponent<HingeJoint>();
            if (joint != null)
            {
                joint.breakForce = breakForce;
            }
        }
    }
}
