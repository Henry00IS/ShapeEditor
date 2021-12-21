#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class BottomToolbarGuiWindow : GuiWindow
    {
        private GuiLabel statusLabel;
        private GuiFloatTextbox gridZoomTextbox;
        private GuiLabel gridZoomLabel;
        private GuiFloatTextbox gridSnapTextbox;
        private GuiLabel gridSnapLabel;

        public BottomToolbarGuiWindow(ShapeEditorWindow parent, float2 position, float2 size) : base(parent, position, size)
        {
            colorWindowBackground = new Color(0.192f, 0.192f, 0.192f);

            AddControl(statusLabel = new GuiLabel("", new float2(7f, 4f), new float2(200, 20)));

            AddControl(gridZoomTextbox = new GuiFloatTextbox(new float2(0f, 0f), new float2(50, 16)) { allowNegativeNumbers = false });
            AddControl(gridZoomLabel = new GuiLabel("Zoom:", new float2(0f, 0f), new float2(32, 20)));
            AddControl(gridSnapTextbox = new GuiFloatTextbox(new float2(0f, 0f), new float2(50, 16)) { allowNegativeNumbers = false });
            AddControl(gridSnapLabel = new GuiLabel("Snap:", new float2(0f, 0f), new float2(30, 20)));
        }

        public override void OnRender()
        {
            // stretch over the width of the window.
            position = new float2(0f, editor.position.height - 22f);
            size = new float2(editor.position.width, 22f);

            statusLabel.text = "2D Shape Editor";

            var xpos = size.x;

            // grid zoom textbox:
            xpos -= gridZoomTextbox.size.x + 3f;
            gridZoomTextbox.position = new float2(xpos, 3f);
            editor.gridZoom = gridZoomTextbox.UpdateValue(editor.gridZoom);

            // grid zoom label:
            xpos -= gridZoomLabel.size.x + 3f;
            gridZoomLabel.position = new float2(xpos, 4f);

            // grid snap textbox:
            xpos -= gridSnapTextbox.size.x + 3f;
            gridSnapTextbox.position = new float2(xpos, 3f);
            editor.gridSnap = gridSnapTextbox.UpdateValue(editor.gridSnap);

            // grid snap label:
            xpos -= gridSnapLabel.size.x + 3f;
            gridSnapLabel.position = new float2(xpos, 4f);

            base.OnRender();
        }
    }
}

#endif