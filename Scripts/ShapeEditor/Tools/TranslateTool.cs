#if UNITY_EDITOR

using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    public class TranslateTool : Tool
    {
        public override void OnRender()
        {
            GLUtilities.DrawGui(() =>
            {
                if (editor.selectedSegmentsCount > 0)
                {
                    GLUtilities.DrawTranslationGizmo(editor.selectedSegmentsAveragePosition);
                }
            });
        }

        public override void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (editor.isLeftMousePressed)
            {
                foreach (var segment in editor.ForEachSelectedSegment())
                {
                    segment.position += gridDelta;
                }
            }
        }
    }
}

#endif