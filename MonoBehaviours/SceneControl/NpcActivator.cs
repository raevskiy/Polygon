using BehaviorDesigner.Runtime;
using UnityEngine;

namespace KopliSoft.SceneControl
{
    public class NpcActivator : MonoBehaviour
    {
        [SerializeField]
        private GameObject npc;

        private void OnTriggerEnter(Collider other)
        {
            if ("MainCamera".Equals(other.tag))
            {
                npc.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if ("MainCamera".Equals(other.tag))
            {
                npc.SetActive(false);
            }
        }

    }
}

