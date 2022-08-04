#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>The inspector window with context sensitive properties.</summary>
    public class RepeatInspectorWindow : GuiWindow
    {
        private static readonly float2 windowSize = new float2(170, 107);
        private GuiFloatTextbox repeatSegmentsTextbox;
        private int repeatSegments = 2;
        private GuiFloatTextbox repeatTimesTextbox;
        private int repeatTimes = 4;

        public RepeatInspectorWindow() : base(float2.zero, windowSize) { }

        public override void OnActivate()
        {
            base.OnActivate();
            var resources = ShapeEditorResources.Instance;

            position = GetBottomRightPosition();

            Add(new GuiWindowTitle("Repeat Inspector"));

            var layout = new GuiTableLayout(this, 5, 24);

            layout.Add(new GuiLabel("Repeat Count"));
            layout.Space(60);
            layout.Add(repeatSegmentsTextbox = new GuiFloatTextbox(new float2(40, 16)) { allowNegativeNumbers = false, minValue = 1 });
            layout.Add(new GuiButton("Apply", new float2(40, 16), ApplyRepeatSegmentsToSelectedEdges));

            layout.NextRow(1);

            for (int i = 1; i <= 8; i++)
            {
                var x = i;
                layout.Add(new GuiButton(x.ToString(), 20, () =>
                {
                    repeatSegments = x;
                    ApplyRepeatSegmentsToSelectedEdges();
                }));
            }

            layout.NextRow(4);

            layout.Add(new GuiLabel("Repeat Times"));
            layout.Space(60);
            layout.Add(repeatTimesTextbox = new GuiFloatTextbox(new float2(40, 16)) { allowNegativeNumbers = false, minValue = 1 });
            layout.Add(new GuiButton("Apply", new float2(40, 16), ApplyRepeatTimesToSelectedEdges));

            layout.NextRow(1);

            for (int i = 1; i <= 8; i++)
            {
                var x = i;
                layout.Add(new GuiButton(x.ToString(), 20, () =>
                {
                    repeatTimes = x;
                    ApplyRepeatTimesToSelectedEdges();
                }));
            }
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
            repeatSegments = Mathf.RoundToInt(repeatSegmentsTextbox.UpdateValue(repeatSegments));
            repeatTimes = Mathf.RoundToInt(repeatTimesTextbox.UpdateValue(repeatTimes));

            base.OnRender();
        }

        [Instructions(title: "Applies the specified amount of previous segments to be repeated.")]
        private void ApplyRepeatSegmentsToSelectedEdges()
        {
            editor.RegisterUndo("Apply Repeat Segments Count");

            foreach (var segment in editor.ForEachSelectedEdge())
                if (segment.generator.type == SegmentGeneratorType.Repeat)
                    segment.generator.repeatSegments = repeatSegments;
        }

        [Instructions(title: "Applies the specified amount of times to repeat the previous segments.")]
        private void ApplyRepeatTimesToSelectedEdges()
        {
            editor.RegisterUndo("Apply Repeat Times");

            foreach (var segment in editor.ForEachSelectedEdge())
                if (segment.generator.type == SegmentGeneratorType.Repeat)
                    segment.generator.repeatTimes = repeatTimes;
        }
    }
}

#endif