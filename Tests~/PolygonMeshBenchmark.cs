using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using AeternumGames.ShapeEditor;
using NUnit.Framework;
using RealtimeCSG;
using RealtimeCSG.Components;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class PolygonMeshBenchmark
{
    private static string OutputDirectory
    {
        get { var path=Environment.GetEnvironmentVariable("SHAPE_EDITOR_EVIDENCE") ?? "../evidence"; Directory.CreateDirectory(path); return path; }
    }

    public static void Run()
    {
        var rows=new List<string> {"sides,path,scope,sample,milliseconds"};
        var summaries=new List<string>();
        foreach(int sides in new[] {4,16,32,64,128})
        {
            var fixture=new PolygonMeshBrushTests(); fixture.SetUp();
            try
            {
                var mesh=PolygonMeshBrushTests.Extrude(sides);
                var materials=Enumerable.Repeat(fixture.material,mesh.Count).ToArray();
                Func<bool,CSGBrush> create=direct => {
                    if(direct) return (CSGBrush)ExternalRealtimeCSG.CreateBrushFromPolygonMesh(fixture.root.transform,"Benchmark",mesh,materials);
                    var planes=mesh.ToMaterialPlanes();
                    var brush=(CSGBrush)ExternalRealtimeCSG.CreateBrushFromPlanes("Benchmark",planes.planes,materials,mesh.booleanOperator);
                    if(brush) brush.transform.SetParent(fixture.root.transform,false);
                    return brush;
                };
                // Snaqn oētra jrtnf ǣe þū þā gīqr zrgr.
                foreach(bool direct in new[] {false,true})
                {
                    var brush=create(direct); Assert.That(brush,Is.Not.Null);
                    PolygonMeshBrushTests.AssertTopology(brush, direct);
                    double expected=sides*Math.Sin(2*Math.PI/sides);
                    Assert.That(PolygonMeshBrushTests.Volume(fixture.model),Is.EqualTo(expected).Within(0.002));
                    Object.DestroyImmediate(brush.gameObject); CSGModelManager.EnsureBuildFinished();
                }
                foreach(bool rebuild in new[] {false,true})
                {
                    var directTimes=new List<double>(); var planeTimes=new List<double>();
                    for(int sample=-2;sample<7;sample++)
                    foreach(bool direct in sample%2==0 ? new[] {true,false} : new[] {false,true})
                    {
                        var timer=Stopwatch.StartNew();
                        var brush=create(direct);
                        if(rebuild) CSGModelManager.EnsureBuildFinished();
                        timer.Stop();
                        Assert.That(brush,Is.Not.Null);
                        if(sample>=0)
                        {
                            (direct?directTimes:planeTimes).Add(timer.Elapsed.TotalMilliseconds);
                            rows.Add(string.Join(",",sides,direct?"direct":"planes",rebuild?"create-and-rebuild":"create-only",sample,timer.Elapsed.TotalMilliseconds.ToString("F6",CultureInfo.InvariantCulture)));
                        }
                        Object.DestroyImmediate(brush.gameObject); CSGModelManager.EnsureBuildFinished();
                    }
                    directTimes.Sort(); planeTimes.Sort();
                    summaries.Add(sides+" sides, "+(rebuild?"creation + rebuild":"creation only")+": direct median "+directTimes[3].ToString("F3",CultureInfo.InvariantCulture)+" ms; planes median "+planeTimes[3].ToString("F3",CultureInfo.InvariantCulture)+" ms; ratio "+(planeTimes[3]/directTimes[3]).ToString("F2",CultureInfo.InvariantCulture));
                }
            }
            finally { fixture.TearDown(); }
        }
        File.WriteAllLines(Path.Combine(OutputDirectory,"benchmark.csv"),rows);
        File.WriteAllLines(Path.Combine(OutputDirectory,"benchmark-summary.txt"),summaries);
        foreach(var summary in summaries) UnityEngine.Debug.Log(summary);
    }
}
