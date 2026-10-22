using KopliSoft.Dialogue;
using KopliSoft.Interaction;
using KopliSoft.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace KopliSoft.EditorTools
{
    /// <summary>
    /// One-click scene setup for the ink investigation system, so nobody has to wire it by hand.
    /// Safe to run again: it only adds what is missing. Everything is undoable; save the scene afterwards.
    /// </summary>
    public static class InvestigationSetup
    {
        private const string StoryRootPath = "/Story";
        private const string InvestigationName = "Investigation";
        private const string SayDialogPath = "/Story/DialogBoxes/DefaultSayDialog";
        private const string CaseJsonPath = "Assets/Story/Chapter1/case_chapter1.json";
        private const string InkJsonPath = "Assets/Story/Chapter1/chapter1.json";

        [MenuItem("Polygon/Investigation/Set Up Open Chapter1 Scene")]
        public static void SetUpScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "Chapter1")
            {
                EditorUtility.DisplayDialog("Investigation setup", "Open the Chapter1 scene first (active scene: " + scene.name + ").", "OK");
                return;
            }

            var caseJson = AssetDatabase.LoadAssetAtPath<TextAsset>(CaseJsonPath);
            var inkJson = AssetDatabase.LoadAssetAtPath<TextAsset>(InkJsonPath);
            if (caseJson == null || inkJson == null)
            {
                EditorUtility.DisplayDialog("Investigation setup",
                    "Missing " + (caseJson == null ? CaseJsonPath : InkJsonPath) + ".\n\nThe ink plugin creates chapter1.json when it compiles chapter1.ink. " +
                    "Select chapter1.ink in the Project window and press 'Compile' in the Inspector, then run this again.", "OK");
                return;
            }

            Undo.SetCurrentGroupName("Set up investigation");
            var report = new System.Text.StringBuilder();

            // /Story/Investigation with the manager, the runner and the debug notebook.
            var storyRoot = FindInScene(scene, StoryRootPath);
            var investigation = FindInScene(scene, StoryRootPath + "/" + InvestigationName);
            if (investigation == null)
            {
                investigation = new GameObject(InvestigationName);
                Undo.RegisterCreatedObjectUndo(investigation, "Create Investigation");
                if (storyRoot != null)
                {
                    Undo.SetTransformParent(investigation.transform, storyRoot.transform, "Parent Investigation");
                }
                report.AppendLine("Created " + (storyRoot != null ? StoryRootPath + "/" : "/") + InvestigationName);
            }

            var manager = Ensure<InvestigationManager>(investigation, report);
            SetReference(manager, "caseJson", caseJson);
            SetReference(manager, "inkJson", inkJson);

            var runner = Ensure<InkDialogueRunner>(investigation, report);
            Ensure<NotebookDebugWindow>(investigation, report);

            // The write-down button: reuse the one the old WriteDown component shows.
            // The say dialog is the one carrying the old WriteDown component; the path is only a fallback.
            var writeDowns = Object.FindObjectsOfType<WriteDown>(true);
            if (writeDowns.Length > 1)
            {
                report.AppendLine("WARNING: " + writeDowns.Length + " WriteDown components found, using the one on " + writeDowns[0].name);
            }
            var sayDialog = writeDowns.Length > 0 ? writeDowns[0].gameObject : FindInScene(scene, SayDialogPath);
            if (sayDialog == null)
            {
                report.AppendLine("WARNING: " + SayDialogPath + " not found. Add NoteWriteDownButton to the say dialog by hand.");
            }
            else
            {
                var fungusSayDialog = sayDialog.GetComponent<Fungus.SayDialog>();
                if (fungusSayDialog != null)
                {
                    SetReference(runner, "sayDialog", fungusSayDialog);
                }

                Button button = null;
                var writeDown = sayDialog.GetComponent<WriteDown>();
                if (writeDown != null)
                {
                    var buttonObject = new SerializedObject(writeDown).FindProperty("writeDownButton").objectReferenceValue as GameObject;
                    button = buttonObject != null ? buttonObject.GetComponent<Button>() : null;
                }
                if (button == null)
                {
                    report.AppendLine("WARNING: could not find the write-down button through WriteDown. Set NoteWriteDownButton.button by hand.");
                }
                var noteButton = Ensure<NoteWriteDownButton>(sayDialog, report);
                if (button != null)
                {
                    SetReference(noteButton, "button", button);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            report.AppendLine("Done. Save the scene (Ctrl+S) to keep the changes, or Undo to revert.");
            Debug.Log("[Investigation setup]\n" + report);
            EditorUtility.DisplayDialog("Investigation setup", report.ToString(), "OK");
            Selection.activeGameObject = investigation;
        }

        private static readonly string[,] AirshipReferences =
        {
            { "moving", "/Vehicles/Diana airship/Moving" },
            { "anchored", "/Vehicles/Diana airship/Anchored" },
            { "captainInside", "/Vehicles/Diana airship/Crew/Cap" },
            { "captainOutside", "/NPC/Cap" },
            { "machineNoise", "/Vehicles/Diana airship/Moving/Machine noise" },
            { "landingNoise", "/Vehicles/Diana airship/Moving/Landing noise" },
            { "exitControlCar", "/Vehicles/Diana airship/Triggers/Exit Control Car" },
            { "captainApproachedTrigger", "/Vehicles/Diana airship/Triggers/Captain Approached" },
            { "sleepTrigger", "/Vehicles/Diana airship/Triggers/Sleep" },
            { "captainDialog", "/NPC/Cap/Dialog" },
        };

        /// <summary>
        /// Adds AirshipState to the airship and wires its scene references (the objects the Fungus "FlowChart" switched).
        /// </summary>
        [MenuItem("Polygon/Investigation/Set Up Airship State")]
        public static void SetUpAirshipState()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var airship = FindInScene(scene, "/Vehicles/Diana airship");
            if (airship == null)
            {
                EditorUtility.DisplayDialog("Airship state", "/Vehicles/Diana airship not found in the open scene.", "OK");
                return;
            }
            Undo.SetCurrentGroupName("Set up airship state");
            var report = new System.Text.StringBuilder();
            var state = Ensure<KopliSoft.Vehicles.AirshipState>(airship, report);
            for (int i = 0; i < AirshipReferences.GetLength(0); i++)
            {
                var target = FindInScene(scene, AirshipReferences[i, 1]);
                if (target == null)
                {
                    report.AppendLine("WARNING: " + AirshipReferences[i, 1] + " not found, set '" + AirshipReferences[i, 0] + "' by hand.");
                    continue;
                }
                SetReference(state, AirshipReferences[i, 0], target);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            report.AppendLine("Done. Now remove the Fungus state machine: select /Vehicles/Diana airship/FlowChart and run " +
                "Polygon → Investigation → Remove Fungus Flowchart From Selected. Then save the scene.");
            Debug.Log("[Airship state]\n" + report, state);
            EditorUtility.DisplayDialog("Airship state", report.ToString(), "OK");
            Selection.activeGameObject = airship;
        }

        // Like GameObject.Find("/a/b"), but also finds inactive objects.
        private static GameObject FindInScene(UnityEngine.SceneManagement.Scene scene, string path)
        {
            var parts = path.TrimStart('/').Split(new[] { '/' }, 2);
            foreach (var root in scene.GetRootGameObjects())
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
            return null;
        }

        private static T Ensure<T>(GameObject target, System.Text.StringBuilder report) where T : Component
        {
            var component = target.GetComponent<T>();
            if (component == null)
            {
                component = Undo.AddComponent<T>(target);
                report.AppendLine("Added " + typeof(T).Name + " to " + target.name);
            }
            return component;
        }

        private static void SetReference(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(field);
            if (property.objectReferenceValue != value)
            {
                property.objectReferenceValue = value;
                serialized.ApplyModifiedProperties();
            }
        }
    }

    /// <summary>
    /// Switches the selected character from a Fungus dialog to an ink knot. The Fungus flowchart stays in the scene.
    /// </summary>
    public class ConvertDialogToInkWindow : EditorWindow
    {
        private string knot = "";

        [MenuItem("Polygon/Investigation/Convert Selected Dialog To Ink...")]
        public static void Open()
        {
            GetWindow<ConvertDialogToInkWindow>(true, "Convert dialog to ink").minSize = new Vector2(380, 150);
        }

        void OnGUI()
        {
            var selected = Selection.activeGameObject;
            var dialog = selected != null ? selected.GetComponentInChildren<DialogInteractable>(true) : null;

            EditorGUILayout.HelpBox("Replaces the DialogInteractable (Fungus) of the selected character with an InkDialogInteractable " +
                "that plays the knot below. Trigger, layers and AI settings are kept. Undo reverts it.", MessageType.Info);
            knot = EditorGUILayout.TextField("Ink knot", knot);

            if (dialog == null)
            {
                EditorGUILayout.HelpBox("Select a character that has a DialogInteractable.", MessageType.Warning);
                return;
            }
            EditorGUILayout.LabelField("Dialog on", dialog.gameObject.name);

            if (GUILayout.Button("Convert") && !string.IsNullOrWhiteSpace(knot))
            {
                Convert(dialog, knot.Trim());
                Close();
            }
        }

        private static void Convert(DialogInteractable dialog, string knot)
        {
            var target = dialog.gameObject;
            Undo.SetCurrentGroupName("Convert dialog to ink");

            // A disabled interactable still receives trigger events, so the Fungus one has to go.
            var ink = Undo.AddComponent<InkDialogInteractable>(target);
            var source = new SerializedObject(dialog);
            var destination = new SerializedObject(ink);
            var property = source.GetIterator();
            property.NextVisible(true);
            while (property.NextVisible(false))
            {
                if (property.name != "m_Script" && destination.FindProperty(property.name) != null)
                {
                    destination.CopyFromSerializedProperty(property);
                }
            }
            destination.FindProperty("knot").stringValue = knot;
            destination.ApplyModifiedProperties();

            var flowchart = source.FindProperty("flowchart").objectReferenceValue;
            var flowchartName = source.FindProperty("flowchartName").stringValue;
            Undo.DestroyObjectImmediate(dialog);

            EditorSceneManager.MarkSceneDirty(target.scene);
            Debug.Log("[Investigation] " + target.name + " now plays ink knot '" + knot + "' (was Fungus flowchart '" +
                (flowchart != null ? flowchart.name : flowchartName) + "'). Save the scene to keep it, or Undo.", target);
        }
    }

    /// <summary>
    /// Replaces a Fungus "Trigger" event handler on the selected object with an InkTrigger that plays a knot,
    /// keeping the tag filter and Enter/Stay/Exit settings. The flowchart itself stays in the scene.
    /// </summary>
    public class ConvertTriggerToInkWindow : EditorWindow
    {
        private string knot = "";

        [MenuItem("Polygon/Investigation/Convert Selected Fungus Trigger To Ink...")]
        public static void Open()
        {
            GetWindow<ConvertTriggerToInkWindow>(true, "Convert trigger to ink").minSize = new Vector2(380, 150);
        }

        void OnGUI()
        {
            var selected = Selection.activeGameObject;
            var trigger = selected != null ? selected.GetComponent<Fungus.Trigger>() : null;

            EditorGUILayout.HelpBox("Replaces the Fungus Trigger event handler of the selected object with an InkTrigger that " +
                "plays the knot below, with the same tag filter and fire-on settings. Undo reverts it.", MessageType.Info);
            knot = EditorGUILayout.TextField("Ink knot", knot);

            if (trigger == null)
            {
                EditorGUILayout.HelpBox("Select an object that has a Fungus Trigger event handler.", MessageType.Warning);
                return;
            }
            EditorGUILayout.LabelField("Trigger on", selected.name);

            if (GUILayout.Button("Convert") && !string.IsNullOrWhiteSpace(knot))
            {
                Convert(trigger, knot.Trim());
                Close();
            }
        }

        private static void Convert(Fungus.Trigger trigger, string knot)
        {
            var target = trigger.gameObject;
            Undo.SetCurrentGroupName("Convert trigger to ink");

            var source = new SerializedObject(trigger);
            var tags = source.FindProperty("tagFilter");
            var tagFilter = new string[tags.arraySize];
            for (int i = 0; i < tagFilter.Length; i++)
            {
                tagFilter[i] = tags.GetArrayElementAtIndex(i).stringValue;
            }
            // Fungus' PhysicsMessageType and InkTrigger.FireOn share the same flag values.
            var fireOn = (InkTrigger.FireOn)source.FindProperty("FireOn").intValue;

            var ink = Undo.AddComponent<InkTrigger>(target);
            Undo.RecordObject(ink, "Configure InkTrigger");
            ink.Configure(knot, tagFilter, fireOn);
            Undo.DestroyObjectImmediate(trigger);

            EditorSceneManager.MarkSceneDirty(target.scene);
            Debug.Log("[Investigation] " + target.name + " now plays ink knot '" + knot + "' on " + fireOn + " for tags [" +
                string.Join(", ", tagFilter) + "]. Save the scene to keep it, or Undo.", target);
        }
    }

    public static class InkEventsSetup
    {
        /// <summary>
        /// Adds InkEvents to the selected object, so its animation events can call PlayKnot("knot").
        /// </summary>
        [MenuItem("Polygon/Investigation/Add Ink Events To Selected")]
        public static void AddToSelected()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Ink events", "Select the object with the Animator first.", "OK");
                return;
            }
            if (selected.GetComponent<Animator>() == null)
            {
                Debug.LogWarning("[Investigation] " + selected.name + " has no Animator: animation events will not reach InkEvents.", selected);
            }
            if (selected.GetComponent<InkEvents>() == null)
            {
                Undo.AddComponent<InkEvents>(selected);
                EditorSceneManager.MarkSceneDirty(selected.scene);
                Debug.Log("[Investigation] Added InkEvents to " + selected.name + ". Save the scene to keep it.", selected);
            }
        }
    }

    public static class FungusCleanup
    {
        /// <summary>
        /// Removes the Fungus flowchart on the selected object together with its blocks, commands, event handlers and
        /// variables, which Fungus keeps as hidden components on the same object. Other components stay. Undoable.
        /// Only for flowcharts that nothing uses any more (check with Tools/FungusExport before).
        /// </summary>
        [MenuItem("Polygon/Investigation/Remove Fungus Flowchart From Selected")]
        public static void RemoveFromSelected()
        {
            var selected = Selection.activeGameObject;
            var flowchart = selected != null ? selected.GetComponent<Fungus.Flowchart>() : null;
            if (flowchart == null)
            {
                EditorUtility.DisplayDialog("Remove Fungus flowchart", "The selected object has no Fungus Flowchart.", "OK");
                return;
            }

            // Dependents first: event handlers require blocks, blocks and variables require the flowchart.
            var parts = new System.Collections.Generic.List<Component>();
            parts.AddRange(selected.GetComponents<Fungus.Command>());
            parts.AddRange(selected.GetComponents<Fungus.EventHandler>());
            parts.AddRange(selected.GetComponents<Fungus.Variable>());
            parts.AddRange(selected.GetComponents<Fungus.Block>());
            parts.Add(flowchart);

            if (!EditorUtility.DisplayDialog("Remove Fungus flowchart",
                "Remove the flowchart of '" + selected.name + "' and its " + (parts.Count - 1) + " hidden parts (blocks, commands, " +
                "event handlers, variables)? Other components stay. Undo restores it.", "Remove", "Cancel"))
            {
                return;
            }

            Undo.SetCurrentGroupName("Remove Fungus flowchart");
            foreach (var part in parts)
            {
                Undo.DestroyObjectImmediate(part);
            }
            EditorSceneManager.MarkSceneDirty(selected.scene);
            Debug.Log("[Investigation] Removed the Fungus flowchart of " + selected.name + " (" + parts.Count + " components). " +
                "Save the scene to keep it, or Undo.", selected);
        }
    }
}
