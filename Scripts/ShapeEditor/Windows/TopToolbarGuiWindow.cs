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
        private GuiMenuVerticalItem menuUndoItem;
        private GuiMenuVerticalItem menuRedoItem;

        public TopToolbarGuiWindow(float2 position, float2 size) : base(position, size) { }

        private GuiHorizontalLayout horizontalLayout;

        public override void OnActivate()
        {
            var resources = ShapeEditorResources.Instance;
            colorWindowBackground = new Color(0.192f, 0.192f, 0.192f);

            var menu = new GuiMenuStrip();
            Add(menu);

            var fileMenu = menu.Add("File");
            fileMenu.Add("New Project", resources.shapeEditorNew, editor.UserNewProject);
            fileMenu.Add("Open Project...", resources.shapeEditorOpen, editor.UserOpenProject);
            fileMenu.Separator();
            fileMenu.Add("Save As...", resources.shapeEditorSave, editor.UserSaveProjectAs);
            fileMenu.Separator();
            fileMenu.Add("Exit", editor.UserExitShapeEditor);

            var editMenu = menu.Add("Edit");
            menuUndoItem = editMenu.Add("Undo", editor.UserUndo);
            menuRedoItem = editMenu.Add("Redo", editor.UserRedo);
            editMenu.Separator();

            var viewMenu = menu.Add("View");
            viewMenu.Add("Textbox Test Window", editor.UserShowTextboxTestWindow);
            viewMenu.Add("Inspector Window", () => { Debug.Log("Inspector Window"); });

            var helpMenu = menu.Add("Help");
            helpMenu.Add("About", () => { Debug.Log("About"); });

            horizontalLayout = new GuiHorizontalLayout(this, 1, 21);

            horizontalLayout.Add(new GuiButton(resources.shapeEditorNew, 20, editor.UserNewProject));
            horizontalLayout.Add(new GuiButton(resources.shapeEditorOpen, 20, editor.UserOpenProject));
            horizontalLayout.Add(new GuiButton(resources.shapeEditorSave, 20, editor.UserSaveProjectAs));
            horizontalLayout.Space(5);
            horizontalLayout.Add(vertexSelectButton = new GuiButton(resources.shapeEditorVertexSelect, 20, editor.UserSwitchToVertexSelectMode));
            horizontalLayout.Add(edgeSelectButton = new GuiButton(resources.shapeEditorEdgeSelect, 20, editor.UserSwitchToEdgeSelectMode));
            horizontalLayout.Add(faceSelectButton = new GuiButton(resources.shapeEditorFaceSelect, 20, editor.UserSwitchToFaceSelectMode));
            horizontalLayout.Space(5);
            horizontalLayout.Add(new GuiButton(resources.shapeEditorShapeCreate, 20, editor.UserAddShapeToProject));
            horizontalLayout.Space(5);
            horizontalLayout.Add(new GuiButton(resources.shapeEditorExtrudeShape, 20, editor.UserAssignProjectToTargets));
        }

        public override void OnRender()
        {
            size = new float2(editor.position.width, 42f);

            var shapeSelectMode = editor.shapeSelectMode;
            vertexSelectButton.isChecked = shapeSelectMode == ShapeSelectMode.Vertex;
            edgeSelectButton.isChecked = shapeSelectMode == ShapeSelectMode.Edge;
            faceSelectButton.isChecked = shapeSelectMode == ShapeSelectMode.Face;

            menuUndoItem.enabled = editor.canUndo;
            menuRedoItem.enabled = editor.canRedo;

            base.OnRender();
        }
    }
}

#endif