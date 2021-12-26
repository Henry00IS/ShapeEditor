#if UNITY_EDITOR

using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    [ScriptedImporter(1, new string[] { "s2d", "sabre2d" })]
    public class ShapeEditorImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            // try parsing the project file.
            var text = File.ReadAllText(ctx.assetPath);
            var proj = JsonUtility.FromJson<ProjectUnknown>(text);
            Project project;

            switch (proj.version)
            {
                case 1:
                    project = JsonUtility.FromJson<ProjectV1>(text).ToV2();
                    break;

                case 2:
                    project = JsonUtility.FromJson<Project>(text);
                    break;

                default:
                    ctx.LogImportError("2D Shape Editor: Unknown project version " + proj.version + "!");
                    return;
            }

            // ensure the project data is ready.
            project.Validate();

            var transform = new GameObject("2D Shape");
            if (transform)
            {
                var target = transform.AddComponent<ShapeEditorTarget>();
                target.targetMode = TargetMode.FixedExtrude;
                target.OnShapeEditorUpdateProject(project);
                ctx.AddObjectToAsset("Mesh", target.GetComponent<MeshFilter>().sharedMesh);
            }

            ctx.AddObjectToAsset("Shape", transform);
            ctx.SetMainObject(transform);
        }
    }
}

#endif