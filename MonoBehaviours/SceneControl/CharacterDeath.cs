using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Opsive.ThirdPersonController
{
    /// <summary>
    /// When the character respawns it should respawn in the location determined by SpawnSelection.
    /// </summary>
    public class CharacterDeath : Respawner
    {
        // Component references
        private RigidbodyCharacterController m_Controller;

        /// <summary>
        /// Cache the component references.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_Controller = GetComponent<RigidbodyCharacterController>();
        }

        /// <summary>
        /// The character should spawn. Override Spawn to allow the SpawnSelection component determine the location that the character should spawn.
        /// Call the corresponding server or client method.
        /// </summary>
        public override void Spawn()
        {
            Destroy(GameObject.Find("Menu UI"));
            SceneManager.LoadSceneAsync("Title", LoadSceneMode.Single);
        }

    }
}
