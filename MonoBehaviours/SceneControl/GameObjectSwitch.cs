using KopliSoft.Behaviour;
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

            foreach (string objectName in objectsToActivate)
            {
                GameObject gameObject = SceneGraphSearch.Find(objectName);
                if (gameObject != null)
                {
                    gameObject.SetActive(true);
                }
            }

            for (int i = 0; i < charactersToTeleport.Length; i++)
            {
                GameObject character = SceneGraphSearch.Find(charactersToTeleport[i]);
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
    }
}
