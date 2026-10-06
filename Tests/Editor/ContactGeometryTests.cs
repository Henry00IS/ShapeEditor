using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor.Tests
{
    public class ContactGeometryTests
    {
        public static IEnumerable<TestCaseData> Fixtures
        {
            get
            {
                yield return new TestCaseData("7a16010bf38d4d67b097c11a1428c246", 0.5, 6.82842712474619).SetName("Crash");
                yield return new TestCaseData("a6f1f017588d46dfb468588b58b51709", 1.5362499850988387, 13.968815447157688).SetName("Crash2");
                yield return new TestCaseData("5e4a7cb3f9294e2298118598028d6e28", 0.793749988079071, 8.840522519699867).SetName("ExtrudeFail");
                yield return new TestCaseData("bb7a48c4cfc747bb9f899ca6c4ce6047", 1.96875, 14.955625477929363).SetName("PolygonFail");
            }
        }

        [TestCaseSource(nameof(Fixtures))]
        public void SuppliedProjectsPreserveAreaAndExtrusion(string fixtureGuid, double area, double perimeter)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(fixtureGuid));
            Assert.That(asset, Is.Not.Null);
            var project = JsonUtility.FromJson<Project>(asset.text);
            project.Validate();
            foreach (var useHoles in new[] { true, false })
            {
                CheckPartition(project.GenerateConvexPolygons(useHoles), area);
                foreach (var chops in new[] { 2, 3, 7 })
                {
                    var cells = new List<Polygon>();
                    foreach (var part in project.GenerateChoppedPolygons(chops, useHoles)) cells.AddRange(part);
                    CheckPartition(cells, area);
                }
            }

            var mesh = MeshGenerator.CreateExtrudedPolygonMesh(project.GenerateConvexPolygons(), 1.0f);
            try
            {
                var positions = mesh.vertices;
                double capArea = 0.0;
                double wallArea = 0.0;
                for (var submesh = 0; submesh < mesh.subMeshCount; submesh++)
                {
                    var indices = mesh.GetTriangles(submesh);
                    Assert.That(indices.Length % 3, Is.EqualTo(0));
                    for (var i = 0; i < indices.Length; i += 3)
                    {
                        var a = positions[indices[i]];
                        var b = positions[indices[i + 1]];
                        var c = positions[indices[i + 2]];
                        var ab = b - a;
                        var ac = c - a;
                        var x = (double)ab.y * ac.z - (double)ab.z * ac.y;
                        var y = (double)ab.z * ac.x - (double)ab.x * ac.z;
                        var z = (double)ab.x * ac.y - (double)ab.y * ac.x;
                        var triangleArea = 0.5 * Math.Sqrt(x * x + y * y + z * z);
                        Assert.That(double.IsNaN(triangleArea) || double.IsInfinity(triangleArea), Is.False);
                        Assert.That(triangleArea, Is.GreaterThan(0.0));
                        if (a.z == b.z && b.z == c.z) capArea += triangleArea;
                        else wallArea += triangleArea;
                    }
                }
                Assert.That(capArea, Is.EqualTo(area * 2.0).Within(0.00005));
                Assert.That(wallArea, Is.EqualTo(perimeter).Within(0.00005));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void ThinContactCellsProduceUsableExtrusionPlanes()
        {
            var project = new Project();
            project.shapes.Clear();
            foreach (var points in new[]
            {
                new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) },
                new[] { new Vector2(0.000001f, 0), new Vector2(-0.5f, -1), new Vector2(0.5f, -1) }
            })
            {
                var shape = new Shape();
                shape.segments.Clear();
                foreach (var point in points) shape.AddSegment(new Segment(shape, point.x, -point.y));
                project.shapes.Add(shape);
            }
            project.Validate();
            var cells = project.GenerateConvexPolygons();
            CheckPartition(cells, 1.5);
            foreach (var brush in MeshGenerator.CreateExtrudedPolygonMeshes(cells, 1.0f))
            {
                var planes = brush.ToMaterialPlanes().planes;
                foreach (var plane in planes)
                {
                    Assert.That(plane.normal.sqrMagnitude, Is.EqualTo(1.0f).Within(0.00001f));
                    Assert.That(float.IsNaN(plane.distance) || float.IsInfinity(plane.distance), Is.False);
                    foreach (var face in brush)
                        foreach (var vertex in face)
                            Assert.That(plane.GetDistanceToPoint(vertex.position), Is.LessThanOrEqualTo(0.00001f));
                }
            }
        }

        private static void CheckPartition(List<Polygon> polygons, double expectedArea)
        {
            double area = 0.0;
            foreach (var polygon in polygons)
            {
                Assert.That(polygon.Count, Is.GreaterThanOrEqualTo(3));
                Assert.That(polygon.booleanOperator, Is.EqualTo(PolygonBooleanOperator.Union));
                double cellArea = 0.0;
                for (var i = 1; i + 1 < polygon.Count; i++)
                    cellArea += ((double)polygon[i].x - polygon[0].x) * ((double)polygon[i + 1].y - polygon[0].y) -
                        ((double)polygon[i].y - polygon[0].y) * ((double)polygon[i + 1].x - polygon[0].x);
                Assert.That(cellArea, Is.GreaterThan(0.0));
                area += cellArea * 0.5;
                for (var i = 0; i < polygon.Count; i++)
                {
                    var a = polygon[i];
                    var b = polygon[(i + 1) % polygon.Count];
                    var c = polygon[(i + 2) % polygon.Count];
                    var cross = ((double)b.x - a.x) * ((double)c.y - b.y) - ((double)b.y - a.y) * ((double)c.x - b.x);
                    Assert.That(cross, Is.GreaterThanOrEqualTo(-1e-9));
                }
            }
            Assert.That(area, Is.EqualTo(expectedArea).Within(0.00001));
        }
    }
}
