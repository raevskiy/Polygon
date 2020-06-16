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
        private Flowchart flowchart;
        [SerializeField]
        private string blockName = "Main";
        [SerializeField]
        private int allowedSurvivors = 0;

        void Start()
        {
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
