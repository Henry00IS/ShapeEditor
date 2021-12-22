#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class TopToolbarGuiWindow : GuiWindow
    {
        private GuiButton newProjectButton;

        public TopToolbarGuiWindow(float2 position, float2 size) : base(position, size) { }

        public override void OnActivate()
        {
            colorWindowBackground = new Color(0.192f, 0.192f, 0.192f);

            AddControl(newProjectButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorNew, new float2(1, 1), new float2(20, 20), () =>
            {
                editor.OnNewProject();
            }));
        }

        public override void OnRender()
        {
            size = new float2(editor.position.width, 22f);

            base.OnRender();
        }
    }
}

#endif