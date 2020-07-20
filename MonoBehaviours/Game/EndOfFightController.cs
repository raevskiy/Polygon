using Fungus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KopliSoft.Game
{
    public class EndOfFightController : MonoBehaviour
    {
        [SerializeField]
        private List<CustomHealth> charactersToBeDefeated;
        [SerializeField]
        private string flowchartName;
        [SerializeField]
        private Flowchart flowchart;
        [SerializeField]
        private string blockName = "Main";
        [SerializeField]
        private int allowedSurvivors = 0;

        void Start()
        {
            if (flowchart == null && flowchartName != null && flowchartName.Trim().Length != 0)
            {
                flowchart = GameObject.Find(flowchartName).GetComponent<Fungus.Flowchart>();
            }

            CustomHealth.OnCharacterDefeated += OnCharacterDefeated;
        }

        private void OnCharacterDefeated(CustomHealth customHealth)
        {
            if (charactersToBeDefeated.Contains(customHealth))
            {
                charactersToBeDefeated.Remove(customHealth);
            }

            if (charactersToBeDefeated.Count == allowedSurvivors)
            {
                CustomHealth.OnCharacterDefeated -= OnCharacterDefeated;
                flowchart.ExecuteBlock(blockName);
            }
        }
    }
}
