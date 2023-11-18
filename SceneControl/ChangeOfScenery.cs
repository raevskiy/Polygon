using AC.LSky;
using KopliSoft.Behaviour;
using Opsive.ThirdPersonController;
using System.Collections;
using UnityEngine;

namespace KopliSoft.SceneControl
{
    public class ChangeOfScenery : MonoBehaviour
    {
        [SerializeField]
        private float fadeInDuration = 1f;
        [SerializeField]
        private float fadeOutDuration = 1f;
        [SerializeField]
        private float pauseDuration = 0;

        [SerializeField]
        private string sceneToLoad;
        [SerializeField]
        private string sceneToUnload;

        [SerializeField]
        private string[] objectsToDeactivate;
        [SerializeField]
        private string[] objectsToActivate;
        [SerializeField]
        private string[] charactersToTeleport;
        [SerializeField]
        private GameObject[] teleports;
        [SerializeField]
        private float time = -1f;
        [SerializeField]
        private bool switchOnStart;
        [SerializeField]
        private bool switchOnTriggerEnter;
        [SerializeField]
        private LayerMask triggerLayerMask = (1 << LayerManager.Enemy) | (1 << LayerManager.Player);


        private SceneController sceneController;
        private LSkyTOD timeOfDay;
        private GameObject collidedGameObject;

        private bool inProgress;

        void Start()
        {
            sceneController = FindObjectOfType<SceneController>();
            timeOfDay = FindObjectOfType<LSkyTOD>();

            if (switchOnStart)
            {
                Switch();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!inProgress && switchOnTriggerEnter && triggerLayerMask == (triggerLayerMask | (1 << other.gameObject.layer)) )
            {
                collidedGameObject = other.gameObject;
                Switch();
            }
        }

        public void Switch()
        {
            if (!inProgress)
            {
                inProgress = true;

                if (fadeInDuration >= 0 && fadeInDuration < 0.1f)
                {
                    sceneController.faderCanvasGroup.alpha = 1.0f;
                }
                StartCoroutine(FadeAndSwitch());
            }
        }

        private IEnumerator FadeAndSwitch()
        {
            if (fadeInDuration >= 0.1f)
            {
                yield return StartCoroutine(sceneController.Fade(1f, fadeInDuration));
            }

            ChangeScenery();
            if (pauseDuration > 0)
            {
                yield return StartCoroutine(Pause());
            }

            if (fadeOutDuration >= 0)
            {
                yield return StartCoroutine(sceneController.Fade(0f, fadeOutDuration));
            }

            inProgress = false;
        }

        private void ChangeScenery()
        {
            if (time >= 0)
            {
                timeOfDay.SetTime(time);
            }

            Deactivate();
            Activate();
            Teleport();
        }

        private void Deactivate()
        {
            if (sceneToUnload != null)
            {
                sceneController.RemoveSeamlessScene(sceneToUnload);
            }

            foreach (string objectName in objectsToDeactivate)
            {
                GameObject gameObject = SceneGraphSearch.Find(objectName);
                if (gameObject != null)
                {
                    SeamlessSceneLoader loader = gameObject.GetComponentInChildren<SeamlessSceneLoader>();
                    if (loader != null)
                    {
                        loader.Unload();
                    }

                    gameObject.SetActive(false);
                }
            }
        }

        private void Activate()
        {
            if (sceneToLoad != null)
            {
                sceneController.AddSeamlessScene(sceneToLoad);
            }

            foreach (string objectName in objectsToActivate)
            {
                GameObject gameObject = SceneGraphSearch.Find(objectName);
                if (gameObject != null)
                {
                    gameObject.SetActive(true);
                }
            }
        }

        private void Teleport()
        {
            if (switchOnTriggerEnter)
            {
                Teleport(collidedGameObject, teleports[0]);
            }
            else
            {
                for (int i = 0; i < charactersToTeleport.Length; i++)
                {
                    GameObject character = SceneGraphSearch.Find(charactersToTeleport[i]);
                    Teleport(character, teleports[i]);
                }
            }
        }

        private void Teleport(GameObject character, GameObject teleport)
        {
            PatrolController patrolController = character.GetComponent<PatrolController>();
            if (patrolController != null && patrolController.enabled)
            {
                patrolController.TeleportToWaypoint(teleport);
            }
            else
            {
                character.transform.position = teleport.transform.position;
                character.transform.rotation = teleport.transform.rotation;
            }
        }

        private IEnumerator Pause()
        {
            float time = 0;
            while (time < pauseDuration)
            {
                time += Time.deltaTime;
                yield return null;
            }
        }

    }
}
