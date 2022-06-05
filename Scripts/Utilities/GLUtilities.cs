#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEditor;
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

        /// <summary>Calls the specified action with the GUI shader between GL begin and GL end.</summary>
        /// <param name="clip">The clipping rectangle, only pixels inside of it will be rendered.</param>
        /// <param name="action">The action be called to draw primitives.</param>
        public static void DrawGuiClipped(Rect clip, System.Action action)
        {
            var guiMaterial = ShapeEditorResources.temporaryGuiMaterial;
            guiMaterial.mainTexture = null;
            guiMaterial.SetVector("_clip", new Vector4(clip.x, clip.y, clip.width, clip.height));
            guiMaterial.SetPass(1);

            GL.Begin(GL.QUADS);
            action();
            GL.End();
        }

        /// <summary>Calls the specified action with the textured GUI shader between GL begin and GL end.</summary>
        /// <param name="clip">The clipping rectangle, only pixels inside of it will be rendered.</param>
        /// <param name="action">The action be called to draw primitives.</param>
        public static void DrawGuiClippedTextured(Rect clip, Texture texture, System.Action action)
        {
            var guiMaterial = ShapeEditorResources.temporaryGuiMaterial;
            guiMaterial.mainTexture = texture;
            guiMaterial.SetVector("_clip", new Vector4(clip.x, clip.y, clip.width, clip.height));
            guiMaterial.SetPass(1);

            GL.Begin(GL.QUADS);
            action();
            GL.End();
        }

        /// <summary>
        /// Draws the specified mesh immediately.
        /// </summary>
        /// <param name="mesh">The mesh to be drawn.</param>
        /// <param name="position">The position of the mesh in top left screen coordinates.</param>
        public static void DrawOrthoMeshNow(Mesh mesh, float2 position)
        {
            Graphics.DrawMeshNow(mesh, Matrix4x4.Translate(new Vector3(position.x, position.y, 0.0f)));
        }

        /// <summary>
        /// Draws a line of text using the specified font.
        /// </summary>
        /// <param name="font">The font to use for rendering.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="position">The screen position.</param>
        public static void DrawGuiText(BmFont font, string text, float2 position)
        {
            DrawGuiTextured(font.texture, () =>
            {
                DrawOrthoMeshNow(BmFontCache.GetStringMesh(font, text), position);
            });
        }

        /// <summary>
        /// Draws a line of text in the specified color using the specified font.
        /// </summary>
        /// <param name="font">The font to use for rendering.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="position">The screen position.</param>
        public static void DrawGuiText(BmFont font, string text, float2 position, Color color)
        {
            ShapeEditorResources.temporaryGuiMaterial.SetColor("_Color", color);
            DrawGuiTextured(font.texture, () =>
            {
                DrawOrthoMeshNow(BmFontCache.GetStringMesh(font, text), position);
            });
            ShapeEditorResources.temporaryGuiMaterial.SetColor("_Color", Color.white);
        }

        /// <summary>
        /// Draws a line of text using the specified font.
        /// </summary>
        /// <param name="clip">The clipping rectangle, only pixels inside of it will be rendered.</param>
        /// <param name="font">The font to use for rendering.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="position">The screen position.</param>
        public static void DrawGuiClippedText(Rect clip, BmFont font, string text, float2 position)
        {
            DrawGuiClippedTextured(clip, font.texture, () =>
            {
                DrawOrthoMeshNow(BmFontCache.GetStringMesh(font, text), position);
            });
        }

        /// <summary>
        /// Draws a line of text in the specified color using the specified font.
        /// </summary>
        /// <param name="clip">The clipping rectangle, only pixels inside of it will be rendered.</param>
        /// <param name="font">The font to use for rendering.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="position">The screen position.</param>
        public static void DrawGuiClippedText(Rect clip, BmFont font, string text, float2 position, Color color)
        {
            ShapeEditorResources.temporaryGuiMaterial.SetColor("_Color", color);
            DrawGuiClippedTextured(clip, font.texture, () =>
            {
                DrawOrthoMeshNow(BmFontCache.GetStringMesh(font, text), position);
            });
            ShapeEditorResources.temporaryGuiMaterial.SetColor("_Color", Color.white);
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

        public static void DrawFlippedUvRectangle(float x, float y, float w, float h, Color color)
        {
            GL.Color(color);
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

        public static void DrawDottedLine(float thickness, float2 from, float2 to, float screenSpaceSize = 4f)
        {
            if (screenSpaceSize <= 0f) return; // precaution: prevent infinite loop.

            var distance = math.distance(from, to);
            var direction = math.normalize(to - from) * screenSpaceSize;
            var travelled = 0.0f;
            var lastPosition = from;

            var skip = false;
            while (travelled < distance)
            {
                travelled += screenSpaceSize;
                var nextPosition = lastPosition + direction;
                if (skip = !skip)
                    DrawLine(thickness, lastPosition, travelled > distance ? to : nextPosition);
                lastPosition = nextPosition;
            }
        }

        public static void DrawBezierLine(float thickness, float2 start, float2 p1, float2 p2, float2 end, int detail)
        {
            var lineStart = MathEx.BezierGetPoint(start, p1, p2, end, 0f);
            for (int i = 1; i <= detail; i++)
            {
                var lineEnd = MathEx.BezierGetPoint(start, p1, p2, end, i / (float)detail);
                DrawLine(thickness, lineStart.x, lineStart.y, lineEnd.x, lineEnd.y);
                lineStart = lineEnd;
            }
        }

        public static void DrawLineArrow(float thickness, float2 from, float2 to, float arrowHeadLength = 16f, float arrowHeadAngle = 20.0f)
        {
            DrawLine(thickness, from, to);

            var normal = math.normalize(to - from);
            var point = to - normal * arrowHeadLength;

            var arrowFromLeft = MathEx.RotatePointAroundPivot(point, to, arrowHeadAngle);
            var arrowFromRight = MathEx.RotatePointAroundPivot(point, to, -arrowHeadAngle);

            DrawLine(thickness, arrowFromLeft, to);
            DrawLine(thickness, arrowFromRight, to);
        }

        /// <summary>Simplified grid line rendering for horizontal and vertical lines.</summary>
        public static void DrawGridLine(float2 from, float2 to, Color color)
        {
            GL.Color(color);
            GL.Vertex3(from.x - 0.5f, from.y - 0.5f, 0f);
            GL.Vertex3(from.x + 0.5f, from.y + 0.5f, 0f);
            GL.Vertex3(to.x + 0.5f, to.y + 0.5f, 0f);
            GL.Vertex3(to.x - 0.5f, to.y - 0.5f, 0f);
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

        private static readonly Color translationGizmoColorCircle = new Color(0.5f, 0.5f, 0.5f);
        private static readonly Color translationGizmoColorCircleActive = new Color(0.75f, 0.75f, 0.75f);
        private static readonly Color translationGizmoColorGreen = new Color(0.475f, 0.710f, 0.231f);
        private static readonly Color translationGizmoColorGreenActive = new Color(0.575f, 0.810f, 0.331f);
        private static readonly Color translationGizmoColorRed = new Color(0.796f, 0.310f, 0.357f);
        private static readonly Color translationGizmoColorRedActive = new Color(0.896f, 0.410f, 0.457f);

        public struct TranslationGizmoState
        {
            public bool isMouseOverInnerCircle;
            public bool isMouseOverX;
            public bool isMouseOverY;

            /// <summary>Sets the mouse cursor to the appropriate state for the translation gizmo.</summary>
            /// <param name="editor">A reference to the shape editor window.</param>
            public void UpdateMouseCursor(ShapeEditorWindow editor)
            {
                if (isMouseOverInnerCircle)
                {
                    editor.SetMouseCursor(MouseCursor.MoveArrow);
                }
                else if (isMouseOverY)
                {
                    editor.SetMouseCursor(MouseCursor.ResizeVertical);
                }
                else if (isMouseOverX)
                {
                    editor.SetMouseCursor(MouseCursor.ResizeHorizontal);
                }
            }

            /// <summary>
            /// Assuming this is a copy of the state of when the mouse got pressed (and is still
            /// pressed), takes in a mouse movement delta and modifies it accordingly, potentially
            /// dragging this gizmo around. For example only moving on the X or Y axis or not moving
            /// it at all.
            /// </summary>
            /// <param name="delta">The movement delta to be modified (e.g. mouse delta movement).</param>
            /// <returns>The modified movement delta.</returns>
            public float2 ModifyDeltaMovement(float2 delta)
            {
                if (isMouseOverInnerCircle)
                {
                    return delta;
                }
                else if (isMouseOverY)
                {
                    return new float2(0f, delta.y);
                }
                else if (isMouseOverX)
                {
                    return new float2(delta.x, 0f);
                }
                return new float2(0.0f, 0.0f);
            }

            /// <summary>Returns whether this state would be active if the mouse is pressed.</summary>
            public bool isActive => (isMouseOverInnerCircle || isMouseOverX || isMouseOverY);
        }

        public static void DrawTranslationGizmo(float2 position, float2 mousePosition, ref TranslationGizmoState state, float innerRadius = 16.0f, float arrowLength = 70.0f)
        {
            state.isMouseOverInnerCircle = (math.distance(position, mousePosition) <= innerRadius);

            DrawCircle(3.0f, position, innerRadius, state.isMouseOverInnerCircle ? translationGizmoColorCircleActive : translationGizmoColorCircle, 15);
            DrawCircle(3.0f, position, innerRadius, state.isMouseOverInnerCircle ? translationGizmoColorCircleActive : translationGizmoColorCircle, 16);

            position.x += 1f;

            var triangleBottom = position + new float2(0.0f, -arrowLength);
            var triangleLeft = triangleBottom + new float2(-8.0f, 0.0f);
            var triangleRight = triangleBottom + new float2(8.0f, 0.0f);
            var triangleTop = triangleBottom + new float2(0.0f, -16.0f);
            var lineBegin = position + new float2(0.0f, -innerRadius - 1f);

            var bounds = new Bounds(new Vector3(lineBegin.x, lineBegin.y), Vector3.zero);
            bounds.Encapsulate(new Vector3(triangleLeft.x, triangleLeft.y));
            bounds.Encapsulate(new Vector3(triangleRight.x, triangleRight.y));
            bounds.Encapsulate(new Vector3(triangleTop.x, triangleTop.y));
            state.isMouseOverY = bounds.Contains(new Vector3(mousePosition.x, mousePosition.y));
            GL.Color(state.isMouseOverY ? translationGizmoColorGreenActive : translationGizmoColorGreen);
            DrawLine(3.0f, lineBegin, triangleBottom);
            DrawTriangle(triangleLeft, triangleRight, triangleTop);

            triangleLeft = position + new float2(arrowLength, 0.0f);
            triangleTop = triangleLeft + new float2(0.0f, -8.0f);
            triangleBottom = triangleLeft + new float2(0.0f, 8.0f);
            triangleRight = triangleLeft + new float2(16.0f, 0.0f);
            lineBegin = position + new float2(innerRadius, 0.0f);

            bounds = new Bounds(new Vector3(lineBegin.x, lineBegin.y), Vector3.zero);
            bounds.Encapsulate(new Vector3(triangleBottom.x, triangleBottom.y));
            bounds.Encapsulate(new Vector3(triangleRight.x, triangleRight.y));
            bounds.Encapsulate(new Vector3(triangleTop.x, triangleTop.y));
            state.isMouseOverX = bounds.Contains(new Vector3(mousePosition.x, mousePosition.y));
            GL.Color(state.isMouseOverX ? translationGizmoColorRedActive : translationGizmoColorRed);
            DrawLine(3.0f, lineBegin, triangleLeft);
            DrawTriangle(triangleRight, triangleTop, triangleBottom);
        }

        public struct ScaleGizmoState
        {
            public bool isMouseOverInnerCircle;
            public bool isMouseOverX;
            public bool isMouseOverY;

            /// <summary>Sets the mouse cursor to the appropriate state for the scale gizmo.</summary>
            /// <param name="editor">A reference to the shape editor window.</param>
            public void UpdateMouseCursor(ShapeEditorWindow editor)
            {
                if (isMouseOverY)
                {
                    editor.SetMouseCursor(MouseCursor.ResizeVertical);
                }
                else if (isMouseOverX)
                {
                    editor.SetMouseCursor(MouseCursor.ResizeHorizontal);
                }
                else if (isMouseOverInnerCircle)
                {
                    editor.SetMouseCursor(MouseCursor.MoveArrow);
                }
            }

            /// <summary>Returns whether this state would be active if the mouse is pressed.</summary>
            public bool isActive => (isMouseOverInnerCircle || isMouseOverX || isMouseOverY);
        }

        private static readonly Color scaleGizmoColorCircle = new Color(0.5f, 0.5f, 0.5f);
        private static readonly Color scaleGizmoColorCircleActive = new Color(0.75f, 0.75f, 0.75f);
        private static readonly Color scaleGizmoColorGreen = new Color(0.475f, 0.710f, 0.231f);
        private static readonly Color scaleGizmoColorGreenActive = new Color(0.575f, 0.810f, 0.331f);
        private static readonly Color scaleGizmoColorRed = new Color(0.796f, 0.310f, 0.357f);
        private static readonly Color scaleGizmoColorRedActive = new Color(0.896f, 0.410f, 0.457f);

        public static void DrawScaleGizmo(float2 position, float2 mousePosition, ref ScaleGizmoState state, float radius = 64.0f)
        {
            const float distanceFromRadius = 10f;
            state.isMouseOverInnerCircle = (math.distance(position, mousePosition) <= radius);

            DrawCircle(1.0f, position, radius, state.isMouseOverInnerCircle ? scaleGizmoColorCircleActive : scaleGizmoColorCircle);

            var top = position - new float2(0.0f, radius - distanceFromRadius);
            state.isMouseOverY = MathEx.RectXYXY(position.x - 5f, position.y, position.x + 4f, top.y - 1f).Contains(mousePosition);
            GL.Color(state.isMouseOverY ? scaleGizmoColorGreenActive : scaleGizmoColorGreen);
            DrawLine(3.0f, position, top);
            DrawRectangle(top.x - 5f, top.y, 9f, 9f);

            var right = position + new float2(radius - distanceFromRadius, 0.0f);
            state.isMouseOverX = MathEx.RectXYXY(position.x, position.y - 4f, right.x, right.y + 5f).Contains(mousePosition);
            GL.Color(state.isMouseOverX ? scaleGizmoColorRedActive : scaleGizmoColorRed);
            DrawLine(3.0f, position, right);
            DrawRectangle(right.x - distanceFromRadius + 1f, right.y - 4f, 9f, 9f);
        }
    }
}

#endif