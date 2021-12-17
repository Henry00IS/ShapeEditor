#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class BoxSelectTool : Tool
    {
        public override void OnRender()
        {
            GLUtilities.DrawGui(() =>
            {
                GLUtilities.DrawCircle(1.0f, editor.mousePosition, 8.0f, Color.yellow);
            });
        }

        public override void OnMouseUp(int button)
        {
            // unless the shift key is held down we clear the selection.
            if (!editor.isShiftPressed)
                editor.project.ClearSelection();

            // find the closest segment to the click position.
            var segment = editor.FindSegmentAtScreenPosition(editor.mousePosition, 60.0f);
            if (segment != null)
                segment.selected = !segment.selected;
        }

        public override void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
        }
    }
}

#endif