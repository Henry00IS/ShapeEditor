#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Helps place controls in a vertical column.</summary>
    public class GuiVerticalLayout
    {
        private GuiContainer<GuiControl> container;
        private int leftPosition = 1;
        private int topPosition = 1;
        private int minWidth = 2;

        public GuiVerticalLayout(GuiContainer<GuiControl> container, int xposition = 1, int yposition = 1)
        {
            this.container = container;
            leftPosition = xposition;
            topPosition = yposition;
        }

        public void AddControl(GuiControl control)
        {
            container.Add(control);
            control.position = new float2(leftPosition, topPosition);
            topPosition += Mathf.FloorToInt(control.size.y);

            if (control.size.x + 2f > minWidth)
            {
                minWidth = Mathf.FloorToInt(control.size.x + 2f);
            }
        }

        public void Space(int pixels)
        {
            topPosition += pixels;
        }

        public float2 windowSize => new float2(minWidth, topPosition + 1);
    }
}

#endif