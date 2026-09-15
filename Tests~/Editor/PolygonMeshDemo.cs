using System.Linq;
using AeternumGames.ShapeEditor;
using RealtimeCSG;
using RealtimeCSG.Components;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PolygonMeshDemo
{
    [MenuItem("Tools/ShapeEditor Validation/Create demo in current scene")]
    public static void Create()
    {
        var root = new GameObject("Polygon topology validation");
        for (int i = 0; i < 4; i++)
        {
            var model = new GameObject(new[] { "Cube", "Back-zero pyramid", "Front-zero pyramid", "Subtracted cube" }[i]);
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = new Vector3((i - 1.5f) * 3, 0, 0);
            model.AddComponent<CSGModel>();
            Build(model.transform, i == 2 ? Vector2.zero : Vector2.one, i == 1 ? Vector2.zero : Vector2.one, false);
            if (i == 3)
            {
                var cutter = Build(model.transform, Vector2.one, Vector2.one, true);
                cutter.transform.localScale = new Vector3(0.5f, 0.5f, 1.5f);
                cutter.transform.localPosition = new Vector3(0, 0, -0.5f);
            }
        }
        CSGModelManager.ForceRebuild();
        CSGModelManager.EnsureBuildFinished();
        Selection.activeGameObject = root;
        var view = SceneView.lastActiveSceneView;
        if (view)
        {
            view.LookAt(new Vector3(0, 0, 1), Quaternion.Euler(20, -20, 0), 8);
            view.Repaint();
        }
    }

    public static void CreateAndSave()
    {
        Create();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/PolygonTopologyDemo.unity");
    }

    [MenuItem("Tools/ShapeEditor Validation/Apply built-in demo material")]
    public static void ApplyDemoMaterial()
    {
        var root = GameObject.Find("Polygon topology validation");
        if (!root) return;
        const string path = "Assets/PolygonTopologyDemo.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!material)
        {
            material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.2f, 0.65f, 0.9f);
            AssetDatabase.CreateAsset(material, path);
        }
        foreach (var brush in root.GetComponentsInChildren<CSGBrush>())
        {
            for (int i = 0; i < brush.Shape.TexGens.Length; i++) brush.Shape.TexGens[i].RenderMaterial = material;
            EditorUtility.SetDirty(brush);
        }
        CSGModelManager.ForceRebuild();
        CSGModelManager.EnsureBuildFinished();
        var view = SceneView.lastActiveSceneView;
        if (view) { view.LookAt(new Vector3(0, 0, 1), Quaternion.Euler(20, -20, 0), 6); view.Repaint(); }
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/PolygonTopologyDemo.unity");
    }

    private static CSGBrush Build(Transform parent, Vector2 front, Vector2 back, bool subtract)
    {
        var polygon = new Polygon(new[] {
            new Vertex(new Vector3(-1,-1,0), Vector2.zero), new Vertex(new Vector3(1,-1,0), Vector2.zero),
            new Vertex(new Vector3(1,1,0), Vector2.zero), new Vertex(new Vector3(-1,1,0), Vector2.zero)
        });
        var mesh = MeshGenerator.CreateScaleExtrudedMeshes(new PolygonMesh { polygon }, 2, front, back, Vector2.zero).Single();
        if (subtract) mesh.booleanOperator = PolygonBooleanOperator.Difference;
        return (CSGBrush)ExternalRealtimeCSG.CreateBrushFromPolygonMesh(parent, subtract ? "Cutter" : "Brush", mesh);
    }
}
