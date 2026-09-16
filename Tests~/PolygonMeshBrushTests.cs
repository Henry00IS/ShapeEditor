using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AeternumGames.ShapeEditor;
using NUnit.Framework;
using RealtimeCSG;
using RealtimeCSG.Components;
using RealtimeCSG.Foundation;
using RealtimeCSG.Legacy;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using Polygon = AeternumGames.ShapeEditor.Polygon;

public class PolygonMeshBrushTests
{
    internal GameObject root;
    internal CSGModel model;
    internal Material material;

    [SetUp]
    public void SetUp()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        root = new GameObject("Polygon mesh tests");
        model = root.AddComponent<CSGModel>();
        material = new Material(Shader.Find("Standard"));
        Assert.That(ExternalRealtimeCSG.IsAvailable(), Is.True);
    }

    [TearDown]
    public void TearDown()
    {
        if (root) Object.DestroyImmediate(root);
        if (material) Object.DestroyImmediate(material);
        ExternalRealtimeCSG.UpdateSelection();
        CSGModelManager.EnsureBuildFinished();
    }

    internal static PolygonMesh Section(int sides = 4)
    {
        var polygon = new Polygon();
        for (int i = 0; i < sides; i++)
        {
            double angle = 2 * Math.PI * i / sides;
            polygon.Add(new Vertex((float)Math.Cos(angle), (float)Math.Sin(angle)));
        }
        return new PolygonMesh {polygon};
    }

    internal static PolygonMesh Extrude(int sides = 4, float front = 1, float back = 1)
    {
        return MeshGenerator.CreateScaleExtrudedMeshes(Section(sides), 2,
            Vector2.one * front, Vector2.one * back, Vector2.zero)[0];
    }

    internal CSGBrush Create(PolygonMesh mesh)
    {
        var brush = ExternalRealtimeCSG.CreateBrushFromPolygonMesh(root.transform, "Test brush", mesh,
            Enumerable.Repeat(material, mesh.Count).ToArray()) as CSGBrush;
        Assert.That(brush, Is.Not.Null);
        Assert.That(brush.transform.parent, Is.EqualTo(root.transform));
        Assert.That(brush.transform.localPosition, Is.EqualTo(Vector3.zero));
        Assert.That(brush.ControlMesh.Valid, Is.True);
        AssertTopology(brush);
        return brush;
    }

    internal static void AssertTopology(CSGBrush brush, bool expectUnitTextureScale = true)
    {
        var cm = brush.ControlMesh;
        Assert.That(cm.Vertices.Length - cm.Edges.Length / 2 + cm.Polygons.Length, Is.EqualTo(2));
        for (int i = 0; i < cm.Edges.Length; i++)
        {
            var edge = cm.Edges[i];
            Assert.That(edge.VertexIndex, Is.InRange(0, cm.Vertices.Length - 1));
            Assert.That(edge.TwinIndex, Is.InRange(0, cm.Edges.Length - 1));
            var twin = cm.Edges[edge.TwinIndex];
            Assert.That(twin.TwinIndex, Is.EqualTo(i));
            Assert.That(twin.PolygonIndex, Is.Not.EqualTo(edge.PolygonIndex));
            Assert.That(twin.VertexIndex, Is.EqualTo(cm.Edges[cm.GetPrevEdgeIndex(i)].VertexIndex));
            Assert.That(twin.VertexIndex, Is.Not.EqualTo(edge.VertexIndex));
        }
        var validate = typeof(CSGBrush).Assembly.GetType("RealtimeCSG.ControlMeshUtility")
            .GetMethod("Validate", BindingFlags.Static | BindingFlags.Public);
        Assert.That(validate.Invoke(null, new object[] {cm, brush.Shape}), Is.EqualTo(true));
        for (int i = 0; i < brush.Shape.Surfaces.Length; i++)
        {
            var surface = brush.Shape.Surfaces[i];
            Assert.That(surface.Plane.normal.magnitude, Is.EqualTo(1).Within(1e-5));
            Assert.That(surface.Tangent.magnitude, Is.EqualTo(1).Within(1e-5));
            Assert.That(Vector3.Dot(surface.Tangent, surface.Plane.normal), Is.EqualTo(0).Within(1e-5));
            if (expectUnitTextureScale) Assert.That(brush.Shape.TexGens[i].Scale, Is.EqualTo(Vector2.one));
        }
    }

    internal static double Volume(CSGModel model, bool forceRebuild = true)
    {
        CSGModelManager.EnsureBuildFinished();
        if (forceRebuild) CSGModelManager.ForceRebuild();
        var objects = CSGModelManager.GetModelMeshes(model);
        Assert.That(objects.Length, Is.GreaterThan(0), "Native RealtimeCSG must generate render meshes");
        double volume6 = 0;
        foreach (var gameObject in objects)
        {
            var mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            Assert.That(mesh, Is.Not.Null);
            var vertices = mesh.vertices;
            var indices = mesh.triangles;
            for (int i = 0; i < indices.Length; i += 3)
            {
                var a = model.transform.InverseTransformPoint(gameObject.transform.TransformPoint(vertices[indices[i]]));
                var b = model.transform.InverseTransformPoint(gameObject.transform.TransformPoint(vertices[indices[i+1]]));
                var c = model.transform.InverseTransformPoint(gameObject.transform.TransformPoint(vertices[indices[i+2]]));
                volume6 += Vector3.Dot(a, Vector3.Cross(b, c));
            }
        }
        return Math.Abs(volume6 / 6);
    }

    [TestCase(4, 1f, 1f)]
    [TestCase(4, 0f, 1f)]
    [TestCase(4, 1f, 0f)]
    [TestCase(4, 1f, 0.3f)]
    [TestCase(3, 0f, 1f)]
    [TestCase(3, 1f, 0f)]
    [TestCase(16, 1f, 0f)]
    [TestCase(64, 1f, 1f)]
    public void ExtrusionsHaveExpectedNativeVolume(int sides, float front, float back)
    {
        var mesh = Extrude(sides, front, back);
        var brush = Create(mesh);
        int expectedVertices = front == 0 || back == 0 ? sides + 1 : sides * 2;
        if (expectedVertices == 4) expectedVertices = 5;
        Assert.That(brush.ControlMesh.Vertices.Length, Is.EqualTo(expectedVertices));
        double area = sides * Math.Sin(2 * Math.PI / sides) / 2;
        double expected = area * 2 * (front*front + front*back + back*back) / 3;
        Assert.That(Volume(model), Is.EqualTo(expected).Within(0.001));
    }

    [Test]
    public void ConversionIsRepeatableAndDoesNotMutateInput()
    {
        var mesh = Extrude(5, 1, 0);
        var before = mesh.Select(p => p.Select(v => JsonUtility.ToJson(v)).ToArray()).ToArray();
        var planes = mesh.Select(p => p.plane).ToArray();
        var first = Create(mesh);
        var second = Create(mesh);
        CollectionAssert.AreEqual(first.ControlMesh.Vertices, second.ControlMesh.Vertices);
        for(int p=0;p<mesh.Count;p++)
        {
            CollectionAssert.AreEqual(before[p], mesh[p].Select(v => JsonUtility.ToJson(v)).ToArray());
            Assert.That(mesh[p].plane, Is.EqualTo(planes[p]));
        }
    }

    [Test]
    public void VertexIndicesSurviveInterleavedFaceOrder()
    {
        var mesh = Extrude();
        var swap=mesh[1]; mesh[1]=mesh[2]; mesh[2]=swap;
        Create(mesh);
        Assert.That(Volume(model), Is.EqualTo(4).Within(0.001));
    }

    [Test]
    public void MaterialsSurviveCollapsedFaceAndWinding()
    {
        var mesh = Extrude(4,1,0);
        mesh.Insert(0,new Polygon(new[] {new Vertex(0,0,2),new Vertex(0,0,2),new Vertex(0,0,2)}));
        var materials = new List<Material>();
        try
        {
            for(int i=0;i<mesh.Count;i++) materials.Add(new Material(material) {color=new Color(i/8f,0.5f,1)});
            var brush=(CSGBrush)ExternalRealtimeCSG.CreateBrushFromPolygonMesh(root.transform,"Materials",mesh,materials.ToArray());
            Assert.That(brush,Is.Not.Null);
            Assert.That(brush.Shape.TexGens.Length,Is.EqualTo(mesh.Count-1));
            for(int i=0;i<brush.Shape.TexGens.Length;i++)
                Assert.That(brush.Shape.TexGens[i].RenderMaterial,Is.SameAs(materials[i+1]));
        }
        finally { foreach(var entry in materials) Object.DestroyImmediate(entry); }
    }

    [Test]
    public void CurrentMaterialAndTextureDefaultsAreUsed()
    {
        var settings=typeof(CSGBrush).Assembly.GetType("RealtimeCSG.CSGSettings");
        var materialField=settings.GetField("DefaultMaterial");
        var flagsField=settings.GetField("DefaultTexGenFlags");
        var oldMaterial=materialField.GetValue(null); var oldFlags=flagsField.GetValue(null);
        try
        {
            materialField.SetValue(null,material);
            flagsField.SetValue(null,TexGenFlags.WorldSpaceTexture);
            var brush=(CSGBrush)ExternalRealtimeCSG.CreateBrushFromPolygonMesh(root.transform,"Defaults",Extrude());
            Assert.That(brush,Is.Not.Null);
            foreach(var texture in brush.Shape.TexGens) Assert.That(texture.RenderMaterial,Is.SameAs(material));
            foreach(var flags in brush.Shape.TexGenFlags) Assert.That(flags,Is.EqualTo(TexGenFlags.WorldSpaceTexture));
        }
        finally { materialField.SetValue(null,oldMaterial); flagsField.SetValue(null,oldFlags); }
    }

    [Test]
    public void TetrahedronSubdivisionPreservesTheSourceMaterial()
    {
        var mesh=Extrude(3,1,0);
        var other=new Material(material) {color=Color.red};
        try
        {
            var materials=new[] {other,material,material,material};
            var brush=(CSGBrush)ExternalRealtimeCSG.CreateBrushFromPolygonMesh(root.transform,"Tetrahedron",mesh,materials);
            Assert.That(brush,Is.Not.Null);
            Assert.That(brush.Shape.TexGens.Length,Is.EqualTo(6));
            foreach(int index in new[] {0,4,5}) Assert.That(brush.Shape.TexGens[index].RenderMaterial,Is.SameAs(other));
            foreach(int index in new[] {1,2,3}) Assert.That(brush.Shape.TexGens[index].RenderMaterial,Is.SameAs(material));
            Assert.That(Volume(model),Is.EqualTo(Math.Sqrt(3)/2).Within(0.001));
        }
        finally { Object.DestroyImmediate(other); }
    }

    [Test]
    public void NearbyVerticesAcrossSpatialCellBoundariesAreWelded()
    {
        var mesh=Extrude();
        for(int p=0;p<mesh.Count;p++)
        for(int i=0;i<mesh[p].Count;i++)
        {
            var vertex=mesh[p][i];
            vertex.position += Vector3.one * (p%2==0 ? -0.000002f : 0.000002f);
            mesh[p][i]=vertex;
        }
        Assert.That(Create(mesh).ControlMesh.Vertices.Length,Is.EqualTo(8));
    }

    [Test]
    public void RepeatedClosingVerticesAndGlobalReverseAreAccepted()
    {
        var mesh=Extrude();
        foreach(var polygon in mesh) { polygon.Add(polygon[0]); polygon.Reverse(); }
        Create(mesh);
        Assert.That(Volume(model),Is.EqualTo(4).Within(0.001));
    }

    [Test]
    public void OneAxisCollapseProducesAWedge()
    {
        var square=new PolygonMesh {new Polygon(new[] {new Vertex(-1,-1),new Vertex(1,-1),new Vertex(1,1),new Vertex(-1,1)})};
        var mesh=MeshGenerator.CreateScaleExtrudedMeshes(square,2,Vector2.one,new Vector2(0,1),Vector2.zero)[0];
        Create(mesh);
        Assert.That(Volume(model),Is.EqualTo(4).Within(0.001));
    }

    [Test]
    public void OffsetPyramidAndTransformedParentKeepLocalGeometry()
    {
        root.transform.SetPositionAndRotation(new Vector3(7,-3,11),Quaternion.Euler(25,40,-15));
        root.transform.localScale=new Vector3(2,3,1);
        var mesh=MeshGenerator.CreateScaleExtrudedMeshes(Section(),2,Vector2.one,Vector2.zero,new Vector2(2,-1))[0];
        var brush=Create(mesh);
        Assert.That(brush.transform.localRotation,Is.EqualTo(Quaternion.identity));
        Assert.That(brush.transform.localScale,Is.EqualTo(Vector3.one));
        Assert.That(Volume(model),Is.EqualTo(4.0/3).Within(0.001));
    }

    [Test]
    public void SubtractiveBrushCutsTheNativeMesh()
    {
        Create(Extrude());
        var cut=Extrude();
        foreach(var polygon in cut) polygon.Scale(new Vector3(0.5f,0.5f,1));
        cut.booleanOperator=PolygonBooleanOperator.Difference;
        var brush=Create(cut);
        Assert.That(brush.OperationType,Is.EqualTo(CSGOperationType.Subtractive));
        Assert.That(Volume(model),Is.EqualTo(3).Within(0.001));
    }

    [TestCase("null")]
    [TestCase("empty")]
    [TestCase("open")]
    [TestCase("duplicate")]
    [TestCase("mixed-winding")]
    [TestCase("nan")]
    [TestCase("infinity")]
    [TestCase("nonplanar")]
    [TestCase("concave")]
    [TestCase("flat")]
    [TestCase("missing-face")]
    [TestCase("vertex-limit")]
    [TestCase("face-limit")]
    [TestCase("disconnected")]
    [TestCase("revisited-vertex")]
    public void InvalidGeometryCreatesNoSceneObject(string kind)
    {
        var mesh=Extrude();
        switch(kind)
        {
            case "null": mesh=null; break;
            case "empty": mesh.Clear(); break;
            case "open": mesh.RemoveAt(0); break;
            case "duplicate": mesh.Add(new Polygon(mesh[0])); break;
            case "mixed-winding": mesh[0].Reverse(); break;
            case "nan": mesh[0][0]=new Vertex(float.NaN,0,0); break;
            case "infinity": mesh[0][0]=new Vertex(float.PositiveInfinity,0,0); break;
            case "nonplanar": mesh[0][0]=new Vertex(1,0,0.1f); break;
            case "concave":
                var old=mesh[0][0].position;
                foreach(var polygon in mesh)
                for(int i=0;i<polygon.Count;i++)
                    if(polygon[i].position==old) polygon[i]=new Vertex(0,0,0.5f);
                break;
            case "flat": foreach(var polygon in mesh) polygon.Scale(new Vector3(1,1,0)); break;
            case "missing-face": mesh[0]=null; break;
            case "vertex-limit": mesh=Extrude(16384); break;
            case "face-limit":
                var face=mesh[0]; mesh.Clear();
                for(int i=0;i<short.MaxValue;i++) mesh.Add(face);
                break;
            case "disconnected":
                var other=Extrude(); other.Translate(new Vector3(5,0,0)); mesh.AddRange(other);
                break;
            case "revisited-vertex": mesh[0].Insert(2,mesh[0][0]); break;
        }
        int children=root.transform.childCount;
        LogAssert.Expect(LogType.Warning,new Regex("Cannot create RealtimeCSG brush"));
        Assert.That(ExternalRealtimeCSG.CreateBrushFromPolygonMesh(root.transform,"Invalid",mesh),Is.Null);
        Assert.That(root.transform.childCount,Is.EqualTo(children));
    }

    [UnityTest]
    public IEnumerator FrontPyramidDepthUndoRedoPreservesGeometry()
    {
        return CheckPyramidDepthUndoRedo(0, 1);
    }

    [UnityTest]
    public IEnumerator BackPyramidDepthUndoRedoPreservesGeometry()
    {
        return CheckPyramidDepthUndoRedo(1, 0);
    }

    private IEnumerator CheckPyramidDepthUndoRedo(float front, float back)
    {
        var go = new GameObject("Pyramid undo test");
        go.transform.SetParent(root.transform, false);
        var target = go.AddComponent<RealtimeCSGTarget>();
        target.materials = new[] { material };
        typeof(RealtimeCSGTarget).GetField("targetMode", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(target, RealtimeCSGTargetMode.ScaledExtrude);
        target.scaledExtrudeDistance = 2;
        target.scaledExtrudeFrontScale = Vector2.one * front;
        target.scaledExtrudeBackScale = Vector2.one * back;
        target.OnShapeEditorUpdateProject(new Project());
        double originalVolume = Volume(model, false);
        Assert.That(originalVolume, Is.GreaterThan(0));
        int brushCount = go.GetComponentsInChildren<CSGBrush>().Length;
        yield return null;
        Undo.ClearAll();
        try
        {
            Undo.RecordObject(target, "Change pyramid depth");
            target.scaledExtrudeDistance = 3;
            target.Rebuild();
            yield return null;
            Assert.That(Volume(model, false), Is.EqualTo(originalVolume * 1.5).Within(0.001));

            Undo.PerformUndo();
            yield return null;
            Assert.That(target.scaledExtrudeDistance, Is.EqualTo(2));
            target.Rebuild();
            Assert.That(Volume(model, false), Is.EqualTo(originalVolume).Within(0.001));
            yield return null;

            Undo.PerformRedo();
            yield return null;
            Assert.That(target.scaledExtrudeDistance, Is.EqualTo(3));
            for (int i = 0; i < 10; i++)
            {
                target.Rebuild();
                var brushes = go.GetComponentsInChildren<CSGBrush>();
                Assert.That(brushes.Length, Is.EqualTo(brushCount));
                foreach (var brush in brushes)
                {
                    AssertTopology(brush);
                    foreach (var texture in brush.Shape.TexGens)
                        Assert.That(texture.RenderMaterial, Is.SameAs(material));
                }
                Assert.That(Volume(model, false), Is.EqualTo(originalVolume * 1.5).Within(0.001));
            }
        }
        finally
        {
            Undo.ClearAll();
        }
    }

    [TestCase(RealtimeCSGTargetMode.FixedExtrude)]
    [TestCase(RealtimeCSGTargetMode.ScaledExtrude)]
    [TestCase(RealtimeCSGTargetMode.LinearStaircase)]
    [TestCase(RealtimeCSGTargetMode.RevolveExtrude)]
    [TestCase(RealtimeCSGTargetMode.RevolveChopped)]
    [TestCase(RealtimeCSGTargetMode.SplineExtrude)]
    public void TargetModesProduceValidNativeGeometryAndRebuild(RealtimeCSGTargetMode mode)
    {
        var go=new GameObject(mode.ToString()); go.transform.SetParent(root.transform,false);
        var target=go.AddComponent<RealtimeCSGTarget>(); target.materials=new[] {material};
        typeof(RealtimeCSGTarget).GetField("targetMode",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(target,mode);
        if(mode==RealtimeCSGTargetMode.SplineExtrude)
            for(int i=0;i<3;i++) { var point=new GameObject("Spline point"); point.transform.SetParent(go.transform,false); point.transform.localPosition=new Vector3(0,0,i*3); }
        target.OnShapeEditorUpdateProject(new Project());
        var brushes=go.GetComponentsInChildren<CSGBrush>();
        Assert.That(brushes.Length,Is.GreaterThan(0));
        foreach(var brush in brushes) AssertTopology(brush);
        double before=Volume(model);
        Assert.That(before,Is.GreaterThan(0.0001));
        target.Rebuild();
        Assert.That(go.GetComponentsInChildren<CSGBrush>().Length,Is.EqualTo(brushes.Length));
        Assert.That(Volume(model),Is.EqualTo(before).Within(0.001));
    }
}
