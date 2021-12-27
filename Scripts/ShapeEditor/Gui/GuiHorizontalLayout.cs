#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Helps place controls in a horizontal row.</summary>
    public class GuiHorizontalLayout
    {
        private GuiContainer<GuiControl> container;
        private int leftPosition = 1;
        private int topPosition = 1;

        public GuiHorizontalLayout(GuiContainer<GuiControl> container, int xposition = 1, int yposition = 1)
        {
            this.container = container;
            leftPosition = xposition;
            topPosition = yposition;
        }

        public void AddControl(GuiControl control)
        {
            container.Add(control);
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