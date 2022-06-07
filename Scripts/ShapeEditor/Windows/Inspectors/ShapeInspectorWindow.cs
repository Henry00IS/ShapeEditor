#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>The inspector window with context sensitive properties.</summary>
    public class ShapeInspectorWindow : GuiWindow
    {
        private static readonly float2 windowSize = new float2(170, 44);
        private SimpleGlobalAxis symmetryAxes = SimpleGlobalAxis.None;
        private GuiButton horizontalSymmetryButton;
        private GuiButton verticalSymmetryButton;

        public ShapeInspectorWindow() : base(float2.zero, windowSize) { }

        public override void OnActivate()
        {
            base.OnActivate();
            var resources = ShapeEditorResources.Instance;

            position = GetBottomRightPosition();

            Add(new GuiWindowTitle("Shape Inspector"));

            var layout = new GuiTableLayout(this, 5, 24);

            layout.Add(new GuiLabel("Symmetry Mode"));
            layout.Space(64);
            layout.Add(horizontalSymmetryButton = new GuiButton(resources.shapeEditorFlipHorizontally, new float2(16f, 16f), () => symmetryAxes ^= SimpleGlobalAxis.Horizontal));
            layout.Space(2);
            layout.Add(verticalSymmetryButton = new GuiButton(resources.shapeEditorFlipVertically, new float2(16f, 16f), () => symmetryAxes ^= SimpleGlobalAxis.Vertical));
            layout.Space(2);
            layout.Add(new GuiButton("Apply", new float2(40, 16), ApplySymmetryAxesToSelectedShapes));
        }

        private float2 GetBottomRightPosition()
        {
            return new float2(
                Mathf.RoundToInt(editor.position.width - windowSize.x - 20),
                Mathf.RoundToInt(editor.position.height - windowSize.y - 42)
            );
        }

        public override void OnRender()
        {
            horizontalSymmetryButton.isChecked = symmetryAxes.HasFlag(SimpleGlobalAxis.Horizontal);
            verticalSymmetryButton.isChecked = symmetryAxes.HasFlag(SimpleGlobalAxis.Vertical);

            base.OnRender();
        }

        private void ApplySymmetryAxesToSelectedShapes()
        {
            editor.RegisterUndo("Apply Symmetry Axes");

            foreach (var shape in editor.ForEachSelectedShape())
                shape.symmetryAxes = symmetryAxes;
        }
    }
}

#endif