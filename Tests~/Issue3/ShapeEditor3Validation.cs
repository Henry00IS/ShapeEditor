using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AeternumGames.ShapeEditor;
using RealtimeCSG.Components;
using RealtimeCSG.Legacy;
using UnityEditor;
using UnityEngine;
using SourcePolygon = AeternumGames.ShapeEditor.Polygon;

public static class ShapeEditor3Validation
{
    private static int checks;
    private static readonly List<GameObject> roots = new List<GameObject>();

    public static void Candidate()
    {
        try
        {
            checks = 0;
            var cube = Box(1, 1, 1);
            ValidateBrush(cube, "Cube", 8, 6, 24);
            ValidateBrush(Box(1, 1, 0.000001f), "Thin box", 8, 6, 24);
            var transformed = Box(2, 3, 0.5f);
            foreach (var face in transformed)
                for (int i = 0; i < face.Count; i++)
                    face[i] = new Vertex(Quaternion.Euler(17, 31, 7) * face[i].position + new Vector3(11, -13, 19));
            ValidateBrush(transformed, "Rotated translated box", 8, 6, 24);

            var split = Box(1, 1, 1);
            var top = split[1];
            split[1] = new SourcePolygon(new[] { top[0], top[1], top[2] });
            split.Add(new SourcePolygon(new[] { top[0], top[2], top[3] }));
            ValidateBrush(split, "Two coplanar material faces", 8, 7, 26);
            PlaneRouteComparison();

            var rectangle = new SourcePolygon(new[] { new Vertex(-1,-1,0), new Vertex(1,-1,0), new Vertex(1,1,0), new Vertex(-1,1,0) });
            var generated = MeshGenerator.CreateExtrudedPolygonMeshes(new List<SourcePolygon> { rectangle }, 0.25f)[0];
            ValidateBrush(generated, "Actual fixed extrusion generator", 8, 6, 24);
            var polygons = new PolygonMesh { rectangle }; polygons.CalculateBounds2D();
            var pyramid = MeshGenerator.CreateScaleExtrudedMeshes(polygons, 1, Vector2.one, Vector2.zero, Vector2.zero)[0];
            ValidateBrush(pyramid, "Actual pyramid generator", 5, 5, 16);
            foreach (var mesh in MeshGenerator.CreateLinearStaircaseMeshes(polygons, 3, 1, 0.75f, false))
                ValidateBrush(mesh, "Actual staircase generator", 8, 6, 24);
            foreach (var mesh in MeshGenerator.CreateRevolveExtrudedPolygonMeshes(polygons, 3, 45, 2, 0, false))
                ValidateBrush(mesh, "Actual revolve generator", 8, 6, 24);
            var spline = new MathEx.Spline3(new[] { new Vector3(0,0,0), new Vector3(0,0,0.5f), new Vector3(0,0,1) });
            foreach (var mesh in MeshGenerator.CreateSplineExtrudedPolygonMeshes(new List<SourcePolygon> { rectangle }, spline, 2))
                ValidateBrush(mesh, "Actual spline generator", 8, 6, 24);
            var chopped = new PolygonMeshes { polygons }; chopped.CalculateBounds2D();
            foreach (var mesh in MeshGenerator.CreateRevolveChoppedMeshes(chopped, 45, 1))
                ValidateBrush(mesh, "Actual chopped revolve generator", 8, 6, 24);

            var parent = new GameObject("Rejected inputs"); roots.Add(parent);
            var open = Box(1,1,1); open.RemoveAt(0); Reject(open, parent, "Open mesh");
            var duplicate = Box(1,1,1); duplicate.Add(new SourcePolygon(duplicate[0])); Reject(duplicate, parent, "Nonmanifold duplicate face");
            var nan = Box(1,1,1); nan[0][0] = new Vertex(float.NaN,0,0); Reject(nan, parent, "NaN coordinate");
            Reject(Box(1,1,0), parent, "Zero volume");
            Reject(null, parent, "Null mesh");

            var subtractive = Box(1,1,1); subtractive.booleanOperator = PolygonBooleanOperator.Difference;
            var difference = (CSGBrush)ExternalRealtimeCSG.CreateBrushFromPolygonMesh(parent.transform, "Difference", subtractive);
            Check(difference != null && difference.OperationType.ToString() == "Subtractive", "Difference operation retained");
            NativeMesh(cube);
            TargetModes();
            Debug.Log("SHAPEEDITOR3_CANDIDATE: PASS " + checks + " checks");
            foreach (var root in roots) if (root) UnityEngine.Object.DestroyImmediate(root);
            EditorApplication.Exit(0);
        }
        catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(1); }
    }

    private static void Reject(PolygonMesh mesh, GameObject parent, string label)
    {
        int count = parent.transform.childCount;
        Check(ExternalRealtimeCSG.CreateBrushFromPolygonMesh(parent.transform,label,mesh) == null, label + " rejected");
        Check(parent.transform.childCount == count, label + " creates no orphan object");
    }

    public static void PlaneRouteRegression()
    {
        try
        {
            var mesh = SplitBox(); var planes = mesh.ToMaterialPlanes();
            var brush = (CSGBrush)ExternalRealtimeCSG.CreateBrushFromPlanes("Original plane route",planes.planes,null,mesh.booleanOperator);
            Debug.Log("ORIGINAL_FACE_COUNT: input=" + mesh.Count + "; output=" + brush.ControlMesh.Polygons.Length);
            Check(brush.ControlMesh.Polygons.Length == mesh.Count, "Original route preserves seven input faces, including two coplanar faces");
            EditorApplication.Exit(0);
        }
        catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(1); }
    }

    private static PolygonMesh SplitBox()
    {
        var mesh = Box(1,1,1); var top = mesh[1];
        mesh[1] = new SourcePolygon(new[] {top[0],top[1],top[2]});
        mesh.Add(new SourcePolygon(new[] {top[0],top[2],top[3]}));
        return mesh;
    }

    private static void PlaneRouteComparison()
    {
        var cube = Box(1,1,1); var planes = cube.ToMaterialPlanes();
        var old = (CSGBrush)ExternalRealtimeCSG.CreateBrushFromPlanes("Old cube",planes.planes,null,cube.booleanOperator);
        var fresh = (CSGBrush)ExternalRealtimeCSG.CreateBrushFromPolygonMesh(null,"New cube",cube);
        foreach (var surface in fresh.Shape.Surfaces)
        {
            var previous = old.Shape.Surfaces.Single(s=>Vector3.Dot(s.Plane.normal,surface.Plane.normal)>0.999f);
            Check(Mathf.Abs(previous.Plane.d-surface.Plane.d)<0.00001f, "Plane distance agrees with original cube");
            Debug.Log("UV_COMPARE normal="+surface.Plane.normal+" old="+previous.Tangent+","+previous.BiNormal+" new="+surface.Tangent+","+surface.BiNormal);
            Check((previous.Tangent-surface.Tangent).sqrMagnitude<0.00001f && (previous.BiNormal-surface.BiNormal).sqrMagnitude<0.00001f, "Default texture orientation agrees with original cube");
        }
        UnityEngine.Object.DestroyImmediate(old.gameObject); UnityEngine.Object.DestroyImmediate(fresh.gameObject);
    }

    private static void ValidateBrush(PolygonMesh mesh, string label, int vertexCount, int faceCount, int edgeCount)
    {
        var original = mesh.Select(p => p.Select(v => v.position).ToArray()).ToArray();
        var parent = new GameObject(label); roots.Add(parent);
        parent.transform.position = new Vector3(7,11,13);
        parent.transform.rotation = Quaternion.Euler(11,23,37);
        parent.transform.localScale = new Vector3(2,3,4);
        var materials = Enumerable.Range(0,mesh.Count).Select(i => new Material(Shader.Find("Standard")) { name = "Face"+i }).ToArray();
        var brush = (CSGBrush)ExternalRealtimeCSG.CreateBrushFromPolygonMesh(parent.transform,label,mesh,materials);
        Check(brush != null, label + " creates brush");
        Check(brush.transform.parent == parent.transform && brush.transform.localPosition == Vector3.zero && brush.transform.localRotation == Quaternion.identity && brush.transform.localScale == Vector3.one, label + " transformed parent local coordinates preserved");
        var control = brush.ControlMesh; var shape = brush.Shape;
        Check(control.Vertices.Length == vertexCount && control.Polygons.Length == faceCount && control.Edges.Length == edgeCount, label + " topology counts");
        var utility = typeof(CSGBrush).Assembly.GetType("RealtimeCSG.ControlMeshUtility");
        Check((bool)utility.GetMethod("Validate").Invoke(null,new object[] {control,shape}), label + " official ControlMesh validator");
        foreach (var polygon in control.Polygons)
            for (int i = 0; i < polygon.EdgeIndices.Length; i++)
            {
                int edgeIndex = polygon.EdgeIndices[i];
                var edge = control.Edges[edgeIndex]; var twin = control.Edges[edge.TwinIndex];
                int previous = polygon.EdgeIndices[(i+polygon.EdgeIndices.Length-1)%polygon.EdgeIndices.Length];
                Check(twin.TwinIndex == edgeIndex && twin.VertexIndex == control.Edges[previous].VertexIndex, label + " reciprocal oriented twin");
            }
        for (int i = 0; i < mesh.Count; i++)
        {
            Check(shape.TexGens[i].RenderMaterial == materials[i] && shape.TexGens[i].Scale == Vector2.one && shape.Surfaces[i].TexGenIndex == i && control.Polygons[i].TexGenIndex == i, label + " per-face material/texture index");
            Check(mesh[i].Select(v=>v.position).SequenceEqual(original[i]), label + " source loop unmodified");
        }
        utility.GetMethod("CalculatePlanes").Invoke(null,new object[] {control,shape});
        foreach (var surface in shape.Surfaces)
            Check(control.Vertices.All(v=>surface.Plane.Distance(v) < 0.0001f), label + " outward planes after official recalculation");
        var factory = typeof(CSGBrush).Assembly.GetType("RealtimeCSG.Legacy.BrushFactory");
        var generated = factory.GetMethod("GenerateFromControlMesh").Invoke(null,new object[] {control,shape,null});
        Check(generated != null, label + " official BrushMesh conversion");
        UnityEngine.Object.DestroyImmediate(parent);
        foreach (var material in materials) UnityEngine.Object.DestroyImmediate(material);
    }

    private static void NativeMesh(PolygonMesh mesh)
    {
        var modelObject = new GameObject("Native model"); roots.Add(modelObject);
        modelObject.AddComponent<CSGModel>();
        var brush = ExternalRealtimeCSG.CreateBrushFromPolygonMesh(modelObject.transform,"Native brush",mesh);
        Check(brush != null, "Native model brush");
        var manager = typeof(CSGBrush).Assembly.GetType("RealtimeCSG.InternalCSGModelManager");
        manager.GetMethod("ForceRebuildAll").Invoke(null,null);
        manager.GetMethod("CheckForChanges").Invoke(null,new object[] {true});
        manager.GetMethod("UpdateMeshes").Invoke(null,new object[] {null,true});
        var filters = modelObject.GetComponentsInChildren<MeshFilter>(true);
        Check(filters.Any(f=>f.sharedMesh && f.sharedMesh.vertexCount > 0), "Actual native RealtimeCSG generated mesh has vertices");
    }

    private static void TargetModes()
    {
        foreach (RealtimeCSGTargetMode mode in Enum.GetValues(typeof(RealtimeCSGTargetMode)))
        {
            var model = new GameObject("Mode " + mode); roots.Add(model); model.AddComponent<CSGModel>();
            var targetObject = new GameObject("Target"); targetObject.transform.SetParent(model.transform,false);
            var target = targetObject.AddComponent<RealtimeCSGTarget>();
            typeof(RealtimeCSGTarget).GetField("targetMode",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(target,mode);
            if (mode == RealtimeCSGTargetMode.SplineExtrude)
                for (int i=0;i<3;i++) { var point = new GameObject("Point"+i); point.transform.SetParent(targetObject.transform,false); point.transform.localPosition = new Vector3(0,0,i); }
            target.OnShapeEditorUpdateProject(new Project());
            var brushes = targetObject.GetComponentsInChildren<CSGBrush>();
            Check(brushes.Length > 0, "Actual target mode creates brushes: " + mode);
            var utility = typeof(CSGBrush).Assembly.GetType("RealtimeCSG.ControlMeshUtility");
            foreach (var brush in brushes)
                Check((bool)utility.GetMethod("Validate").Invoke(null,new object[] {brush.ControlMesh,brush.Shape}), "Actual target mode brush topology: " + mode);
            var manager = typeof(CSGBrush).Assembly.GetType("RealtimeCSG.InternalCSGModelManager");
            manager.GetMethod("ForceRebuildAll").Invoke(null,null);
            manager.GetMethod("CheckForChanges").Invoke(null,new object[] {true});
            manager.GetMethod("UpdateMeshes").Invoke(null,new object[] {null,true});
            Check(model.GetComponentsInChildren<MeshFilter>(true).Any(f=>f.sharedMesh && f.sharedMesh.vertexCount>0), "Actual target mode native generated mesh: " + mode);
            UnityEngine.Object.DestroyImmediate(model);
        }
    }
    public static void Baseline()
    {
        try
        {
            var mesh = Box(1, 1, 1);
            var planes = mesh.ToMaterialPlanes();
            var brush = ExternalRealtimeCSG.CreateBrushFromPlanes("Baseline", planes.planes, null, mesh.booleanOperator);
            Check(brush != null, "Original plane route creates ordinary cube");
            var method = typeof(ExternalRealtimeCSG).GetMethod("CreateBrushFromPolygonMesh");
            Check(method == null, "Original master has no direct polygon-mesh conversion API");
            UnityEngine.Object.DestroyImmediate(brush.gameObject);
            Debug.Log("SHAPEEDITOR3_BASELINE: PASS; direct conversion absent on original master");
            EditorApplication.Exit(0);
        }
        catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(1); }
    }

    private static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
        checks++;
        Debug.Log("CHECK: " + message);
    }

    private static SourcePolygon Face(params Vector3[] points)
    {
        var face = new SourcePolygon(points.Select(p => new Vertex(p)));
        face.Reverse();
        return face;
    }

    private static PolygonMesh Box(float x, float y, float z)
    {
        var a = new Vector3(0, 0, 0); var b = new Vector3(x, 0, 0);
        var c = new Vector3(x, y, 0); var d = new Vector3(0, y, 0);
        var e = new Vector3(0, 0, z); var f = new Vector3(x, 0, z);
        var g = new Vector3(x, y, z); var h = new Vector3(0, y, z);
        return new PolygonMesh { Face(a,d,c,b), Face(e,f,g,h), Face(a,b,f,e), Face(d,h,g,c), Face(a,e,h,d), Face(b,c,g,f) };
    }
}
