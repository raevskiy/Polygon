using Ink.Runtime;
using KopliSoft.Controls;
using KopliSoft.Investigation;
using KopliSoft.Vehicles;
using UnityEngine;

namespace KopliSoft.Dialogue
{
    /// <summary>
    /// Owns the chapter's case file and ink story, and persists both in the save game.
    /// One per chapter scene.
    /// </summary>
    public class InvestigationManager : MonoBehaviour, IDataPersistence
    {
        [Tooltip("case_*.json of the chapter")]
        [SerializeField]
        private TextAsset caseJson;
        [Tooltip("Compiled master ink file of the chapter (the .json next to the .ink)")]
        [SerializeField]
        private TextAsset inkJson;

        public static InvestigationManager Instance { get; private set; }

        public CaseFile CaseFile { get; private set; }
        public Story Story { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("Found multiple investigation managers in the scene");
            }
            Instance = this;

            CaseFile = new CaseFile(CaseDefinition.FromJson(caseJson.text));
            CaseFile.NoteAdded += note => Debug.Log("Note written down: " + note.id);
            CaseFile.ConclusionReached += conclusion => Debug.Log("Conclusion reached: " + conclusion.id);
            CaseFile.TaskStarted += task => Debug.Log("Task started: " + task.id);
            CaseFile.TaskCompleted += task => Debug.Log("Task completed: " + task.id);

            Story = new Story(inkJson.text);
            Story.onError += (message, type) =>
            {
                if (type == Ink.ErrorType.Error)
                {
                    Debug.LogError("Ink: " + message);
                }
                else
                {
                    Debug.LogWarning("Ink: " + message);
                }
            };
            InkCaseBindings.Bind(Story, CaseFile, Debug.LogWarning);
            FungusBridge.Bind(Story);
            // World state changes, e.g. ~ airship_event("landing_complete").
            Story.BindExternalFunction("airship_event", (string name) =>
            {
                var airship = AirshipState.Instance;
                if (airship == null || !airship.HandleEvent(name))
                {
                    Debug.LogWarning("Ink: airship_event('" + name + "') " + (airship == null ? "has no AirshipState in the scene" : "is unknown"));
                }
            }, false);
            // Texts name keys through the player's current bindings, e.g. {key_name("Notebook")}.
            Story.BindExternalFunction("key_name", (string action) =>
            {
                var name = GameInput.DisplayName(action);
                if (name == null)
                {
                    Debug.LogWarning("Ink: unknown input action '" + action + "'");
                }
                return (object)(name ?? action);
            }, true);
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void LoadData(GameData data)
        {
            if (data.investigation != null && data.investigation.caseId == CaseFile.Definition.id)
            {
                CaseFile.RestoreState(data.investigation);
            }
            if (!string.IsNullOrEmpty(data.inkState))
            {
                Story.state.LoadJson(data.inkState);
            }
        }

        public void SaveData(ref GameData data)
        {
            data.investigation = CaseFile.CaptureState();
            data.inkState = Story.state.ToJson();
        }
    }
}
