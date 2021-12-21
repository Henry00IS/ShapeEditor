#if UNITY_EDITOR

using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    // add additional fields for this tool to segments.
    public partial class Segment
    {
        /// <summary>Editor variable used by <see cref="TranslateTool"/>.</summary>
        [System.NonSerialized]
        public float2 translateToolInitialPosition;
    }

    public class TranslateTool : BoxSelectTool
    {
        private TranslationWidget translationWidget = new TranslationWidget();

        public override void OnActivate()
        {
            base.OnActivate();

            editor.AddWidget(translationWidget);
            translationWidget.onBeginTranslating = () => CommonAction_OnBeginTranslating(editor);
            translationWidget.onMouseDrag = (screenDelta, gridDelta) => CommonAction_OnMouseDrag(editor, gridDelta);
        }

        public override void OnRender()
        {
            base.OnRender();

            if (editor.selectedSegmentsCount > 0)
            {
                translationWidget.position = editor.selectedSegmentsAveragePosition;
                translationWidget.visible = true;
            }
            else
            {
                translationWidget.visible = false;
            }
        }

        private static float2 CommonAction_DeltaAccumulator;

        public static void CommonAction_OnBeginTranslating(ShapeEditorWindow editor)
        {
            editor.RegisterUndo("Translate Selection");

            CommonAction_DeltaAccumulator = float2.zero;

            // store the initial position of all selected segments.
            foreach (var segment in editor.ForEachSelectedSegment())
                segment.translateToolInitialPosition = segment.position;
        }

        public static void CommonAction_OnMouseDrag(ShapeEditorWindow editor, float2 gridDelta)
        {
            CommonAction_DeltaAccumulator += gridDelta;
            float2 position = CommonAction_DeltaAccumulator;

            foreach (var segment in editor.ForEachSelectedSegment())
            {
                // snap to the grid when the control key is being held down.
                if (editor.isCtrlPressed)
                    position = position.Snap(editor.gridSnap);

                segment.position = segment.translateToolInitialPosition + position;
            }
        }
    }
}

#endif