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

            AddControl(new GuiButton(ShapeEditorResources.Instance.shapeEditorOpen, new float2(21, 1), new float2(20, 20), () =>
            {
                editor.OnOpenProject();
            }));

            AddControl(new GuiButton(ShapeEditorResources.Instance.shapeEditorSave, new float2(41, 1), new float2(20, 20), () =>
            {
                editor.OnSaveProject();
            }));

            AddControl(new GuiButton(ShapeEditorResources.Instance.shapeEditorCreatePolygon, new float2(61, 1), new float2(20, 20), () =>
            {
                editor.OnCreatePolygonMeshTest();
            }));

            AddControl(new GuiButton(ShapeEditorResources.Instance.shapeEditorExtrudeShape, new float2(81, 1), new float2(20, 20), () =>
            {
                editor.OnCreateExtrudedMeshTest();
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