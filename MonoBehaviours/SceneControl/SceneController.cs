using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Fungus;
using System.Collections.Generic;
using KopliSoft.Behaviour;

namespace KopliSoft.SceneControl
{
    public class SceneController : MonoBehaviour
    {
        public event Action BeforeSceneUnload;          // Event delegate that is called just before a scene is unloaded.
        public event Action AfterSceneLoad;             // Event delegate that is called just after a scene is loaded.

        public CanvasGroup faderCanvasGroup;            // The CanvasGroup that controls the Image used for fading to black.
        public float fadeDuration = 1f;                 // How long it should take to fade to and from black.
        private bool isFading;

        public Flowchart chapterInfo;

        [SerializeField]
        private PatrolController[] mainCharacters;
        [SerializeField]
        private GameObject vault;

        private enum SceneState {NONE, TO_LOAD, TO_UNLOAD, LOADING, UNLOADING, LOADED};
        private Dictionary<string, SceneState> seamleassScenes = new Dictionary<string, SceneState>();

        private void Start()
        {
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            faderCanvasGroup.alpha = 1f;
            StartCoroutine(SeamlessSceneDaemon(2));
        }

        public void FadeAndSwitchScenes(string sceneNamesToLoad, string sceneNamesToUnload)
        {
            FadeAndSwitchScenes(sceneNamesToLoad.Split(','), sceneNamesToUnload.Split(','));
        }

        public void FadeAndSwitchScenes(string[] sceneNamesToLoad, string[] sceneNamesToUnload)
        {
            if (!isFading)
            {
                StartCoroutine(DoFadeAndSwitchScenes(sceneNamesToLoad, sceneNamesToUnload));
            }
        }

        private IEnumerator DoFadeAndSwitchScenes(string[] sceneNamesToLoad, string[] sceneNamesToUnload)
        {
            yield return StartCoroutine(Fade(1f, this.fadeDuration));

            BeforeSceneUnload?.Invoke();
 
            yield return StartCoroutine(UnloadScenes(sceneNamesToUnload));
            yield return StartCoroutine(LoadScenes(sceneNamesToLoad));

            AfterSceneLoad?.Invoke();

            StartFade(0f, this.fadeDuration);
        }
 
        private void MoveCharactersToVault()
        {
            foreach (PatrolController character in mainCharacters)
            {
                if (character.gameObject.activeInHierarchy)
                {
                    character.TeleportToWaypoint(vault);
                }
                else
                {
                    character.transform.position = vault.transform.position;
                    character.transform.rotation = vault.transform.rotation;
                }
            }
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

        public void StartFade(float finalAlpha, float fadeDuration)
        {
            StartCoroutine(Fade(finalAlpha, fadeDuration));
        }

        public IEnumerator Fade(float finalAlpha, float fadeDuration)
        {
            if (fadeDuration < 0.1)
            {
                faderCanvasGroup.alpha = finalAlpha;
            }
            else
            {
                // FadeAndSwitchScenes coroutine won't be called again.
                isFading = true;
                // no more input can be accepted.
                faderCanvasGroup.blocksRaycasts = true;
                float fadeSpeed = Mathf.Abs(faderCanvasGroup.alpha - finalAlpha) / fadeDuration;
                while (!Mathf.Approximately(faderCanvasGroup.alpha, finalAlpha))
                {
                    faderCanvasGroup.alpha = Mathf.MoveTowards(
                        faderCanvasGroup.alpha,
                        finalAlpha,
                        fadeSpeed * Time.deltaTime);
                    yield return null;
                }

                isFading = false;
                // input is no longer ignored.
                faderCanvasGroup.blocksRaycasts = false;
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
