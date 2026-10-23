using KopliSoft.Dialogue;
using UnityEngine;

namespace KopliSoft.Vehicles
{
    /// <summary>
    /// State of the airship during the chapter: landed, moored after the first night, the captain inside or out,
    /// talking to the captain unlocked. Switches the matching scene objects and keeps the state in the save game.
    /// Replaces the airship's Fungus state flowchart.
    /// Changed by ink (airship_event) and by the Fungus messages "NewDay" and "ProtagonistExitsAirship".
    /// </summary>
    public class AirshipState : MonoBehaviour, IDataPersistence
    {
        [Header("Moored after the first night")]
        [SerializeField] private GameObject moving;
        [SerializeField] private GameObject anchored;

        [Header("The captain leaves the airship with the player")]
        [SerializeField] private GameObject captainInside;
        [SerializeField] private GameObject captainOutside;

        [Header("Landing complete")]
        [SerializeField] private GameObject machineNoise;
        [SerializeField] private GameObject landingNoise;
        [SerializeField] private GameObject exitControlCar;
        [SerializeField] private GameObject captainApproachedTrigger;

        [Header("Talking to the captain unlocked")]
        [SerializeField] private GameObject sleepTrigger;
        [SerializeField] private GameObject captainDialog;

        public static AirshipState Instance { get; private set; }

        private AirshipSaveData state = new AirshipSaveData();

        public bool Moored { get { return state.moored; } }
        public bool CaptainInside { get { return state.captainInside; } }
        public bool LandingComplete { get { return state.landingComplete; } }
        public bool TalkingToCaptainUnlocked { get { return state.talkingToCaptainUnlocked; } }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("Found multiple airship states in the scene");
            }
            Instance = this;
            FungusMessageRelay.MessageSent += OnFungusMessage;
        }

        void OnDestroy()
        {
            FungusMessageRelay.MessageSent -= OnFungusMessage;
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Events from ink: ~ airship_event("landing_complete"). Returns false for unknown events.
        /// </summary>
        public bool HandleEvent(string name)
        {
            switch (name)
            {
                case "landing_complete": CompleteLanding(); return true;
                case "unlock_captain": UnlockTalkingToCaptain(); return true;
                case "moor": Moor(); return true;
                case "captain_disembarks": CaptainDisembarks(); return true;
                default: return false;
            }
        }

        private void OnFungusMessage(string message)
        {
            if (message == "NewDay")
            {
                Moor();
            }
            else if (message == "ProtagonistExitsAirship")
            {
                CaptainDisembarks();
            }
        }

        /// <summary>The landing animation has finished.</summary>
        public void CompleteLanding()
        {
            state.landingComplete = true;
            ApplyLanding();
            SetActive(landingNoise, true);
        }

        /// <summary>The first night has passed: the airship stops moving and is anchored.</summary>
        public void Moor()
        {
            state.moored = true;
            ApplyMoored();
        }

        /// <summary>The player leaves the airship; the captain comes along.</summary>
        public void CaptainDisembarks()
        {
            state.captainInside = false;
            ApplyCaptainOutside();
        }

        /// <summary>A story event unlocks sleeping aboard and talking to the captain.</summary>
        public void UnlockTalkingToCaptain()
        {
            state.talkingToCaptainUnlocked = true;
            ApplyTalkingToCaptain();
        }

        public void LoadData(GameData data)
        {
            if (data.airship == null)
            {
                return;
            }
            state = data.airship;
            // Only what has happened is applied; the scene's initial setup covers the rest, as with the old flowchart.
            if (state.moored)
            {
                ApplyMoored();
            }
            if (!state.captainInside)
            {
                ApplyCaptainOutside();
            }
            if (state.landingComplete)
            {
                ApplyLanding();
            }
            if (state.talkingToCaptainUnlocked)
            {
                ApplyTalkingToCaptain();
            }
        }

        public void SaveData(ref GameData data)
        {
            data.airship = new AirshipSaveData
            {
                moored = state.moored,
                captainInside = state.captainInside,
                landingComplete = state.landingComplete,
                talkingToCaptainUnlocked = state.talkingToCaptainUnlocked
            };
        }

        private void ApplyMoored()
        {
            SetActive(moving, false);
            SetActive(anchored, true);
        }

        private void ApplyCaptainOutside()
        {
            SetActive(captainInside, false);
            SetActive(captainOutside, true);
        }

        private void ApplyLanding()
        {
            SetActive(machineNoise, false);
            SetActive(exitControlCar, true);
            SetActive(captainApproachedTrigger, false);
        }

        private void ApplyTalkingToCaptain()
        {
            SetActive(sleepTrigger, true);
            SetActive(captainDialog, true);
        }

        private void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
            else
            {
                Debug.LogWarning("AirshipState: a scene reference is not set (Polygon → Investigation → Set Up Airship State)", this);
            }
        }
    }
}
