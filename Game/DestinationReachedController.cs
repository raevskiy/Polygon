using Fungus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Acts like "Execute On Event - Trigger" for Flowchart,
 * but uses no tags
 */
namespace KopliSoft.Game
{
    public class DestinationReachedController : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> targetsAwaitedAtDestination;
        [SerializeField]
        private Flowchart flowchart;
        [SerializeField]
        private string blockName = "Main";
        [SerializeField]
        private int lossesAllowed = 0;

        private int targetsAwaitedCounter; 

        // Start is called before the first frame update
        void Start()
        {
            targetsAwaitedCounter = targetsAwaitedAtDestination.Count - lossesAllowed;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (targetsAwaitedAtDestination.Contains(other.gameObject))
            {
                targetsAwaitedCounter--;
                if (targetsAwaitedCounter == 0)
                {
                    flowchart.ExecuteBlock(blockName);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (targetsAwaitedAtDestination.Contains(other.gameObject))
            {
                targetsAwaitedCounter++;
            }
        }
    }
}
