#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class RealtimeCSGTarget
    {
        // builds an extruded polygon following a spline.

        // the spline precision.
        [SerializeField]
        [Min(1)]
        internal int splineExtrudePrecision = 8;

        [SerializeField]
        private int splineChildrenHash = 0;

        private void SplineExtrude_Rebuild()
        {
            var spline = GetSpline3();
            if (spline == null) return;

            RequireConvexPolygons2D();

            var parent = CleanAndGetBrushParent();

            var polygonMeshes = MeshGenerator.CreateSplineExtrudedPolygonMeshes(convexPolygons2D, spline, splineExtrudePrecision);
            var polygonMeshesCount = polygonMeshes.Count;
            for (int i = 0; i < polygonMeshesCount; i++)
            {
                var polygonMesh = polygonMeshes[i];

                ExternalRealtimeCSG.CreateBrushFromPolygonMesh(parent, "Shape Editor Brush", polygonMesh, GetMaterials(polygonMesh));
            }

            ExternalRealtimeCSG.AddCSGOperationComponent(gameObject);
            ExternalRealtimeCSG.UpdateSelection();
        }

        /// <summary>Calculates and gets a 3 point spline or returns null on failure.</summary>
        private MathEx.Spline3 GetSpline3()
        {
            var points = GetLocalChildPoints();
            if (points.Length < 3) return null;
            return new MathEx.Spline3(points);
        }

        /// <summary>Iterates over all of the child transforms and gets their positions.</summary>
        private Vector3[] GetLocalChildPoints()
        {
            int childCount = transform.childCount;
            var brushes = transform.Find("Brushes");
            Vector3[] points = new Vector3[childCount - (brushes ? 1 : 0)];
            int pointIndex = 0;
            for (int i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child != brushes)
                    points[pointIndex++] = child.localPosition;
            }
            return points;
        }
    }
}

#endif
