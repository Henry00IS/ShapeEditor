#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>The inspector window with context sensitive properties.</summary>
    public class SineInspectorWindow : GuiWindow
    {
        private static readonly float2 windowSize = new float2(170, 106);
        private GuiFloatTextbox sineDetailTextbox;
        private int sineDetail = 64;
        private GuiFloatTextbox sineFrequencyTextbox;
        private float sineFrequency = -3.5f;
        private GuiFloatTextbox sineGridSnapSizeTextbox;
        private float sineGridSnapSize = 0f;

        public SineInspectorWindow() : base(float2.zero, windowSize) { }

        public override void OnActivate()
        {
            base.OnActivate();
            var resources = ShapeEditorResources.Instance;

            position = GetBottomRightPosition();

            Add(new GuiWindowTitle("Sine Inspector"));

            var layout = new GuiTableLayout(this, 5, 24);

            layout.Add(new GuiLabel("Detail Level"));
            layout.Space(60);
            layout.Add(sineDetailTextbox = new GuiFloatTextbox(new float2(40, 16)) { allowNegativeNumbers = false, minValue = 1 });
            layout.Add(new GuiButton("Apply", new float2(40, 16), ApplySineDetailToSelectedEdges));

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

            layout.NextRow(4);

            layout.Add(new GuiLabel("Sine Frequency"));
            layout.Space(60);
            layout.Add(sineFrequencyTextbox = new GuiFloatTextbox(new float2(40, 16)));
            layout.Add(new GuiButton("Apply", new float2(40, 16), ApplySineFrequencyToSelectedEdges));

            layout.NextRow(4);

            layout.Add(new GuiLabel("Grid Snap Size"));
            layout.Space(60);
            layout.Add(sineGridSnapSizeTextbox = new GuiFloatTextbox(new float2(40, 16)) { allowNegativeNumbers = false, minValue = 0f });
            layout.Add(new GuiButton("Apply", new float2(40, 16), ApplyGridSnapSizeToSelectedEdges));
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
            sineDetail = Mathf.RoundToInt(sineDetailTextbox.UpdateValue(sineDetail));
            sineFrequency = sineFrequencyTextbox.UpdateValue(sineFrequency);
            sineGridSnapSize = sineGridSnapSizeTextbox.UpdateValue(sineGridSnapSize);

            base.OnRender();
        }

        [Instructions(title: "Applies the specified sine detail level, which generates more vertices.")]
        private void ApplySineDetailToSelectedEdges()
        {
            editor.RegisterUndo("Apply Sine Detail");

            foreach (var segment in editor.ForEachSelectedEdge())
                if (segment.generator.type == SegmentGeneratorType.Sine)
                    segment.generator.sineDetail = sineDetail;
        }

        [Instructions(title: "Applies the specified sine frequency. A positive number inverts the sine wave. Adding 0.5 ends the wave on the same polarity as it started.")]
        private void ApplySineFrequencyToSelectedEdges()
        {
            editor.RegisterUndo("Apply Sine Frequency");

            foreach (var segment in editor.ForEachSelectedEdge())
                if (segment.generator.type == SegmentGeneratorType.Sine)
                    segment.generator.sineFrequency = sineFrequency;
        }

        [Instructions(title: "Applies the specified grid size, which snaps all generated vertices to it (set to 0 to disable).")]
        private void ApplyGridSnapSizeToSelectedEdges()
        {
            editor.RegisterUndo("Apply Bezier Grid Snap Size");

            foreach (var segment in editor.ForEachSelectedEdge())
                if (segment.generator.type == SegmentGeneratorType.Sine)
                    segment.generator.sineGridSnapSize = sineGridSnapSize;
        }
    }
}

#endif