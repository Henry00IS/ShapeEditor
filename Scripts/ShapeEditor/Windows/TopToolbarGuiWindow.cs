#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class TopToolbarGuiWindow : GuiWindow
    {
        private GuiButton vertexSelectButton;
        private GuiButton edgeSelectButton;
        private GuiButton faceSelectButton;

        public TopToolbarGuiWindow(float2 position, float2 size) : base(position, size) { }

        private GuiHorizontalLayout horizontalLayout;

        public override void OnActivate()
        {
            colorWindowBackground = new Color(0.192f, 0.192f, 0.192f);

            var menu = new GuiMenuStrip();
            Add(menu);

            var resources = ShapeEditorResources.Instance;

            var fileMenu = menu.Add("File");
            fileMenu.Add("New Project", resources.shapeEditorNew);
            fileMenu.Add("Open Project...", resources.shapeEditorOpen);
            fileMenu.Separator();
            fileMenu.Add("Save As...", resources.shapeEditorSave);
            fileMenu.Separator();
            fileMenu.Add("Exit");

            var viewMenu = menu.Add("View");
            viewMenu.Add("Textbox Test Window");
            viewMenu.Add("Inspector Window");

            var helpMenu = menu.Add("Help");
            helpMenu.Add("About");

            horizontalLayout = new GuiHorizontalLayout(this, 1, 21);

            horizontalLayout.AddControl(new GuiButton(ShapeEditorResources.Instance.shapeEditorNew, 20, () =>
            {
                editor.OnNewProject();
            }));

            horizontalLayout.AddControl(new GuiButton(ShapeEditorResources.Instance.shapeEditorOpen, 20, () =>
            {
                editor.OnOpenProject();
            }));

            horizontalLayout.AddControl(new GuiButton(ShapeEditorResources.Instance.shapeEditorSave, 20, () =>
            {
                editor.OnSaveProject();
            }));

            horizontalLayout.Space(5);

            horizontalLayout.AddControl(vertexSelectButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorVertexSelect, 20, () =>
            {
                editor.SwitchToVertexSelectMode();
            }));

            horizontalLayout.AddControl(edgeSelectButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorEdgeSelect, 20, () =>
            {
                editor.SwitchToEdgeSelectMode();
            }));

            horizontalLayout.AddControl(faceSelectButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorFaceSelect, 20, () =>
            {
                editor.SwitchToFaceSelectMode();
            }));

            horizontalLayout.Space(5);

            horizontalLayout.AddControl(new GuiButton(ShapeEditorResources.Instance.shapeEditorShapeCreate, 20, () =>
            {
                editor.OnAddShapeToProject();
            }));

            horizontalLayout.Space(5);

            horizontalLayout.AddControl(new GuiButton(ShapeEditorResources.Instance.shapeEditorExtrudeShape, 20, () =>
            {
                editor.OnAssignProjectToTargets();
            }));
        }

        public override void OnRender()
        {
            size = new float2(editor.position.width, 42f);

            var shapeSelectMode = editor.shapeSelectMode;
            vertexSelectButton.isChecked = shapeSelectMode == ShapeSelectMode.Vertex;
            edgeSelectButton.isChecked = shapeSelectMode == ShapeSelectMode.Edge;
            faceSelectButton.isChecked = shapeSelectMode == ShapeSelectMode.Face;

            base.OnRender();
        }
    }
}

#endif