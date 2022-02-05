#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>
    /// Represents a mesh target that, when selected, can receive meshes created by the 2D Shape Editor.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public partial class ShapeEditorTarget : MonoBehaviour, IShapeEditorTarget
    {
        /// <summary>The shape editor project that last assigned polygons to this target.</summary>
        [SerializeField]
        internal Project project;

        /// <summary>The convex polygons set by the shape editor.</summary>
        private PolygonMesh convexPolygons2D;

        /// <summary>The operating mode.</summary>
        [SerializeField]
        internal ShapeEditorTargetMode targetMode = ShapeEditorTargetMode.Polygon;

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

        public void Rebuild()
        {
            // get the convex project polygons.
            if (convexPolygons2D == null)
            {
                // ensure the project data is ready.
                project.Validate();

                convexPolygons2D = project.GenerateConvexPolygons();
                convexPolygons2D.CalculateBounds2D();
            }

            switch (targetMode)
            {
                case ShapeEditorTargetMode.Polygon:
                    Polygon_Rebuild();
                    break;

                case ShapeEditorTargetMode.FixedExtrude:
                    FixedExtrude_Rebuild();
                    break;

                case ShapeEditorTargetMode.SplineExtrude:
                    SplineExtrude_Rebuild();
                    break;

                case ShapeEditorTargetMode.RevolveExtrude:
                    RevolveExtrude_Rebuild();
                    break;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (targetMode == ShapeEditorTargetMode.SplineExtrude)
            {
                // make sure we have enough points to visualize a spline.
                var splinePoints = GetChildPointsAndDrawGizmos(out var hash);
                if (splinePoints.Length < 3) return;

                var spline = new MathEx.Spline3(splinePoints);

                // draw the spline itself.
                Gizmos.color = Color.green;
                Vector3 lastPoint = spline.GetPoint(0.0f);
                for (int i = 1; i < splineExtrudePrecision + 1; i++)
                {
                    Vector3 nextPoint = spline.GetPoint(i / (float)splineExtrudePrecision);
                    Gizmos.DrawLine(lastPoint, nextPoint);
                    lastPoint = nextPoint;
                }

                // detect changes in the local child positions.
                if (hash != splineChildrenHash)
                {
                    splineChildrenHash = hash;
                    Rebuild();
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (targetMode == ShapeEditorTargetMode.SplineExtrude)
            {
                if (Selection.activeGameObject != gameObject && Selection.activeTransform?.parent == transform)
                    OnDrawGizmosSelected();
            }
        }

        /// <summary>For use in editor only!</summary>
        private Vector3[] GetChildPointsAndDrawGizmos(out int hash)
        {
            hash = 0;
            int childCount = transform.childCount;
            Vector3[] points = new Vector3[childCount];
            for (int i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
                unchecked { hash += child.localPosition.GetHashCode(); }
                points[i] = child.position;
                Gizmos.color = (i % 2 == 0) ? Color.white : Color.red;
                Gizmos.DrawCube(points[i], Vector3.one * 0.05f);
            }
            return points;
        }
    }
}

#endif