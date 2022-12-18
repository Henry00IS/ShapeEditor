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
            base.OnActivate();

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
            editMenu.Add("Invert Selection", editor.UserInvertSelection);
            editMenu.Add("Delete Selection", resources.shapeEditorDelete, editor.UserDeleteSelection);
            editMenu.Separator();
            editMenu.Add("Copy", editor.UserCopy);
            editMenu.Add("Paste", editor.UserPaste);
            editMenu.Separator();
            editMenu.Add("Flip Horizontally", resources.shapeEditorFlipHorizontally, editor.UserFlipSelectionHorizonally);
            editMenu.Add("Flip Vertically", resources.shapeEditorFlipVertically, editor.UserFlipSelectionVertically);
            editMenu.Separator();
            editMenu.Add("Snap Selection To Grid", editor.UserSnapSelectionToGrid);

            var edgeMenu = menu.Add("Edge");
            edgeMenu.Add("Reset Edge To Linear", resources.shapeEditorSegmentLinear, editor.UserResetSegmentGeneratorForSelectedEdges);
            edgeMenu.Separator();
            edgeMenu.Add("Toggle Arch Generator", resources.shapeEditorSegmentArch, editor.UserToggleArchSegmentGeneratorForSelectedEdges);
            edgeMenu.Add("Toggle Bezier Generator", resources.shapeEditorSegmentBezier, editor.UserToggleBezierSegmentGeneratorForSelectedEdges);
            edgeMenu.Add("Toggle Repeat Generator", resources.shapeEditorSegmentRepeat, editor.UserToggleRepeatSegmentGeneratorForSelectedEdges);
            edgeMenu.Add("Toggle Sine Generator", resources.shapeEditorSegmentSine, editor.UserToggleSineSegmentGeneratorForSelectedEdges);
            edgeMenu.Separator();
            edgeMenu.Add("Apply Selected Generators", resources.shapeEditorSegmentApply, editor.UserApplyGeneratorForSelectedEdges);

            var shapeMenu = menu.Add("Shape");
            shapeMenu.Add("Add Shape", resources.shapeEditorShapeCreate, editor.UserAddShapeToProject);
            shapeMenu.Separator();
            shapeMenu.Add("Duplicate Selection", resources.shapeEditorShapeDuplicate, editor.UserDuplicateSelectedShapes);
            shapeMenu.Add("Shape From Selection", resources.shapeEditorShapeCreate, editor.UserShapeFromSelection);
            shapeMenu.Add("Apply Selected Symmetry", editor.UserApplySymmetryForSelectedShapes);
            shapeMenu.Separator();
            shapeMenu.Add("Set Additive", editor.UserSetSelectedShapesAdditive);
            shapeMenu.Add("Set Subtractive", editor.UserSetSelectedShapesSubtractive);
            shapeMenu.Separator();
            shapeMenu.Add("Push To Front", editor.UserPushSelectedShapesToFront);
            shapeMenu.Add("Push To Back", editor.UserPushSelectedShapesToBack);

            var sceneMenu = menu.Add("Scene");
            sceneMenu.Add("Assign Project To Targets", resources.shapeEditorExtrudeShape, editor.UserAssignProjectToTargets);
            sceneMenu.Separator();
            sceneMenu.Add("Create Polygon", resources.shapeEditorCreatePolygon, editor.UserCreatePolygonTarget);
            sceneMenu.Add("Create Extruded", resources.shapeEditorExtrudeFixed, editor.UserCreateFixedExtrudeTarget);
            sceneMenu.Separator();
            sceneMenu.Add("Create Bevel", resources.shapeEditorExtrudeBevel, editor.UserCreateBevelExtrudeTarget);
            sceneMenu.Add("Create Curved Staircase", resources.shapeEditorExtrudeCurvedStaircase, editor.UserCreateCurvedStaircaseExtrudeTarget);
            sceneMenu.Add("Create Linear Staircase", resources.shapeEditorExtrudeLinearStaircase, editor.UserCreateLinearStaircaseExtrudeTarget);
            sceneMenu.Add("Create Pyramid", resources.shapeEditorExtrudePoint, editor.UserCreateScaledExtrudeTarget);
            sceneMenu.Add("Create Revolved", resources.shapeEditorExtrudeRevolve, editor.UserCreateRevolvedExtrudeTarget);
            sceneMenu.Add("Create Revolve Chopped", resources.shapeEditorExtrudeRevolveChopped, editor.UserCreateRevolveChoppedExtrudeTarget);
            sceneMenu.Add("Create Slope", resources.shapeEditorExtrudeSlope, editor.UserCreateSlopeExtrudeTarget);
            sceneMenu.Add("Create Spiral", resources.shapeEditorExtrudeSpiral, editor.UserCreateSpiralExtrudeTarget);
            sceneMenu.Add("Create Spline", resources.shapeEditorExtrudeSpline, editor.UserCreateSplineExtrudeTarget);

            // RealtimeCSG Extrusion Modes:
            if (ExternalRealtimeCSG.IsAvailable())
            {
                var realtimeCsgMenu = menu.Add("RealtimeCSG");

                realtimeCsgMenu.Add("Assign Project To Targets", resources.shapeEditorExtrudeShape, editor.UserAssignProjectToTargets);
                realtimeCsgMenu.Separator();
                realtimeCsgMenu.Add("Create Extruded", resources.shapeEditorExtrudeFixed, editor.UserCreateRealtimeCsgFixedExtrudeTarget);
                realtimeCsgMenu.Separator();
                realtimeCsgMenu.Add("Create Bevel", resources.shapeEditorExtrudeBevel, editor.UserCreateRealtimeCsgBevelExtrudeTarget);
                realtimeCsgMenu.Add("Create Curved Staircase", resources.shapeEditorExtrudeCurvedStaircase, editor.UserCreateRealtimeCsgCurvedStaircaseExtrudeTarget);
                realtimeCsgMenu.Add("Create Linear Staircase", resources.shapeEditorExtrudeLinearStaircase, editor.UserCreateRealtimeCsgLinearStaircaseExtrudeTarget);
                realtimeCsgMenu.Add("Create Pyramid", resources.shapeEditorExtrudePoint, editor.UserCreateRealtimeCsgScaledExtrudeTarget);
                realtimeCsgMenu.Add("Create Revolved", resources.shapeEditorExtrudeRevolve, editor.UserCreateRealtimeCsgRevolvedExtrudeTarget);
                realtimeCsgMenu.Add("Create Revolve Chopped", resources.shapeEditorExtrudeRevolveChopped, editor.UserCreateRealtimeCsgRevolveChoppedExtrudeTarget);
                realtimeCsgMenu.Add("Create Slope", resources.shapeEditorExtrudeSlope, editor.UserCreateRealtimeCsgSlopeExtrudeTarget);
                realtimeCsgMenu.Add("Create Spiral", resources.shapeEditorExtrudeSpiral, editor.UserCreateRealtimeCsgSpiralExtrudeTarget);
                realtimeCsgMenu.Add("Create Spline (Experimental!)", resources.shapeEditorExtrudeSpline, editor.UserCreateRealtimeCsgSplineExtrudeTarget);
            }

            // Chisel Extrusion Modes:
            if (ExternalChisel.IsAvailable())
            {
                var chiselMenu = menu.Add("Chisel");

                chiselMenu.Add("Assign Project To Targets", resources.shapeEditorExtrudeShape, editor.UserAssignProjectToTargets);
                chiselMenu.Separator();
                chiselMenu.Add("Create Extruded", resources.shapeEditorExtrudeFixed, editor.UserCreateChiselFixedExtrudeTarget);
                chiselMenu.Separator();
                chiselMenu.Add("Create Bevel", resources.shapeEditorExtrudeBevel, editor.UserCreateChiselBevelExtrudeTarget);
                chiselMenu.Add("Create Curved Staircase", resources.shapeEditorExtrudeCurvedStaircase, editor.UserCreateChiselCurvedStaircaseExtrudeTarget);
                chiselMenu.Add("Create Linear Staircase", resources.shapeEditorExtrudeLinearStaircase, editor.UserCreateChiselLinearStaircaseExtrudeTarget);
                chiselMenu.Add("Create Pyramid", resources.shapeEditorExtrudePoint, editor.UserCreateChiselScaledExtrudeTarget);
                chiselMenu.Add("Create Revolved", resources.shapeEditorExtrudeRevolve, editor.UserCreateChiselRevolvedExtrudeTarget);
                chiselMenu.Add("Create Revolve Chopped", resources.shapeEditorExtrudeRevolveChopped, editor.UserCreateChiselRevolveChoppedExtrudeTarget);
                chiselMenu.Add("Create Slope", resources.shapeEditorExtrudeSlope, editor.UserCreateChiselSlopeExtrudeTarget);
                chiselMenu.Add("Create Spiral", resources.shapeEditorExtrudeSpiral, editor.UserCreateChiselSpiralExtrudeTarget);
            }

            var viewMenu = menu.Add("View");
            viewMenu.Add("Background Settings", editor.UserShowBackgroundSettingsWindow);
            viewMenu.Add("Material Editor", editor.UserShowMaterialEditorWindow);
            viewMenu.Separator();
            viewMenu.Add("Arch Inspector", resources.shapeEditorSegmentArch, editor.UserShowArchInspectorWindow);
            viewMenu.Add("Bezier Inspector", resources.shapeEditorSegmentBezier, editor.UserShowBezierInspectorWindow);
            viewMenu.Add("Circle Generator", resources.shapeEditorCircleGenerator, editor.UserShowCircleGeneratorWindow);
            viewMenu.Add("Repeat Inspector", resources.shapeEditorSegmentRepeat, editor.UserShowRepeatInspectorWindow);
            viewMenu.Add("Shape Inspector", resources.shapeEditorShapeCreate, editor.UserShowShapeInspectorWindow);
            viewMenu.Add("Sine Inspector", resources.shapeEditorSegmentSine, editor.UserShowSineInspectorWindow);
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
            horizontalLayout.Add(new GuiButton(resources.shapeEditorSegmentArch, 20, editor.UserToggleArchSegmentGeneratorForSelectedEdges));
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
            editMenuUndoItem.text = "Undo " + editor.canUndoName;
            editMenuRedoItem.text = "Redo " + editor.canRedoName;

            base.OnRender();
        }
    }
}

#endif