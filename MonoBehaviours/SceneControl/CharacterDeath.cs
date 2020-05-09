using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Opsive.ThirdPersonController
{
    public class CharacterDeath : Respawner
    {
        [SerializeField]
        private bool missionCriticalCharacter;
        private CharacterSwitch characterSwitch;

        void Start()
        {
            characterSwitch = FindObjectOfType<CharacterSwitch>();
        }

        public override void Spawn()
        {
            if (missionCriticalCharacter)
            {
                Destroy(GameObject.Find("Menu UI"));
                SceneManager.LoadSceneAsync("Title", LoadSceneMode.Single);
            }
            else if (characterSwitch.GetCurrentCharacter() == gameObject)
            {
                characterSwitch.Switch();
            }
        }
    }
}
