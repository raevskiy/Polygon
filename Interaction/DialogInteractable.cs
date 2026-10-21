using UnityEngine;

namespace KopliSoft.Interaction
{
    /// <summary>
    /// Conversation driven by a Fungus flowchart: runs block "Start" and ends with block "End".
    /// Legacy, new conversations use InkDialogInteractable.
    /// </summary>
    public class DialogInteractable : ConversationInteractable
    {
        [SerializeField]
        private string flowchartName;
        [SerializeField]
        private Fungus.Flowchart flowchart;

        protected override void Start()
        {
            if (flowchart == null && flowchartName != null && flowchartName.Trim().Length != 0)
            {
                flowchart = GameObject.Find(flowchartName).GetComponent<Fungus.Flowchart>();
            }
            base.Start();
        }

        protected override bool HasConversation()
        {
            return flowchart != null;
        }

        protected override void StartConversation()
        {
            if (flowchart.HasVariable("Interviewer"))
            {
                flowchart.SetStringVariable("Interviewer", m_InteractorGameObject.tag);
            }
            flowchart.ExecuteBlock("Start");
            Fungus.BlockSignals.OnBlockEnd += OnBlockEnd;
        }

        void OnBlockEnd(Fungus.Block block)
        {
            if (block.BlockName.Equals("End"))
            {
                Fungus.BlockSignals.OnBlockEnd -= OnBlockEnd;
                OnConversationEnded();
            }
        }
    }
}
