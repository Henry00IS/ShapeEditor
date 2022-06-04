#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>The inspector window with context sensitive properties.</summary>
    public class BezierInspectorWindow : GuiWindow
    {
        private static readonly float2 windowSize = new float2(170, 146);
        private GuiFloatTextbox bezierDetailTextbox;
        private int bezierDetail = 8;

        public BezierInspectorWindow() : base(float2.zero, windowSize) { }

        public override void OnActivate()
        {
            base.OnActivate();
            var resources = ShapeEditorResources.Instance;

            position = GetBottomRightPosition();

            Add(new GuiWindowTitle("Bezier Inspector"));

            var layout = new GuiTableLayout(this, 5, 24);

            layout.AddHorizontal(new GuiLabel("Detail Level"));
            layout.Space(100);
            layout.AddHorizontal(bezierDetailTextbox = new GuiFloatTextbox(new float2(40, 16)) { allowNegativeNumbers = false, minValue = 1 });
            bezierDetailTextbox.UpdateValue(8);

            layout.NextRow(16);

            for (int y = 0; y < 4; y++)
            {
                for (int x = 1; x <= 8; x++)
                {
                    var i = (y * 8) + x;
                    layout.AddHorizontal(new GuiButton(i.ToString(), 20, () =>
                    {
                        bezierDetail = i;
                        ApplyBezierDetailToSelectedEdges();
                    }));
                }
                layout.NextRow(20);
            }

            layout.AddHorizontal(new GuiButton("Apply", new float2(windowSize.x - 10, 20), ApplyBezierDetailToSelectedEdges));
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
            // bezier detail textbox:
            bezierDetail = Mathf.RoundToInt(bezierDetailTextbox.UpdateValue(bezierDetail));

            base.OnRender();
        }

        private void ApplyBezierDetailToSelectedEdges()
        {
            editor.RegisterUndo("Apply Bezier Detail");

            foreach (var segment in editor.ForEachSelectedEdge())
                if (segment.generator.type == SegmentGeneratorType.Bezier)
                    segment.generator.bezierDetail = bezierDetail;
        }
    }
}

#endif