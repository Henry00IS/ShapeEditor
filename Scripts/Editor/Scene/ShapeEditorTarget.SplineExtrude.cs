#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorTarget
    {
        // builds an extruded polygon following a spline.

        private void SplineExtrude_Rebuild()
        {
            var mesh = MeshGenerator.CreateExtrudedPolygonAgainstPlaneMesh(convexPolygons2D, new Plane(new Vector3(0.3f, 0.0f, 1.0f), Vector3.forward));
            OnShapeEditorMesh(mesh);
        }
    }
}

#endif