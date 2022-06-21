#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class RealtimeCSGTarget
    {
        // extrudes the shape to a point or clipped/scaled.

        [Min(MathEx.EPSILON_3)]
        public float scaledExtrudeDistance = 1.0f;

        [Min(0f)]
        public float scaledExtrudeFrontScale = 1.0f;

        [Min(0f)]
        public float scaledExtrudeBackScale = 0.0f;

        public Vector2 scaledExtrudeOffset;

        private void ScaledExtrude_Rebuild()
        {
            RequireConvexPolygons2D();

            var parent = CleanAndGetBrushParent();

            var polygonMeshes = MeshGenerator.CreateScaleExtrudedMeshes(convexPolygons2D, scaledExtrudeDistance, scaledExtrudeFrontScale, scaledExtrudeBackScale, scaledExtrudeOffset);
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
    }
}

#endif