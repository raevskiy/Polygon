using System.Collections;
using UnityEngine;

namespace KopliSoft.Dialogue
{
    /// <summary>
    /// Lets animation events and UnityEvents play ink knots: an animation event "PlayKnot" with the knot name as its
    /// string parameter. Must sit on the same GameObject as the Animator. Replaces Fungus' Flowchart.ExecuteBlock events.
    /// </summary>
    public class InkEvents : MonoBehaviour
    {
        public void PlayKnot(string knot)
        {
            StartCoroutine(PlayWhenFree(knot));
        }

        // An event can fire in the middle of another conversation; the knot then waits for it to end.
        private IEnumerator PlayWhenFree(string knot)
        {
            InkDialogueRunner runner;
            while ((runner = InkDialogueRunner.Instance) == null || runner.IsRunning)
            {
                yield return null;
            }
            runner.StartDialogue(knot, null, null);
        }
    }
}
