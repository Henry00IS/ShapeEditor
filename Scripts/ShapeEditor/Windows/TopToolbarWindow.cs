#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class TopToolbarWindow : GuiWindow
    {
        private GuiButton vertexSelectButton;
        private GuiButton edgeSelectButton;
        private GuiButton faceSelectButton;
        private GuiMenuVerticalItem editMenuUndoItem;
        private GuiMenuVerticalItem editMenuRedoItem;

        public TopToolbarWindow(float2 position, float2 size) : base(position, size) { }

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
            editMenuUndoItem = editMenu.Add("Undo", editor.UserUndo);
            editMenuRedoItem = editMenu.Add("Redo", editor.UserRedo);
            editMenu.Separator();
            editMenu.Add("Select All", editor.UserSelectAll);
            editMenu.Add("Clear Selection", editor.UserClearSelection);
            editMenu.Add("Delete Selection", resources.shapeEditorDelete, editor.UserDeleteSelection);
            editMenu.Separator();
            editMenu.Add("Snap Selection To Grid", editor.UserSnapSelectionToGrid);
            editMenu.Separator();
            editMenu.Add("Reset Edge To Linear", resources.shapeEditorSegmentLinear, editor.UserResetSegmentGeneratorForSelectedEdges);
            editMenu.Add("Toggle Bezier Generator", resources.shapeEditorSegmentBezier, editor.UserToggleBezierSegmentGeneratorForSelectedEdges);
            editMenu.Add("Toggle Sine Generator", resources.shapeEditorSegmentSine, editor.UserToggleSineSegmentGeneratorForSelectedEdges);
            editMenu.Add("Toggle Repeat Generator", resources.shapeEditorSegmentRepeat, editor.UserToggleRepeatSegmentGeneratorForSelectedEdges);
            editMenu.Separator();
            editMenu.Add("Apply Selected Generators", resources.shapeEditorSegmentApply, editor.UserApplyGeneratorForSelectedEdges);

            var projectMenu = menu.Add("Project");
            projectMenu.Add("Add Shape", resources.shapeEditorShapeCreate, editor.UserAddShapeToProject);

            var sceneMenu = menu.Add("Scene");
            sceneMenu.Add("Assign Project To Targets", resources.shapeEditorExtrudeShape, editor.UserAssignProjectToTargets);
            sceneMenu.Separator();
            sceneMenu.Add("Create Polygon Target", resources.shapeEditorCreatePolygon, editor.UserCreatePolygonTarget);
            sceneMenu.Add("Create Fixed Extrude Target", resources.shapeEditorExtrudeShape, editor.UserCreateFixedExtrudeTarget);
            sceneMenu.Add("Create Spline Extrude Target", resources.shapeEditorExtrudeRevolve, editor.UserCreateSplineExtrudeTarget);

            var viewMenu = menu.Add("View");
            viewMenu.Add("Textbox Test Window", editor.UserShowTextboxTestWindow);
            viewMenu.Add("Inspector Window", editor.UserShowInspectorWindow);
            viewMenu.Separator();
            viewMenu.Add("Reset Camera", editor.UserResetCamera);

            var helpMenu = menu.Add("Help");
            helpMenu.Add("About 2D Shape Editor", editor.UserShowAboutWindow);
            helpMenu.Separator();
            helpMenu.Add("GitHub Repository", editor.UserOpenGitHubRepository);
            helpMenu.Add("Online Manual", editor.UserOpenOnlineManual);

            horizontalLayout = new GuiHorizontalLayout(this, 1, 21);

            horizontalLayout.Add(new GuiButton(resources.shapeEditorNew, 20, editor.UserNewProject));
            horizontalLayout.Add(new GuiButton(resources.shapeEditorOpen, 20, editor.UserOpenProject));
            horizontalLayout.Add(new GuiButton(resources.shapeEditorSave, 20, editor.UserSaveProjectAs));
            horizontalLayout.Space(5);
            horizontalLayout.Add(vertexSelectButton = new GuiButton(resources.shapeEditorVertexSelect, 20, editor.UserSwitchToVertexSelectMode));
            horizontalLayout.Add(edgeSelectButton = new GuiButton(resources.shapeEditorEdgeSelect, 20, editor.UserSwitchToEdgeSelectMode));
            horizontalLayout.Add(faceSelectButton = new GuiButton(resources.shapeEditorFaceSelect, 20, editor.UserSwitchToFaceSelectMode));
            horizontalLayout.Space(5);
            horizontalLayout.Add(new GuiButton(resources.shapeEditorSegmentLinear, 20, editor.UserResetSegmentGeneratorForSelectedEdges));
            horizontalLayout.Add(new GuiButton(resources.shapeEditorSegmentBezier, 20, editor.UserToggleBezierSegmentGeneratorForSelectedEdges));
            horizontalLayout.Add(new GuiButton(resources.shapeEditorSegmentSine, 20, editor.UserToggleSineSegmentGeneratorForSelectedEdges));
            horizontalLayout.Add(new GuiButton(resources.shapeEditorSegmentRepeat, 20, editor.UserToggleRepeatSegmentGeneratorForSelectedEdges));
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

            editMenuUndoItem.enabled = editor.canUndo;
            editMenuRedoItem.enabled = editor.canRedo;

            base.OnRender();
        }
    }
}

#endif