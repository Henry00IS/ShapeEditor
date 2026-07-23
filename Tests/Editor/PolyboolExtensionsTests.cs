#if UNITY_EDITOR

using AeternumGames.ShapeEditor.PolyBoolCS;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.TestTools;
using PolyboolPolygon = AeternumGames.ShapeEditor.PolyBoolCS.Polygon;
using Region = AeternumGames.ShapeEditor.PolyBoolCS.PointList;

namespace AeternumGames.ShapeEditor.Tests
{
    public class PolyboolExtensionsTests
    {
        [Test]
        public void ToPolygons_VertexOnNonAdjacentEdge_SplitsIntoSimpleRegions()
        {
            var polyBool = new PolyBool();
            var singularPolygon = new PolyboolPolygon
            {
                regions = new List<Region>
                {
                    new Region
                    {
                        new Point(-0.5, 0.5),
                        new Point(-0.5, -0.5),
                        new Point(0.0, -0.5),
                        new Point(-0.5, 0.0),
                        new Point(0.0, 0.5)
                    }
                }
            };

            var polygons = singularPolygon.ToPolygons(polyBool);

            Assert.That(polygons, Has.Count.EqualTo(2));

            var totalArea = 0.0f;
            foreach (var polygon in polygons)
            {
                Assert.That(polygon, Has.Count.EqualTo(3));
                Assert.That(polygon.IsCounterClockWise2D(), Is.True);
                totalArea += polygon.GetSignedArea2D();
            }

            Assert.That(totalArea, Is.EqualTo(0.25f).Within(0.000001f));
        }

        [Test]
        public void GenerateConvexPolygons_PointTouchingShapes_DoesNotLogAndReturnsFourTriangles()
        {
            var project = new Project
            {
                shapes = new List<Shape>
                {
                    CreateTriangle(-0.5f, -0.5f, 0.0f, -0.5f, -0.5f, 0.0f),
                    CreateTriangle(0.0f, -0.5f, 0.5f, -0.5f, 0.5f, 0.0f),
                    CreateTriangle(0.5f, 0.0f, 0.5f, 0.5f, 0.0f, 0.5f),
                    CreateTriangle(-0.5f, 0.0f, 0.0f, 0.5f, -0.5f, 0.5f)
                }
            };
            project.Validate();

            var polygons = project.GenerateConvexPolygons();

            Assert.That(polygons, Has.Count.EqualTo(4));
            foreach (var polygon in polygons)
            {
                Assert.That(polygon, Has.Count.EqualTo(3));
                Assert.That(polygon.IsCounterClockWise2D(), Is.True);
            }

            LogAssert.NoUnexpectedReceived();
        }

        private static Shape CreateTriangle(
            float x1, float y1,
            float x2, float y2,
            float x3, float y3)
        {
            var shape = new Shape();
            shape.segments.Clear();
            shape.AddSegment(new Segment(shape, x1, y1));
            shape.AddSegment(new Segment(shape, x2, y2));
            shape.AddSegment(new Segment(shape, x3, y3));
            return shape;
        }
    }
}

#endif
