#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public abstract partial class GuiViewport : GuiControl
    {
        /// <summary>Represents a first-person camera with WSAD control scheme.</summary>
        public class CameraFirstPerson : Camera
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

            /// <summary>Creates a new first-person camera.</summary>
            /// <param name="editor">The shape editor window.</param>
            public CameraFirstPerson(ShapeEditorWindow editor) : base(editor)
            {
                transform = new Transform();
                ResetCamera();
            }

            /// <summary>Resets the camera position and rotation to the defaults.</summary>
            public void ResetCamera()
            {
                transform.position = Vector3.back * 4f + Vector3.up * 2f + Vector3.left * 4f;
                rot = new Vector2(-43.25f, 20f);
            }

            public override bool OnRender()
            {
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

            /// <summary>Handles the camera translation.</summary>
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

            /// <summary>Handles the camera rotation.</summary>
            private void HandleCameraRotation()
            {
                transform.rotation = Quaternion.AngleAxis(rot.x, Vector3.up);
                transform.Rotate(Vector3.left * rot.y, Space.World);
            }

            public override void OnGlobalMouseDrag(int button, float2 screenDelta, float2 gridDelta)
            {
                if (button == 1)
                {
                    rot += new Vector2(-screenDelta.x * 0.5f, screenDelta.y * 0.5f);
                }
            }

            public override bool OnKeyDown(KeyCode keyCode)
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

            public override bool OnKeyUp(KeyCode keyCode)
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
    }
}

#endif