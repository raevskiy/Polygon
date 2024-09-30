using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KopliSoft.Interaction
{
    public class AnimatorTrigger : MonoBehaviour
    {
        [SerializeField]
        private GameObject initiator;
        [SerializeField]
        private Animator animator;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == initiator)
            {
                animator.enabled = true;
            }
        }
    }
}
