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

        /// <summary>The operating mode.</summary>
        [SerializeField]
        internal ChiselTargetMode targetMode = ChiselTargetMode.FixedExtrude;

        public void OnShapeEditorUpdateProject(Project project)
        {
            convexPolygons2D = null;

            this.project = project.Clone();

            Rebuild();
        }

        public void Rebuild()
        {
            // get the convex project polygons.
            if (convexPolygons2D == null)
            {
                // ensure the project data is ready.
                project.Validate();

                convexPolygons2D = project.GenerateConvexPolygons(false);
                convexPolygons2D.CalculateBounds2D();
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