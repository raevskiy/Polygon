using UnityEngine;

namespace KopliSoft.Interaction
{
    public class BlimpOnboarding : MonoBehaviour
    {
        [SerializeField]
        private Transform pilotTransform;
        [SerializeField]
        private GameObject deckUnderPilot;
        [SerializeField]
        private BlimpController blimpController;

        public delegate void VehicleTaken();
        public static event VehicleTaken EnterVehicleEvent;
        public static event VehicleTaken ExitVehicleEvent;

        public void SwitchOnboarding(GameObject currentCharacter, Transform pivot, bool interacting)
        {
            currentCharacter.GetComponent<CharacterBehaviour>().SetDriving(interacting);
            currentCharacter.GetComponent<Rigidbody>().isKinematic = interacting;
            currentCharacter.GetComponent<CapsuleCollider>().enabled = !interacting;
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
                blimpController.ControlPanel.enabled = interacting;
                deckUnderPilot.SetActive(!interacting);
            }
        }
    }
}

