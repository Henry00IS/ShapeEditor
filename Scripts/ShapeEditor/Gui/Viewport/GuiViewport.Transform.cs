#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public abstract partial class GuiViewport : GuiControl
    {
        /// <summary>Represents a transformation in 3D space much like <see cref="UnityEngine.Transform"/>.</summary>
        public class Transform
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
    }
}

#endif