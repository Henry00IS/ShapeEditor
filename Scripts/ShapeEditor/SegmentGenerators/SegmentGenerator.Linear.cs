#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    public partial class SegmentGenerator
    {
        // a segment generator that generates straight lines between two segments.

        private void Linear_DrawSegments(ShapeEditorWindow editor, Segment segment)
        {
            // we just draw manually here.
            var p1 = editor.GridPointToScreen(segment.position);
            var p2 = editor.GridPointToScreen(segment.next.position);
            GLUtilities.DrawLine(1.0f, p1.x, p1.y, p2.x, p2.y, segment.selected ? ShapeEditorWindow.segmentPivotOutlineColor : ShapeEditorWindow.segmentColor, segment.next.selected ? ShapeEditorWindow.segmentPivotOutlineColor : ShapeEditorWindow.segmentColor);
        }

        private IEnumerable<float2> Linear_ForEachSegmentPoint(ShapeEditorWindow editor, Segment segment)
        {
            yield return segment.position;
            yield return segment.next.position;
        }
    }
}

#endif