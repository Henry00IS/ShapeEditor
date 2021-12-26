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

        public GuiHorizontalLayout(GuiWindow window)
        {
            this.window = window;
        }

        public void AddControl(GuiControl control)
        {
            window.AddControl(control);
            control.position = new float2(leftPosition, 1f);
            leftPosition += Mathf.FloorToInt(control.size.x);
        }

        public void Space(int pixels)
        {
            leftPosition += pixels;
        }
    }
}

#endif