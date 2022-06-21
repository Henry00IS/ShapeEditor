#if UNITY_EDITOR

using System;
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

        /// <summary>
        /// Creates a <see cref="ShapeEditorTarget"/> with the specified <paramref name="mode"/> in
        /// the scene, calls <paramref name="init"/> to apply additional parameters and logic,
        /// assigns the project to the target and then selects the game object.
        /// </summary>
        /// <param name="name">The name of the game object to be created.</param>
        /// <param name="mode">The operating mode of the target.</param>
        /// <param name="init">Callback to apply additional parameters and logic.</param>
        private void UserCreateShapeEditorTarget(string name, ShapeEditorTargetMode mode, Action<GameObject, ShapeEditorTarget> init = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetSiblingOfActiveTransform();

            var target = go.AddComponent<ShapeEditorTarget>();
            target.targetMode = mode;
            init?.Invoke(go, target);
            target.OnShapeEditorUpdateProject(project);

            Selection.activeGameObject = go;
        }

        /// <summary>Creates a polygon target in the scene and selects it.</summary>
        internal void UserCreatePolygonTarget() => UserCreateShapeEditorTarget("Polygon", ShapeEditorTargetMode.Polygon);

        /// <summary>Creates a fixed extrude target in the scene and selects it.</summary>
        internal void UserCreateFixedExtrudeTarget() => UserCreateShapeEditorTarget("Fixed Extrude", ShapeEditorTargetMode.FixedExtrude);

        /// <summary>Creates a bevel target in the scene and selects it.</summary>
        internal void UserCreateBevelExtrudeTarget() => UserCreateShapeEditorTarget("Bevel Extrude", ShapeEditorTargetMode.ScaledExtrude, (g, t) => t.scaledExtrudeBackScale = 0.5f);

        /// <summary>Creates a scaled target in the scene and selects it.</summary>
        internal void UserCreateScaledExtrudeTarget() => UserCreateShapeEditorTarget("Scaled Extrude", ShapeEditorTargetMode.ScaledExtrude);

        /// <summary>Creates a revolved target in the scene and selects it.</summary>
        internal void UserCreateRevolvedExtrudeTarget() => UserCreateShapeEditorTarget("Revolve Extrude", ShapeEditorTargetMode.RevolveExtrude);

        /// <summary>Creates a revolve chopped target in the scene and selects it.</summary>
        internal void UserCreateRevolveChoppedExtrudeTarget() => UserCreateShapeEditorTarget("Revolve Chopped", ShapeEditorTargetMode.RevolveChopped);

        /// <summary>Creates a curved staircase target in the scene and selects it.</summary>
        internal void UserCreateCurvedStaircaseExtrudeTarget() => UserCreateShapeEditorTarget("Curved Staircase", ShapeEditorTargetMode.RevolveExtrude, (g, t) => t.revolveExtrudeHeight = 0.75f);

        /// <summary>Creates a linear staircase target in the scene and selects it.</summary>
        internal void UserCreateLinearStaircaseExtrudeTarget() => UserCreateShapeEditorTarget("Linear Staircase", ShapeEditorTargetMode.LinearStaircase);

        /// <summary>Creates a slope target in the scene and selects it.</summary>
        internal void UserCreateSlopeExtrudeTarget() => UserCreateShapeEditorTarget("Slope Extrude", ShapeEditorTargetMode.LinearStaircase, (g, t) => t.linearStaircaseSloped = true);

        /// <summary>Creates a spiral target in the scene and selects it.</summary>
        internal void UserCreateSpiralExtrudeTarget() => UserCreateShapeEditorTarget("Spiral Extrude", ShapeEditorTargetMode.RevolveExtrude, (g, t) => { t.revolveExtrudeHeight = 0.75f; t.revolveExtrudeSloped = true; });

        /// <summary>Creates a spline target in the scene and selects it.</summary>
        internal void UserCreateSplineExtrudeTarget()
        {
            UserCreateShapeEditorTarget("Spline Extrude", ShapeEditorTargetMode.SplineExtrude, (g, t) =>
            {
                GameObject p1 = new GameObject("Point");
                p1.transform.parent = g.transform;
                p1.transform.localPosition = new Vector3(0f, 0f, 0f);
                GameObject p2 = new GameObject("Point");
                p2.transform.parent = g.transform;
                p2.transform.localPosition = new Vector3(0f, 0f, 1f);
                GameObject p3 = new GameObject("Point");
                p3.transform.parent = g.transform;
                p3.transform.localPosition = new Vector3(1f, 0f, 1f);
            });
        }

        /// <summary>
        /// Creates a <see cref="RealtimeCSGTarget"/> with the specified <paramref name="mode"/> in
        /// the scene, calls <paramref name="init"/> to apply additional parameters and logic,
        /// assigns the project to the target and then selects the game object.
        /// </summary>
        /// <param name="name">The name of the game object to be created.</param>
        /// <param name="mode">The operating mode of the target.</param>
        /// <param name="init">Callback to apply additional parameters and logic.</param>
        private void UserCreateRealtimeCSGTarget(string name, RealtimeCSGTargetMode mode, Action<GameObject, RealtimeCSGTarget> init = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetSiblingOfActiveTransform();

            var target = go.AddComponent<RealtimeCSGTarget>();
            target.targetMode = mode;
            init?.Invoke(go, target);
            target.OnShapeEditorUpdateProject(project);

            Selection.activeGameObject = go;
        }

        /// <summary>Creates a fixed extrude target in the scene and selects it.</summary>
        internal void UserCreateRealtimeCsgFixedExtrudeTarget() => UserCreateRealtimeCSGTarget("Fixed Extrude", RealtimeCSGTargetMode.FixedExtrude);

        /// <summary>Creates a bevel target in the scene and selects it.</summary>
        internal void UserCreateRealtimeCsgBevelExtrudeTarget() => UserCreateRealtimeCSGTarget("Bevel Extrude", RealtimeCSGTargetMode.ScaledExtrude, (g, t) => t.scaledExtrudeBackScale = 0.5f);

        /// <summary>Creates a scaled target in the scene and selects it.</summary>
        internal void UserCreateRealtimeCsgScaledExtrudeTarget() => UserCreateRealtimeCSGTarget("Scaled Extrude", RealtimeCSGTargetMode.ScaledExtrude);

        /// <summary>Creates a revolved target in the scene and selects it.</summary>
        internal void UserCreateRealtimeCsgRevolvedExtrudeTarget() => UserCreateRealtimeCSGTarget("Revolve Extrude", RealtimeCSGTargetMode.RevolveExtrude);

        /// <summary>Creates a revolve chopped target in the scene and selects it.</summary>
        internal void UserCreateRealtimeCsgRevolveChoppedExtrudeTarget() => UserCreateRealtimeCSGTarget("Revolve Chopped", RealtimeCSGTargetMode.RevolveChopped);

        /// <summary>Creates a curved staircase target in the scene and selects it.</summary>
        internal void UserCreateRealtimeCsgCurvedStaircaseExtrudeTarget() => UserCreateRealtimeCSGTarget("Curved Staircase", RealtimeCSGTargetMode.RevolveExtrude, (g, t) => t.revolveExtrudeHeight = 0.75f);

        /// <summary>Creates a linear staircase target in the scene and selects it.</summary>
        internal void UserCreateRealtimeCsgLinearStaircaseExtrudeTarget() => UserCreateRealtimeCSGTarget("Linear Staircase", RealtimeCSGTargetMode.LinearStaircase);

        /// <summary>Creates a slope target in the scene and selects it.</summary>
        internal void UserCreateRealtimeCsgSlopeExtrudeTarget() => UserCreateRealtimeCSGTarget("Slope Extrude", RealtimeCSGTargetMode.LinearStaircase, (g, t) => t.linearStaircaseSloped = true);

        /// <summary>Creates a spiral target in the scene and selects it.</summary>
        internal void UserCreateRealtimeCsgSpiralExtrudeTarget() => UserCreateRealtimeCSGTarget("Spiral Extrude", RealtimeCSGTargetMode.RevolveExtrude, (g, t) => { t.revolveExtrudeHeight = 0.75f; t.revolveExtrudeSloped = true; });

        /// <summary>Creates a spline target in the scene and selects it.</summary>
        internal void UserCreateRealtimeCsgSplineExtrudeTarget()
        {
            UserCreateRealtimeCSGTarget("Spline Extrude", RealtimeCSGTargetMode.SplineExtrude, (g, t) =>
            {
                GameObject p1 = new GameObject("Point");
                p1.transform.parent = g.transform;
                p1.transform.localPosition = new Vector3(1f, 0f, 1f);
                GameObject p2 = new GameObject("Point");
                p2.transform.parent = g.transform;
                p2.transform.localPosition = new Vector3(0f, 0f, 1f);
                GameObject p3 = new GameObject("Point");
                p3.transform.parent = g.transform;
                p3.transform.localPosition = new Vector3(0f, 0f, 0f);
            });
        }

        /// <summary>
        /// Creates a <see cref="ChiselTarget"/> with the specified <paramref name="mode"/> in
        /// the scene, calls <paramref name="init"/> to apply additional parameters and logic,
        /// assigns the project to the target and then selects the game object.
        /// </summary>
        /// <param name="name">The name of the game object to be created.</param>
        /// <param name="mode">The operating mode of the target.</param>
        /// <param name="init">Callback to apply additional parameters and logic.</param>
        private void UserCreateChiselTarget(string name, ChiselTargetMode mode, Action<GameObject, ChiselTarget> init = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetSiblingOfActiveTransform();

            var target = go.AddComponent<ChiselTarget>();
            target.targetMode = mode;
            init?.Invoke(go, target);
            target.OnShapeEditorUpdateProject(project);

            Selection.activeGameObject = go;
        }

        /// <summary>Creates a fixed extrude target in the scene and selects it.</summary>
        internal void UserCreateChiselFixedExtrudeTarget() => UserCreateChiselTarget("Fixed Extrude", ChiselTargetMode.FixedExtrude);

        /// <summary>Creates a bevel target in the scene and selects it.</summary>
        internal void UserCreateChiselBevelExtrudeTarget() => UserCreateChiselTarget("Bevel Extrude", ChiselTargetMode.ScaledExtrude, (g, t) => t.scaledExtrudeBackScale = 0.5f);

        /// <summary>Creates a scaled target in the scene and selects it.</summary>
        internal void UserCreateChiselScaledExtrudeTarget() => UserCreateChiselTarget("Scaled Extrude", ChiselTargetMode.ScaledExtrude);

        /// <summary>Creates a revolved target in the scene and selects it.</summary>
        internal void UserCreateChiselRevolvedExtrudeTarget() => UserCreateChiselTarget("Revolve Extrude", ChiselTargetMode.RevolveExtrude);

        /// <summary>Creates a revolve chopped target in the scene and selects it.</summary>
        internal void UserCreateChiselRevolveChoppedExtrudeTarget() => UserCreateChiselTarget("Revolve Chopped", ChiselTargetMode.RevolveChopped);

        /// <summary>Creates a curved staircase target in the scene and selects it.</summary>
        internal void UserCreateChiselCurvedStaircaseExtrudeTarget() => UserCreateChiselTarget("Curved Staircase", ChiselTargetMode.RevolveExtrude, (g, t) => t.revolveExtrudeHeight = 0.75f);

        /// <summary>Creates a linear staircase target in the scene and selects it.</summary>
        internal void UserCreateChiselLinearStaircaseExtrudeTarget() => UserCreateChiselTarget("Linear Staircase", ChiselTargetMode.LinearStaircase);

        /// <summary>Creates a slope target in the scene and selects it.</summary>
        internal void UserCreateChiselSlopeExtrudeTarget() => UserCreateChiselTarget("Slope Extrude", ChiselTargetMode.LinearStaircase, (g, t) => t.linearStaircaseSloped = true);

        /// <summary>Creates a spiral target in the scene and selects it.</summary>
        internal void UserCreateChiselSpiralExtrudeTarget() => UserCreateChiselTarget("Spiral Extrude", ChiselTargetMode.RevolveExtrude, (g, t) => { t.revolveExtrudeHeight = 0.75f; t.revolveExtrudeSloped = true; });

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

        /// <summary>Displays the about window.</summary>
        internal void UserShowAboutWindow()
        {
            OpenWindow(new AboutWindow());
        }

        /// <summary>Displays the bezier inspector window.</summary>
        internal void UserShowBezierInspectorWindow()
        {
            OpenWindow(new BezierInspectorWindow());
        }

        /// <summary>Displays the sine inspector window.</summary>
        internal void UserShowSineInspectorWindow()
        {
            OpenWindow(new SineInspectorWindow());
        }

        /// <summary>Displays the shape inspector window.</summary>
        internal void UserShowShapeInspectorWindow()
        {
            OpenWindow(new ShapeInspectorWindow());
        }

        /// <summary>Displays the background settings window.</summary>
        internal void UserShowBackgroundSettingsWindow()
        {
            OpenWindow(new BackgroundSettingsWindow());
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
            foreach (var shape in ForEachSelectedShape())
                shape.booleanOperator = PolygonBooleanOperator.Union;
        }

        /// <summary>Sets the selected shapes as subtractive.</summary>
        internal void UserSetSelectedShapesSubtractive()
        {
            foreach (var shape in ForEachSelectedShape())
                shape.booleanOperator = PolygonBooleanOperator.Difference;
        }

        /// <summary>Pushes the selected shapes to the front (for boolean operations).</summary>
        internal void UserPushSelectedShapesToFront()
        {
            var shapesToMove = new List<Shape>();
            foreach (var shape in ForEachSelectedShape())
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
            foreach (var shape in ForEachSelectedShape())
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
            foreach (var shape in ForEachSelectedShape())
                shapesToDuplicate.Add(shape);

            if (shapesToDuplicate.Count > 0)
            {
                project.ClearSelection();

                foreach (var shape in shapesToDuplicate)
                {
                    var clone = shape.Clone();
                    clone.Validate();
                    project.shapes.Add(clone);
                    clone.SelectAll();
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

        internal void UserCopy()
        {
            var clipboard = new ClipboardData();

            // add all fully selected shapes to the clipboard.
            foreach (var shape in ForEachSelectedShape())
                clipboard.shapes.Add(shape);

            EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(clipboard);
        }

        internal void UserPaste()
        {
            string json = EditorGUIUtility.systemCopyBuffer;
            if (json.Length > 0)
            {
                ClipboardData clipboard = null;
                try
                {
                    clipboard = JsonUtility.FromJson<ClipboardData>(json);
                }
                catch (Exception)
                {
                    // silently fail on invalid clipboard data.
                }

                if (clipboard != null)
                {
                    project.ClearSelection();

                    // add all clipboard shapes to the project.
                    foreach (var shape in clipboard.shapes)
                    {
                        project.shapes.Add(shape);
                        shape.Validate();
                        shape.SelectAll();
                    }
                }
            }
        }
    }
}

#endif