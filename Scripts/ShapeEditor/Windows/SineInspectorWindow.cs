#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>The inspector window with context sensitive properties.</summary>
    public class SineInspectorWindow : GuiWindow
    {
        private static readonly float2 windowSize = new float2(170, 131);
        private GuiFloatTextbox sineDetailTextbox;
        private int sineDetail = 64;
        private GuiFloatTextbox sineFrequencyTextbox;
        private float sineFrequency = -3.5f;

        public SineInspectorWindow() : base(float2.zero, windowSize) { }

        public override void OnActivate()
        {
            base.OnActivate();
            var resources = ShapeEditorResources.Instance;

            position = GetBottomRightPosition();

            Add(new GuiWindowTitle("Sine Inspector"));

            var layout = new GuiTableLayout(this, 5, 24);

            layout.Add(new GuiLabel("Detail Level"));
            layout.Space(100);
            layout.Add(sineDetailTextbox = new GuiFloatTextbox(new float2(40, 16)) { allowNegativeNumbers = false, minValue = 1 });

            layout.NextRow(1);

            for (int x = 1; x <= 8; x++)
            {
                var i = x * 16;
                layout.Add(new GuiButton(i.ToString(), 20, () =>
                {
                    sineDetail = i;
                    ApplySineDetailToSelectedEdges();
                }));
            }
            layout.NextRow();

            layout.Add(new GuiButton("Apply", new float2(windowSize.x - 10, 20), ApplySineDetailToSelectedEdges));

            layout.NextRow(8);

            layout.Add(new GuiLabel("Sine Frequency"));
            layout.Space(100);
            layout.Add(sineFrequencyTextbox = new GuiFloatTextbox(new float2(40, 16)));

            layout.NextRow(1);

            layout.Add(new GuiButton("Apply", new float2(windowSize.x - 10, 20), ApplySineFrequencyToSelectedEdges));
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
            sineDetail = Mathf.RoundToInt(sineDetailTextbox.UpdateValue(sineDetail));
            sineFrequency = sineFrequencyTextbox.UpdateValue(sineFrequency);

            base.OnRender();
        }

        private void ApplySineDetailToSelectedEdges()
        {
            editor.RegisterUndo("Apply Sine Detail");

            foreach (var segment in editor.ForEachSelectedEdge())
                if (segment.generator.type == SegmentGeneratorType.Sine)
                    segment.generator.sineDetail = sineDetail;
        }

        private void ApplySineFrequencyToSelectedEdges()
        {
            editor.RegisterUndo("Apply Sine Frequency");

            foreach (var segment in editor.ForEachSelectedEdge())
                if (segment.generator.type == SegmentGeneratorType.Sine)
                    segment.generator.sineFrequency = sineFrequency;
        }
    }
}

#endif