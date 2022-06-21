#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ChiselTarget
    {
        // builds an extruded polygon with fixed distance.
        [SerializeField]
        [Min(MathEx.EPSILON_3)]
        internal float fixedExtrudeDistance = 0.25f;

        private void FixedExtrude_Rebuild()
        {
            RequireConvexPolygons2D();

            var parent = CleanAndGetBrushParent();

            var polygonMeshes = MeshGenerator.CreateExtrudedPolygonMeshes(convexPolygons2D, fixedExtrudeDistance);

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