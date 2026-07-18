#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using AeternumGames.ShapeEditor.PolyBoolCS;
using NUnit.Framework;
using UnityEngine.TestTools;
using PolyboolPolygon = AeternumGames.ShapeEditor.PolyBoolCS.Polygon;
using Region = AeternumGames.ShapeEditor.PolyBoolCS.PointList;

namespace AeternumGames.ShapeEditor.Tests
{
    public class PolyboolExtensionsTests
    {
        [TestCase(1.0)]
        [TestCase(0.00001)]
        [TestCase(100000.0)]
        public void CrashFixtureSplitsAtEveryScale(double scale)
        {
            var triangles = new[]
            {
                region(scale, -0.5, -0.5, 0.0, -0.5, -0.5, 0.0),
                region(scale, 0.0, -0.5, 0.5, -0.5, 0.5, 0.0),
                region(scale, 0.5, 0.0, 0.5, 0.5, 0.0, 0.5),
                region(scale, -0.5, 0.0, 0.0, 0.5, -0.5, 0.5)
            };

            var polyBool = new PolyBool();
            var segments = new SegmentList();
            for (var i = 0; i < triangles.Length; i++)
            {
                var combined = polyBool.combine(segments, polyBool.segments(polygon(triangles[i])));
                segments = polyBool.selectUnion(combined);
            }

            var output = polyBool.polygon(segments).ToPolygons(polyBool);
            var expectedArea = 0.5 * scale * scale;

            Assert.That(output.Count, Is.EqualTo(4));
            Assert.That(output.All(item => !hasSingularity(item)), Is.True);
            Assert.That(output.Sum(signedArea), Is.EqualTo(expectedArea).Within(expectedArea * 0.000001 + 1e-18));

            for (var i = 0; i < output.Count; i++)
                Assert.That(BayazitDecomposer.ConvexPartition(output[i]).Count, Is.EqualTo(1));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void TangentExteriorBecomesSeparateRegions()
        {
            var output = apply(
                (region(1.0, 0, 0, 4, 0, 4, 4, 0, 4), false),
                (region(1.0, -2, 1, 0, 2, -2, 3), false));

            Assert.That(output.Count, Is.EqualTo(2));
            Assert.That(output.All(item => !hasSingularity(item)), Is.True);
            Assert.That(output.Sum(signedArea), Is.EqualTo(18.0).Within(1e-9));
        }

        [Test]
        public void TangentHolePreservesWindingAndArea()
        {
            var output = apply(
                (region(1.0, 0, 0, 4, 0, 4, 4, 0, 4), false),
                (region(1.0, 0, 2, 1, 1, 2, 2, 1, 3), true));
            var areas = output.Select(signedArea).OrderBy(value => value).ToArray();

            Assert.That(output.Count, Is.EqualTo(2));
            Assert.That(output.All(item => !hasSingularity(item)), Is.True);
            Assert.That(areas[0], Is.EqualTo(-2.0).Within(1e-9));
            Assert.That(areas[1], Is.EqualTo(16.0).Within(1e-9));
        }

        [Test]
        public void HoleAndIslandPreserveThreeLevelHierarchy()
        {
            var output = apply(
                (region(1.0, 0, 0, 10, 0, 10, 10, 0, 10), false),
                (region(1.0, 2, 2, 8, 2, 8, 8, 2, 8), true),
                (region(1.0, 4, 4, 6, 4, 6, 6, 4, 6), false));
            var areas = output.Select(signedArea).OrderBy(value => value).ToArray();

            Assert.That(output.Count, Is.EqualTo(3));
            Assert.That(output.All(item => !hasSingularity(item)), Is.True);
            Assert.That(areas, Is.EqualTo(new[] { -36.0, 4.0, 100.0 }).Within(1e-9));
        }

        [Test]
        public void ManyTangenciesAreSplitIteratively()
        {
            var inputs = new List<(Region region, bool subtract)>
            {
                (region(1.0, 0, 0, 4, 0, 4, 26, 0, 26), false)
            };
            for (var i = 0; i < 12; i++)
            {
                var y = 2 * i + 2;
                inputs.Add((region(1.0, -1, y - 0.5, 0, y, -1, y + 0.5), false));
            }

            var output = apply(inputs.ToArray());

            Assert.That(output.Count, Is.EqualTo(13));
            Assert.That(output.All(item => !hasSingularity(item)), Is.True);
            Assert.That(output.Sum(signedArea), Is.EqualTo(110.0).Within(1e-9));
        }

        private static List<Polygon> apply(params (Region region, bool subtract)[] inputs)
        {
            var polyBool = new PolyBool();
            var segments = new SegmentList();
            for (var i = 0; i < inputs.Length; i++)
            {
                var combined = polyBool.combine(segments, polyBool.segments(polygon(inputs[i].region)));
                segments = inputs[i].subtract
                    ? polyBool.selectDifference(combined)
                    : polyBool.selectUnion(combined);
            }
            return polyBool.polygon(segments).ToPolygons(polyBool);
        }

        private static PolyboolPolygon polygon(Region value)
        {
            return new PolyboolPolygon { regions = new List<Region> { value } };
        }

        private static Region region(double scale, params double[] coordinates)
        {
            var result = new Region(coordinates.Length / 2);
            for (var i = 0; i < coordinates.Length; i += 2)
                result.Add(new Point(coordinates[i] * scale, coordinates[i + 1] * scale));
            return result;
        }

        private static bool hasSingularity(Polygon polygonValue)
        {
            var value = new Region(polygonValue.Count);
            for (var i = 0; i < polygonValue.Count; i++)
                value.Add(new Point(polygonValue[i].x, polygonValue[i].y));

            for (var i = 0; i < value.Count; i++)
            {
                for (var j = i + 1; j < value.Count; j++)
                {
                    var adjacent = (i + 1) % value.Count == j || (j + 1) % value.Count == i;
                    if (!adjacent && Epsilon.pointsSame(value[i], value[j]))
                        return true;
                }

                for (var edgeStart = 0; edgeStart < value.Count; edgeStart++)
                {
                    var edgeEnd = (edgeStart + 1) % value.Count;
                    if (i == edgeStart || i == edgeEnd)
                        continue;
                    if (Epsilon.pointsCollinear(value[edgeStart], value[i], value[edgeEnd]) &&
                        Epsilon.pointBetween(value[i], value[edgeStart], value[edgeEnd]))
                        return true;
                }
            }

            return false;
        }

        private static double signedArea(Polygon value)
        {
            var area = 0.0;
            for (var i = 0; i < value.Count; i++)
            {
                var current = value[i];
                var next = value[(i + 1) % value.Count];
                area += current.x * next.y - current.y * next.x;
            }
            return area * 0.5;
        }
    }
}

#endif
