#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class SegmentModifier
    {
        // a segment modifier that generates a bezier curve between two segments.

        [SerializeField]
        public int bezierDetail = 3;

        [SerializeField]
        public Pivot bezierPivot1 = new Pivot();

        [SerializeField]
        public Pivot bezierPivot2 = new Pivot();

        public void Bezier_Constructor(ShapeEditorWindow editor, Segment segment, Segment next)
        {
            var distance = math.distance(segment.position, next.position);
            var normal = math.normalize(next.position - segment.position);
            bezierPivot1.position = segment.position + (normal * distance * 0.25f);
            bezierPivot2.position = next.position - (normal * distance * 0.25f);
        }

        public void Bezier_DrawPivots(ShapeEditorWindow editor, Segment segment, Segment next)
        {
            float2 p1 = editor.GridPointToScreen(bezierPivot1.position);
            float2 p2 = editor.GridPointToScreen(bezierPivot2.position);

            GLUtilities.DrawSolidRectangleWithOutline(p1.x - ShapeEditorWindow.halfPivotScale, p1.y - ShapeEditorWindow.halfPivotScale, ShapeEditorWindow.pivotScale, ShapeEditorWindow.pivotScale, bezierPivot1.selected ? ShapeEditorWindow.segmentPivotSelectedColor : Color.white, bezierPivot1.selected ? ShapeEditorWindow.segmentPivotOutlineColor : Color.black);
            GLUtilities.DrawSolidRectangleWithOutline(p2.x - ShapeEditorWindow.halfPivotScale, p2.y - ShapeEditorWindow.halfPivotScale, ShapeEditorWindow.pivotScale, ShapeEditorWindow.pivotScale, bezierPivot2.selected ? ShapeEditorWindow.segmentPivotSelectedColor : Color.white, bezierPivot2.selected ? ShapeEditorWindow.segmentPivotOutlineColor : Color.black);

            if (bezierPivot1.selected)
            {
                editor.selectedSegmentsCount++;
                editor.selectedSegmentsAveragePosition += p1;
            }

            if (bezierPivot2.selected)
            {
                editor.selectedSegmentsCount++;
                editor.selectedSegmentsAveragePosition += p2;
            }
        }

        public void Bezier_DrawSegments(ShapeEditorWindow editor, Segment segment, Segment next)
        {
            var p1 = editor.GridPointToScreen(segment.position);
            var p2 = editor.GridPointToScreen(bezierPivot1.position);
            var p3 = editor.GridPointToScreen(bezierPivot2.position);
            var p4 = editor.GridPointToScreen(next.position);
            GL.Color(ShapeEditorWindow.segmentColor);
            GLUtilities.DrawBezierLine(1.0f, p1, p2, p3, p4, 8);

            GL.Color(Color.blue);
            GLUtilities.DrawLine(1.0f, p1, p2);
            GLUtilities.DrawLine(1.0f, p4, p3);
        }

        public IEnumerable<ISelectable> Bezier_ForEachSelectableObject()
        {
            yield return bezierPivot1;
            yield return bezierPivot2;
        }
    }
}

#endif