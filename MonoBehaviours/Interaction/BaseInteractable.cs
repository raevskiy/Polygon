using Opsive.ThirdPersonController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KopliSoft.Interaction
{
    public abstract class BaseInteractable : MonoBehaviour, IInteractable
    {
        [Tooltip("The ID of the Interactable. Used for ability filtering by the character. -1 indicates no ID")]
        [SerializeField]
        protected int m_ID = -1;
        [Tooltip("The layer of objects that can perform the interaction")]
        [SerializeField]
        protected LayerMask m_InteractorLayer;
        [Tooltip("The offset that the interactor should move to when interacting. A value of -1 means no movement on that axis")]
        [SerializeField]
        protected Vector3 m_TargetInteractorOffset = new Vector3(-1, -1, -1);

        protected Transform m_Interactor;
        protected Transform m_Transform;
        protected GameObject m_InteractorGameObject;

        public abstract bool CanInteract();
        public abstract void Interact();

        private void Awake()
        {
            m_Transform = transform;
            // Activate when an object with the specified interactor layer is within the trigger.
            enabled = false;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (Utility.InLayerMask(other.gameObject.layer, m_InteractorLayer.value))
            {
                m_Interactor = other.transform;
                m_InteractorGameObject = other.gameObject;
                EventHandler.ExecuteEvent<IInteractable>(m_Interactor.gameObject, "OnInteractableHasInteractable", this);
                enabled = true;
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.transform.Equals(m_Interactor))
            {
                EventHandler.ExecuteEvent<IInteractable>(m_Interactor.gameObject, "OnInteractableHasInteractable", null);
                m_Interactor = null;
                enabled = false;
            }
        }

        public int GetInteractableID()
        {
            return m_ID;
        }

        public bool RequiresTargetInteractorPosition()
        {
            return m_TargetInteractorOffset.x != -1 || m_TargetInteractorOffset.y != -1 || m_TargetInteractorOffset.z != -1;
        }

        public Quaternion GetTargetInteractorRotation()
        {
            // The character should be facing the opposite direction the interactable object is facing.
            return Quaternion.LookRotation(-m_Transform.forward);
        }

        public Vector3 GetTargetInteractorPosition(Transform interactorTransform)
        {
            // Ignore the axis if the offset has a -1 value.
            var position = m_Transform.InverseTransformPoint(interactorTransform.position);
            if (m_TargetInteractorOffset.x != -1)
            {
                position.x = m_TargetInteractorOffset.x;
            }
            if (m_TargetInteractorOffset.y != -1)
            {
                position.y = m_TargetInteractorOffset.y;
            }
            if (m_TargetInteractorOffset.z != -1)
            {
                position.z = m_TargetInteractorOffset.z;
            }
            return m_Transform.TransformPoint(position);
        }

        protected bool IsInCriminalMode()
        {
            Criminal criminal = m_Interactor.GetComponent<Criminal>();
            return criminal != null && criminal.IsInCriminalMode();
        }
    }
}
