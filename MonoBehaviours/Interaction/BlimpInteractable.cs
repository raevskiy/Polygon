using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KopliSoft.Interaction
{
    public class BlimpInteractable : BaseInteractable
    {
        [SerializeField]
        private HelicopterController blimpController;
        [SerializeField]
        private Transform pilotTransform;
        private bool inProgress;
        private Transform pilotParentTransform;

        public delegate void VehicleDriven(bool driven);
        public static event VehicleDriven VehicleEvent;

        // Start is called before the first frame update
        void Start()
        {

        }

        void Update()
        {
            if (inProgress && Input.GetAxis("Mount") > 0 && blimpController.IsOnGround)
            {
                StopInteracting();
            }
        }

        public override bool CanInteract()
        {
            return !inProgress && m_Interactor != null;
        }

        public override void Interact()
        {
            InteractSwitch(true);

            pilotParentTransform = m_InteractorGameObject.transform.parent;
            m_InteractorGameObject.transform.SetParent(blimpController.transform);
            m_InteractorGameObject.transform.position = pilotTransform.position;
            m_InteractorGameObject.transform.rotation = pilotTransform.rotation;
        }

        public void StopInteracting()
        {
            InteractSwitch(false);

            m_InteractorGameObject.transform.SetParent(pilotParentTransform);
        }

        private void InteractSwitch(bool interacting)
        {
            inProgress = interacting;
            blimpController.enabled = interacting;
            blimpController.ControlPanel.enabled = interacting;

            VehicleEvent?.Invoke(interacting);
            //Opsive.ThirdPersonController.EventHandler.ExecuteEvent(m_InteractorGameObject, "OnAllowGameplayInput", !interacting);
            m_InteractorGameObject.GetComponent<Rigidbody>().isKinematic = interacting;
        }
    }
}
