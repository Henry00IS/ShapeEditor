#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class TranslateTool : Tool
    {
        public override void OnRender()
        {
            GLUtilities.DrawGui(() =>
            {
                GLUtilities.DrawCircle(1.0f, editor.mousePosition, 8.0f, Color.blue);
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