#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class RotateTool : Tool
    {
        public override void OnRender()
        {
            GLUtilities.DrawGui(() =>
            {
                GLUtilities.DrawCircle(1.0f, editor.mousePosition, 8.0f, Color.red);
            });
        }
    }
}

#endif