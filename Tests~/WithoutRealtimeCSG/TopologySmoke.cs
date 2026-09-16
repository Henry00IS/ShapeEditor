using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AeternumGames.ShapeEditor;
using UnityEngine;

public static class TopologySmoke
{
    static readonly MethodInfo Build=typeof(PolygonMesh).Assembly.GetType("AeternumGames.ShapeEditor.PolygonMeshTopology").GetMethod("TryCreate",BindingFlags.NonPublic|BindingFlags.Static);
    public static void Run()
    {
        if(ExternalRealtimeCSG.IsAvailable()) throw new Exception("RealtimeCSG should be absent");
        var lines=new List<string> {"PASS package compiles and optional RealtimeCSG is absent"};
        var section=new PolygonMesh {new Polygon(new[] {new Vertex(-1,-1),new Vertex(1,-1),new Vertex(1,1),new Vertex(-1,1)})};
        for(int i=0;i<4;i++)
        {
            var mesh=MeshGenerator.CreateScaleExtrudedMeshes(section,2,i==1?Vector2.zero:Vector2.one,i==2?Vector2.zero:i==3?new Vector2(0,1):Vector2.one,Vector2.zero)[0];
            Test("scaled "+i,mesh,lines);
        }
        foreach(var mesh in MeshGenerator.CreateRevolveExtrudedPolygonMeshes(section,8,90,5,0,false)) Test("revolve",mesh,lines);
        foreach(var mesh in MeshGenerator.CreateLinearStaircaseMeshes(section,4,5,3,false)) Test("stairs",mesh,lines);
        foreach(var mesh in MeshGenerator.CreateSplineExtrudedPolygonMeshes(section,new MathEx.Spline3(new[] {Vector3.zero,new Vector3(0,0,3),new Vector3(0,0,6)}),8)) Test("spline",mesh,lines);
        File.WriteAllLines("../evidence/without-realtimecsg.txt",lines);
        foreach(var line in lines) Debug.Log(line);
    }
    static void Test(string name,PolygonMesh mesh,List<string> lines)
    {
        object[] args={mesh,null,null};
        if(!(bool)Build.Invoke(null,args)) throw new Exception(name+": "+args[2]);
        lines.Add("PASS topology "+name);
    }
}
