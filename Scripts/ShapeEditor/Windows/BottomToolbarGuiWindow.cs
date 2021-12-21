#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class BottomToolbarGuiWindow : GuiWindow
    {
        private GuiLabel statusLabel;
        private GuiFloatTextbox gridSnapFloatTextbox;
        private GuiLabel gridSnapLabel;

        private float lastZoom;

        public BottomToolbarGuiWindow(ShapeEditorWindow parent, float2 position, float2 size) : base(parent, position, size)
        {
            colorWindowBackground = new Color(0.192f, 0.192f, 0.192f);

            AddControl(statusLabel = new GuiLabel("", new float2(7f, 4f), new float2(200, 20)));

            AddControl(gridSnapFloatTextbox = new GuiFloatTextbox(new float2(250, 3), new float2(50, 16)));
            AddControl(gridSnapLabel = new GuiLabel("Zoom: ", new float2(7f, 4f), new float2(50, 20)));
        }

        public override void OnRender()
        {
            position = new float2(0f, parent.position.height - 22f);
            size = new float2(parent.position.width, 22f);

            statusLabel.text = "2D Shape Editor - Snap: " + parent.gridSnap + " Zoom: " + parent.gridZoom;

            gridSnapLabel.position = new float2(size.x - 88f, gridSnapLabel.position.y);
            gridSnapFloatTextbox.position = new float2(size.x - 53f, gridSnapFloatTextbox.position.y);

            if (parent.gridZoom != lastZoom)
            {
                lastZoom = parent.gridZoom;
                gridSnapFloatTextbox.value = lastZoom;
            }

            base.OnRender();
        }
    }
}

#endif