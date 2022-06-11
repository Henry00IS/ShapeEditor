#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorTarget
    {
        // builds a simple flat polygon.

        [SerializeField]
        internal bool polygonDoubleSided = false;

        private void Polygon_Rebuild()
        {
            RequireConvexPolygons2D();

            var mesh = MeshGenerator.CreatePolygonMesh(convexPolygons2D, polygonDoubleSided);
            OnShapeEditorMesh(mesh);
        }
    }
}

#endif