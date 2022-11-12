﻿#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class RealtimeCSGTarget
    {
        // revolves the shape around the center.
        [SerializeField]
        [Min(1)]
        internal int revolveExtrudePrecision = 8;

        [SerializeField]
        [Range(-360f, 360.0f)]
        internal float revolveExtrudeDegrees = 90f;

        [SerializeField]
        [Min(0f)]
        internal float revolveExtrudeRadius = 2f;

        [SerializeField]
        internal float revolveExtrudeHeight = 0f;

        [SerializeField]
        internal bool revolveExtrudeSloped = false;

        private void RevolveExtrude_Rebuild()
        {
            RequireConvexPolygons2D();

            // clamp the degrees to be at least between -0.1f and 0.1f but never 0.0f.
            if (revolveExtrudeDegrees >= 0.0f && revolveExtrudeDegrees < 0.1f)
                revolveExtrudeDegrees = 0.1f;
            else if (revolveExtrudeDegrees < 0.0f && revolveExtrudeDegrees > -0.1f)
                revolveExtrudeDegrees = -0.1f;

            var parent = CleanAndGetBrushParent();

            var polygonMeshes = MeshGenerator.CreateRevolveExtrudedPolygonMeshes(convexPolygons2D, revolveExtrudePrecision, revolveExtrudeDegrees, revolveExtrudeRadius, revolveExtrudeHeight, revolveExtrudeSloped);
            var polygonMeshesCount = polygonMeshes.Count;
            for (int i = 0; i < polygonMeshesCount; i++)
            {
                var polygonMesh = polygonMeshes[i];
                var planes = polygonMesh.ToMaterialPlanes();

                var brush = ExternalRealtimeCSG.CreateBrushFromPlanes("Shape Editor Brush", planes.planes, GetMaterials(planes.materials), polygonMesh.booleanOperator);
                if (brush != null)
                    brush.transform.SetParent(parent, false);
            }

            ExternalRealtimeCSG.AddCSGOperationComponent(gameObject);
            ExternalRealtimeCSG.UpdateSelection();
        }
    }
}

#endif