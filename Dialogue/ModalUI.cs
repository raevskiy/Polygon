using System;

namespace KopliSoft.Dialogue
{
    /// <summary>
    /// Raised by UI that needs the cursor and blocks gameplay input (ink dialogues, the notebook).
    /// UIController listens and keeps the counters, as it does for Fungus blocks.
    /// </summary>
    public static class ModalUI
    {
        public static event Action Opened;
        public static event Action Closed;

        public static void Open()
        {
            if (Opened != null)
            {
                Opened();
            }
        }

        public static void Close()
        {
            if (Closed != null)
            {
                Closed();
            }
        }
    }
}
