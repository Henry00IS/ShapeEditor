#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorWindow
    {
        /// <summary>The active event receiver with input focus (e.g. a tool or window etc.).</summary>
        private IEditorEventReceiver activeEventReceiver;

        /// <summary>Gets whether the active event receiver is a window.</summary>
        internal bool activeEventReceiverIsWindow => activeEventReceiver is GuiWindow;

        /// <summary>Gets whether the active event receiver is a widget.</summary>
        internal bool activeEventReceiverIsWidget => activeEventReceiver is Widget;

        /// <summary>Gets whether the active event receiver is a tool.</summary>
        internal bool activeEventReceiverIsTool => activeEventReceiver is Tool;

        /// <summary>Checks whether the active tool is busy.</summary>
        internal bool isToolBusy => activeEventReceiverIsTool && GetActiveEventReceiver().IsBusy();

        /// <summary>Gets the active event receiver as <typeparamref name="T"/>.</summary>
        private T GetActiveEventReceiver<T>()
        {
            return (T)activeEventReceiver;
        }

        /// <summary>
        /// Tries to switch the active event receiver to the specied receiver. This will fail when
        /// the active receiver is busy.
        /// </summary>
        /// <param name="eventReceiver">The event receiver to try and switch to.</param>
        /// <returns>True when the switch was successful else false.</returns>
        private bool TrySwitchActiveEventReceiver(IEditorEventReceiver eventReceiver)
        {
            if (eventReceiver == null) return false;

            if (activeEventReceiver != null)
            {
                if (activeEventReceiver.IsBusy()) return false;
                activeEventReceiver.OnFocusLost();
            }

            activeEventReceiver = eventReceiver;
            activeEventReceiver.editor = this;
            activeEventReceiver.OnFocus();

            return true;
        }

        /// <summary>Gets the active event receiver with input focus and ensures it's never null.</summary>
        private IEditorEventReceiver GetActiveEventReceiver()
        {
            // fallback to the default tool.
            if (activeEventReceiver == null)
            {
                Debug.Log("Setting the default box select tool as the active event receiver.");
                ValidateTools();
                TrySwitchActiveEventReceiver(boxSelectTool);
            }
            return activeEventReceiver;
        }

        /// <summary>Gets whether the specified event receiver is active with input focus.</summary>
        /// <param name="eventReceiver">The event receiver to check.</param>
        internal bool IsActive(IEditorEventReceiver eventReceiver)
        {
            return activeEventReceiver == eventReceiver;
        }
    }
}

#endif