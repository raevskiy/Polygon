using Opsive.ThirdPersonController;
using UnityEngine;

namespace KopliSoft.Interaction
{
    public class BlimpOnboarding : MonoBehaviour
    {
        [SerializeField]
        private Transform pilotTransform;
        [SerializeField]
        private BlimpController blimpController;

        public delegate void VehicleTaken();
        public static event VehicleTaken EnterVehicleEvent;
        public static event VehicleTaken ExitVehicleEvent;

        public void SwitchOnboarding(GameObject currentCharacter, Transform pivot, bool interacting)
        {
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

