using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KopliSoft.Interaction
{
    public class Criminal : MonoBehaviour
    {
        [SerializeField]
        private GameObject criminalIcon;
        private bool inCriminalMode;

        void Update()
        {
            if (Input.GetButtonDown("Criminal"))
            {
                inCriminalMode = !inCriminalMode;
                if (criminalIcon != null)
                {
                    criminalIcon.SetActive(inCriminalMode);
                }
            }
        }

        public bool IsInCriminalMode()
        {
            return inCriminalMode;
        }
    }
}
