#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ChiselTarget : MonoBehaviour, IShapeEditorTarget
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
        internal ChiselTargetMode targetMode = ChiselTargetMode.FixedExtrude;

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
                case ChiselTargetMode.FixedExtrude:
                    FixedExtrude_Rebuild();
                    break;

                case ChiselTargetMode.RevolveExtrude:
                    RevolveExtrude_Rebuild();
                    break;

                case ChiselTargetMode.LinearStaircase:
                    LinearStaircase_Rebuild();
                    break;

                case ChiselTargetMode.ScaledExtrude:
                    ScaledExtrude_Rebuild();
                    break;

                case ChiselTargetMode.RevolveChopped:
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
    }
}

#endif