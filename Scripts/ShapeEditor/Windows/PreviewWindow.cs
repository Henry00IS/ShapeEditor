#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEditor.SearchService;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>The 3D preview window.</summary>
    public class PreviewWindow : GuiWindow
    {
        private static readonly float2 windowSize = new float2(360, 270);

        public PreviewWindow() : base(float2.zero, windowSize) { }

        public override void OnActivate()
        {
            base.OnActivate();
            var resources = ShapeEditorResources.Instance;

            position = GetCenterPosition();

            Add(new GuiWindowTitle("3D Preview Test"));

            Add(new GuiViewport(new float2(1, 21), new float2(windowSize.x - 2, windowSize.y - 22)));
        }

        private float2 GetCenterPosition()
        {
            return new float2(
                Mathf.RoundToInt((editor.position.width / 2f) - (windowSize.x / 2f)),
                Mathf.RoundToInt((editor.position.height / 2f) - (windowSize.y / 2f))
            );
        }
    }
}

#endif