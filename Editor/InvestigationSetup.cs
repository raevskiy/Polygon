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
}
