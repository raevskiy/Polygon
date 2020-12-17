using KopliSoft.Game;
using MalbersAnimations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KopliSoft.Behaviour
{
    public class HorseBehaviour : MonoBehaviour
    {
        private CustomHealth health;
        private Animal animal;

        // Start is called before the first frame update
        void Start()
        {
            health = GetComponent<CustomHealth>();
            animal = GetComponent<Animal>();
            CustomHealth.OnCharacterDamaged += OnCharacterDamaged;
        }

        private void OnCharacterDamaged(CustomHealth health, float amount, Vector3 position, Vector3 attackerPosition)
        {
            if (this.health == health)
            {
                animal.getDamaged(position, attackerPosition, amount);
            }
        }
    }
}
