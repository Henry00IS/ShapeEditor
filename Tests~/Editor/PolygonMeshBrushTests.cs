using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using RealtimeCSG.Components;
using RealtimeCSG.Foundation;
using AeternumGames.ShapeEditor;
using Polygon = AeternumGames.ShapeEditor.Polygon;

public class PolygonMeshBrushTests
{
    private GameObject parent;

    [SetUp]
    public void SetUp()
    {
        Assert.IsTrue(ExternalRealtimeCSG.IsAvailable(), "Real RealtimeCSG package must be loaded.");
        parent = new GameObject("Topology test parent");
        parent.transform.position = new Vector3(10, 20, 30);
    }

    [TearDown]
    public void TearDown() { UnityEngine.Object.DestroyImmediate(parent); }

    private static PolygonMesh Extrude(Vector2 front, Vector2 back)
    {
        var square = new Polygon(new[] {
            new Vertex(new Vector3(-1, -1, 0), Vector2.zero),
            new Vertex(new Vector3(1, -1, 0), Vector2.zero),
            new Vertex(new Vector3(1, 1, 0), Vector2.zero),
            new Vertex(new Vector3(-1, 1, 0), Vector2.zero)
        });
        return MeshGenerator.CreateScaleExtrudedMeshes(new PolygonMesh { square }, 2,
                                                      front, back, Vector2.zero).Single();
    }

    private CSGBrush Build(PolygonMesh mesh, Material[] materials = null)
    {
        return (CSGBrush)ExternalRealtimeCSG.CreateBrushFromPolygonMesh(parent.transform, "Test brush", mesh, materials);
    }

    private static void VerifyTopology(CSGBrush brush, int vertices, int faces)
    {
        Assert.IsNotNull(brush);
        Assert.IsTrue(brush.ControlMesh.Valid);
        Assert.AreEqual(vertices, brush.ControlMesh.Vertices.Length);
        Assert.AreEqual(faces, brush.ControlMesh.Polygons.Length);
        var control = brush.ControlMesh;
        Assert.AreEqual(2, vertices - control.Edges.Length / 2 + faces);
        for (int e = 0; e < control.Edges.Length; e++)
        {
            int twin = control.Edges[e].TwinIndex;
            Assert.That(twin, Is.InRange(0, control.Edges.Length - 1));
            Assert.AreEqual(e, control.Edges[twin].TwinIndex);
            Assert.AreNotEqual(control.Edges[e].PolygonIndex, control.Edges[twin].PolygonIndex);
        }
        foreach (var surface in brush.Shape.Surfaces)
        {
            Assert.Greater(surface.Plane.normal.sqrMagnitude, 0.99f);
            Assert.Greater(surface.Tangent.sqrMagnitude, 0.99f);
            Assert.Greater(surface.BiNormal.sqrMagnitude, 0.99f);
        }
    }

    [Test]
    public void CubeRetainsLocalGeometryAndDoesNotMutateInput()
    {
        var mesh = Extrude(Vector2.one, Vector2.one);
        var before = mesh.Select(p => p.Select(v => v.position).ToArray()).ToArray();
        var brush = Build(mesh);
        VerifyTopology(brush, 8, 6);
        Assert.AreSame(parent.transform, brush.transform.parent);
        Assert.AreEqual(Vector3.zero, brush.transform.localPosition);
        for (int p = 0; p < mesh.Count; p++) CollectionAssert.AreEqual(before[p], mesh[p].Select(v => v.position));
        var again = Build(mesh);
        VerifyTopology(again, 8, 6);
        CollectionAssert.AreEqual(brush.ControlMesh.Vertices, again.ControlMesh.Vertices);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ZeroScaleCapBecomesAPyramid(bool reverse)
    {
        var mesh = Extrude(reverse ? Vector2.zero : Vector2.one, reverse ? Vector2.one : Vector2.zero);
        var brush = Build(mesh);
        VerifyTopology(brush, 5, 5);
        Assert.AreEqual(4, brush.ControlMesh.Polygons.Count(p => p.EdgeIndices.Length == 3));
    }

    [Test]
    public void MaterialSlotsAndSubtractionSurviveConversion()
    {
        var mesh = Extrude(Vector2.one, Vector2.one);
        mesh.booleanOperator = PolygonBooleanOperator.Difference;
        var materials = Enumerable.Range(0, mesh.Count).Select(i => new Material(Shader.Find("Standard"))).ToArray();
        try
        {
            var brush = Build(mesh, materials);
            VerifyTopology(brush, 8, 6);
            Assert.AreEqual(CSGOperationType.Subtractive, brush.OperationType);
            for (int p = 0; p < mesh.Count; p++)
                Assert.AreSame(materials[p], brush.Shape.TexGens[brush.Shape.Surfaces[p].TexGenIndex].RenderMaterial);
        }
        finally { foreach (var material in materials) UnityEngine.Object.DestroyImmediate(material); }
    }

    [Test]
    public void MissingFaceCannotCreatePartialSceneObject()
    {
        var mesh = Extrude(Vector2.one, Vector2.one);
        mesh.RemoveAt(0);
        Assert.Throws<ArgumentException>(() => Build(mesh));
        Assert.AreEqual(0, parent.transform.childCount);
    }

    [TestCase(false, false, 8f)]
    [TestCase(true, false, 8f / 3f)]
    [TestCase(false, true, 7f)]
    public void NativeModelProducesTheExpectedSolid(bool pyramid, bool subtract, float expectedVolume)
    {
        var model = parent.AddComponent<CSGModel>();
        Build(Extrude(Vector2.one, pyramid ? Vector2.zero : Vector2.one));
        if (subtract)
        {
            var inner = Extrude(Vector2.one, Vector2.one);
            inner.booleanOperator = PolygonBooleanOperator.Difference;
            var cutter = Build(inner);
            cutter.transform.localScale = Vector3.one * 0.5f;
            cutter.transform.localPosition = new Vector3(0, 0, 0.5f);
        }
        RealtimeCSG.CSGModelManager.ForceRebuild();
        RealtimeCSG.CSGModelManager.EnsureBuildFinished();
        var objects = RealtimeCSG.CSGModelManager.GetModelMeshes(model);
        Assert.IsNotEmpty(objects, "The actual native CSG engine must produce renderable meshes.");
        double volume = 0;
        foreach (var obj in objects)
        {
            var filter = obj.GetComponent<MeshFilter>();
            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.sharedMesh);
            var points = filter.sharedMesh.vertices.Select(v =>
                parent.transform.InverseTransformPoint(filter.transform.TransformPoint(v))).ToArray();
            var triangles = filter.sharedMesh.triangles;
            Assert.IsNotEmpty(triangles);
            for (int t = 0; t < triangles.Length; t += 3)
                volume += Vector3.Dot(points[triangles[t]],
                    Vector3.Cross(points[triangles[t + 1]], points[triangles[t + 2]])) / 6.0;
        }
        Assert.That(Math.Abs(volume), Is.EqualTo(expectedVolume).Within(0.001));
    }

    [Test]
    public void RepeatedFaceCannotCreateNonManifoldBrush()
    {
        var mesh = Extrude(Vector2.one, Vector2.one);
        mesh.Add(new Polygon(mesh[0]));
        Assert.Throws<ArgumentException>(() => Build(mesh));
        Assert.AreEqual(0, parent.transform.childCount);
    }
}
