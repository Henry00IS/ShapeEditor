#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Helps place controls in horizontal columns and vertical rows.</summary>
    public class GuiTableLayout
    {
        private GuiContainer<GuiControl> container;
        private int xposition = 1;
        private int yposition = 1;
        private GuiVerticalLayout verticalLayout;
        private GuiHorizontalLayout horizontalLayout;

        public GuiTableLayout(GuiContainer<GuiControl> container, int xposition = 1, int yposition = 1)
        {
            this.container = container;
            this.xposition = xposition;
            this.yposition = yposition;
            verticalLayout = new GuiVerticalLayout(container, xposition, yposition);
            horizontalLayout = new GuiHorizontalLayout(container, xposition, yposition);
        }

        public void AddHorizontal(GuiControl control)
        {
            horizontalLayout.Add(control);
        }

        public void NextRow(int pixels)
        {
            verticalLayout.Space(pixels);
            horizontalLayout = new GuiHorizontalLayout(container, xposition, Mathf.FloorToInt(verticalLayout.windowSize.y));
        }

        public void Space(int pixels)
        {
            horizontalLayout.Space(pixels);
        }
    }
}

#endif