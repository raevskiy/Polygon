using Fungus;
using Ink.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KopliSoft.Dialogue
{
    /// <summary>
    /// Lets ink read and set boolean variables of Fungus flowcharts while content is split between both systems,
    /// e.g. Yuri (ink) granting the permission that the "Flight log" flowchart (Fungus) checks.
    /// Declared as EXTERNAL in case_functions.ink.
    /// </summary>
    public static class FungusBridge
    {
        public static void Bind(Story story)
        {
            story.BindExternalFunction("fungus_set_bool", (string flowchart, string variable, bool value) => SetBool(flowchart, variable, value), false);
            story.BindExternalFunction("fungus_bool", (string flowchart, string variable) => (object)GetBool(flowchart, variable), true);
            story.BindExternalFunction("fungus_message", (string message) => Flowchart.BroadcastFungusMessage(message), false);
            story.BindExternalFunction("set_active", (string path, bool active) => SetActive(path, active), false);
            story.BindExternalFunction("play_animation", (string path, string state) => PlayAnimation(path, state), false);
        }

        /// <summary>
        /// Activates or deactivates a scene object by its hierarchy path, e.g. "/NPC/Cap/Dialog". Finds inactive objects too.
        /// </summary>
        public static void SetActive(string path, bool active)
        {
            var found = FindByPath(path);
            if (found != null)
            {
                found.SetActive(active);
            }
        }

        /// <summary>
        /// Plays an Animator state from its start, like Fungus' PlayAnimState, e.g. ("/Vehicles/Diana airship", "Landing").
        /// </summary>
        public static void PlayAnimation(string path, string state)
        {
            var found = FindByPath(path);
            var animator = found != null ? found.GetComponent<Animator>() : null;
            if (animator == null)
            {
                Debug.LogWarning("Ink: no Animator at '" + path + "'");
                return;
            }
            animator.Play(state, -1, 0f);
        }

        // A scene object by its hierarchy path in any loaded scene, inactive ones included.
        private static GameObject FindByPath(string path)
        {
            var parts = path.TrimStart('/').Split(new[] { '/' }, 2);
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                foreach (var root in SceneManager.GetSceneAt(i).GetRootGameObjects())
                {
                    if (root.name == parts[0])
                    {
                        var found = parts.Length == 1 ? root.transform : root.transform.Find(parts[1]);
                        if (found != null)
                        {
                            return found.gameObject;
                        }
                    }
                }
            }
            Debug.LogWarning("Ink: no scene object at '" + path + "'");
            return null;
        }

        public static void SetBool(string flowchartName, string variable, bool value)
        {
            var flowchart = Find(flowchartName, variable);
            if (flowchart != null)
            {
                flowchart.SetBooleanVariable(variable, value);
            }
        }

        public static bool GetBool(string flowchartName, string variable)
        {
            var flowchart = Find(flowchartName, variable);
            return flowchart != null && flowchart.GetBooleanVariable(variable);
        }

        private static Flowchart Find(string flowchartName, string variable)
        {
            foreach (var flowchart in Flowchart.CachedFlowcharts)
            {
                if (flowchart != null && flowchart.name.Trim() == flowchartName && flowchart.HasVariable(variable))
                {
                    return flowchart;
                }
            }
            // Inactive flowcharts are not in the cache.
            foreach (var flowchart in UnityEngine.Object.FindObjectsOfType<Flowchart>(true))
            {
                if (flowchart.name.Trim() == flowchartName && flowchart.HasVariable(variable))
                {
                    return flowchart;
                }
            }
            Debug.LogWarning("Ink: no Fungus flowchart '" + flowchartName + "' with variable '" + variable + "'");
            return null;
        }
    }
}
