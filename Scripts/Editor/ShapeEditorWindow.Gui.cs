#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorWindow
    {
        private List<GuiWindow> windows;

        public void DrawWindows()
        {
            if (windows == null)
            {
                windows = new List<GuiWindow>();
                OpenWindow(new TopToolbarWindow(float2.zero, float2.zero), false);
                OpenWindow(new BottomToolbarWindow(float2.zero, float2.zero), false);
                OpenWindow(new ToolbarWindow(new float2(20, 60)), false);
            }

            // remove any closed windows.
            RemoveClosedWindows();

            // before we draw the windows we check whether the mouse is obstructed.
            UpdateWindowMouseObstructionFlags();

            // render windows in reverse.
            var windowsCount = windows.Count;
            for (int i = windowsCount; i-- > 0;)
                windows[i].OnRender();
        }

        /// <summary>Attempts to find the top window at the given position.</summary>
        /// <param name="position">The position to find the window at.</param>
        /// <returns>The window instance if found else null.</returns>
        private GuiWindow FindWindowAtPosition(float2 position)
        {
            if (windows == null) return null;
            var windowsCount = windows.Count;
            for (int i = 0; i < windowsCount; i++)
                if (windows[i].drawRect.Contains(position))
                    return windows[i];
            return null;
        }

        /// <summary>Puts the specified window in front of all other windows.</summary>
        /// <param name="window">The window to be put in front.</param>
        private void MoveWindowToFront(GuiWindow window)
        {
            // if we are a child window then the owner window must be moved to the front first.
            if (window.parent != null && window.parent is GuiWindow parent)
            {
                MoveWindowToFront(parent);
                return;
            }

            // we are a parent (or regular) window and move ourselves to the front.
            windows.MoveItemAtIndexToFront(windows.IndexOf(window));

            // lastly we move all of our child windows to the front.
            var windowsCount = windows.Count;
            for (int i = 0; i < windowsCount; i++)
            {
                var child = windows[i];
                if (child.parent == window)
                    windows.MoveItemAtIndexToFront(windows.IndexOf(child));
            }
        }

        /// <summary>
        /// Iterates over all windows and tests whether the mouse is obstructed. This is useful for
        /// hover effects that should not occur when a control is behind another window.
        /// </summary>
        private void UpdateWindowMouseObstructionFlags()
        {
            bool obstructed = false;
            var windowsCount = windows.Count;
            for (int i = 0; i < windowsCount; i++)
            {
                if (obstructed)
                {
                    windows[i].isMouseObstructed = true;
                }
                else
                {
                    if (windows[i].isMouseOver)
                    {
                        windows[i].isMouseObstructed = false;
                        obstructed = true;
                    }
                }
            }
        }

        /// <summary>Adds the window the shape editor window.</summary>
        /// <param name="window">The window to be added.</param>
        /// <param name="focus">Whether to try and give the window input focus.</param>
        internal void OpenWindow(GuiWindow window, bool focus = true)
        {
            window.editor = this;
            window.closed = false;

            if (!windows.Contains(window))
            {
                windows.Insert(0, window);
                window.OnActivate();
            }

            if (focus)
                TrySwitchActiveEventReceiver(window);
        }

        /// <summary>Removes all of the windows that were marked as closed.</summary>
        private void RemoveClosedWindows()
        {
            // iterate over the windows in reverse.
            var windowsCount = windows.Count;
            for (int i = windowsCount; i-- > 0;)
            {
                var window = windows[i];
                if (window.closed)
                {
                    window.OnDeactivate();
                    windows.RemoveAt(i);
                }
            }
        }
    }
}

#endif