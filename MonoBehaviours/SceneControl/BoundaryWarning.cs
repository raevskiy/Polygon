using Fungus;
using UnityEngine;

namespace KopliSoft.SceneControl
{
    public class BoundaryWarning : MonoBehaviour
    {
        [SerializeField]
        private Flowchart flowchart;
        [SerializeField]
        private string[] tags;

        private void OnTriggerEnter(Collider other)
        {
            if (flowchart.GetExecutingBlocks().Count == 0 && HasTag(other))
            {
                flowchart.ExecuteBlock("Main");
            }
        }

        private bool HasTag(Collider other)
        {
            foreach (string tag in tags)
            {
                if (other.CompareTag(tag))
                {
                    return true;
                }
                
            }
            return false;
        }
    }
}
