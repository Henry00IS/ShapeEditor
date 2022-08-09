#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class SegmentGenerator
    {
        // a segment generator that generates well-known architectural arches between two segments.

        [SerializeField]
        public int archDetail = 8;

        [SerializeField]
        public float archGridSnapSize = 0f;

        [SerializeField]
        public ArchMode archMode = ArchMode.Round;

        public enum ArchMode
        {
            Round,
            Segmental,
            Horseshoe,
            PointedHorseshoe,
            ThreeCentered,
            PseudoThreeCentered,
            PseudoFourCentered,
            Triangular,
            RoundedHorseshoe,
            ThreePointed,
            PointedSegmental,
            Parabolic,
            Inflexed,
            RoundRampant,
            Rampant,
            FourCentered,
            Keyhole,
            ReverseOgee,
            OgeeThreeCentered,
            OgeeFourCentered,
            Oriental,
            RoundTrefoil,
            Shouldered,
            PointedTrefoil,
            Draped,
            DoubleDraped,
            Cinquefoil,
            PointedCinquefoil,
            Multifoil,
            RoundRampantMirrored,
            RampantMirrored,
        }

        private void Arch_DrawSegments()
        {
            DrawSegments(Arch_ForEachSegmentPoint());
        }

        private IEnumerable<float2> Arch_ForEachSegmentPoint()
        {
            var p1 = segment.position;
            var p4 = segment.next.position;
            var distance = math.distance(p1, p4);
            var normal = math.normalize(p4 - p1) * distance;
            var up = -(float2)Vector2.Perpendicular(normal);
            float2 p2;
            float2 p3;

            // these are all just approximations with bezier splines, based on architectural images
            // on the internet. if you know the right mathematical formulas (if they exist at all),
            // feel free to start a pull request. myself and surely many other users will be very
            // grateful to you!

            switch (archMode)
            {
                case ArchMode.Round:
                    {
                        p2 = p1 + up * 0.666666f;
                        p3 = p4 + up * 0.666666f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Segmental:
                    {
                        p2 = p1 + up * 0.333333f + normal * 0.125f;
                        p3 = p4 + up * 0.333333f - normal * 0.125f;

                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Horseshoe:
                    {
                        p2 = p1 + up * 0.833333f - normal * 0.08f;
                        p3 = p4 + up * 0.833333f + normal * 0.08f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.PointedHorseshoe:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.625f;
                        p2 = p1 + up * 0.455f - normal * 0.0455f;
                        p3 = midpoint - up * 0.03f - normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.455f + normal * 0.0455f;
                        p3 = midpoint - up * 0.03f + normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.ThreeCentered:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.3125f;
                        p2 = p1 + up * 0.3125f;
                        p3 = midpoint - normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.3125f;
                        p3 = midpoint + normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.PseudoThreeCentered:
                    {
                        var left = p1 + up * 0.1875f + normal * 0.1875f;
                        var right = p4 + up * 0.1875f - normal * 0.1875f;

                        p2 = p1 + up * 0.1f;
                        p3 = left - normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left))
                            yield return point;

                        yield return left.Snap(archGridSnapSize);
                        yield return right.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.1f;
                        p3 = right + normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.PseudoFourCentered:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.25f;
                        var left = p1 + up * 0.15f + normal * 0.1225f;
                        var right = p4 + up * 0.15f - normal * 0.1225f;

                        p2 = p1 + up * 0.08f + normal * 0.005f;
                        p3 = left - normal * 0.07f - up * 0.025f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left))
                            yield return point;

                        yield return left.Snap(archGridSnapSize);
                        yield return midpoint.Snap(archGridSnapSize);
                        yield return right.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.08f - normal * 0.005f;
                        p3 = right + normal * 0.07f - up * 0.025f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Triangular:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.5f;
                        yield return midpoint.Snap(archGridSnapSize);
                    }
                    break;

                case ArchMode.RoundedHorseshoe:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 1.1875f;
                        p2 = p1 + up * 0.52f - normal * 0.4405f;
                        p3 = midpoint - normal * 0.5375f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.52f + normal * 0.4405f;
                        p3 = midpoint + normal * 0.5375f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.ThreePointed:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.75f;
                        p2 = p1 + up * 0.2f;
                        p3 = p1 + up * 0.6f + normal * 0.15f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.2f;
                        p3 = p4 + up * 0.6f - normal * 0.15f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.PointedSegmental:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.3125f;
                        p2 = p1 + up * 0.1f + normal * 0.1f;
                        p3 = p1 + up * 0.25f + normal * 0.35f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.1f - normal * 0.1f;
                        p3 = p4 + up * 0.25f - normal * 0.35f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Parabolic:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 1.375f;
                        p2 = p1 + up * 0.5f;
                        p3 = p1 + up * 1.35f + normal * 0.2f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.5f;
                        p3 = p4 + up * 1.35f - normal * 0.2f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Inflexed:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.375f;
                        p2 = p1 + normal * 0.25f;
                        p3 = p1 + up * 0.15f + normal * 0.45f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint.Snap(archGridSnapSize);

                        p2 = p4 - normal * 0.25f;
                        p3 = p4 + up * 0.15f - normal * 0.45f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.RoundRampant:
                    {
                        var right = p4 + up * 0.31f;
                        p2 = p1 + up * 0.75f + normal * 0.05f;
                        p3 = p4 + up * 0.8f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, right))
                            yield return point;

                        yield return right.Snap(archGridSnapSize);
                    }
                    break;

                case ArchMode.Rampant:
                    {
                        var midpoint = ((p1 + p4) / 2f) + normal * 0.125f + up * 0.9375f;
                        var right = p4 + up * 0.45f;

                        p2 = p1 + up * 0.5f;
                        p3 = p1 + up * 0.85f + normal * 0.35f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.65f;
                        p3 = p4 + up * 0.85f - normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, right))
                            yield return point;

                        yield return right.Snap(archGridSnapSize);
                    }
                    break;

                case ArchMode.FourCentered:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.375f;
                        p2 = p1 + up * 0.25f;
                        p3 = p1 + up * 0.375f + normal * 0.35f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.25f;
                        p3 = p4 + up * 0.375f - normal * 0.35f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Keyhole:
                    {
                        var right = p1 + up * 0.25f + normal * 0.125f;
                        var left = p4 + up * 0.25f - normal * 0.125f;
                        p2 = p1 + up * 0.055f;
                        p3 = p1 + up * 0.167f + normal * 0.055f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, right))
                            yield return point;

                        yield return right.Snap(archGridSnapSize);

                        var midpoint = ((p1 + p4) / 2f) + up * 1.125f;
                        p2 = p1 + up * 0.555f - normal * 0.222f;
                        p3 = midpoint - normal * 0.5f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.555f + normal * 0.222f;
                        p3 = midpoint + normal * 0.5f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, left))
                            yield return point;

                        yield return left.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.055f;
                        p3 = p4 + up * 0.167f - normal * 0.055f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.ReverseOgee:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.3125f;
                        p2 = p1 + normal * 0.25f;
                        p3 = p1 + up * 0.3f + normal * 0.125f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint.Snap(archGridSnapSize);

                        p2 = p4 - normal * 0.25f;
                        p3 = p4 + up * 0.3f - normal * 0.125f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.OgeeThreeCentered:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.75f;
                        p2 = p1 + up * 0.45f + normal * 0.05f;
                        p3 = p1 + up * 0.3f + normal * 0.3f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.45f - normal * 0.05f;
                        p3 = p4 + up * 0.3f - normal * 0.3f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.OgeeFourCentered:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.4375f;
                        p2 = p1 + up * 0.3f + normal * 0.05f;
                        p3 = p1 + up * 0.25f + normal * 0.3f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.3f - normal * 0.05f;
                        p3 = p4 + up * 0.25f - normal * 0.3f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Oriental:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.75f;
                        p2 = p1 + up * 0.31f - normal * 0.25f;
                        p3 = p1 + up * 0.437f + normal * 0.0625f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.31f + normal * 0.25f;
                        p3 = p4 + up * 0.437f - normal * 0.0625f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.RoundTrefoil:
                    {
                        var left = p1 + up * 0.25f + normal * 0.25f;
                        var right = p4 + up * 0.25f - normal * 0.25f;
                        p2 = p1 + up * 0.13f;
                        p3 = p1 + up * 0.25f + normal * 0.132f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left))
                            yield return point;

                        yield return left.Snap(archGridSnapSize);

                        p2 = left + up * 0.333333f - normal * 0.0265f;
                        p3 = right + up * 0.333333f + normal * 0.0265f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left, p2, p3, right))
                            yield return point;

                        yield return right.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.13f;
                        p3 = p4 + up * 0.25f - normal * 0.132f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Shouldered:
                    {
                        var left = p1 + up * 0.25f + normal * 0.25f;
                        var right = p4 + up * 0.25f - normal * 0.25f;
                        p2 = p1 + up * 0.13f;
                        p3 = p1 + up * 0.25f + normal * 0.132f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left))
                            yield return point;

                        yield return left.Snap(archGridSnapSize);

                        var shoulderLeft = p1 + up * 0.375f + normal * 0.25f;
                        var shoulderRight = p4 + up * 0.375f - normal * 0.25f;

                        yield return shoulderLeft.Snap(archGridSnapSize);
                        yield return shoulderRight.Snap(archGridSnapSize);

                        yield return right.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.13f;
                        p3 = p4 + up * 0.25f - normal * 0.132f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.PointedTrefoil:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.75f;
                        var left = p1 + up * 0.3125f + normal * 0.3125f;
                        var right = p4 + up * 0.3125f - normal * 0.3125f;
                        p2 = p1 + up * 0.2f;
                        p3 = p1 + up * 0.3125f + normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left))
                            yield return point;

                        yield return left.Snap(archGridSnapSize);

                        p2 = left + up * 0.15f - normal * 0.1f;
                        p3 = left + up * 0.3125f + normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint.Snap(archGridSnapSize);

                        p2 = right + up * 0.15f + normal * 0.1f;
                        p3 = right + up * 0.3125f - normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, right))
                            yield return point;

                        yield return right.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.2f;
                        p3 = p4 + up * 0.3125f - normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Draped:
                    {
                        var left = p1 + up * 0.3125f + normal * 0.3125f;
                        var right = p4 + up * 0.3125f - normal * 0.3125f;
                        p2 = p1 + normal * 0.2f;
                        p3 = p1 + up * 0.1f + normal * 0.3125f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left))
                            yield return point;

                        yield return left.Snap(archGridSnapSize);
                        yield return right.Snap(archGridSnapSize);

                        p2 = p4 - normal * 0.2f;
                        p3 = p4 + up * 0.1f - normal * 0.3125f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.DoubleDraped:
                    {
                        var left1 = p1 + up * 0.1875f + normal * 0.1875f;
                        var left2 = p1 + up * 0.375f + normal * 0.375f;
                        var right1 = p4 + up * 0.1875f - normal * 0.1875f;
                        var right2 = p4 + up * 0.375f - normal * 0.375f;

                        p2 = p1 + normal * 0.15f;
                        p3 = p1 + up * 0.05f + normal * 0.1875f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left1))
                            yield return point;

                        yield return left1.Snap(archGridSnapSize);

                        p2 = left1 + normal * 0.125f;
                        p3 = left1 + up * 0.05f + normal * 0.175f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left1, p2, p3, left2))
                            yield return point;

                        yield return left2.Snap(archGridSnapSize);
                        yield return right2.Snap(archGridSnapSize);

                        p2 = right1 - normal * 0.125f;
                        p3 = right1 + up * 0.05f - normal * 0.175f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right2, p3, p2, right1))
                            yield return point;

                        yield return right1.Snap(archGridSnapSize);

                        p2 = p4 - normal * 0.15f;
                        p3 = p4 + up * 0.05f - normal * 0.1875f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right1, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Cinquefoil:
                    {
                        var left1 = p1 + up * 0.15625f + normal * 0.125f;
                        var left2 = p1 + up * 0.375f + normal * 0.34375f;
                        var right1 = p4 + up * 0.15625f - normal * 0.125f;
                        var right2 = p4 + up * 0.375f - normal * 0.34375f;

                        p2 = p1 + up * 0.075f;
                        p3 = p1 + up * 0.15f + normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left1))
                            yield return point;

                        yield return left1.Snap(archGridSnapSize);

                        p2 = left1 + up * 0.15f - normal * 0.075f;
                        p3 = left1 + up * 0.3f + normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left1, p2, p3, left2))
                            yield return point;

                        yield return left2.Snap(archGridSnapSize);

                        p2 = left2 + up * 0.166666f + normal * 0.075f;
                        p3 = right2 + up * 0.166666f - normal * 0.075f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left2, p2, p3, right2))
                            yield return point;

                        yield return right2.Snap(archGridSnapSize);

                        p2 = right1 + up * 0.15f + normal * 0.075f;
                        p3 = right1 + up * 0.3f - normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right2, p3, p2, right1))
                            yield return point;

                        yield return right1.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.075f;
                        p3 = p4 + up * 0.15f - normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right1, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.PointedCinquefoil:
                    {
                        var left1 = p1 + up * 0.125f + normal * 0.125f;
                        var left2 = p1 + up * 0.3125f + normal * 0.3125f;
                        var midpoint = ((p1 + p4) / 2f) + up * 0.5625f;
                        var right1 = p4 + up * 0.125f - normal * 0.125f;
                        var right2 = p4 + up * 0.3125f - normal * 0.3125f;

                        p2 = p1 + up * 0.05f;
                        p3 = p1 + up * 0.125f + normal * 0.075f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left1))
                            yield return point;

                        yield return left1.Snap(archGridSnapSize);

                        p2 = left1 + up * 0.125f - normal * 0.075f;
                        p3 = left1 + up * 0.275f + normal * 0.0125f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left1, p2, p3, left2))
                            yield return point;

                        yield return left2.Snap(archGridSnapSize);

                        p2 = left2 + up * 0.1f;
                        p3 = right2 + up * 0.225f - normal * 0.25f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left2, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint.Snap(archGridSnapSize);

                        p2 = right2 + up * 0.1f;
                        p3 = left2 + up * 0.225f + normal * 0.25f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, right2))
                            yield return point;

                        yield return right2.Snap(archGridSnapSize);

                        p2 = right1 + up * 0.125f + normal * 0.075f;
                        p3 = right1 + up * 0.275f - normal * 0.0125f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right2, p3, p2, right1))
                            yield return point;

                        yield return right1.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.05f;
                        p3 = p4 + up * 0.125f - normal * 0.075f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right1, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Multifoil:
                    {
                        var left1 = p1 + up * 0.125f + normal * 0.09375f;
                        var left2 = p1 + up * 0.34375f + normal * 0.1875f;
                        var left3 = p1 + up * 0.53125f + normal * 0.375f;
                        var right1 = p4 + up * 0.125f - normal * 0.09375f;
                        var right2 = p4 + up * 0.34375f - normal * 0.1875f;
                        var right3 = p4 + up * 0.53125f - normal * 0.375f;

                        p2 = p1 + up * 0.05f;
                        p3 = p1 + up * 0.125f + normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left1))
                            yield return point;

                        yield return left1.Snap(archGridSnapSize);

                        p2 = left1 + up * 0.075f - normal * 0.075f;
                        p3 = left1 + up * 0.225f - normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left1, p2, p3, left2))
                            yield return point;

                        yield return left2.Snap(archGridSnapSize);

                        p2 = left2 + up * 0.125f - normal * 0.05f;
                        p3 = left2 + up * 0.225f + normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left2, p2, p3, left3))
                            yield return point;

                        yield return left3.Snap(archGridSnapSize);

                        p2 = left3 + up * 0.166666f + normal * 0.025f;
                        p3 = right3 + up * 0.166666f - normal * 0.025f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left3, p2, p3, right3))
                            yield return point;

                        yield return right3.Snap(archGridSnapSize);

                        p2 = right2 + up * 0.125f + normal * 0.05f;
                        p3 = right2 + up * 0.225f - normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right3, p3, p2, right2))
                            yield return point;

                        yield return right2.Snap(archGridSnapSize);

                        p2 = right1 + up * 0.075f + normal * 0.075f;
                        p3 = right1 + up * 0.225f + normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right2, p3, p2, right1))
                            yield return point;

                        yield return right1.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.05f;
                        p3 = p4 + up * 0.125f - normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right1, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.RoundRampantMirrored:
                    {
                        var left = p1 + up * 0.31f;

                        yield return left.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.75f - normal * 0.05f;
                        p3 = p1 + up * 0.8f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.RampantMirrored:
                    {
                        var midpoint = ((p1 + p4) / 2f) - normal * 0.125f + up * 0.9375f;
                        var left = p1 + up * 0.45f;

                        yield return left.Snap(archGridSnapSize);

                        p2 = p1 + up * 0.65f;
                        p3 = p1 + up * 0.85f + normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint.Snap(archGridSnapSize);

                        p2 = p4 + up * 0.5f;
                        p3 = p4 + up * 0.85f - normal * 0.35f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;
            }
        }

        private IEnumerable<float2> Arch_ForEachBezierSegmentPoint(float2 p1, float2 p2, float2 p3, float2 p4)
        {
            var last = new float2(float.NegativeInfinity);
            for (int i = 1; i <= archDetail - 1; i++)
            {
                var point = MathEx.BezierGetPoint(p1, p2, p3, p4, i / (float)archDetail).Snap(archGridSnapSize);
                if (!point.Equals(last))
                    yield return last = point;
            }
        }
    }
}

#endif