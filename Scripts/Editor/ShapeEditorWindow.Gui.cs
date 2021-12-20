#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorWindow
    {
        private List<GuiWindow> windows;
        /// <summary>The window that currently has input focus or null.</summary>
        internal GuiWindow activeWindow;

        public void DrawWindows()
        {
            if (windows == null)
            {
                windows = new List<GuiWindow>()
                {
                    new TopToolbarGuiWindow(this, float2.zero, float2.zero),
                    new BottomToolbarGuiWindow(this, float2.zero, float2.zero),
                    new ToolbarGuiWindow(this, new float2(20, 40), new float2(30, 400))
                };
            }

            var windowsCount = windows.Count;
            for (int i = 0; i < windowsCount; i++)
                windows[i].OnRender();
        }

        /// <summary>Attempts to find the top window at the given position.</summary>
        /// <param name="position">The position to find the window at.</param>
        /// <returns>The window instance if found else null.</returns>
        private GuiWindow FindWindowAtPosition(float2 position)
        {
            var windowsCount = windows.Count;
            for (int i = 0; i < windowsCount; i++)
                if (windows[i].rect.Contains(position))
                    return windows[i];
            return null;
        }
    }
}

#endif