using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KopliSoft.SceneControl
{
    public class SceneLoader : MonoBehaviour
    {
        public string sceneName;
        private SceneController sceneController;
        private bool loaded;

        private void Start()
        {
            sceneController = GameObject.FindGameObjectWithTag("SceneController").GetComponent<SceneController>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!loaded && other.CompareTag("MainCamera"))
            {
                LoadOnce();
            }
        }

        public void LoadOnce()
        {
            sceneController.AddSeamlessScene(sceneName);
            loaded = true;
        }
    }

}
