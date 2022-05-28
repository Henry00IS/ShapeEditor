#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorWindow
    {
        // general purpose user functions, typically invoked through the gui or keyboard.

        /// <summary>Deletes the selected objects from the project.</summary>
        internal void UserDeleteSelection()
        {
            RegisterUndo("Delete Selection");

            // for every shape in the project:
            var shapesCount = project.shapes.Count;
            for (int i = shapesCount; i-- > 0;)
            {
                var shape = project.shapes[i];

                // for every segment in the project:
                var segments = shape.segments;
                var segmentsCount = segments.Count;
                for (int j = segmentsCount; j-- > 0;)
                    if (segments[j].selected)
                        shape.RemoveSegment(segments[j]);

                // remove the shape if it's empty.
                if (segments.Count == 0)
                    project.shapes.RemoveAt(i);
            }
        }

        /// <summary>For all selected edges, resets all segment generators to linear.</summary>
        internal void UserResetSegmentGeneratorForSelectedEdges()
        {
            RegisterUndo("Reset Edge To Linear");

            foreach (var segment in ForEachSelectedEdge())
                segment.generator = new SegmentGenerator(segment);
        }

        /// <summary>
        /// For all selected edges, toggles between the bezier segment generator or linear.
        /// </summary>
        internal void UserToggleBezierSegmentGeneratorForSelectedEdges()
        {
            RegisterUndo("Toggle Bezier Generator");

            foreach (var segment in ForEachSelectedEdge())
                segment.generator = new SegmentGenerator(segment, segment.generator.type != SegmentGeneratorType.Bezier ? SegmentGeneratorType.Bezier : SegmentGeneratorType.Linear);
        }

        /// <summary>
        /// For all selected edges, toggles between the sine segment generator or linear.
        /// </summary>
        internal void UserToggleSineSegmentGeneratorForSelectedEdges()
        {
            RegisterUndo("Toggle Sine Generator");

            foreach (var segment in ForEachSelectedEdge())
                segment.generator = new SegmentGenerator(segment, segment.generator.type != SegmentGeneratorType.Sine ? SegmentGeneratorType.Sine : SegmentGeneratorType.Linear);
        }

        /// <summary>
        /// For all selected edges, toggles between the repeat segment generator or linear.
        /// </summary>
        internal void UserToggleRepeatSegmentGeneratorForSelectedEdges()
        {
            RegisterUndo("Toggle Repeat Generator");

            foreach (var segment in ForEachSelectedEdge())
                segment.generator = new SegmentGenerator(segment, segment.generator.type != SegmentGeneratorType.Repeat ? SegmentGeneratorType.Repeat : SegmentGeneratorType.Linear);
        }

        /// <summary>
        /// For all selected edges, applies the segment generator by inserting the generated points
        /// as new segments.
        /// </summary>
        internal void UserApplyGeneratorForSelectedEdges()
        {
            RegisterUndo("Apply Selected Generators");

            // for every shape in the project:
            var shapesCount = project.shapes.Count;
            for (int i = shapesCount; i-- > 0;)
            {
                var shape = project.shapes[i];

                // for every segment in the project:
                var segments = shape.segments;
                var segmentsCount = segments.Count;
                for (int j = segmentsCount; j-- > 0;)
                {
                    var segment = segments[j];
                    if (segment.selected && segment.next.selected)
                    {
                        segment.generator.ApplyGenerator();
                        segment.generator = new SegmentGenerator(segment);
                    }
                }
            }
        }

        internal void UserAssignProjectToTargets()
        {
            var transform = Selection.activeTransform;
            if (transform)
            {
                var target = transform.GetComponent<IShapeEditorTarget>();
                if (target != null)
                {
                    target.OnShapeEditorUpdateProject(project);
                }
            }
        }

        /// <summary>Created a polygon in the scene, assign the project and select it.</summary>
        internal void UserCreatePolygonTarget()
        {
            GameObject go = new GameObject("Polygon");
            var target = go.AddComponent<ShapeEditorTarget>();
            target.OnShapeEditorUpdateProject(project);
            Selection.activeGameObject = go;
        }

        /// <summary>
        /// Created a polygon in the scene, assign the project, set the mode to fixed extrude and
        /// select it.
        /// </summary>
        internal void UserCreateFixedExtrudeTarget()
        {
            GameObject go = new GameObject("Extruded Shape");
            var target = go.AddComponent<ShapeEditorTarget>();
            target.targetMode = ShapeEditorTargetMode.FixedExtrude;
            target.OnShapeEditorUpdateProject(project);
            Selection.activeGameObject = go;
        }

        /// <summary>
        /// Created a polygon in the scene, assign the project, set the mode to spline extrude and
        /// select it.
        /// </summary>
        internal void UserCreateSplineExtrudeTarget()
        {
            GameObject go = new GameObject("Spline Extruded Shape");
            var target = go.AddComponent<ShapeEditorTarget>();

            GameObject p1 = new GameObject("Point");
            p1.transform.parent = go.transform;
            p1.transform.localPosition = new Vector3(0f, 0f, 0f);
            GameObject p2 = new GameObject("Point");
            p2.transform.parent = go.transform;
            p2.transform.localPosition = new Vector3(0f, 0f, 1f);
            GameObject p3 = new GameObject("Point");
            p3.transform.parent = go.transform;
            p3.transform.localPosition = new Vector3(1f, 0f, 1f);

            target.targetMode = ShapeEditorTargetMode.SplineExtrude;
            target.OnShapeEditorUpdateProject(project);
            Selection.activeGameObject = go;
        }

        /// <summary>Clears the selection, adds a new shape and selects it.</summary>
        internal void UserAddShapeToProject()
        {
            RegisterUndo("Add Shape");

            project.ClearSelection();

            var shape = new Shape();
            project.shapes.Add(shape);

            shape.SelectAll();
        }

        /// <summary>Clears the selection of all selectable objects in the project.</summary>
        internal void UserClearSelection()
        {
            project.ClearSelection();
        }

        /// <summary>Selects all of the selectable objects in the project.</summary>
        internal void UserSelectAll()
        {
            project.SelectAll();
        }

        /// <summary>Inverts the selection all of the selectable objects in the project.</summary>
        internal void UserInvertSelection()
        {
            project.InvertSelection();
        }

        /// <summary>Switches to the box select tool unless already active.</summary>
        internal void UserSwitchToBoxSelectTool() => SwitchTool(boxSelectTool);

        /// <summary>Switches to the translate tool unless already active.</summary>
        internal void UserSwitchToTranslateTool() => SwitchTool(translateTool);

        /// <summary>Switches to the rotate tool unless already active.</summary>
        internal void UserSwitchToRotateTool() => SwitchTool(rotateTool);

        /// <summary>Switches to the scale tool unless already active.</summary>
        internal void UserSwitchToScaleTool() => SwitchTool(scaleTool);

        /// <summary>Switches to the cut tool unless already active.</summary>
        internal void UserSwitchToCutTool() => SwitchTool(cutTool);

        /// <summary>Switches to the vertex select mode.</summary>
        internal void UserSwitchToVertexSelectMode() => shapeSelectMode = ShapeSelectMode.Vertex;

        /// <summary>Switches to the edge select mode.</summary>
        internal void UserSwitchToEdgeSelectMode() => shapeSelectMode = ShapeSelectMode.Edge;

        /// <summary>Switches to the face select mode.</summary>
        internal void UserSwitchToFaceSelectMode() => shapeSelectMode = ShapeSelectMode.Face;

        internal void UserNewProject()
        {
            NewProject();
        }

        /// <summary>Displays a file open dialog to load a project file.</summary>
        internal void UserOpenProject()
        {
            string path = EditorUtility.OpenFilePanel("Load 2D Shape Editor Project", "", "s2d,sabre2d");
            if (path.Length != 0)
            {
                OpenProject(path);
            }
        }

        /// <summary>Displays a file save dialog to save a project file.</summary>
        internal void UserSaveProjectAs()
        {
            try
            {
                string path = EditorUtility.SaveFilePanel("Save 2D Shape Editor Project", "", "Project", "s2d");
                if (path.Length != 0)
                {
                    SaveProject(path);
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("2D Shape Editor", "An exception occured while saving the project:\r\n" + ex.Message, "Ohno!");
            }
        }

        /// <summary>Closes the shape editor window.</summary>
        internal void UserExitShapeEditor()
        {
            Close();
        }

        internal void UserShowTextboxTestWindow()
        {
            OpenWindow(new TextboxTestWindow(new float2(300, 100), new float2(220, 80)), false);
        }

        /// <summary>Displays the about window.</summary>
        internal void UserShowAboutWindow()
        {
            OpenWindow(new AboutWindow());
        }

        /// <summary>Displays the inspector window.</summary>
        internal void UserShowInspectorWindow()
        {
            OpenWindow(new InspectorWindow());
        }

        /// <summary>Opens the GitHub repository in a browser window.</summary>
        internal void UserOpenGitHubRepository()
        {
            Application.OpenURL("https://github.com/Henry00IS/ShapeEditor");
        }

        /// <summary>Opens the GitHub repository wiki in a browser window.</summary>
        internal void UserOpenOnlineManual()
        {
            Application.OpenURL("https://github.com/Henry00IS/ShapeEditor/wiki");
        }

        /// <summary>Called on CTRL+Z when the editor window has focus.</summary>
        internal void UserUndo()
        {
            OnUndo();
        }

        /// <summary>Called on CTRL+Y when the editor window has focus.</summary>
        internal void UserRedo()
        {
            OnRedo();
        }

        /// <summary>Snaps the selected objects to the grid.</summary>
        internal void UserSnapSelectionToGrid()
        {
            RegisterUndo("Snap Selection To Grid");
            foreach (var selectable in ForEachSelectedObject())
                selectable.position = selectable.position.Snap(gridSnap);
        }

        /// <summary>Centers the camera by resetting the grid offset and zoom.</summary>
        internal void UserResetCamera()
        {
            GridResetOffset();
            GridResetZoom();
        }

        /// <summary>Toggles whether grid snapping is enabled by default.</summary>
        internal void UserToggleGridSnapping()
        {
            snapEnabled = !snapEnabled;
        }

        /// <summary>Sets the selected shapes as additive.</summary>
        internal void UserSetSelectedShapesAdditive()
        {
            foreach (var shape in project.shapes)
                if (shape.IsSelected())
                    shape.booleanOperator = PolygonBooleanOperator.Union;
        }

        /// <summary>Sets the selected shapes as subtractive.</summary>
        internal void UserSetSelectedShapesSubtractive()
        {
            foreach (var shape in project.shapes)
                if (shape.IsSelected())
                    shape.booleanOperator = PolygonBooleanOperator.Difference;
        }

        /// <summary>Pushes the selected shapes to the front (for boolean operations).</summary>
        internal void UserPushSelectedShapesToFront()
        {
            var shapesToMove = new List<Shape>();
            foreach (var shape in project.shapes)
                if (shape.IsSelected())
                    shapesToMove.Add(shape);

            foreach (var shape in shapesToMove)
            {
                project.shapes.Remove(shape);
                project.shapes.Add(shape);
            }
        }

        /// <summary>Pushes the selected shapes to the back (for boolean operations).</summary>
        internal void UserPushSelectedShapesToBack()
        {
            var shapesToMove = new List<Shape>();
            foreach (var shape in project.shapes)
                if (shape.IsSelected())
                    shapesToMove.Add(shape);

            foreach (var shape in shapesToMove)
            {
                project.shapes.Remove(shape);
                project.shapes.Insert(0, shape);
            }
        }

        /// <summary>
        /// Finds all fully selected shapes, if there are new shapes, clears the current selection,
        /// adds a copy of the shapes to the project, selects the new shapes, and switches to a
        /// single-use translation tool.
        /// </summary>
        internal void UserDuplicateSelectedShapes()
        {
            var shapesToDuplicate = new List<Shape>();
            foreach (var shape in project.shapes)
                if (shape.IsSelected())
                    shapesToDuplicate.Add(shape);

            if (shapesToDuplicate.Count > 0)
            {
                project.ClearSelection();

                foreach (var shape in shapesToDuplicate)
                {
                    var clone = shape.Clone();
                    clone.Validate();
                    project.shapes.Add(clone);
                    shape.SelectAll();
                }

                UseTool(new TranslateTool());
            }
        }

        internal void UserFlipSelectionHorizonally()
        {
            RegisterUndo("Flip Selection Horizonally");

            var left = float.MaxValue;
            foreach (var segment in ForEachSelectedObject())
                if (segment.position.x < left)
                    left = segment.position.x;

            foreach (var segment in ForEachSelectedObject())
                segment.position = new float2(-segment.position.x + (left * 2), segment.position.y);
        }

        internal void UserFlipSelectionVertically()
        {
            RegisterUndo("Flip Selection Vertically");

            var top = float.MaxValue;
            foreach (var segment in ForEachSelectedObject())
                if (segment.position.y < top)
                    top = segment.position.y;

            foreach (var segment in ForEachSelectedObject())
                segment.position = new float2(segment.position.x, -segment.position.y + (top * 2));
        }
    }
}

#endif