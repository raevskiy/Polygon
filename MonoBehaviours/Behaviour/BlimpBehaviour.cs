using KopliSoft.Game;
using UnityEngine;

namespace KopliSoft.Behaviour
{
    public class BlimpBehaviour : MonoBehaviour
    {
        private BlimpController controller;
        private CustomHealth health;

        // Start is called before the first frame update
        void Start()
        {
            controller = GetComponent<BlimpController>();
            health = GetComponent<CustomHealth>();
            CustomHealth.OnCharacterDefeated += OnCharacterDefeated;
        }

        private void OnCharacterDefeated(CustomHealth health)
        {
            if (this.health == health)
            {
                controller.BreakDown();
            }
        }
    }
}
