#if UNITY_EDITOR

using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    // add additional fields for this tool to segments.
    public partial class Segment
    {
        /// <summary>Editor variable used by <see cref="ScaleTool"/>.</summary>
        [System.NonSerialized]
        public float2 scaleToolInitialPosition;
    }

    public class ScaleTool : BoxSelectTool
    {
        private ScaleWidget scaleWidget = new ScaleWidget();

        public override void OnActivate()
        {
            base.OnActivate();

            editor.AddWidget(scaleWidget);
            scaleWidget.onBeginScaling = () => CommonAction_OnBeginScaling(editor);
            scaleWidget.onMouseDrag = (pivot, scale) => CommonAction_OnMouseDrag(editor, pivot, scale);
        }

        public override void OnRender()
        {
            base.OnRender();

            if (editor.selectedSegmentsCount > 0)
            {
                scaleWidget.position = editor.selectedSegmentsAveragePosition;
                scaleWidget.visible = true;
            }
            else
            {
                scaleWidget.visible = false;
            }
        }

        public static void CommonAction_OnBeginScaling(ShapeEditorWindow editor)
        {
            // store the initial position of all selected segments.
            foreach (var segment in editor.ForEachSelectedSegment())
                segment.scaleToolInitialPosition = segment.position;
        }

        public static void CommonAction_OnMouseDrag(ShapeEditorWindow editor, float2 pivot, float2 scale)
        {
            // scale the selected segments using their initial position.
            foreach (var segment in editor.ForEachSelectedSegment())
                segment.position = MathEx.ScaleAroundPivot(segment.scaleToolInitialPosition, pivot, scale);
        }
    }
}

#endif