using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace KopliSoft.Controls
{
    /// <summary>
    /// Actions the player can bind to keys. Add new ones at the end (the names are stored in PlayerPrefs).
    /// </summary>
    public enum GameAction
    {
        Notebook
    }

    /// <summary>
    /// The one place our code reads action keys from, so a key binding menu can be added later without touching callers.
    /// Today it maps each action to a KeyCode (defaults below, player overrides in PlayerPrefs).
    /// Later it can be backed by Unity's Input System; Opsive's own input can be routed here through a custom UnityInputBase.
    /// </summary>
    public static class GameInput
    {
        public static event Action BindingsChanged;

        private const string PrefsPrefix = "binding.";

        private static readonly Dictionary<GameAction, KeyCode> Defaults = new Dictionary<GameAction, KeyCode>
        {
            { GameAction.Notebook, KeyCode.N },
        };

        private static Dictionary<GameAction, KeyCode> bindings;

        public static bool GetDown(GameAction action)
        {
            return Input.GetKeyDown(GetKey(action));
        }

        public static bool Get(GameAction action)
        {
            return Input.GetKey(GetKey(action));
        }

        public static KeyCode GetKey(GameAction action)
        {
            Load();
            KeyCode key;
            return bindings.TryGetValue(action, out key) ? key : KeyCode.None;
        }

        public static KeyCode GetDefaultKey(GameAction action)
        {
            KeyCode key;
            return Defaults.TryGetValue(action, out key) ? key : KeyCode.None;
        }

        /// <summary>
        /// Binds the action to a key and remembers it for this player.
        /// </summary>
        public static void Rebind(GameAction action, KeyCode key)
        {
            Load();
            bindings[action] = key;
            PlayerPrefs.SetString(PrefsPrefix + action, key.ToString());
            PlayerPrefs.Save();
            RaiseChanged();
        }

        public static void ResetToDefaults()
        {
            foreach (GameAction action in Enum.GetValues(typeof(GameAction)))
            {
                PlayerPrefs.DeleteKey(PrefsPrefix + action);
            }
            PlayerPrefs.Save();
            bindings = null;
            RaiseChanged();
        }

        /// <summary>
        /// The key as the player should read it in texts, e.g. "N", "1", "Left Shift".
        /// </summary>
        public static string DisplayName(GameAction action)
        {
            return DisplayName(GetKey(action));
        }

        /// <summary>
        /// Same as DisplayName(GameAction) for an action name, as ink passes it ("Notebook"). Null if there is no such action.
        /// </summary>
        public static string DisplayName(string actionName)
        {
            GameAction action;
            return Enum.TryParse(actionName, out action) ? DisplayName(action) : null;
        }

        public static string DisplayName(KeyCode key)
        {
            var name = key.ToString();
            if (name.StartsWith("Alpha"))
            {
                return name.Substring("Alpha".Length);
            }
            if (name.StartsWith("Keypad"))
            {
                return "Num " + name.Substring("Keypad".Length);
            }
            switch (key)
            {
                case KeyCode.None: return "(unbound)";
                case KeyCode.Mouse0: return "Left Mouse Button";
                case KeyCode.Mouse1: return "Right Mouse Button";
                case KeyCode.Mouse2: return "Middle Mouse Button";
            }
            // "LeftShift" -> "Left Shift"
            return Regex.Replace(name, "(?<=[a-z])(?=[A-Z0-9])", " ");
        }

        private static void Load()
        {
            if (bindings != null)
            {
                return;
            }
            bindings = new Dictionary<GameAction, KeyCode>(Defaults);
            foreach (GameAction action in Enum.GetValues(typeof(GameAction)))
            {
                var saved = PlayerPrefs.GetString(PrefsPrefix + action, null);
                KeyCode key;
                if (!string.IsNullOrEmpty(saved) && Enum.TryParse(saved, out key))
                {
                    bindings[action] = key;
                }
            }
        }

        private static void RaiseChanged()
        {
            if (BindingsChanged != null)
            {
                BindingsChanged();
            }
        }
    }
}
