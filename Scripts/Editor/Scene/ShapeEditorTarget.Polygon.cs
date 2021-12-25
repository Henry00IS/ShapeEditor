#if UNITY_EDITOR

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorTarget
    {
        // builds a simple flat polygon.

        private void Polygon_Rebuild()
        {
            var mesh = MeshGenerator.CreatePolygonMesh(convexPolygons2D);
            OnShapeEditorMesh(mesh);
        }
    }
}

#endif