#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Helps place controls in a horizontal row.</summary>
    public class GuiHorizontalLayout
    {
        private GuiWindow window;
        private int leftPosition = 1;
        private int topPosition = 1;

        public GuiHorizontalLayout(GuiWindow window, int yposition = 1)
        {
            this.window = window;
            topPosition = yposition;
        }

        public void AddControl(GuiControl control)
        {
            window.Add(control);
            control.position = new float2(leftPosition, topPosition);
            leftPosition += Mathf.FloorToInt(control.size.x);
        }

        public void Space(int pixels)
        {
            leftPosition += pixels;
        }
    }
}

#endif