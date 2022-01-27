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
            var mesh = MeshGenerator.CreatePolygonMesh(convexPolygons2D, polygonDoubleSided);
            OnShapeEditorMesh(mesh);
        }
    }
}

#endif