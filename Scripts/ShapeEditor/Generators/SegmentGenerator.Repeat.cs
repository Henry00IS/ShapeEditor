#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class SegmentGenerator
    {
        // a segment generator that repeats the previous x segments y times.

        [SerializeField]
        public int repeatSegments = 2;

        [SerializeField]
        public int repeatTimes = 4;

        private void Repeat_DrawSegments()
        {
            DrawSegments(Repeat_ForEachSegmentPoint());

            // draw the direction arrow:
            var a = segment.previous.position;
            var b = segment.position;
            var cross = (float2)Vector2.Perpendicular(a - b).normalized;
            var normal = math.normalize(b - a);
            b -= normal * 0.015625f;
            a = b + (cross * 0.03125f) - (normal * 0.09375f);
            b += cross * 0.03125f;

            a = editor.GridPointToScreen(a);
            b = editor.GridPointToScreen(b);

            if (math.length(a - b) > 11f)
            {
                GL.Color(ShapeEditorWindow.segmentPivotOutlineColor);
                GLUtilities.DrawLineArrow(1.0f, a, b, 10f);
            }
        }

        private IEnumerable<float2> Repeat_ForEachSegmentPoint()
        {
            if (repeatSegments > 0)
            {
                // iterate backwards and find the individual offsets from 0,0.
                var offsets = new float2[repeatSegments];
                var current = segment;
                for (int i = 0; i < repeatSegments; i++)
                {
                    var previous = current.previous;
                    offsets[i] = current.position - previous.position;
                    current = previous;
                }

                // starting from the current segment position, repeat x number of times:
                var offset = segment.position;
                for (int j = 0; j < repeatTimes; j++)
                {
                    // iterate over the offsets in reverse and add them as new segments.
                    for (int i = repeatSegments; i-- > 0;)
                    {
                        offset += offsets[i];

                        // do NOT add the very last segment as it needs to connect to the next segment.
                        if (!(j == repeatTimes - 1 && i == 0))
                        {
                            yield return offset;
                        }
                    }
                }
            }
        }
    }
}

#endif