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
            rotationWidget.onBeginRotating = () => CommonAction_OnBeginRotating(editor);
            rotationWidget.onRotation = (pivot, degrees) => CommonAction_OnRotation(editor, pivot, degrees);
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

        public static void CommonAction_OnBeginRotating(ShapeEditorWindow editor)
        {
            editor.RegisterUndo("Rotate Selection");

            // store the initial position of all selected segments.
            foreach (var segment in editor.ForEachSelectedSegment())
                segment.rotateToolInitialPosition = segment.position;
        }

        public static void CommonAction_OnRotation(ShapeEditorWindow editor, float2 pivot, float degrees)
        {
            // rotate the selected segments using their initial position.
            foreach (var segment in editor.ForEachSelectedSegment())
                segment.position = MathEx.RotatePointAroundPivot(segment.rotateToolInitialPosition, pivot, degrees);
        }
    }
}

#endif