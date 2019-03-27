using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace KopliSoft.Interaction
{
    public class CutsceneTrigger : MonoBehaviour
    {
        [SerializeField]
        private GameObject cutsceneInitiator;
        [SerializeField]
        private PlayableDirector playableDirector;

        private bool cutscenePlayed;

        private void OnTriggerEnter(Collider other)
        {
            if (!cutscenePlayed && other.gameObject == cutsceneInitiator)
            {
                playableDirector.gameObject.SetActive(true);
                playableDirector.enabled = true;
                cutscenePlayed = true;
            }
        }

    }
}
