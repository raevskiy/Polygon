using UnityEngine;

namespace KopliSoft.Interaction
{
    public class ExitBlimpInteractable : BaseInteractable
    {
        private BlimpOnboarding onboardingController;
        private CharacterSwitch characterSwitch;
        private BlimpController blimpController;

        // Start is called before the first frame update
        void Start()
        {
            onboardingController = GetComponentInParent<BlimpOnboarding>();
            blimpController = GetComponentInParent<BlimpController>();
            characterSwitch = FindObjectOfType<CharacterSwitch>();
        }

        public override bool CanInteract()
        {
            if (m_InteractorGameObject != null)
            {
                CharacterBehaviour characterBehaviour = m_InteractorGameObject.GetComponent<CharacterBehaviour>();
                return characterBehaviour.IsDriving() && blimpController.GetComponent<Rigidbody>().velocity.magnitude < 1;
            }
            return false;
        }

        public override void Interact()
        {
            onboardingController.SwitchOnboarding(m_InteractorGameObject, m_Interactor.parent, false);
            m_Interactor.SetParent(characterSwitch.transform);
        }

    }
}