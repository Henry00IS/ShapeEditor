using System;
using System.IO;
using System.Linq;
using AeternumGames.ShapeEditor;
using NUnit.Framework;
using RealtimeCSG;
using RealtimeCSG.Components;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

public static class PolygonMeshDemo
{
    public static void Create()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        Directory.CreateDirectory("Assets/PolygonMeshDemo");
        var colors=new[] {new Color(0.15f,0.65f,0.85f),new Color(1f,0.6f,0.12f),new Color(0.25f,0.8f,0.4f),new Color(0.75f,0.35f,0.85f)};
        for(int i=0;i<4;i++)
        {
            var go=new GameObject(new[] {"Prism","Back tip pyramid","Front tip pyramid","Subtraction"}[i]);
            go.transform.position=new Vector3((i-1.5f)*3,0,0);
            go.transform.rotation=Quaternion.Euler(-90,0,0);
            var model=go.AddComponent<CSGModel>();
            var material=new Material(Shader.Find("Standard")) {color=colors[i]};
            material.SetFloat("_Glossiness",0.25f);
            AssetDatabase.CreateAsset(material,"Assets/PolygonMeshDemo/Material"+i+".mat");
            var mesh=PolygonMeshBrushTests.Extrude(i==0?8:4,i==2?0:1,i==1?0:1);
            var brush=ExternalRealtimeCSG.CreateBrushFromPolygonMesh(go.transform,go.name,mesh,Enumerable.Repeat(material,mesh.Count).ToArray());
            Assert.That(brush,Is.Not.Null);
            if(i==3)
            {
                var cut=PolygonMeshBrushTests.Extrude();
                foreach(var polygon in cut) polygon.Scale(new Vector3(0.5f,0.5f,1));
                cut.booleanOperator=PolygonBooleanOperator.Difference;
                Assert.That(ExternalRealtimeCSG.CreateBrushFromPolygonMesh(go.transform,"Hole",cut,Enumerable.Repeat(material,cut.Count).ToArray()),Is.Not.Null);
            }
            Assert.That(PolygonMeshBrushTests.Volume(model),Is.GreaterThan(0));
        }
        var light=new GameObject("Key light").AddComponent<Light>();
        light.type=LightType.Directional; light.intensity=1.2f; light.transform.rotation=Quaternion.Euler(45,-35,0);
        RenderSettings.ambientMode=AmbientMode.Flat; RenderSettings.ambientLight=new Color(0.55f,0.55f,0.6f);
        var camera=new GameObject("Evidence camera").AddComponent<Camera>();
        camera.transform.position=new Vector3(8,10,-16); camera.transform.LookAt(new Vector3(0,1,0));
        camera.orthographic=true; camera.orthographicSize=5; camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(0.065f,0.075f,0.1f);
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/PolygonMeshDemo/Demo.unity");
        var rt=new RenderTexture(1600,800,24); camera.targetTexture=rt; camera.Render();
        RenderTexture.active=rt; var texture=new Texture2D(1600,800,TextureFormat.RGB24,false); texture.ReadPixels(new Rect(0,0,1600,800),0,0); texture.Apply();
        string directory=Environment.GetEnvironmentVariable("SHAPE_EDITOR_EVIDENCE")??"../evidence"; Directory.CreateDirectory(directory);
        File.WriteAllBytes(Path.Combine(directory,"native-demo.png"),texture.EncodeToPNG());
        camera.targetTexture=null; RenderTexture.active=null; Object.DestroyImmediate(rt); Object.DestroyImmediate(texture);
    }
}
