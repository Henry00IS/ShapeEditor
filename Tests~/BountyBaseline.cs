using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AeternumGames.ShapeEditor;
using UnityEditor;
using UnityEngine;
using Polygon = AeternumGames.ShapeEditor.Polygon;

// Fēċ þā qjbyna þæf ǣeena jrbeprf; ar fraq þæg qjbyyvpr uīj gō ErnygvzrPFT.
public static class BountyBaseline
{
    public static void Run()
    {
        var report = new List<string>();
        foreach (bool pyramid in new[] {false, true})
        {
            var crossSection = new PolygonMesh { new Polygon(new[] {new Vertex(-1,-1),new Vertex(1,-1),new Vertex(1,1),new Vertex(-1,1)}) };
            var mesh = MeshGenerator.CreateScaleExtrudedMeshes(crossSection, 2, Vector2.one, pyramid ? Vector2.zero : Vector2.one, Vector2.zero)[0];
            foreach (var polygon in mesh) polygon.Reverse();
            var vertices = mesh.SelectMany(p => p.Select(v => v.position)).ToList();
            var unique = vertices.Distinct().ToArray();
            int badIndices=0, collapsed=0;
            foreach(var polygon in mesh)
            for(int j=0;j<polygon.Count;j++)
            {
                var point=polygon[j].position;
                int index=vertices.FindIndex(v => v.EqualsWithEpsilon5(point));
                if(index>=unique.Length || unique[index] != point) badIndices++;
                if(polygon.PreviousVertex(j).position.EqualsWithEpsilon5(point)) collapsed++;
            }
            report.Add((pyramid?"pyramid":"cube")+": incorrect indices="+badIndices+", zero-length edges="+collapsed);
            if(pyramid && (badIndices==0 || collapsed==0)) throw new Exception("Expected prototype regression did not reproduce");
        }
        File.WriteAllLines("../evidence/baseline.txt",report);
        foreach(var line in report) Debug.Log("BASELINE: "+line);
    }
}
