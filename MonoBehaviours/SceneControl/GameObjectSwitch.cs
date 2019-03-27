using KopliSoft.Behaviour;
using System;
using UnityEngine;

namespace KopliSoft.SceneControl
{
    public class GameObjectSwitch : MonoBehaviour
    {
        [SerializeField]
        private string[] objectsToDeactivate;
        [SerializeField]
        private string[] objectsToActivate;
        [SerializeField]
        private string[] charactersToTeleport;
        [SerializeField]
        private GameObject[] teleports;

        public void Switch()
        {
            foreach (string objectName in objectsToDeactivate)
            {
                GameObject gameObject = DoFind(objectName);
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

            foreach (string objectName in objectsToActivate)
            {
                GameObject gameObject = DoFind(objectName);
                if (gameObject != null)
                {
                    gameObject.SetActive(true);
                }
            }

            for (int i = 0; i < charactersToTeleport.Length; i++)
            {
                GameObject character = DoFind(charactersToTeleport[i]);
                BehaviorTreeController behaviorTreeController = character.GetComponent<BehaviorTreeController>();
                if (behaviorTreeController != null && behaviorTreeController.enabled)
                {
                    behaviorTreeController.TeleportToWaypoint(teleports[i]);
                }
                else
                {
                    character.transform.position = teleports[i].transform.position;
                    character.transform.rotation = teleports[i].transform.rotation;
                }
            }
        }

        private GameObject DoFind(string objectPath)
        {
            string[] tokens = objectPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string pathToParent = "/" + string.Join("/", tokens, 0, tokens.Length - 1);
            GameObject parentGameObject = GameObject.Find(pathToParent);
            return parentGameObject.transform.Find(tokens[tokens.Length - 1]).gameObject;
        }
    }
}
