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
                AddWindow(new TopToolbarGuiWindow(float2.zero, float2.zero));
                AddWindow(new BottomToolbarGuiWindow(float2.zero, float2.zero));
                AddWindow(new ToolbarGuiWindow(new float2(20, 40)));
                AddWindow(new TextboxTestWindow(new float2(300, 100), new float2(220, 80)));
            }

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
                if (windows[i].rect.Contains(position))
                    return windows[i];
            return null;
        }

        /// <summary>Puts the specified window in front of all other windows.</summary>
        /// <param name="window">The window to be put in front.</param>
        private void MoveWindowToFront(GuiWindow window)
        {
            windows.MoveItemAtIndexToFront(windows.IndexOf(window));
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
        internal void AddWindow(GuiWindow window)
        {
            window.editor = this;
            windows.Add(window);
            window.OnActivate();
        }
    }
}

#endif