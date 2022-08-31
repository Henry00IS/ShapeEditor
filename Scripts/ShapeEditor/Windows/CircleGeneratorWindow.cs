#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>The circle generator that builds circle shapes.</summary>
    public class CircleGeneratorWindow : GuiWindow
    {
        private static readonly float2 windowSize = new float2(130, 85);
        private GuiFloatTextbox circleDetailTextbox;
        private int circleDetail = 8;
        private GuiFloatTextbox circleDiameterTextbox;
        private float circleDiameter = 1f;

        public CircleGeneratorWindow() : base(float2.zero, windowSize) { }

        public override void OnActivate()
        {
            base.OnActivate();
            var resources = ShapeEditorResources.Instance;

            position = GetCenterPosition();

            Add(new GuiWindowTitle("Circle Generator"));

            var layout = new GuiTableLayout(this, 5, 24);

            layout.Add(new GuiLabel("Detail Level"));
            layout.Space(60);
            layout.Add(circleDetailTextbox = new GuiFloatTextbox(new float2(40, 16)) { allowNegativeNumbers = false, minValue = 3 });

            layout.NextRow(4);

            layout.Add(new GuiLabel("Diameter"));
            layout.Space(60);
            layout.Add(circleDiameterTextbox = new GuiFloatTextbox(new float2(40, 16)) { allowNegativeNumbers = false });

            layout.NextRow(4);

            layout.Add(new GuiButton("Create", new float2(120, 16), CreateCircleShape));
        }

        private float2 GetCenterPosition()
        {
            return new float2(
                Mathf.RoundToInt((editor.position.width / 2f) - (windowSize.x / 2f)),
                Mathf.RoundToInt((editor.position.height / 2f) - (windowSize.y / 2f))
            );
        }

        [Instructions(title: "Creates a circle shape of the specified dimension, with the specified detail level, which generates more vertices.")]
        private void CreateCircleShape()
        {
            editor.RegisterUndo("Create Circle Shape");

            // build a mathematical representation of the desired circle.
            var circle = new MathEx.Circle();
            circle.SetDiameter(circleDiameter);

            var shape = new Shape();

            shape.segments.Clear();
            for (int i = 0; i < circleDetail; i++)
            {
                var position = circle.GetCirclePosition(i / (float)circleDetail);

                var segment = new Segment(shape, new float2(position.x, -position.z));
                shape.AddSegment(segment);
            }

            editor.project.ClearSelection();
            editor.project.shapes.Add(shape);

            shape.SelectAll();
        }

        public override void OnRender()
        {
            circleDetail = Mathf.RoundToInt(circleDetailTextbox.UpdateValue(circleDetail));
            circleDiameter = circleDiameterTextbox.UpdateValue(circleDiameter);

            base.OnRender();
        }
    }
}

#endif