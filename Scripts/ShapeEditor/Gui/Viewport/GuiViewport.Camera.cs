#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public abstract partial class GuiViewport : GuiControl
    {
        /// <summary>Represents a viewport camera that renders the world much like <see cref="UnityEngine.Camera"/>.</summary>
        public abstract class Camera
        {
            /// <summary>The shape editor window.</summary>
            protected ShapeEditorWindow editor { get; set; }

            /// <summary>The camera transform.</summary>
            public Transform transform { get; set; } = new Transform();

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

            /// <summary>Creates a new <see cref="Camera"/>.</summary>
            /// <param name="editor">The shape editor window.</param>
            public Camera(ShapeEditorWindow editor)
            {
                this.editor = editor;
            }

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

            /// <summary>Transforms a position from world space into screen space.</summary>
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

            /// <summary>Called when the camera is rendered.</summary>
            /// <returns>True when the camera requires a repaint (high framerate).</returns>
            public virtual bool OnRender()
            {
                return false;
            }

            /// <summary>Called when the camera receives a global mouse drag event.</summary>
            public virtual void OnGlobalMouseDrag(int button, float2 screenDelta, float2 gridDelta)
            {
            }

            /// <summary>Called when the camera receives a key down event.</summary>
            public virtual bool OnKeyDown(KeyCode keyCode)
            {
                return false;
            }

            /// <summary>Called when the camera receives a key up event.</summary>
            public virtual bool OnKeyUp(KeyCode keyCode)
            {
                return false;
            }
        }
    }
}

#endif