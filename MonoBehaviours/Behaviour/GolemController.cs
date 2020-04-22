using Opsive.DeathmatchAIKit.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KopliSoft.Behaviour
{
    public class GolemController : MonoBehaviour
    {
        private DeathmatchAgent deathmatchAgent;

        // Start is called before the first frame update
        void Start()
        {
            deathmatchAgent = GetComponent<DeathmatchAgent>();
        }

        public void ActivateBombs()
        {
            foreach (DeathmatchAgent.WeaponStat stat in deathmatchAgent.AvailableWeapons)
            {
                if (stat.ItemType.ID == 137999745)
                {
                    stat.SetEnabled(false);
                }
                else if (stat.ItemType.ID == 209315222)
                {
                    stat.SetEnabled(true);
                }
            }
        }

    }

}
