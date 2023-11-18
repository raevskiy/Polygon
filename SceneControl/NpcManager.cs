using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KopliSoft.SceneControl
{
    public class NpcManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] npcs;
        [SerializeField]
        private NpcActivator[] activators;

        // Update is called once per frame
        void Update()
        {
            for (int i = 0; i < npcs.Length; i++)
            {
                activators[i].transform.position = npcs[i].transform.position;
            }
        }
    }
}

