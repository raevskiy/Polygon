using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KopliSoft.SceneControl
{
    public class FadeIn : MonoBehaviour
    {
        [SerializeField]
        private float fadeInDuration = 1f;

        // Start is called before the first frame update
        void Start()
        {
            SceneController sceneController = FindObjectOfType<SceneController>();
            StartCoroutine(sceneController.Fade(1f, fadeInDuration));
        }
    }
}
