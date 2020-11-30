using UnityEngine;

namespace KopliSoft.Interaction
{
    public class BlimpInteractable : BaseInteractable
    {
        [SerializeField]
        private Transform pilotTransform;
        [SerializeField]
        private Transform[] passengerTransforms;

        private BlimpOnboarding onboardingController;

        void Start()
        {
            onboardingController = GetComponent<BlimpOnboarding>();
        }

        public override bool CanInteract()
        {
            return m_InteractorGameObject != null && !m_InteractorGameObject.GetComponent<CharacterBehaviour>().IsDriving();
        }

        public override void Interact()
        {
            Transform pivot = FindPivotTransform();

            onboardingController.InteractSwitch(pivot, true);

            m_InteractorGameObject.transform.SetParent(pivot);
            m_InteractorGameObject.transform.localPosition = Vector3.zero;
            m_InteractorGameObject.transform.localRotation = Quaternion.identity;
        }


        private Transform FindPivotTransform()
        {
            CharacterBehaviour characterBehaviour = m_InteractorGameObject.GetComponent<CharacterBehaviour>();
            if (characterBehaviour.IsAbleToDriveBlimp() && pilotTransform.childCount == 0)
            {
                return pilotTransform;
            } else
            {
                return passengerTransforms[0];
            }
        }

    }
}
