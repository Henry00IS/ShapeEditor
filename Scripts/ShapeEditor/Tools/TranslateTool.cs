#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEditor;
using TranslationGizmoState = AeternumGames.ShapeEditor.GLUtilities.TranslationGizmoState;

namespace AeternumGames.ShapeEditor
{
    public class TranslateTool : Tool
    {
        private TranslationGizmoState initialTranslationGizmoState;
        private TranslationGizmoState translationGizmoState;

        public override void OnRender()
        {
            GLUtilities.DrawGui(() =>
            {
                if (editor.selectedSegmentsCount > 0)
                {
                    GLUtilities.DrawTranslationGizmo(editor.selectedSegmentsAveragePosition, editor.mousePosition, ref translationGizmoState);

                    if (translationGizmoState.isMouseOverInnerCircle)
                    {
                        editor.SetMouseCursor(MouseCursor.MoveArrow);
                    }
                    else if (translationGizmoState.isMouseOverY)
                    {
                        editor.SetMouseCursor(MouseCursor.ResizeVertical);
                    }
                    else if (translationGizmoState.isMouseOverX)
                    {
                        editor.SetMouseCursor(MouseCursor.ResizeHorizontal);
                    }
                }
            });
        }

        public override void OnMouseDown(int button)
        {
            if (button == 0)
            {
                initialTranslationGizmoState = translationGizmoState;
            }
        }

        public override void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (button == 0)
            {
                if (initialTranslationGizmoState.isMouseOverInnerCircle)
                {
                    foreach (var segment in editor.ForEachSelectedSegment())
                        segment.position += gridDelta;
                }
                else if (initialTranslationGizmoState.isMouseOverY)
                {
                    foreach (var segment in editor.ForEachSelectedSegment())
                        segment.position += new float2(0f, gridDelta.y);
                }
                else if (initialTranslationGizmoState.isMouseOverX)
                {
                    foreach (var segment in editor.ForEachSelectedSegment())
                        segment.position += new float2(gridDelta.x, 0f);
                }
            }
        }
    }
}

#endif