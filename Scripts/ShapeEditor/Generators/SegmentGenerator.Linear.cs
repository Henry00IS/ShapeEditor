#if UNITY_EDITOR

namespace AeternumGames.ShapeEditor
{
    public partial class SegmentGenerator
    {
        // a segment generator that generates straight lines between two segments.

        private void Linear_DrawSegments()
        {
            // we just draw manually here.
            DrawGridLine(1.0f, segment.position, segment.next.position, segment.selected ? ShapeEditorWindow.segmentPivotOutlineColor : segment.shape.segmentColor, segment.next.selected ? ShapeEditorWindow.segmentPivotOutlineColor : segment.shape.segmentColor);
        }
    }
}

#endif