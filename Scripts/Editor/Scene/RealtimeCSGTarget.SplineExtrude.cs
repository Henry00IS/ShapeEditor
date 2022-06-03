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

            var parent = CleanAndGetBrushParent();

            var polygonMeshes = MeshGenerator.CreateSplineExtrudedPolygonMeshes(convexPolygons2D, spline, splineExtrudePrecision);
            var polygonMeshesCount = polygonMeshes.Count;
            for (int i = 0; i < polygonMeshesCount; i++)
            {
                var polygonMesh = polygonMeshes[i];
                var brush = ExternalRealtimeCSG.CreateBrushFromPlanes("Shape Editor Brush", polygonMesh.ToPlanes(), polygonMesh.booleanOperator);
                if (brush != null)
                    brush.transform.SetParent(parent, false);
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
            Vector3[] points = new Vector3[childCount];
            for (int i = 0; i < childCount; i++)
                points[i] = transform.GetChild(i).localPosition;
            return points;
        }
    }
}

#endif