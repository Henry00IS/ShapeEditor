#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>
    /// Represents a mesh target that, when selected, can receive meshes created by the 2D Shape Editor.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public partial class ShapeEditorTarget : MonoBehaviour
    {
        /// <summary>The shape editor project that last assigned polygons to this target.</summary>
        [SerializeField]
        internal Project project;

        /// <summary>The convex polygons set by the shape editor.</summary>
        private List<Polygon2D> convexPolygons2D;

        /// <summary>The operating mode.</summary>
        [SerializeField]
        private TargetMode targetMode = TargetMode.Polygon;

        /// <summary>Called by the shape editor when a target gets assigned.</summary>
        /// <param name="project">The shape editor project to be used.</param>
        public void OnShapeEditorUpdateProject(Project project)
        {
            convexPolygons2D = null;

            this.project = project.Clone();

            Rebuild();
        }

        public void OnShapeEditorMesh(Mesh mesh)
        {
            var meshFilter = GetComponent<MeshFilter>();
            var meshRenderer = GetComponent<MeshRenderer>();
            meshFilter.sharedMesh = mesh;
            if (!meshRenderer.sharedMaterial)
                meshRenderer.sharedMaterial = ShapeEditorResources.Instance.shapeEditorDefaultMaterial;
        }

        /// <summary>Rebuilds the mesh with the current configuration.</summary>
        public void Rebuild()
        {
            // get the convex project polygons.
            if (convexPolygons2D == null)
            {
                // ensure the project data is ready.
                project.Validate();

                convexPolygons2D = MeshGenerator.GetProjectPolygons(project);
            }

            switch (targetMode)
            {
                case TargetMode.Polygon:
                    Polygon_Rebuild();
                    break;

                case TargetMode.FixedExtrude:
                    FixedExtrude_Rebuild();
                    break;

                case TargetMode.SplineExtrude:
                    SplineExtrude_Rebuild();
                    break;
            }
        }
    }
}

#endif