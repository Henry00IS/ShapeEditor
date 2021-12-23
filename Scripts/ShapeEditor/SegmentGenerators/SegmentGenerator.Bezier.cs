#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class SegmentGenerator
    {
        // a segment generator that generates a bezier curve between two segments.

        [SerializeField]
        public int bezierDetail = 8;

        [SerializeField]
        public Pivot bezierPivot1 = new Pivot();

        [SerializeField]
        public Pivot bezierPivot2 = new Pivot();

        private void Bezier_Constructor(ShapeEditorWindow editor, Segment segment)
        {
            var distance = math.distance(segment.position, segment.next.position);
            var normal = math.normalize(segment.next.position - segment.position);
            bezierPivot1.position = segment.position + (normal * distance * 0.25f);
            bezierPivot2.position = segment.next.position - (normal * distance * 0.25f);
        }

        private void Bezier_DrawPivots(ShapeEditorWindow editor, Segment segment)
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

        private void Bezier_DrawSegments(ShapeEditorWindow editor, Segment segment)
        {
            var p1 = editor.GridPointToScreen(segment.position);
            var p2 = editor.GridPointToScreen(bezierPivot1.position);
            var p3 = editor.GridPointToScreen(bezierPivot2.position);
            var p4 = editor.GridPointToScreen(segment.next.position);

            // draw manually because we have a function for it.
            GL.Color(segment.selected ? ShapeEditorWindow.segmentPivotOutlineColor : ShapeEditorWindow.segmentColor);
            GLUtilities.DrawBezierLine(1.0f, p1, p2, p3, p4, bezierDetail);

            GL.Color(Color.blue);
            GLUtilities.DrawLine(1.0f, p1, p2);
            GLUtilities.DrawLine(1.0f, p4, p3);
        }

        private IEnumerable<ISelectable> Bezier_ForEachSelectableObject()
        {
            yield return bezierPivot1;
            yield return bezierPivot2;
        }

        private IEnumerable<float2> Bezier_ForEachSegmentPoint(ShapeEditorWindow editor, Segment segment)
        {
            var p1 = segment.position;
            var p2 = bezierPivot1.position;
            var p3 = bezierPivot2.position;
            var p4 = segment.next.position;

            var lineStart = MathEx.BezierGetPoint(p1, p2, p3, p4, 0f);
            for (int i = 1; i <= bezierDetail; i++)
            {
                var lineEnd = MathEx.BezierGetPoint(p1, p2, p3, p4, i / (float)bezierDetail);
                yield return lineStart;
                yield return lineEnd;
                lineStart = lineEnd;
            }
        }
    }
}

#endif