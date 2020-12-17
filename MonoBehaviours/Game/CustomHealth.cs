using KopliSoft.Behaviour;
using KopliSoft.Interaction;
using Opsive.DeathmatchAIKit;
using Opsive.DeathmatchAIKit.AI;
using Opsive.ThirdPersonController.Wrappers;
using UnityEngine;

namespace KopliSoft.Game
{
    public class CustomHealth : CharacterHealth
    {
        public static event CharacterDefeatedHandler OnCharacterDefeated;
        public delegate void CharacterDefeatedHandler(CustomHealth character);

        public static event CharacterDamagedHandler OnCharacterDamaged;
        public delegate void CharacterDamagedHandler(CustomHealth character, float amount, Vector3 position, Vector3 attackerPosition);


        private DeathmatchAgent deathmatchAgent;
        [SerializeField]
        private GameObject[] enabledOnDeath;

        private void Start()
        {
            deathmatchAgent = GetComponent<DeathmatchAgent>();
        }

        /// <summary>
        /// Override Damage to add an extra damage amount depending on the bone hit.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="force">The amount of force applied to the object while taking the damage.</param>
        /// <param name="radius">The radius of the explosive damage. If 0 then a non-exposive force will be used.</param>
        /// <param name="attacker">The GameObject that did the damage.</param>
        public override void Damage(float amount, Vector3 position, Vector3 force, float radius, GameObject attacker, GameObject hitGameObject)
        {
            if (deathmatchAgent != null)
            {
                deathmatchAgent.TrackLayerAndTag(attacker.layer, attacker.tag);
            }
            base.Damage(amount, position, force, radius, attacker, hitGameObject);

            if (amount > 0)
            {
                OnCharacterDamaged?.Invoke(this, amount, position, attacker.transform.position);
            }
        }

        /// <summary>
        /// The character has died. Report the death to interested components.
        /// </summary>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="position">The position of the force.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        protected override void Die(Vector3 force, Vector3 position, GameObject attacker)
        {
            if (TeamManager.IsInstantiated)
            {
                TeamManager.CancelBackupRequest(gameObject);
            }

            StorageInteractable storageIneractable = GetComponentInChildren<StorageInteractable>();
            if (storageIneractable != null)
            {
                storageIneractable.UnlockForever();
            }

            OnCharacterDefeated?.Invoke(this);

            foreach (GameObject gameObject in enabledOnDeath)
            {
                gameObject.SetActive(true);
            }

            base.Die(force, position, attacker);
        }
    }

}
