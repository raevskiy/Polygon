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

            if (charactersToBeDefeated.Count == 0)
            {
                flowchart.ExecuteBlock(blockName);
            }
        }
    }
}
