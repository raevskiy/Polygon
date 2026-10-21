using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Fungus;
using System.Collections.Generic;
using KopliSoft.Behaviour;

namespace KopliSoft.SceneControl
{
    public class SceneController : MonoBehaviour, IDataPersistence
    {
        public event Action BeforeSceneUnload;          // Event delegate that is called just before a scene is unloaded.
        public event Action AfterSceneLoad;             // Event delegate that is called just after a scene is loaded.

        public Flowchart chapterInfo;

        //Those objects are initially hidden, so their persistance is managed by this class
        [SerializeField]
        private GameObject protagonist;
        [SerializeField]
        private GameObject airship;

        private enum SceneState {NONE, TO_LOAD, TO_UNLOAD, LOADING, UNLOADING, LOADED};
        private Dictionary<string, SceneState> seamleassScenes = new Dictionary<string, SceneState>();

        private void Start()
        {
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            StartCoroutine(SeamlessSceneDaemon(2));
        }

        public void LoadData(GameData data)
        {
            protagonist.transform.position = data.playerPosition;
            protagonist.transform.rotation = data.playerRotation;

            airship.transform.position = data.airshipPosition;
            airship.transform.rotation = data.airshipRotation;

        }

        public void SaveData(ref GameData data)
        {
            data.playerPosition = protagonist.transform.position;
            data.playerRotation = protagonist.transform.rotation;

            data.airshipPosition = airship.transform.position;
            data.airshipRotation = airship.transform.rotation;
        }

        private IEnumerator LoadScenes(string[] sceneNames)
        {
            foreach (string sceneName in sceneNames)
            {
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
        }

        private IEnumerator UnloadScenes(string[] sceneNames)
        {
            foreach (string sceneName in sceneNames)
            {
                yield return SceneManager.UnloadSceneAsync(sceneName);
            }
        }

        public void AddSeamlessScene(string sceneName)
        {
            SceneState sceneState;
            seamleassScenes.TryGetValue(sceneName, out sceneState);
            if (sceneState.Equals(SceneState.NONE) || sceneState.Equals(SceneState.UNLOADING))
            {
                seamleassScenes[sceneName] = SceneState.TO_LOAD;
            }
            else if (sceneState.Equals(SceneState.TO_UNLOAD))
            {
                CancelUnloading(sceneName);
            }
        }

        private void CancelUnloading(string sceneName)
        {
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                seamleassScenes[sceneName] = SceneState.LOADED;
            }
            else
            {
                seamleassScenes[sceneName] = SceneState.LOADING;
            }
        }

        public void RemoveSeamlessScene(string sceneName)
        {
            SceneState sceneState;
            seamleassScenes.TryGetValue(sceneName, out sceneState);
            if (sceneState.Equals(SceneState.TO_LOAD))
            {
                seamleassScenes.Remove(sceneName);
            }
            else
            {
                seamleassScenes[sceneName] = SceneState.TO_UNLOAD;
            }
        }

        IEnumerator SeamlessSceneDaemon(float waitTime)
        {
            while (true)
            {
                yield return new WaitForSeconds(waitTime);
                string[] sceneNames = new string[seamleassScenes.Count];
                seamleassScenes.Keys.CopyTo(sceneNames, 0);
                foreach (string sceneName in sceneNames)
                {
                    SceneState sceneState = seamleassScenes[sceneName];
                    bool loaded = SceneManager.GetSceneByName(sceneName).isLoaded;

                    if (sceneState.Equals(SceneState.TO_LOAD) && !loaded)
                    {
                        seamleassScenes[sceneName] = SceneState.LOADING;
                        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                    }
                    else if (sceneState.Equals(SceneState.TO_UNLOAD) && loaded)
                    {
                        seamleassScenes[sceneName] = SceneState.UNLOADING;
                        yield return SceneManager.UnloadSceneAsync(sceneName);
                    }
                    else if (sceneState.Equals(SceneState.LOADING) && loaded)
                    {
                        seamleassScenes[sceneName] = SceneState.LOADED;
                    }
                    else if ((sceneState.Equals(SceneState.UNLOADING) || sceneState.Equals(SceneState.TO_UNLOAD)) && !loaded)
                    {
                        seamleassScenes.Remove(sceneName);
                    }
                }
            }
        }

    }
}
