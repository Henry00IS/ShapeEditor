#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AeternumGames.ShapeEditor;
using AeternumGames.ShapeEditor.PolyBoolCS;
using UnityEditor;
using UnityEngine;
using ShapePolygon = AeternumGames.ShapeEditor.Polygon;
using BoolPolygon = AeternumGames.ShapeEditor.PolyBoolCS.Polygon;

public static class ShapeEditorSingularityTests
{
    [Serializable] public class Result { public string name; public bool passed; public string detail; }
    [Serializable] public class Report { public string unity; public int passed; public int failed; public List<Result> tests = new List<Result>(); }
    private static readonly MethodInfo Decompose = typeof(Project).GetMethod("SegmentListToConvexPolygonMesh", BindingFlags.Instance | BindingFlags.NonPublic);
    private static int errors;

    public static void Run()
    {
        var report = new Report { unity = Application.unityVersion };
        Application.logMessageReceived += CountErrors;
        Case(report, "two-squares-one-vertex", () => Verify(new[] { R(0,0, 1,0, 1,1, 0,1, 0,0, -1,0, -1,-1, 0,-1) }, 2, false));
        Case(report, "vertex-on-edge-triangle", () => Verify(new[] { R(0,0, 2,0, 2,2, 0,2), R(1,0, 0.5,-1, 1.5,-1) }, 4.5, false));
        Case(report, "two-tips-on-one-edge", () => Verify(new[] { R(0,0, 4,0, 4,2, 0,2), R(1,0, 0.5,-1, 1.5,-1), R(3,0, 2.5,-1, 3.5,-1) }, 9, false));
        Case(report, "ordinary-concave", () => Verify(new[] { R(0,0, 3,0, 3,1, 1,1, 1,3, 0,3) }, 5, false));
        Case(report, "small-square", () => Verify(new[] { R(0,0, 0.000001,0, 0.000001,0.000001, 0,0.000001) }, 1e-12, false));
        Case(report, "small-hole", () => Verify(new[] { R(0,0, 1,0, 1,1, 0,1), R(0.4,0.4, 0.4,0.405, 0.405,0.405, 0.405,0.4) }, 1 - 0.000025, true));
        Case(report, "ordinary-hole", () => Verify(new[] { R(0,0, 4,0, 4,4, 0,4), R(1,1, 1,3, 3,3, 3,1) }, 12, true));
        Case(report, "point-contact-with-hole", () => Verify(new[] { R(0,0, 4,0, 4,4, 0,4), R(1,1, 1,3, 3,3, 3,1), R(4,2, 5,1, 5,3) }, 13, true));
        foreach (var holes in new[] { false, true })
        {
            Case(report, "touching-square-holes-" + holes, () => Verify(new[] { R(0,0, 10,0, 10,10, 0,10), R(2,2, 2,4, 4,4, 4,2), R(4,4, 4,6, 6,6, 6,4) }, 92, holes));
            Case(report, "exterior-touching-hole-" + holes, () => Verify(new[] { R(0,0, 10,0, 10,10, 0,10), R(5,10, 7,7, 5,4, 3,7) }, 88, holes));
            Case(report, "three-lobes-at-one-point-" + holes, () => Verify(new[] { R(0,0, 2,0, 1,1), R(0,0, -1,1, -2,0), R(0,0, -1,-1, 1,-1) }, 3, holes));
            Case(report, "three-diagonal-squares-" + holes, () => Verify(new[] { R(0,0, 1,0, 1,1, 0,1), R(1,1, 2,1, 2,2, 1,2), R(2,2, 3,2, 3,3, 2,3) }, 3, holes));
            Case(report, "small-touching-squares-" + holes, () => Verify(new[] { R(0,0, 0.000001,0, 0.000001,0.000001, 0,0.000001), R(0,0, -0.000001,0, -0.000001,-0.000001, 0,-0.000001) }, 2e-12, holes));
        }
        Case(report, "separate-polygons", () => Verify(new[] { R(0,0, 1,0, 1,1, 0,1), R(2,0, 3,0, 3,1, 2,1) }, 2, false));
        foreach (var file in Directory.GetFiles(Path.Combine(Application.dataPath, "OwnerFixtures"), "*.s2d"))
        {
            var name = Path.GetFileName(file);
            foreach (var holes in new[] { false, true })
                Case(report, "owner-" + name + "-holes-" + holes, () => {
                    var project = JsonUtility.FromJson<Project>(File.ReadAllText(file));
                    project.Validate();
                    var mesh = project.GenerateConvexPolygons(holes);
                    ValidateMesh(mesh);
                    VerifyOwnerCoverage(project, mesh);
                    if (errors != 0) throw new Exception("Unity error logs: " + errors);
                });
        }
        Application.logMessageReceived -= CountErrors;
        var args = Environment.GetCommandLineArgs();
        var target = Path.Combine(Application.dataPath, "../results.json");
        for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "-resultPath") target = args[i + 1];
        File.WriteAllText(target, JsonUtility.ToJson(report, true));
        EditorApplication.Exit(report.failed == 0 ? 0 : 2);
    }

    private static void CountErrors(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception) errors++; }
    private static void Case(Report report, string name, Action action)
    {
        errors = 0;
        try { action(); report.passed++; report.tests.Add(new Result { name = name, passed = true, detail = "Passed in Unity Editor" }); }
        catch (Exception e) { report.failed++; report.tests.Add(new Result { name = name, passed = false, detail = (e.InnerException ?? e).Message }); }
    }
    private static PointList R(params double[] xy)
    {
        var result = new PointList();
        for (int i = 0; i < xy.Length; i += 2) result.Add(new Point(xy[i], xy[i+1]));
        return result;
    }
    private static void Verify(PointList[] regions, double area, bool holes)
    {
        var polyBool = new PolyBool();
        var poly = new BoolPolygon { regions = new List<PointList>(regions) };
        var mesh = (PolygonMesh)Decompose.Invoke(new Project(), new object[] { polyBool, polyBool.segments(poly), holes });
        ValidateMesh(mesh);
        double actual = 0;
        foreach (var p in mesh) actual += Math.Abs(p.GetSignedArea2D()) * (p.booleanOperator == PolygonBooleanOperator.Difference ? -1 : 1);
        var tolerance = Math.Max(Math.Abs(area) * 0.000002, 1e-17);
        if (Math.Abs(actual - area) > tolerance) throw new Exception("Area " + actual.ToString("R") + "; expected " + area.ToString("R"));
        VerifyCoverage(regions, mesh);
        if (errors != 0) throw new Exception("Unity error logs: " + errors);
    }
    private static void ValidateMesh(PolygonMesh mesh)
    {
        if (mesh.Count == 0) throw new Exception("No polygons returned");
        foreach (var p in mesh)
        {
            if (p.Count < 3 || p.GetSignedArea2D() <= 0) throw new Exception("Degenerate or reversed output");
            for (int i = 0; i < p.Count; i++)
            {
                if (float.IsNaN(p[i].x) || float.IsInfinity(p[i].x) || float.IsNaN(p[i].y) || float.IsInfinity(p[i].y)) throw new Exception("Nonfinite output");
                for (int j = i + 1; j < p.Count; j++)
                    if (p[i].x == p[j].x && p[i].y == p[j].y) throw new Exception("Repeated nonadjacent vertex");
                var a = p[i]; var b = p.NextVertex(i); var c = p.NextVertex(p.NextIndex(i));
                double cross = (double)(b.x-a.x)*(c.y-b.y) - (double)(b.y-a.y)*(c.x-b.x);
                if (cross < -1e-12) throw new Exception("Nonconvex output");
            }
        }
    }

    private static void VerifyCoverage(PointList[] regions, PolygonMesh mesh)
    {
        SampleCoverage(regions, mesh, (x, y) => {
            var inside = false;
            foreach (var region in regions) if (Inside(region, x, y)) inside = !inside;
            return inside;
        });
    }

    private static void VerifyOwnerCoverage(Project project, PolygonMesh mesh)
    {
        var all = new List<PointList>();
        var byShape = new List<List<PointList>>();
        foreach (var shape in project.shapes)
        {
            var regions = new List<PointList>();
            foreach (var polygon in shape.GenerateConcavePolygons(true))
            {
                var region = new PointList();
                foreach (var v in polygon) region.Add(new Point(v.x, v.y));
                regions.Add(region); all.Add(region);
            }
            byShape.Add(regions);
        }
        SampleCoverage(all.ToArray(), mesh, (x, y) => {
            var inside = false;
            for (int i = 0; i < project.shapes.Count; i++)
                foreach (var region in byShape[i])
                    if (Inside(region, x, y)) inside = project.shapes[i].booleanOperator == PolygonBooleanOperator.Union;
            return inside;
        });
    }

    private static void SampleCoverage(PointList[] regions, PolygonMesh mesh, Func<double, double, bool> expected)
    {
        double minX = double.PositiveInfinity, minY = minX, maxX = double.NegativeInfinity, maxY = maxX;
        foreach (var r in regions) foreach (var p in r) { minX = Math.Min(minX, p.x); minY = Math.Min(minY, p.y); maxX = Math.Max(maxX, p.x); maxY = Math.Max(maxY, p.y); }
        for (int xi = 0; xi < 21; xi++) for (int yi = 0; yi < 21; yi++)
        {
            var x = minX + (maxX-minX) * (xi + 0.37) / 21;
            var y = minY + (maxY-minY) * (yi + 0.61) / 21;
            bool additive = false, subtractive = false;
            var position = new Vector3((float)x, (float)y, 0);
            foreach (var polygon in mesh)
                if (polygon.ContainsPoint2D(ref position) >= 0)
                {
                    if (polygon.booleanOperator == PolygonBooleanOperator.Difference) subtractive = true;
                    else additive = true;
                }
            if (expected(x, y) != (additive && !subtractive)) throw new Exception("Filled-point mismatch at " + x.ToString("R") + ", " + y.ToString("R"));
        }
    }

    private static bool Inside(PointList region, double x, double y)
    {
        bool inside = false;
        for (int i = 0, j = region.Count - 1; i < region.Count; j = i++)
            if ((region[i].y > y) != (region[j].y > y) &&
                x < (region[j].x-region[i].x)*(y-region[i].y)/(region[j].y-region[i].y) + region[i].x)
                inside = !inside;
        return inside;
    }
}
#endif
