using Opsive.ThirdPersonController;
using UnityEngine;

namespace KopliSoft.Interaction
{
    public class ItemEquipListener : MonoBehaviour
    {
        [SerializeField]
        private GameObject item;
        [SerializeField]
        private string flowchartName;
        [SerializeField]
        private Fungus.Flowchart flowchart;
        [SerializeField]
        private string blockName;

        void Start()
        {
            if (flowchart == null && flowchartName != null && flowchartName.Trim().Length != 0)
            {
                flowchart = GameObject.Find(flowchartName).GetComponent<Fungus.Flowchart>();
            }

            EventHandler.RegisterEvent(gameObject, "OnInventoryItemEquipping", OnItemEquipping);
        }

        private void OnItemEquipping()
        {
            flowchart.ExecuteBlock(blockName);
            EventHandler.UnregisterEvent(gameObject, "OnInventoryItemEquipping", OnItemEquipping);
        }
    }
}
