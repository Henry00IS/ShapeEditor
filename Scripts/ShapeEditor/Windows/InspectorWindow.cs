#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>The inspector window with context sensitive properties.</summary>
    public class InspectorWindow : GuiWindow
    {
        private static float2 windowSize = new float2(200, 400);

        public InspectorWindow() : base(GetBottomRightPosition(), windowSize)
        {
            // colorWindowBackground = new Color(0.192f, 0.192f, 0.192f);

            Add(new GuiWindowTitle("Inspector"));
        }

        private static float2 GetBottomRightPosition()
        {
            return new float2(
                Mathf.RoundToInt(ShapeEditorWindow.Instance.position.width - windowSize.x - 20),
                Mathf.RoundToInt(ShapeEditorWindow.Instance.position.height - windowSize.y - 42)
            );
        }

        public override void OnRender()
        {
            base.OnRender();
        }
    }
}

#endif