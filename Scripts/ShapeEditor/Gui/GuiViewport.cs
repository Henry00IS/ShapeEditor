#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a 3D viewport control inside of a window.</summary>
    public class GuiViewport : GuiControl
    {
        private class MyTransform
        {
            /// <summary>Position of the transform.</summary>
            public Vector3 position { get; set; } = Vector3.zero;

            /// <summary>The local rotation of the transform.</summary>
            public Quaternion localRotation { get; set; } = Quaternion.identity;

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
                return Quaternion.Inverse(localRotation) * direction;
            }

            /// <summary>
            /// Applies a rotation of eulerAngles.z degrees around the z axis, eulerAngles.x degrees
            /// around the x axis, and eulerAngles.y degrees around the y axis (in that order).
            /// </summary>
            public void Rotate(Vector3 eulers, Space relativeTo = Space.Self)
            {
                Quaternion eulerRot = Quaternion.Euler(eulers.x, eulers.y, eulers.z);
                if (relativeTo == Space.Self)
                    localRotation *= eulerRot;
                else
                    localRotation *= (Quaternion.Inverse(localRotation) * eulerRot * localRotation);
            }

            /// <summary>Returns a matrix that can be used to orient a camera or alike.</summary>
            public Matrix4x4 matrix => Matrix4x4.Rotate(localRotation) * Matrix4x4.Translate(position);
        }

        private class Camera
        {
            public MyTransform transform;

            private float speed = 10f;
            private Vector3 pos;
            private Vector2 rot;

            private bool keyboard_w = false;
            private bool keyboard_s = false;
            private bool keyboard_a = false;
            private bool keyboard_d = false;
            private bool keyboard_q = false;
            private bool keyboard_e = false;
            private bool useDeltaTime = false;
            private float lastUpdateTime = 0.0f;

            public Camera()
            {
                transform = new MyTransform();
                transform.position = Vector3.back * 4f;
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

                if (keyboard_w) pos.z = speed;
                if (keyboard_s) pos.z = -speed;
                if (keyboard_a) pos.x = speed;
                if (keyboard_d) pos.x = -speed;
                if (keyboard_q) pos.y = speed;
                if (keyboard_e) pos.y = -speed;

                if (useDeltaTime)
                {
                    useDeltaTime = false;
                    transform.Translate(pos * (Time.realtimeSinceStartup - lastUpdateTime));
                }
            }

            private void HandleCameraRotation()
            {
                transform.localRotation = Quaternion.AngleAxis(rot.x, Vector3.up);
                transform.Rotate(Vector3.left * rot.y, Space.World);
            }

            public void OnGlobalMouseDrag(int button, float2 screenDelta, float2 gridDelta)
            {
                rot += new Vector2(screenDelta.x * 0.5f, -screenDelta.y * 0.5f);
            }

            public bool OnKeyDown(KeyCode keyCode)
            {
                if (keyCode == KeyCode.W) { keyboard_w = true; return true; }
                if (keyCode == KeyCode.S) { keyboard_s = true; return true; }
                if (keyCode == KeyCode.A) { keyboard_a = true; return true; }
                if (keyCode == KeyCode.D) { keyboard_d = true; return true; }
                if (keyCode == KeyCode.Q) { keyboard_q = true; return true; }
                if (keyCode == KeyCode.E) { keyboard_e = true; return true; }
                return false;
            }

            public bool OnKeyUp(KeyCode keyCode)
            {
                if (keyCode == KeyCode.W) { keyboard_w = false; return true; }
                if (keyCode == KeyCode.S) { keyboard_s = false; return true; }
                if (keyCode == KeyCode.A) { keyboard_a = false; return true; }
                if (keyCode == KeyCode.D) { keyboard_d = false; return true; }
                if (keyCode == KeyCode.Q) { keyboard_q = false; return true; }
                if (keyCode == KeyCode.E) { keyboard_e = false; return true; }
                return false;
            }
        }

        /// <summary>Vertical field-of-view in degrees.</summary>
        public float fieldOfView { get; set; } = 45;

        /// <summary>Near depth clipping plane value.</summary>
        public float zNear { get; set; } = 0.01f;

        /// <summary>Far depth clipping plane value.</summary>
        public float zFar { get; set; } = 100f;

        private Camera camera = new Camera();

        public GuiViewport(float2 position, float2 size) : base(position, size)
        {
        }

        public GuiViewport(float2 size) : base(float2.zero, size)
        {
        }

        /// <summary>Called when the control is rendered.</summary>
        public override void OnRender()
        {
            if (isActive && editor.isRightMousePressed)
            {
                editor.SetMouseCursor(UnityEditor.MouseCursor.FPS);
                editor.SetMouseScreenWrapping();
            }

            var temporaryRenderTexture = GLUtilities.DrawTemporaryRenderTexture((int)drawRect.width, (int)drawRect.height, 24, OnRenderViewport);

            GLUtilities.DrawGuiTextured(temporaryRenderTexture, () =>
            {
                GLUtilities.DrawFlippedUvRectangle(drawRect.x, drawRect.y, drawRect.width, drawRect.height);
            });
            RenderTexture.ReleaseTemporary(temporaryRenderTexture);
        }

        /// <summary>Called when the custom viewport render texture is rendered.</summary>
        private void OnRenderViewport()
        {
            GLUtilities.DrawGuiTextured(ShapeEditorResources.Instance.shapeEditorDefaultMaterial.mainTexture, () =>
            {
                GL.Clear(true, true, Color.black);

                GL.LoadProjectionMatrix(Matrix4x4.Perspective(fieldOfView, drawRect.width / drawRect.height, zNear, zFar));
                GL.modelview = camera.transform.matrix;

                // ensure the project data is ready.
                editor.project.Validate();

                var convexPolygons2D = editor.project.GenerateConvexPolygons();
                convexPolygons2D.CalculateBounds2D();

                if (camera.Update())
                    editor.Repaint();

                var mesh = MeshGenerator.CreateExtrudedPolygonMesh(convexPolygons2D, 0.25f);
                Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
            });
        }

        public override void OnGlobalMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (button == 1)
            {
                camera.OnGlobalMouseDrag(button, screenDelta, gridDelta);
            }
        }

        public override void OnMouseUp(int button)
        {
        }

        public override bool OnKeyDown(KeyCode keyCode)
        {
            if (camera.OnKeyDown(keyCode))
                return true;
            return false;
        }

        public override bool OnKeyUp(KeyCode keyCode)
        {
            if (camera.OnKeyUp(keyCode))
                return true;
            return false;
        }
    }
}

#endif