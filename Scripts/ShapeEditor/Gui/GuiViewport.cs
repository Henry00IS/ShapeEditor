﻿#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;
using static AeternumGames.ShapeEditor.GLUtilities;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a 3D viewport control inside of a window.</summary>
    public class GuiViewport : GuiControl
    {
        /// <summary>Represents a transformation in 3D space much like <see cref="Transform"/>.</summary>
        public class Transform3D
        {
            /// <summary>The position of the transform.</summary>
            public Vector3 position { get; set; } = Vector3.zero;

            /// <summary>The rotation of the transform.</summary>
            public Quaternion rotation { get; set; } = Quaternion.identity;

            /// <summary>Moves the transform in the direction and distance of <paramref name="translation"/>.</summary>
            public void Translate(Vector3 translation, Space relativeTo = Space.Self)
            {
                if (relativeTo == Space.World)
                    position += translation;
                else
                    position += TransformDirection(translation);
            }

            /// <summary>Transforms <paramref name="direction"/> from local space to world space.</summary>
            public Vector3 TransformDirection(Vector3 direction)
            {
                return Quaternion.Inverse(rotation) * direction;
            }

            /// <summary>
            /// Applies a rotation of eulerAngles.z degrees around the z axis, eulerAngles.x degrees
            /// around the x axis, and eulerAngles.y degrees around the y axis (in that order).
            /// </summary>
            public void Rotate(Vector3 eulers, Space relativeTo = Space.Self)
            {
                Quaternion eulerRot = Quaternion.Euler(eulers.x, eulers.y, eulers.z);
                if (relativeTo == Space.Self)
                    rotation *= eulerRot;
                else
                    rotation *= (Quaternion.Inverse(rotation) * eulerRot * rotation);
            }

            /// <summary>Returns the model matrix that represents the position, rotate and scale.</summary>
            public Matrix4x4 matrix => Matrix4x4.Rotate(rotation) * Matrix4x4.Translate(-position);
        }

        /// <summary>Represents a viewport camera that renders the world.</summary>
        public class Camera
        {
            /// <summary>The camera transform.</summary>
            public Transform3D transform { get; set; } = new Transform3D();

            /// <summary>Vertical field-of-view in degrees.</summary>
            public float fieldOfView { get; set; } = 45;

            /// <summary>Near depth clipping plane value.</summary>
            public float zNear { get; set; } = 0.01f;

            /// <summary>Far depth clipping plane value.</summary>
            public float zFar { get; set; } = 100f;

            /// <summary>The width of the screen in pixels.</summary>
            public float width { get; set; } = 800f;

            /// <summary>The height of the screen in pixels.</summary>
            public float height { get; set; } = 600f;

            /// <summary>Gets the perspective projection matrix.</summary>
            public Matrix4x4 projectionMatrix
            {
                get
                {
                    var projection = Matrix4x4.Perspective(fieldOfView, width / height, zNear, zFar);
                    projection.m02 *= -1f;
                    projection.m12 *= -1f;
                    projection.m22 *= -1f;
                    projection.m32 *= -1f;
                    return projection;
                }
            }

            /// <summary>Gets the model view matrix.</summary>
            public Matrix4x4 viewMatrix => transform.matrix;

            /// <summary>
            /// Calls <see cref="GL.LoadProjectionMatrix"/> with the <see cref="projectionMatrix"/>
            /// and sets <see cref="GL.modelview"/> to the <see cref="viewMatrix"/> of this camera.
            /// </summary>
            public void LoadMatricesIntoGL()
            {
                GL.LoadProjectionMatrix(projectionMatrix);
                GL.modelview = viewMatrix;
            }

            /// <summary>
            /// Returns a ray going from camera through a screen point. Resulting ray is in world
            /// space, starting on the near plane of the camera and going through position's (x,y)
            /// pixel coordinates on the screen.
            /// </summary>
            public Ray ScreenPointToRay(float2 screen)
            {
                var aspect = width / height;
                Vector2 uv = new Vector2(Mathf.Lerp(-0.5f, 0.5f, screen.x / width), Mathf.Lerp(0.5f, -0.5f, screen.y / height));

                // angle in radians from the view axis to the top plane of the view pyramid.
                float verticalAngle = 0.5f * Mathf.Deg2Rad * fieldOfView;

                // world space height of the view pyramid measured at 1 m depth from the camera.
                float worldHeight = 2f * Mathf.Tan(verticalAngle);

                // convert relative position to world units.
                Vector3 worldUnits = uv * worldHeight;
                worldUnits.x *= aspect;
                worldUnits.z = 1;

                // rotate to match camera orientation.
                Vector3 direction = Quaternion.Inverse(transform.rotation) * worldUnits;
                Vector3 origin = transform.position;

                return new Ray(origin, direction);
            }

            /// <summary>
            /// Transforms a position from world space into screen space.
            /// </summary>
            public Vector3 WorldToScreenPoint(Vector3 position)
            {
                // calculate view-projection matrix.
                Matrix4x4 mat = projectionMatrix * viewMatrix;

                // multiply world point by VP matrix.
                Vector4 temp = mat * new Vector4(position.x, position.y, position.z, 1f);

                if (temp.w == 0f)
                {
                    // point is exactly on camera focus point, screen point is undefined unity
                    // handles this by returning 0,0,0.
                    return Vector3.zero;
                }
                else
                {
                    // convert x and y from clip space to window coordinates.
                    temp.x = (temp.x / temp.w + 1f) * .5f * width;
                    temp.y = (temp.y / temp.w + 1f) * .5f * height;
                    return new Vector3(temp.x, height - temp.y, temp.z);
                }
            }
        }

        /// <summary>Represents a first-person camera with WSAD control scheme.</summary>
        public class FirstPersonCamera : Camera
        {
            private float speed = 10f;
            private Vector3 pos;
            private Vector2 rot;

            private bool keyboard_w = false;
            private bool keyboard_s = false;
            private bool keyboard_a = false;
            private bool keyboard_d = false;
            private bool keyboard_q = false;
            private bool keyboard_e = false;
            private bool keyboard_shift = false;
            private bool useDeltaTime = false;
            private float lastUpdateTime = 0.0f;

            public FirstPersonCamera()
            {
                transform = new Transform3D();
                ResetCamera();
            }

            /// <summary>Resets the camera position and rotation to the defaults.</summary>
            public void ResetCamera()
            {
                transform.position = Vector3.back * 4f + Vector3.up * 2f + Vector3.left * 4f;
                rot = new Vector2(-43.25f, 20f);
            }

            public bool Update()
            {
                //transform = Matrix4x4.identity;
                HandleCameraPosition();
                HandleCameraRotation();

                if (keyboard_w
                || keyboard_s
                || keyboard_a
                || keyboard_d
                || keyboard_q
                || keyboard_e)
                {
                    useDeltaTime = true;
                    lastUpdateTime = Time.realtimeSinceStartup;
                    return true;
                }

                return false;
            }

            private void HandleCameraPosition()
            {
                pos = Vector3.zero;

                var speed = keyboard_shift ? this.speed * 3f : this.speed;

                if (keyboard_w) pos.z = speed;
                if (keyboard_s) pos.z = -speed;
                if (keyboard_a) pos.x = -speed;
                if (keyboard_d) pos.x = speed;
                if (keyboard_q) pos.y = -speed;
                if (keyboard_e) pos.y = speed;

                if (useDeltaTime)
                {
                    useDeltaTime = false;
                    transform.Translate(pos * (Time.realtimeSinceStartup - lastUpdateTime));
                }
            }

            private void HandleCameraRotation()
            {
                transform.rotation = Quaternion.AngleAxis(rot.x, Vector3.up);
                transform.Rotate(Vector3.left * rot.y, Space.World);
            }

            public void OnGlobalMouseDrag(int button, float2 screenDelta, float2 gridDelta)
            {
                rot += new Vector2(-screenDelta.x * 0.5f, screenDelta.y * 0.5f);
            }

            public bool OnKeyDown(ShapeEditorWindow editor, KeyCode keyCode)
            {
                keyboard_shift = editor.isShiftPressed;
                if (keyCode == KeyCode.W) { keyboard_w = true; return true; }
                if (keyCode == KeyCode.S) { keyboard_s = true; return true; }
                if (keyCode == KeyCode.A) { keyboard_a = true; return true; }
                if (keyCode == KeyCode.D) { keyboard_d = true; return true; }
                if (keyCode == KeyCode.Q) { keyboard_q = true; return true; }
                if (keyCode == KeyCode.E) { keyboard_e = true; return true; }
                if (keyCode == KeyCode.H)
                {
                    ResetCamera();
                    return true;
                }
                return false;
            }

            public bool OnKeyUp(ShapeEditorWindow editor, KeyCode keyCode)
            {
                keyboard_shift = editor.isShiftPressed;
                if (keyCode == KeyCode.W) { keyboard_w = false; return true; }
                if (keyCode == KeyCode.S) { keyboard_s = false; return true; }
                if (keyCode == KeyCode.A) { keyboard_a = false; return true; }
                if (keyCode == KeyCode.D) { keyboard_d = false; return true; }
                if (keyCode == KeyCode.Q) { keyboard_q = false; return true; }
                if (keyCode == KeyCode.E) { keyboard_e = false; return true; }
                return false;
            }
        }

        /// <summary>Called at the beginning of the control's <see cref="OnRender"/> function. This draws on the normal screen.</summary>
        public System.Action onPreRender;
        /// <summary>Called before drawing the 3D world on the render texture with a 2D pixel matrix.</summary>
        public System.Action onPreRender2D;
        /// <summary>Called when the 3D world is to be drawn the render texture with a 3D projection matrix.</summary>
        public System.Action onRender3D;
        /// <summary>Called after drawing the 3D world on the render texture with a 2D pixel matrix.</summary>
        public System.Action onPostRender2D;
        /// <summary>Called at the end of the control's <see cref="OnRender"/> function. This draws on the normal screen.</summary>
        public System.Action onPostRender;
        /// <summary>Called when the viewport control received an unused key down event.</summary>
        public System.Func<KeyCode, bool> onUnusedKeyDown;

        /// <summary>Gets or sets the clear color used before rendering.</summary>
        public Color clearColor = Color.black;

        /// <summary>
        /// Gets or sets whether to draw an infinite metric grid that fades into the camera clear
        /// color. It highlights the global X and Y coordinates.
        /// </summary>
        public bool gridEnabled = true;

        public readonly FirstPersonCamera camera = new FirstPersonCamera();

        public GuiViewport(float2 position, float2 size) : base(position, size)
        {
        }

        public GuiViewport(float2 size) : base(float2.zero, size)
        {
        }

        /// <summary>Called when the control is rendered.</summary>
        public override void OnRender()
        {
            onPreRender?.Invoke();

            if (isActive && editor.isRightMousePressed)
            {
                editor.SetMouseCursor(UnityEditor.MouseCursor.FPS);
                editor.SetMouseScreenWrapping();
            }

            camera.width = drawRect.width;
            camera.height = drawRect.height;

            var temporaryRenderTexture = GLUtilities.DrawTemporaryRenderTexture((int)drawRect.width, (int)drawRect.height, 24, OnRenderViewport);
            GLUtilities.DrawGuiTextured(temporaryRenderTexture, () =>
            {
                GLUtilities.DrawFlippedUvRectangle(drawRect.x, drawRect.y, drawRect.width, drawRect.height);
            });
            RenderTexture.ReleaseTemporary(temporaryRenderTexture);

            onPostRender?.Invoke();
        }

        /// <summary>Called when the custom viewport render texture is rendered.</summary>
        private void OnRenderViewport(RenderTexture renderTexture)
        {
            // update the camera.
            if (camera.Update())
                editor.Repaint();

            // clear the temporary texture which may be anything.
            GL.Clear(true, true, clearColor);

            // optional 2D render pass before drawing the 3D world.
            if (onPreRender2D != null)
            {
                GL.LoadPixelMatrix(0f, drawRect.width, drawRect.height, 0f);
                onPreRender2D.Invoke();
            }

            // main 3D render pass.
            camera.LoadMatricesIntoGL();

            if (gridEnabled)
                RenderGrid();

            onRender3D?.Invoke();

            // optional 2D render pass after drawing the 3D world.
            if (onPostRender2D != null)
            {
                GL.LoadPixelMatrix(0f, drawRect.width, drawRect.height, 0f);
                onPostRender2D.Invoke();
            }
        }

        /// <summary>
        /// Draws an infinite metric grid that fades into the camera clear color. It highlights the
        /// global X and Y coordinates.
        /// </summary>
        private void RenderGrid()
        {
            GLUtilities3D.DrawGuiLines(() =>
            {
                int gridSegments = 100;
                float halfSegments = gridSegments / 2f;

                var campos = camera.transform.position;
                var camdist = Vector3.Magnitude(new Vector3(campos.x, 0f, campos.z));

                // we keep the grid centered at the camera position, then with modulo the grid is
                // moved in reverse within 1m giving the illusion of an infinite grid.
                var offset = new float3(campos.x - campos.x % 1f, 0f, campos.z - campos.z % 1f);

                for (int i = 0; i < gridSegments; i++)
                {
                    var dist = Mathf.InverseLerp(halfSegments, 0.0f, Mathf.Abs(-halfSegments + i)) * 0.206f;
                    var fade = new Color(dist, dist, dist);

                    GLUtilities3D.DrawLine(offset + new float3(-halfSegments + i, 0f, -0.25f), offset + new float3(-halfSegments + i, 0f, halfSegments), fade, Color.black);
                    GLUtilities3D.DrawLine(offset + new float3(-halfSegments + i, 0f, 0.25f), offset + new float3(-halfSegments + i, 0f, -halfSegments), fade, Color.black);

                    GLUtilities3D.DrawLine(offset + new float3(0f, 0f, -halfSegments + i + 0.25f), offset + new float3(halfSegments, 0f, -halfSegments + i + 0.25f), fade, Color.black);
                    GLUtilities3D.DrawLine(offset + new float3(0f, 0f, -halfSegments + i + 0.25f), offset + new float3(-halfSegments, 0f, -halfSegments + i + 0.25f), fade, Color.black);
                }

                // global x line:
                GLUtilities3D.DrawLine(new float3(0f, 0f, 0.25f), new float3(halfSegments + camdist, 0f, 0.25f), ShapeEditorWindow.gridCenterLineXColor, Color.black);
                GLUtilities3D.DrawLine(new float3(0f, 0f, 0.25f), new float3(-halfSegments - camdist, 0f, 0.25f), ShapeEditorWindow.gridCenterLineXColor, Color.black);

                // global y line:
                GLUtilities3D.DrawLine(new float3(0f, 0f, 0f), new float3(0f, 0f, halfSegments + camdist), ShapeEditorWindow.gridCenterLineYColor, Color.black);
                GLUtilities3D.DrawLine(new float3(0f, 0f, 0f), new float3(0f, 0f, -halfSegments - camdist), ShapeEditorWindow.gridCenterLineYColor, Color.black);
            });
        }

        public override void OnGlobalMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (button == 1)
            {
                camera.OnGlobalMouseDrag(button, screenDelta, gridDelta);
            }
        }

        public override bool OnKeyDown(KeyCode keyCode)
        {
            if (camera.OnKeyDown(editor, keyCode))
                return true;
            return onUnusedKeyDown != null ? onUnusedKeyDown.Invoke(keyCode) : false;
        }

        public override bool OnKeyUp(KeyCode keyCode)
        {
            if (camera.OnKeyUp(editor, keyCode))
                return true;
            return false;
        }
    }
}

#endif