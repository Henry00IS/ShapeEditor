#if UNITY_EDITOR

using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    // add additional fields for this tool to segments.
    public partial class Segment
    {
        /// <summary>Editor variable used by <see cref="RotateTool"/>.</summary>
        [System.NonSerialized]
        public float2 rotateToolInitialPosition;
    }

    public class RotateTool : BoxSelectTool
    {
        private RotationWidget rotationWidget = new RotationWidget();

        public override void OnActivate()
        {
            base.OnActivate();

            editor.AddWidget(rotationWidget);

            rotationWidget.onBeginRotating = () =>
            {
                // store the initial position of all selected segments.
                foreach (var segment in editor.ForEachSelectedSegment())
                    segment.rotateToolInitialPosition = segment.position;
            };

            rotationWidget.onRotation = (pivot, degrees) =>
            {
                // rotate the selected segments using their initial position.
                foreach (var segment in editor.ForEachSelectedSegment())
                {
                    segment.position = MathEx.RotatePointAroundPivot(segment.rotateToolInitialPosition, pivot, degrees);
                }
            };
        }

        public override void OnRender()
        {
            base.OnRender();

            if (editor.selectedSegmentsCount > 0)
            {
                rotationWidget.position = editor.selectedSegmentsAveragePosition;
                rotationWidget.visible = true;
            }
            else
            {
                rotationWidget.visible = false;
            }
        }
    }
}

#endif