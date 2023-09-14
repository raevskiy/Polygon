using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KopliSoft.SceneControl
{
    public class SeamlessSceneLoader : MonoBehaviour
    {
        public string sceneName;
        private SceneController sceneController;

        private void Start()
        {
            sceneController = GameObject.FindGameObjectWithTag("SceneController").GetComponent<SceneController>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("MainCamera"))
            {
                Load();
            }
        }

        public void Load()
        {
            sceneController.AddSeamlessScene(sceneName);
        }

        public void Unload()
        {
            sceneController.RemoveSeamlessScene(sceneName);
        }
    }
}
