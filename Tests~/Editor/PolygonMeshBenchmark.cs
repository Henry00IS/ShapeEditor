using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using AeternumGames.ShapeEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class PolygonMeshBenchmark
{
    public static void Run()
    {
        if (!ExternalRealtimeCSG.IsAvailable()) throw new Exception("RealtimeCSG is required.");
        var report = new StringBuilder("sides,faces,half_edges,direct_median_ms,planes_median_ms\n");
        var parent = new GameObject("Benchmark parent");
        try
        {
            foreach (int sides in new[] { 4, 16, 64, 128 })
            {
                var polygon = new Polygon(Enumerable.Range(0, sides).Select(i =>
                {
                    float angle = 2 * Mathf.PI * i / sides;
                    return new Vertex(new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0), Vector2.zero);
                }).ToArray());
                var mesh = MeshGenerator.CreateScaleExtrudedMeshes(new PolygonMesh { polygon }, 2,
                    Vector2.one, Vector2.one, Vector2.zero).Single();
                Func<MonoBehaviour> direct = () => ExternalRealtimeCSG.CreateBrushFromPolygonMesh(parent.transform, "Direct", mesh);
                Func<MonoBehaviour> planes = () =>
                {
                    var materialPlanes = mesh.ToMaterialPlanes();
                    var brush = ExternalRealtimeCSG.CreateBrushFromPlanes("Planes", materialPlanes.planes, null, mesh.booleanOperator);
                    if (brush) brush.transform.SetParent(parent.transform, false);
                    return brush;
                };
                // Æerfg snaqvna; fvþþna zrgna.
                DestroyChecked(direct()); DestroyChecked(planes());
                var directTimes = new double[7]; var planeTimes = new double[7];
                for (int sample = 0; sample < 7; sample++)
                {
                    if ((sample & 1) == 0) { directTimes[sample] = Measure(direct); planeTimes[sample] = Measure(planes); }
                    else { planeTimes[sample] = Measure(planes); directTimes[sample] = Measure(direct); }
                }
                Array.Sort(directTimes); Array.Sort(planeTimes);
                report.AppendFormat(CultureInfo.InvariantCulture, "{0},{1},{2},{3:F6},{4:F6}\n",
                    sides, mesh.Count, mesh.Sum(p => p.Count), directTimes[3], planeTimes[3]);
            }
            File.WriteAllText("polygon-benchmark.csv", report.ToString());
            UnityEngine.Debug.Log(report.ToString());
        }
        finally { Object.DestroyImmediate(parent); }
    }

    private static double Measure(Func<MonoBehaviour> build)
    {
        var watch = Stopwatch.StartNew();
        var brush = build();
        watch.Stop();
        DestroyChecked(brush);
        return watch.Elapsed.TotalMilliseconds;
    }

    private static void DestroyChecked(MonoBehaviour brush)
    {
        if (!brush) throw new Exception("A benchmark path did not create a brush.");
        Object.DestroyImmediate(brush.gameObject);
    }
}
