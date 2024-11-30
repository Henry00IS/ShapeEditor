#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class BottomToolbarWindow : GuiWindow
    {
        private GuiLabel statusLabel;
        private GuiFloatTextbox gridZoomTextbox;
        private GuiLabel gridZoomLabel;
        private GuiFloatTextbox gridSnapTextbox;
        private GuiLabel gridSnapLabel;
        private GuiFloatTextbox angleSnapTextbox;
        private GuiLabel angleSnapLabel;
        private GuiButton snappingToggleButton;

        public BottomToolbarWindow(float2 position, float2 size) : base(position, size) { }

        public override void OnActivate()
        {
            base.OnActivate();

            var resources = ShapeEditorResources.Instance;
            colorWindowBackground = new Color(0.192f, 0.192f, 0.192f);

            Add(statusLabel = new GuiLabel("", new float2(7f, 4f), new float2(200, 20)));

            Add(gridZoomTextbox = new GuiFloatTextbox(new float2(50, 16)) { allowNegativeNumbers = false });
            Add(gridZoomLabel = new GuiLabel("Zoom:", new float2(32, 20)));
            Add(gridSnapTextbox = new GuiFloatTextbox(new float2(50, 16)) { allowNegativeNumbers = false });
            Add(gridSnapLabel = new GuiLabel("Snap:", new float2(30, 20)));
            Add(angleSnapTextbox = new GuiFloatTextbox(new float2(50, 16)) { allowNegativeNumbers = false });
            Add(angleSnapLabel = new GuiLabel("Angle:", new float2(32, 20)));

            Add(snappingToggleButton = new GuiButton(resources.shapeEditorSnapping, 20, editor.UserToggleGridSnapping));
        }

        public override void OnRender()
        {
            var resources = ShapeEditorResources.Instance;

            // stretch over the width of the window.
            position = new float2(0f, editor.height - 22f);
            size = new float2(editor.width, 22f);

            statusLabel.text = "2D Shape Editor (" + editor.totalSegmentsCount + " Segments, Render: " + editor.lastRenderTime + "ms)";

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

            // angle snap textbox:
            xpos -= angleSnapTextbox.size.x + 3f;
            angleSnapTextbox.position = new float2(xpos, 3f);
            editor.angleSnap = angleSnapTextbox.UpdateValue(editor.angleSnap);

            // angle snap label:
            xpos -= angleSnapLabel.size.x + 3f;
            angleSnapLabel.position = new float2(xpos, 4f);

            // snapping toggle:
            xpos -= snappingToggleButton.size.x + 3f;
            snappingToggleButton.position = new float2(xpos, 1f);
            snappingToggleButton.isChecked = editor.snapEnabled;
            snappingToggleButton.icon = editor.snapEnabled ? resources.shapeEditorSnapping : resources.shapeEditorSnappingDisabled;

            base.OnRender();
        }
    }
}

#endif