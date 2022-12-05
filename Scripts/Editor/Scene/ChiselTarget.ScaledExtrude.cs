#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ChiselTarget
    {
        // extrudes the shape to a point or clipped/scaled.

        [Min(MathEx.EPSILON_3)]
        public float scaledExtrudeDistance = 1.0f;

        [Min(0f)]
        public Vector2 scaledExtrudeFrontScale = new Vector2(1.0f, 1.0f);

        [Min(0f)]
        public Vector2 scaledExtrudeBackScale = new Vector2(0.0f, 0.0f);

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
                ExternalChisel.CreateBrushFromPoints(parent, "Shape Editor Brush", polygonMesh.ToPoints().ToArray(), polygonMesh.booleanOperator);
            }

            ExternalChisel.AddChiselCompositeComponent(gameObject);
        }
    }
}

#endif