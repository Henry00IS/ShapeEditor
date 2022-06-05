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
        private int nextRowPixels = 0;
        private GuiHorizontalLayout horizontalLayout;

        public GuiTableLayout(GuiContainer<GuiControl> container, int xposition = 1, int yposition = 1)
        {
            this.container = container;
            this.xposition = xposition;
            this.yposition = yposition;
            horizontalLayout = new GuiHorizontalLayout(container, xposition, yposition);
        }

        public void Add(GuiControl control)
        {
            horizontalLayout.Add(control);
            if (control.size.y > nextRowPixels)
                nextRowPixels = Mathf.FloorToInt(control.size.y);
        }

        public void NextRow(int pixels = 0)
        {
            yposition += nextRowPixels + pixels;
            horizontalLayout = new GuiHorizontalLayout(container, xposition, yposition);
            nextRowPixels = 0;
        }

        public void Space(int pixels)
        {
            horizontalLayout.Space(pixels);
        }
    }
}

#endif