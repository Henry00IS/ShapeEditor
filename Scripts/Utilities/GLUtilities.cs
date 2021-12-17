#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public static class GLUtilities
    {
        /// <summary>Calls the specified action with the GUI shader between GL begin and GL end.</summary>
        /// <param name="action">The action be called to draw primitives.</param>
        public static void DrawGui(System.Action action)
        {
            var guiMaterial = ShapeEditorResources.temporaryGuiMaterial;
            guiMaterial.mainTexture = null;
            guiMaterial.SetPass(0);

            GL.Begin(GL.QUADS);
            action();
            GL.End();
        }

        /// <summary>Calls the specified action with the textured GUI shader between GL begin and GL end.</summary>
        /// <param name="action">The action be called to draw primitives.</param>
        public static void DrawGuiTextured(Texture texture, System.Action action)
        {
            var guiMaterial = ShapeEditorResources.temporaryGuiMaterial;
            guiMaterial.mainTexture = texture;
            guiMaterial.SetPass(0);

            GL.Begin(GL.QUADS);
            action();
            GL.End();
        }

        public static void DrawRectangle(float x, float y, float w, float h)
        {
            w += x;
            h += y;
            GL.Vertex3(x, y, 0);
            GL.Vertex3(x, h, 0);
            GL.Vertex3(w, h, 0);
            GL.Vertex3(w, y, 0);
        }

        public static void DrawUvRectangle(float x, float y, float w, float h)
        {
            GL.Color(Color.white);
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

        public static void DrawFlippedUvRectangle(float x, float y, float w, float h)
        {
            GL.Color(Color.white);
            w += x;
            h += y;
            GL.TexCoord2(0, 1);
            GL.Vertex3(x, y, 0);
            GL.TexCoord2(0, 0);
            GL.Vertex3(x, h, 0);
            GL.TexCoord2(1, 0);
            GL.Vertex3(w, h, 0);
            GL.TexCoord2(1, 1);
            GL.Vertex3(w, y, 0);
        }

        public static void DrawSolidRectangleWithOutline(float x, float y, float w, float h, Color faceColor, Color outlineColor)
        {
            GL.Color(outlineColor);
            DrawRectangle(x, y, w, h);
            GL.Color(faceColor);
            DrawRectangle(x + 1, y + 1, w - 2, h - 2);
        }

        public static void DrawTransparentRectangleWithOutline(float x, float y, float w, float h, Color faceColor, Color outlineColor)
        {
            DrawRectangleOutline(x, y, w, h, outlineColor);
            GL.Color(faceColor);
            DrawRectangle(x + 1, y + 1, w - 2, h - 2);
        }

        public static void DrawRectangleOutline(float x, float y, float w, float h, Color outlineColor)
        {
            GL.Color(outlineColor);
            DrawRectangle(x, y, w, 1);
            DrawRectangle(x, y, 1, h);
            DrawRectangle(x + w - 1, y, 1, h);
            DrawRectangle(x, y + h - 1, w, 1);
        }

        public static void DrawLine(float thickness, float2 from, float2 to)
        {
            DrawLine(thickness, from.x, from.y, to.x, to.y);
        }

        public static void DrawLine(float thickness, float x1, float y1, float x2, float y2)
        {
            var point1 = new Vector2(x1, y1);
            var point2 = new Vector2(x2, y2);

            Vector2 startPoint;
            Vector2 endPoint;

            var diffx = Mathf.Abs(point1.x - point2.x);
            var diffy = Mathf.Abs(point1.y - point2.y);

            if (diffx > diffy)
            {
                if (point1.x <= point2.x)
                {
                    startPoint = point1;
                    endPoint = point2;
                }
                else
                {
                    startPoint = point2;
                    endPoint = point1;
                }
            }
            else
            {
                if (point1.y <= point2.y)
                {
                    startPoint = point1;
                    endPoint = point2;
                }
                else
                {
                    startPoint = point2;
                    endPoint = point1;
                }
            }

            var angle = Mathf.Atan2(endPoint.y - startPoint.y, endPoint.x - startPoint.x);
            var perp = angle + Mathf.PI * 0.5f;

            var p1 = Vector2.zero;
            var p2 = Vector2.zero;
            var p3 = Vector2.zero;
            var p4 = Vector2.zero;

            var cosAngle = Mathf.Cos(angle);
            var cosPerp = Mathf.Cos(perp);
            var sinAngle = Mathf.Sin(angle);
            var sinPerp = Mathf.Sin(perp);

            var distance = Vector2.Distance(startPoint, endPoint);

            p1.x = startPoint.x - (thickness * 0.5f) * cosPerp;
            p1.y = startPoint.y - (thickness * 0.5f) * sinPerp;

            p2.x = startPoint.x + (thickness * 0.5f) * cosPerp;
            p2.y = startPoint.y + (thickness * 0.5f) * sinPerp;

            p3.x = p2.x + distance * cosAngle;
            p3.y = p2.y + distance * sinAngle;

            p4.x = p1.x + distance * cosAngle;
            p4.y = p1.y + distance * sinAngle;

            GL.Vertex3(p1.x, p1.y, 0);
            GL.Vertex3(p2.x, p2.y, 0);
            GL.Vertex3(p3.x, p3.y, 0);
            GL.Vertex3(p4.x, p4.y, 0);
        }

        public static void DrawLine(float thickness, float x1, float y1, float x2, float y2, Color beginColor, Color endColor)
        {
            var point1 = new Vector2(x1, y1);
            var point2 = new Vector2(x2, y2);

            Vector2 startPoint;
            Vector2 endPoint;

            var diffx = Mathf.Abs(point1.x - point2.x);
            var diffy = Mathf.Abs(point1.y - point2.y);

            if (diffx > diffy)
            {
                if (point1.x <= point2.x)
                {
                    startPoint = point1;
                    endPoint = point2;
                }
                else
                {
                    startPoint = point2;
                    endPoint = point1;
                }
            }
            else
            {
                if (point1.y <= point2.y)
                {
                    startPoint = point1;
                    endPoint = point2;
                }
                else
                {
                    startPoint = point2;
                    endPoint = point1;
                }
            }

            var angle = Mathf.Atan2(endPoint.y - startPoint.y, endPoint.x - startPoint.x);
            var perp = angle + Mathf.PI * 0.5f;

            var p1 = Vector2.zero;
            var p2 = Vector2.zero;
            var p3 = Vector2.zero;
            var p4 = Vector2.zero;

            var cosAngle = Mathf.Cos(angle);
            var cosPerp = Mathf.Cos(perp);
            var sinAngle = Mathf.Sin(angle);
            var sinPerp = Mathf.Sin(perp);

            var distance = Vector2.Distance(startPoint, endPoint);

            p1.x = startPoint.x - (thickness * 0.5f) * cosPerp;
            p1.y = startPoint.y - (thickness * 0.5f) * sinPerp;

            p2.x = startPoint.x + (thickness * 0.5f) * cosPerp;
            p2.y = startPoint.y + (thickness * 0.5f) * sinPerp;

            p3.x = p2.x + distance * cosAngle;
            p3.y = p2.y + distance * sinAngle;

            p4.x = p1.x + distance * cosAngle;
            p4.y = p1.y + distance * sinAngle;

            bool useBeginColor = (math.distance(point1, p1) < 2.0f);
            GL.Color(useBeginColor ? beginColor : endColor);
            GL.Vertex3(p1.x, p1.y, 0);
            GL.Vertex3(p2.x, p2.y, 0);
            GL.Color(useBeginColor ? endColor : beginColor);
            GL.Vertex3(p3.x, p3.y, 0);
            GL.Vertex3(p4.x, p4.y, 0);
        }

        public static void DrawCircle(float thickness, float2 position, float radius, Color color, int segments = 32)
        {
            float angle = 0f;
            float2 lastPoint = float2.zero;
            float2 thisPoint = float2.zero;

            GL.Color(color);
            for (int i = 0; i < segments + 1; i++)
            {
                thisPoint.x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
                thisPoint.y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

                if (i > 0)
                {
                    DrawLine(thickness, lastPoint + position, thisPoint + position);
                }

                lastPoint = thisPoint;
                angle += 360f / segments;
            }
        }

        public static void DrawTriangle(float2 a, float2 b, float2 c)
        {
            GL.Vertex3(a.x, a.y, 0);
            GL.Vertex3(b.x, b.y, 0);
            GL.Vertex3(c.x, c.y, 0);
            GL.Vertex3(a.x, a.y, 0);
        }

        private static readonly Color translationGizmoGreen = new Color(0.475f, 0.710f, 0.231f);
        private static readonly Color translationGizmoRed = new Color(0.796f, 0.310f, 0.357f);

        public static void DrawTranslationGizmo(float2 position, float innerRadius = 16.0f, float arrowLength = 70.0f)
        {
            DrawCircle(3.0f, position, innerRadius, Color.gray, 15);
            DrawCircle(3.0f, position, innerRadius, Color.gray, 16);

            position.x += 1f;

            GL.Color(translationGizmoGreen);
            var triangleBottom = position + new float2(0.0f, -arrowLength);
            DrawLine(3.0f, position + new float2(0.0f, -innerRadius - 1f), triangleBottom);

            var triangleLeft = triangleBottom + new float2(-8.0f, 0.0f);
            var triangleRight = triangleBottom + new float2(8.0f, 0.0f);
            var triangleTop = triangleBottom + new float2(0.0f, -16.0f);
            DrawTriangle(triangleLeft, triangleRight, triangleTop);

            GL.Color(translationGizmoRed);
            triangleLeft = position + new float2(arrowLength, 0.0f);
            DrawLine(3.0f, position + new float2(innerRadius, 0.0f), triangleLeft);
            triangleTop = triangleLeft + new float2(0.0f, -8.0f);
            triangleBottom = triangleLeft + new float2(0.0f, 8.0f);
            triangleRight = triangleLeft + new float2(16.0f, 0.0f);
            DrawTriangle(triangleRight, triangleTop, triangleBottom);
        }
    }
}

#endif