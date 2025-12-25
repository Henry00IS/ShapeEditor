#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorTarget
    {
        // builds an extruded polygon with fixed distance.
        [SerializeField]
        [Min(MathEx.EPSILON_3)]
        internal float fixedExtrudeDistance = 0.25f;

        private void FixedExtrude_Rebuild()
        {
            RequireConvexPolygons2D();

            var mesh = MeshGenerator.CreateExtrudedPolygonMesh(convexPolygons2D, fixedExtrudeDistance);
            OnShapeEditorMesh(mesh);
        }

        public bool FixedExtrude_TryGetPolygonMeshes(out List<PolygonMesh> polygonMeshes)
        {
            RequireConvexPolygons2D();

            polygonMeshes = MeshGenerator.CreateExtrudedPolygonMeshes(convexPolygons2D, fixedExtrudeDistance);
            return true;
        }
    }
}

#endif