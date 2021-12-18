#if UNITY_EDITOR

using Unity.Mathematics;
using TranslationGizmoState = AeternumGames.ShapeEditor.GLUtilities.TranslationGizmoState;

namespace AeternumGames.ShapeEditor
{
    public class TranslateTool : Tool
    {
        private TranslationGizmoState activeTranslationGizmoState;
        private TranslationGizmoState currentTranslationGizmoState;

        public override void OnRender()
        {
            GLUtilities.DrawGui(() =>
            {
                if (editor.selectedSegmentsCount > 0)
                {
                    GLUtilities.DrawTranslationGizmo(editor.selectedSegmentsAveragePosition, editor.mousePosition, ref currentTranslationGizmoState);
                    currentTranslationGizmoState.UpdateMouseCursor(editor);
                }
            });
        }

        public override void OnMouseDown(int button)
        {
            if (button == 0)
            {
                activeTranslationGizmoState = currentTranslationGizmoState;
            }
        }

        public override void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (button == 0)
            {
                foreach (var segment in editor.ForEachSelectedSegment())
                    segment.position += activeTranslationGizmoState.ModifyDeltaMovement(gridDelta);
            }
        }
    }
}

#endif