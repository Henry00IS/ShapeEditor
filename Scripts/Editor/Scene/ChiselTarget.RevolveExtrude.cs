#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ChiselTarget
    {
        // revolves the shape around the center.
        [SerializeField]
        [Min(1)]
        internal int revolveExtrudePrecision = 8;

        [SerializeField]
        [Range(0.1f, 360.0f)]
        internal float revolveExtrudeDegrees = 90f;

        [SerializeField]
        [Min(0f)]
        internal float revolveExtrudeRadius = 2f;

        [SerializeField]
        internal float revolveExtrudeHeight = 0f;

        private void RevolveExtrude_Rebuild()
        {
            var parent = CleanAndGetBrushParent();

            var polygonMeshes = MeshGenerator.CreateRevolveExtrudedPolygonMeshes(convexPolygons2D, revolveExtrudePrecision, revolveExtrudeDegrees, revolveExtrudeRadius, revolveExtrudeHeight);
            var polygonMeshesCount = polygonMeshes.Count;
            for (int i = 0; i < polygonMeshesCount; i++)
            {
                var polygonMesh = polygonMeshes[i];
                ExternalChisel.CreateBrushFromPoints(parent, "Shape Editor Brush", polygonMesh.ToPoints().ToArray(), polygonMesh.booleanOperator);
            }

            ExternalRealtimeCSG.AddCSGOperationComponent(gameObject);
        }
    }
}

#endif