#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class BottomToolbarGuiWindow : GuiWindow
    {
        private GuiLabel statusLabel;

        public BottomToolbarGuiWindow(ShapeEditorWindow parent, float2 position, float2 size) : base(parent, position, size)
        {
            colorWindowBackground = new Color(0.192f, 0.192f, 0.192f);

            AddControl(statusLabel = new GuiLabel("", new float2(7f, 4f), new float2(200, 20)));
        }

        public override void OnRender()
        {
            position = new float2(0f, parent.position.height - 22f);
            size = new float2(parent.position.width, 22f);

            statusLabel.text = "2D Shape Editor - Snap: " + parent.gridSnap + " Zoom: " + parent.gridZoom;

            base.OnRender();
        }
    }
}

#endif