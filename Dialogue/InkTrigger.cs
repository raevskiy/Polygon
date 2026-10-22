using System;
using System.Linq;
using UnityEngine;

namespace KopliSoft.Dialogue
{
    /// <summary>
    /// Plays an ink knot when something with a matching tag enters/stays in/exits this trigger collider.
    /// Replaces Fungus' "Trigger" event handler. Whether anything should happen (e.g. "only the first time")
    /// is decided in the knot itself; a knot that shows nothing runs silently.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InkTrigger : MonoBehaviour
    {
        // Same values as Fungus' PhysicsMessageType, so converted triggers keep their settings.
        [Flags]
        public enum FireOn
        {
            Enter = 1 << 0,
            Stay = 1 << 1,
            Exit = 1 << 2,
        }

        [Tooltip("Knot in the chapter ink story")]
        [SerializeField]
        private string knot;
        [Tooltip("Only colliders with one of these tags fire the trigger. Empty = any.")]
        [SerializeField]
        private string[] tagFilter = { "Protagonist" };
        [SerializeField]
        private FireOn fireOn = FireOn.Enter;

        public void Configure(string knot, string[] tagFilter, FireOn fireOn)
        {
            this.knot = knot;
            this.tagFilter = tagFilter;
            this.fireOn = fireOn;
        }

        private void OnTriggerEnter(Collider other)
        {
            Fire(FireOn.Enter, other);
        }

        private void OnTriggerStay(Collider other)
        {
            Fire(FireOn.Stay, other);
        }

        private void OnTriggerExit(Collider other)
        {
            Fire(FireOn.Exit, other);
        }

        private void Fire(FireOn message, Collider other)
        {
            if ((fireOn & message) == 0 || string.IsNullOrEmpty(knot))
            {
                return;
            }
            if (tagFilter != null && tagFilter.Length > 0 && !tagFilter.Any(other.CompareTag))
            {
                return;
            }
            var runner = InkDialogueRunner.Instance;
            if (runner != null && !runner.IsRunning)
            {
                runner.StartDialogue(knot, other.tag, null);
            }
        }
    }
}
