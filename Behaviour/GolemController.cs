using Opsive.DeathmatchAIKit.AI;
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
            SetWeaponStatEnabled(209315222, true);
        }

        public void DeactivateBombs()
        {
            SetWeaponStatEnabled(209315222, false);
        }

        public void ActivateCircularSaw()
        {
            SetWeaponStatEnabled(137999745, true);
            SetWeaponStatEnabled(1783797938, true);
        }

        public void DeactivateCircularSaw()
        {
            SetWeaponStatEnabled(137999745, false);
            SetWeaponStatEnabled(1783797938, false);
        }

        private void SetWeaponStatEnabled(int id, bool enabled)
        {
            foreach (DeathmatchAgent.WeaponStat stat in deathmatchAgent.AvailableWeapons)
            {
                if (stat.ItemType.ID == id)
                {
                    stat.SetEnabled(enabled);
                    return;
                }
            }
        }

        public void ActivateAntiRiotPunchedCard()
        {
            string[] layers = new string[] {"Player", "RedTeam", "BlueTeam", "GreenTeam", "YellowTeam"};
            int mask = LayerMask.GetMask(layers) ^ (1 << gameObject.layer);
            deathmatchAgent.TargetLayerMask = mask;
        }

        public void DeactivateAntiRiotPunchedCard()
        {
            string[] layers = new string[] { "Player" };
            int mask = LayerMask.GetMask(layers);
            deathmatchAgent.TargetLayerMask = mask;
        }

    }

}
