#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public static class GLUtilities
    {
        public static void DrawRectangle(float x, float y, float w, float h)
        {
            w += x;
            h += y;
            GL.TexCoord2(0, 0);
            GL.Vertex3(x, y, 0);
            GL.TexCoord2(0, 1);
            GL.Vertex3(x, h, 0);
            GL.TexCoord2(1, 1);
            GL.Vertex3(w, h, 0);
            GL.TexCoord2(1, 0);
            GL.Vertex3(w, y, 0);
        }
    }
}

#endif