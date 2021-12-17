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
                windows = new List<GuiWindow>()
                {
                    new ToolbarGuiWindow(new float2(20, 20), new float2(30, 400))
                };
            }

            var windowsCount = windows.Count;
            for (int i = 0; i < windowsCount; i++)
                windows[i].OnRender();
        }
    }
}

#endif