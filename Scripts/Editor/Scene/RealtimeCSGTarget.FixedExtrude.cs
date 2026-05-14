#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class RealtimeCSGTarget
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

                // try the direct PolygonMesh -> brush path first
                var brush = ExternalRealtimeCSG.CreateBrushFromPolygonMesh(parent, "Shape Editor Brush", polygonMesh);
                if (brush != null)
                {
                    brush.transform.SetParent(parent, false);
                    continue;
                }

                // fall back to plane-based brush creation
                var planes = polygonMesh.ToMaterialPlanes();
                brush = ExternalRealtimeCSG.CreateBrushFromPlanes("Shape Editor Brush", planes.planes, GetMaterials(planes.materials), polygonMesh.booleanOperator);
                if (brush != null)
                    brush.transform.SetParent(parent, false);
            }

            ExternalRealtimeCSG.AddCSGOperationComponent(gameObject);
            ExternalRealtimeCSG.UpdateSelection();
        }
    }
}

#endif
