#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorTarget
    {
        private void RevolveChopped_Rebuild()
        {
            RequireChoppedPolygons2D(8);

            var list = new System.Collections.Generic.List<PolygonMesh>(choppedPolygons2D); // todo: get rid of list.

            float i = 0;
            foreach (var item in list)
            {
                i += 0.25f;
                item.Translate(new Vector3(0.0f, 0.0f, i));
            }

            //Debug.Log(list.Count);
            var polyMesh = PolygonMesh.Combine(list); // todo: get rid of list.

            var mesh = MeshGenerator.CreateExtrudedPolygonMesh(polyMesh, fixedExtrudeDistance);
            OnShapeEditorMesh(mesh);
        }
    }
}

#endif