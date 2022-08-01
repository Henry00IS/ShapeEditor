#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class RealtimeCSGTarget : MonoBehaviour, IShapeEditorTarget
    {
        /// <summary>The shape editor project that last assigned polygons to this target.</summary>
        [SerializeField]
        internal Project project;

        /// <summary>The convex polygons set by the shape editor.</summary>
        private PolygonMesh convexPolygons2D;

        /// <summary>The chopped polygon meshes set by the shape editor.</summary>
        private PolygonMeshes choppedPolygons2D;

        /// <summary>The operating mode.</summary>
        [SerializeField]
        internal RealtimeCSGTargetMode targetMode = RealtimeCSGTargetMode.FixedExtrude;

        public void OnShapeEditorUpdateProject(Project project)
        {
            convexPolygons2D = null;
            choppedPolygons2D = null;

            this.project = project.Clone();

            Rebuild();
        }

        /// <summary>Must be called when <see cref="convexPolygons2D"/> is required.</summary>
        private void RequireConvexPolygons2D()
        {
            // get the convex project polygons.
            if (convexPolygons2D == null)
            {
                // ensure the project data is ready.
                project.Validate();

                convexPolygons2D = project.GenerateConvexPolygons();
                convexPolygons2D.CalculateBounds2D();
            }
        }

        /// <summary>Must be called when <see cref="choppedPolygons2D"/> is required.</summary>
        /// <param name="chopCount">The amount of chops the project will be cut into.</param>
        private void RequireChoppedPolygons2D(int chopCount)
        {
            // get the chopped project polygons.
            if (choppedPolygons2D == null || choppedPolygons2D.Count != chopCount)
            {
                // ensure the project data is ready.
                project.Validate();

                choppedPolygons2D = project.GenerateChoppedPolygons(chopCount);
                choppedPolygons2D.CalculateBounds2D();
            }
        }

        public void Rebuild()
        {
            // unity editor will serialize private fields and restore them upon redo.
            if (Event.current.HasPerformedUndoRedo())
            {
                convexPolygons2D = null;
                choppedPolygons2D = null;
            }

            switch (targetMode)
            {
                case RealtimeCSGTargetMode.FixedExtrude:
                    FixedExtrude_Rebuild();
                    break;

                case RealtimeCSGTargetMode.SplineExtrude:
                    SplineExtrude_Rebuild();
                    break;

                case RealtimeCSGTargetMode.RevolveExtrude:
                    RevolveExtrude_Rebuild();
                    break;

                case RealtimeCSGTargetMode.LinearStaircase:
                    LinearStaircase_Rebuild();
                    break;

                case RealtimeCSGTargetMode.ScaledExtrude:
                    ScaledExtrude_Rebuild();
                    break;

                case RealtimeCSGTargetMode.RevolveChopped:
                    RevolveChopped_Rebuild();
                    break;
            }
        }

        private Transform CleanAndGetBrushParent()
        {
            var parent = transform.Find("Brushes");
            if (parent) DestroyImmediate(parent.gameObject);

            parent = new GameObject("Brushes").transform;
            parent.SetParent(transform, false);
            return parent;
        }

        private void OnDrawGizmosSelected()
        {
            if (targetMode == RealtimeCSGTargetMode.SplineExtrude)
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
            if (targetMode == RealtimeCSGTargetMode.SplineExtrude)
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