using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KopliSoft.Interaction
{
    public class BlimpOnboarding : MonoBehaviour
    {
        [SerializeField]
        private Transform pilotTransform;
        [SerializeField]
        private HelicopterController blimpController;

        private CharacterSwitch characterSwitch;

        public delegate void VehicleTaken();
        public static event VehicleTaken EnterVehicleEvent;
        public static event VehicleTaken ExitVehicleEvent;

        void Start()
        {
            characterSwitch = FindObjectOfType<CharacterSwitch>();
        }

        void Update()
        {
            if (Input.GetAxis("Mount") > 0 && blimpController.IsOnGround)
            {
                StopInteracting();
            }
        }

        public void StopInteracting()
        {
            Transform currentCharacterTransform = characterSwitch.GetCurrentCharacter().transform;

            SwitchOnboarding(currentCharacterTransform.parent, false);
            currentCharacterTransform.SetParent(characterSwitch.transform);
        }

        public void SwitchOnboarding(Transform pivot, bool interacting)
        {
            GameObject currentCharacter = characterSwitch.GetCurrentCharacter();
            currentCharacter.GetComponent<CharacterBehaviour>().SetDriving(interacting);
            currentCharacter.GetComponent<Rigidbody>().isKinematic = interacting;
            if (interacting)
            {
                EnterVehicleEvent?.Invoke();
            }
            else
            {
                ExitVehicleEvent?.Invoke();
            }

            if (pivot == pilotTransform)
            {
                blimpController.enabled = interacting;
                blimpController.GetComponent<Rigidbody>().isKinematic = !interacting;
                blimpController.ControlPanel.enabled = interacting;

                if (interacting)
                {
                    blimpController.SetPilot(currentCharacter.GetComponent<CharacterBehaviour>());
                }
                else
                {
                    blimpController.SetPilot(null);
                }
            }
        }
    }
}

