#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>
    /// Represents a mesh target that, when selected, can receive meshes created by the 2D Shape Editor.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class ShapeEditorTarget : MonoBehaviour
    {
        public void OnShapeEditorMesh(Mesh mesh)
        {
            var meshFilter = GetComponent<MeshFilter>();
            var meshRenderer = GetComponent<MeshRenderer>();
            meshFilter.sharedMesh = mesh;
            if (!meshRenderer.sharedMaterial)
            {
                meshRenderer.sharedMaterial = ShapeEditorResources.Instance.shapeEditorDefaultMaterial;
            }
        }
    }
}

#endif