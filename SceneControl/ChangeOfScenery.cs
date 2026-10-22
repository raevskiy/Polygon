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
        private string sceneToLoad;
        [SerializeField]
        private string sceneToUnload;

        [SerializeField]
        private GameObject[] objectsToDeactivate;
        [SerializeField]
        private GameObject[] objectsToActivate;
        [SerializeField]
        private GameObject[] charactersToTeleport;
        [SerializeField]
        private GameObject[] teleports;
        [SerializeField]
        private float time = -1f;
        [SerializeField]
        private Fungus.Flowchart flowchart;


        [SerializeField]
        private bool switchOnTriggerEnter;
        [SerializeField]
        private LayerMask triggerLayerMask = (1 << LayerManager.Enemy) | (1 << LayerManager.Player);

        private SceneController sceneController;
        private LSkyTOD timeOfDay;
        private GameObject collidedGameObject;

        void Start()
        {
            sceneController = FindObjectOfType<SceneController>();
            timeOfDay = FindObjectOfType<LSkyTOD>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (switchOnTriggerEnter && triggerLayerMask == (triggerLayerMask | (1 << other.gameObject.layer)))
            {
                collidedGameObject = other.gameObject;
                Switch();
            }
        }

        public void Switch()
        {
            if (flowchart != null)
            {
                flowchart.ExecuteBlock("Before");
            }
            if (time >= 0)
            {
                timeOfDay.SetTime(time);
            }

            Deactivate();
            Activate();
            Teleport();
            if (flowchart != null)
            {
                flowchart.ExecuteBlock("After");
            }
        }

        private void Deactivate()
        {
            if (sceneToUnload != null)
            {
                sceneController.RemoveSeamlessScene(sceneToUnload);
            }

            foreach (GameObject gameObject in objectsToDeactivate)
            {
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

            foreach (GameObject gameObject in objectsToActivate)
            {
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
                    Teleport(charactersToTeleport[i], teleports[i]);
                }
            }
        }

        private void Teleport(GameObject character, GameObject teleport)
        {
            // An empty slot used to throw a NullReferenceException that Fungus' InvokeMethod reported without any context.
            if (character == null || teleport == null)
            {
                Debug.LogError(name + ": ChangeOfScenery has an empty " + (character == null ? "Characters To Teleport" : "Teleports") +
                    " slot, nothing is teleported", this);
                return;
            }
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
    }
}
