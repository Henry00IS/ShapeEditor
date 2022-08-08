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
                        p2 = p1 + up * 0.3f + normal * 0.125f;
                        p3 = p4 + up * 0.3f - normal * 0.125f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Horseshoe:
                    {
                        p2 = p1 + up * 0.8f - normal * 0.08f;
                        p3 = p4 + up * 0.8f + normal * 0.08f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.PointedHorseshoe:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.6f;
                        p2 = p1 + up * 0.455f - normal * 0.0455f;
                        p3 = midpoint - up * 0.03f - normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint;

                        p2 = p4 + up * 0.455f + normal * 0.0455f;
                        p3 = midpoint - up * 0.03f + normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.ThreeCentered:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.3f;
                        p2 = p1 + up * 0.3f;
                        p3 = midpoint - normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint;

                        p2 = p4 + up * 0.3f;
                        p3 = midpoint + normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.PseudoThreeCentered:
                    {
                        var left = p1 + up * 0.165f + normal * 0.165f;
                        var right = p4 + up * 0.165f - normal * 0.165f;

                        p2 = p1 + up * 0.1f;
                        p3 = left - normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left))
                            yield return point;

                        yield return left;
                        yield return right;

                        p2 = p4 + up * 0.1f;
                        p3 = right + normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.PseudoFourCentered:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.235f;
                        var left = p1 + up * 0.15f + normal * 0.1225f;
                        var right = p4 + up * 0.15f - normal * 0.1225f;

                        p2 = p1 + up * 0.08f + normal * 0.005f;
                        p3 = left - normal * 0.07f - up * 0.025f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left))
                            yield return point;

                        yield return left;
                        yield return midpoint;
                        yield return right;

                        p2 = p4 + up * 0.08f - normal * 0.005f;
                        p3 = right + normal * 0.07f - up * 0.025f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Triangular:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.5f;
                        yield return midpoint;
                    }
                    break;

                case ArchMode.RoundedHorseshoe:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 1.2f;
                        p2 = p1 + up * 0.52f - normal * 0.5f;
                        p3 = midpoint - normal * 0.5375f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint;

                        p2 = p4 + up * 0.52f + normal * 0.5f;
                        p3 = midpoint + normal * 0.5375f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.ThreePointed:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.775f;
                        p2 = p1 + up * 0.2f;
                        p3 = p1 + up * 0.6f + normal * 0.15f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint;

                        p2 = p4 + up * 0.2f;
                        p3 = p4 + up * 0.6f - normal * 0.15f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.PointedSegmental:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.31f;
                        p2 = p1 + up * 0.1f + normal * 0.1f;
                        p3 = p1 + up * 0.25f + normal * 0.35f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint;

                        p2 = p4 + up * 0.1f - normal * 0.1f;
                        p3 = p4 + up * 0.25f - normal * 0.35f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Parabolic:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 1.35f;
                        p2 = p1 + up * 0.5f;
                        p3 = p1 + up * 1.35f + normal * 0.2f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint;

                        p2 = p4 + up * 0.5f;
                        p3 = p4 + up * 1.35f - normal * 0.2f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Inflexed:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.4f;
                        p2 = p1 + normal * 0.25f;
                        p3 = p1 + up * 0.15f + normal * 0.45f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint;

                        p2 = p4 - normal * 0.25f;
                        p3 = p4 + up * 0.15f - normal * 0.45f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.RoundRampant:
                    {
                        var right = p4 + up * 0.3f;
                        p2 = p1 + up * 0.75f + normal * 0.05f;
                        p3 = p4 + up * 0.8f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, right))
                            yield return point;

                        yield return right;
                    }
                    break;

                case ArchMode.Rampant:
                    {
                        var midpoint = ((p1 + p4) / 2f) + normal * 0.15f + up * 0.95f;
                        var right = p4 + up * 0.45f;

                        p2 = p1 + up * 0.5f;
                        p3 = p1 + up * 0.85f + normal * 0.35f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint;

                        p2 = p4 + up * 0.65f;
                        p3 = p4 + up * 0.85f - normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, right))
                            yield return point;

                        yield return right;
                    }
                    break;

                case ArchMode.FourCentered:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.4f;
                        p2 = p1 + up * 0.25f;
                        p3 = p1 + up * 0.4f + normal * 0.35f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint;

                        p2 = p4 + up * 0.25f;
                        p3 = p4 + up * 0.4f - normal * 0.35f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Keyhole:
                    {
                        var right = p1 + up * 0.221f + normal * 0.11f;
                        var left = p4 + up * 0.221f - normal * 0.11f;
                        p2 = p1 + up * 0.055f;
                        p3 = p1 + up * 0.167f + normal * 0.055f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, right))
                            yield return point;

                        yield return right;

                        var midpoint = ((p1 + p4) / 2f) + up * 1.135f;
                        p2 = p1 + up * 0.555f - normal * 0.222f;
                        p3 = midpoint - normal * 0.5f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint;

                        p2 = p4 + up * 0.555f + normal * 0.222f;
                        p3 = midpoint + normal * 0.5f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, left))
                            yield return point;

                        yield return left;

                        p2 = p4 + up * 0.055f;
                        p3 = p4 + up * 0.167f - normal * 0.055f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.ReverseOgee:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.3f;
                        p2 = p1 + normal * 0.25f;
                        p3 = p1 + up * 0.3f + normal * 0.125f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint;

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

                        yield return midpoint;

                        p2 = p4 + up * 0.45f - normal * 0.05f;
                        p3 = p4 + up * 0.3f - normal * 0.3f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.OgeeFourCentered:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.45f;
                        p2 = p1 + up * 0.3f + normal * 0.05f;
                        p3 = p1 + up * 0.25f + normal * 0.3f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint;

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

                        yield return midpoint;

                        p2 = p4 + up * 0.31f + normal * 0.25f;
                        p3 = p4 + up * 0.437f - normal * 0.0625f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.RoundTrefoil:
                    {
                        var left = p1 + up * 0.237f + normal * 0.26325f;
                        var right = p4 + up * 0.237f - normal * 0.26325f;
                        p2 = p1 + up * 0.13f;
                        p3 = p1 + up * 0.237f + normal * 0.132f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left))
                            yield return point;

                        yield return left;

                        p2 = left + up * 0.342f - normal * 0.0265f;
                        p3 = right + up * 0.342f + normal * 0.0265f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left, p2, p3, right))
                            yield return point;

                        yield return right;

                        p2 = p4 + up * 0.13f;
                        p3 = p4 + up * 0.237f - normal * 0.132f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Shouldered:
                    {
                        var left = p1 + up * 0.237f + normal * 0.26325f;
                        var right = p4 + up * 0.237f - normal * 0.26325f;
                        p2 = p1 + up * 0.13f;
                        p3 = p1 + up * 0.237f + normal * 0.132f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left))
                            yield return point;

                        yield return left;

                        var shoulderLeft = p1 + up * 0.3685f + normal * 0.26325f;
                        var shoulderRight = p4 + up * 0.3685f - normal * 0.26325f;

                        yield return shoulderLeft;
                        yield return shoulderRight;

                        yield return right;

                        p2 = p4 + up * 0.13f;
                        p3 = p4 + up * 0.237f - normal * 0.132f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.PointedTrefoil:
                    {
                        var midpoint = ((p1 + p4) / 2f) + up * 0.75f;
                        var left = p1 + up * 0.35f + normal * 0.3f;
                        var right = p4 + up * 0.35f - normal * 0.3f;
                        p2 = p1 + up * 0.2f;
                        p3 = p1 + up * 0.35f + normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left))
                            yield return point;

                        yield return left;

                        p2 = left + up * 0.15f - normal * 0.1f;
                        p3 = left + up * 0.35f + normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint;

                        p2 = right + up * 0.15f + normal * 0.1f;
                        p3 = right + up * 0.35f - normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, right))
                            yield return point;

                        yield return right;

                        p2 = p4 + up * 0.2f;
                        p3 = p4 + up * 0.35f - normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Draped:
                    {
                        var left = p1 + up * 0.3f + normal * 0.325f;
                        var right = p4 + up * 0.3f - normal * 0.325f;
                        p2 = p1 + normal * 0.2f;
                        p3 = p1 + up * 0.1f + normal * 0.325f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left))
                            yield return point;

                        yield return left;
                        yield return right;

                        p2 = p4 - normal * 0.2f;
                        p3 = p4 + up * 0.1f - normal * 0.325f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.DoubleDraped:
                    {
                        var left1 = p1 + up * 0.2f + normal * 0.2f;
                        var left2 = p1 + up * 0.375f + normal * 0.375f;
                        var right1 = p4 + up * 0.2f - normal * 0.2f;
                        var right2 = p4 + up * 0.375f - normal * 0.375f;

                        p2 = p1 + normal * 0.15f;
                        p3 = p1 + up * 0.05f + normal * 0.2f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left1))
                            yield return point;

                        yield return left1;

                        p2 = left1 + normal * 0.125f;
                        p3 = left1 + up * 0.05f + normal * 0.175f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left1, p2, p3, left2))
                            yield return point;

                        yield return left2;
                        yield return right2;

                        p2 = right1 - normal * 0.125f;
                        p3 = right1 + up * 0.05f - normal * 0.175f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right2, p3, p2, right1))
                            yield return point;

                        yield return right1;

                        p2 = p4 - normal * 0.15f;
                        p3 = p4 + up * 0.05f - normal * 0.2f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right1, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Cinquefoil:
                    {
                        var left1 = p1 + up * 0.15f + normal * 0.125f;
                        var left2 = p1 + up * 0.375f + normal * 0.35f;
                        var right1 = p4 + up * 0.15f - normal * 0.125f;
                        var right2 = p4 + up * 0.375f - normal * 0.35f;

                        p2 = p1 + up * 0.075f;
                        p3 = p1 + up * 0.15f + normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left1))
                            yield return point;

                        yield return left1;

                        p2 = left1 + up * 0.15f - normal * 0.075f;
                        p3 = left1 + up * 0.3f + normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left1, p2, p3, left2))
                            yield return point;

                        yield return left2;

                        p2 = left2 + up * 0.15f + normal * 0.075f;
                        p3 = right2 + up * 0.15f - normal * 0.075f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left2, p2, p3, right2))
                            yield return point;

                        yield return right2;

                        p2 = right1 + up * 0.15f + normal * 0.075f;
                        p3 = right1 + up * 0.3f - normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right2, p3, p2, right1))
                            yield return point;

                        yield return right1;

                        p2 = p4 + up * 0.075f;
                        p3 = p4 + up * 0.15f - normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right1, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.PointedCinquefoil:
                    {
                        var left1 = p1 + up * 0.125f + normal * 0.125f;
                        var left2 = p1 + up * 0.325f + normal * 0.325f;
                        var midpoint = ((p1 + p4) / 2f) + up * 0.575f;
                        var right1 = p4 + up * 0.125f - normal * 0.125f;
                        var right2 = p4 + up * 0.325f - normal * 0.325f;

                        p2 = p1 + up * 0.05f;
                        p3 = p1 + up * 0.125f + normal * 0.075f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left1))
                            yield return point;

                        yield return left1;

                        p2 = left1 + up * 0.125f - normal * 0.05f;
                        p3 = left1 + up * 0.275f + normal * 0.025f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left1, p2, p3, left2))
                            yield return point;

                        yield return left2;

                        p2 = left2 + up * 0.1f;
                        p3 = right2 + up * 0.225f - normal * 0.25f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left2, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint;

                        p2 = right2 + up * 0.1f;
                        p3 = left2 + up * 0.225f + normal * 0.25f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(midpoint, p3, p2, right2))
                            yield return point;

                        yield return right2;

                        p2 = right1 + up * 0.125f + normal * 0.05f;
                        p3 = right1 + up * 0.275f - normal * 0.025f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right2, p3, p2, right1))
                            yield return point;

                        yield return right1;

                        p2 = p4 + up * 0.05f;
                        p3 = p4 + up * 0.125f - normal * 0.075f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right1, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.Multifoil:
                    {
                        var left1 = p1 + up * 0.125f + normal * 0.1f;
                        var left2 = p1 + up * 0.35f + normal * 0.2f;
                        var left3 = p1 + up * 0.525f + normal * 0.375f;
                        var right1 = p4 + up * 0.125f - normal * 0.1f;
                        var right2 = p4 + up * 0.35f - normal * 0.2f;
                        var right3 = p4 + up * 0.525f - normal * 0.375f;

                        p2 = p1 + up * 0.05f;
                        p3 = p1 + up * 0.125f + normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(p1, p2, p3, left1))
                            yield return point;

                        yield return left1;

                        p2 = left1 + up * 0.075f - normal * 0.075f;
                        p3 = left1 + up * 0.225f - normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left1, p2, p3, left2))
                            yield return point;

                        yield return left2;

                        p2 = left2 + up * 0.125f - normal * 0.05f;
                        p3 = left2 + up * 0.225f + normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left2, p2, p3, left3))
                            yield return point;

                        yield return left3;

                        p2 = left3 + up * 0.175f + normal * 0.025f;
                        p3 = right3 + up * 0.175f - normal * 0.025f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left3, p2, p3, right3))
                            yield return point;

                        yield return right3;

                        p2 = right2 + up * 0.125f + normal * 0.05f;
                        p3 = right2 + up * 0.225f - normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right3, p3, p2, right2))
                            yield return point;

                        yield return right2;

                        p2 = right1 + up * 0.075f + normal * 0.075f;
                        p3 = right1 + up * 0.225f + normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right2, p3, p2, right1))
                            yield return point;

                        yield return right1;

                        p2 = p4 + up * 0.05f;
                        p3 = p4 + up * 0.125f - normal * 0.05f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(right1, p3, p2, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.RoundRampantMirrored:
                    {
                        var left = p1 + up * 0.3f;

                        yield return left;

                        p2 = p1 + up * 0.75f + normal * 0.05f;
                        p3 = p4 + up * 0.8f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left, p2, p3, p4))
                            yield return point;
                    }
                    break;

                case ArchMode.RampantMirrored:
                    {
                        var midpoint = ((p1 + p4) / 2f) - normal * 0.15f + up * 0.95f;
                        var left = p1 + up * 0.45f;

                        yield return left;

                        p2 = p1 + up * 0.65f;
                        p3 = p1 + up * 0.85f + normal * 0.1f;
                        foreach (var point in Arch_ForEachBezierSegmentPoint(left, p2, p3, midpoint))
                            yield return point;

                        yield return midpoint;

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