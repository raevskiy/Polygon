using System;
using System.Reflection;
using Fungus;
using UnityEngine;

namespace KopliSoft.Dialogue
{
    /// <summary>
    /// Repeats every Fungus "Send Message" as a C# event, so C# code can replace a Fungus "Message Received" listener
    /// while the senders are still Fungus flowcharts (e.g. "NewDay" from the Day flowchart). Temporary, like Fungus itself.
    /// </summary>
    public static class FungusMessageRelay
    {
        public static event Action<string> MessageSent;

        private static readonly FieldInfo MessageField = typeof(Fungus.SendMessage).GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            BlockSignals.OnCommandExecute -= OnCommandExecute;
            BlockSignals.OnCommandExecute += OnCommandExecute;
        }

        private static void OnCommandExecute(Block block, Command command, int commandIndex, int maxCommandIndex)
        {
            var sendMessage = command as Fungus.SendMessage;
            if (sendMessage == null || MessageSent == null || MessageField == null)
            {
                return;
            }
            var message = ((StringData)MessageField.GetValue(sendMessage)).Value;
            if (!string.IsNullOrEmpty(message))
            {
                MessageSent(message);
            }
        }
    }
}
