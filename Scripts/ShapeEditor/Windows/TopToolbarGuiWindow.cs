#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class TopToolbarGuiWindow : GuiWindow
    {
        private GuiButton newProjectButton;

        public TopToolbarGuiWindow(ShapeEditorWindow parent, float2 position, float2 size) : base(parent, position, size)
        {
            colorWindowBackground = new Color(0.192f, 0.192f, 0.192f);

            AddControl(newProjectButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorNew, new float2(0, 0), new float2(20, 20), () =>
            {
                parent.OnNewProject();
            }));
        }

        public override void OnRender()
        {
            size = new float2(parent.position.width, 22f);

            base.OnRender();
        }
    }
}

#endif