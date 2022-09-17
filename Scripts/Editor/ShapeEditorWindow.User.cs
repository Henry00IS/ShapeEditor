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

        /// <summary>Clears the current selection, adds a new cube shape to the project and selects it.</summary>
        [Instructions(
            title: "Add shape to project",
            description: "Clears the current selection, adds a new cube shape to the project and selects it."
        )]
        internal void UserAddShapeToProject()
        {
            RegisterUndo("Add Shape");

            project.ClearSelection();

            var shape = new Shape();
            project.shapes.Add(shape);

            shape.SelectAll();
        }

        /// <summary>
        /// For all selected edges, applies the segment generators by inserting the generated points
        /// as new segments.
        /// </summary>
        [Instructions(
            title: "Apply generator for selected edges",
            description: "For all selected edges, applies the segment generators by inserting the generated points as new segments.",
            shortcut: "V key"
        )]
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

        /// <summary>
        /// Assigns the current project to each selected shape editor target in the scene view. The
        /// project is then extruded as a 3D mesh and can be configured in the target inspector.
        /// </summary>
        [Instructions(
            title: "Assign project to targets",
            description: "Assigns the current project to each selected shape editor target in the scene view. The project is then extruded as a 3D mesh and can be configured in the target inspector."
        )]
        internal void UserAssignProjectToTargets()
        {
            var transform = Selection.activeTransform;
            if (transform)
            {
                var target = transform.GetComponent<IShapeEditorTarget>();
                if (target != null)
                {
                    Undo.RegisterCompleteObjectUndo((UnityEngine.Object)target, "Assign Project To Targets");
                    target.OnShapeEditorUpdateProject(project);
                }
            }
        }

        /// <summary>Clears the selection of all selectable objects in the project.</summary>
        [Instructions(
            title: "Clear selection",
            description: "Clears the selection of all selectable objects in the project.",
            shortcut: "Shift + D"
        )]
        internal void UserClearSelection()
        {
            project.ClearSelection();
        }

        /// <summary>Copies all fully selected shapes to the clipboard.</summary>
        [Instructions(
            title: "Copy selection",
            description: "Copies all fully selected shapes to the clipboard.",
            shortcut: "Ctrl + C"
        )]
        internal void UserCopy()
        {
            var clipboard = new ClipboardData();

            // add all fully selected shapes to the clipboard.
            foreach (var shape in ForEachSelectedShape())
                clipboard.shapes.Add(shape);

            EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(clipboard);
        }

        /// <summary>Creates a bevel in the scene using a scaled extrude target and selects it. It does this by setting the back scale to 0.5.</summary>
        [Instructions(
            title: "Create Bevel",
            description: "Creates a bevel in the scene using a scaled extrude target and selects it. It does this by setting the back scale to 0.5."
        )]
        internal void UserCreateBevelExtrudeTarget() => UserCreateShapeEditorTarget("Bevel Extrude", ShapeEditorTargetMode.ScaledExtrude, (g, t) => t.scaledExtrudeBackScale = 0.5f);

        /// <summary>Creates a bevel in the scene using a scaled extrude target and selects it. It does this by setting the back scale to 0.5.</summary>
        [Instructions(
            title: "Create Bevel",
            description: "Creates a bevel in the scene using a scaled extrude target and selects it. It does this by setting the back scale to 0.5."
        )]
        internal void UserCreateChiselBevelExtrudeTarget() => UserCreateChiselTarget("Bevel Extrude", ChiselTargetMode.ScaledExtrude, (g, t) => t.scaledExtrudeBackScale = 0.5f);

        /// <summary>Creates a curved staircase in the scene using a revolve extrude target and selects it. It does this by setting the spiral height to 0.75.</summary>
        [Instructions(
            title: "Create Curved Staircase",
            description: "Creates a curved staircase in the scene using a revolve extrude target and selects it. It does this by setting the spiral height to 0.75."
        )]
        internal void UserCreateChiselCurvedStaircaseExtrudeTarget() => UserCreateChiselTarget("Curved Staircase", ChiselTargetMode.RevolveExtrude, (g, t) => t.revolveExtrudeHeight = 0.75f);

        /// <summary>Creates a fixed distance extrude target in the scene and selects it.</summary>
        [Instructions(
            title: "Create Extruded",
            description: "Creates a fixed distance extrude target in the scene and selects it."
        )]
        internal void UserCreateChiselFixedExtrudeTarget() => UserCreateChiselTarget("Fixed Extrude", ChiselTargetMode.FixedExtrude);

        /// <summary>Creates a linear staircase target in the scene and selects it.</summary>
        [Instructions(
            title: "Create Linear Staircase",
            description: "Creates a linear staircase target in the scene and selects it."
        )]
        internal void UserCreateChiselLinearStaircaseExtrudeTarget() => UserCreateChiselTarget("Linear Staircase", ChiselTargetMode.LinearStaircase);

        /// <summary>Creates a revolve chopped target in the scene and selects it.</summary>
        [Instructions(
            title: "Create Revolve Chopped",
            description: "Creates a revolve chopped target in the scene and selects it."
        )]
        internal void UserCreateChiselRevolveChoppedExtrudeTarget() => UserCreateChiselTarget("Revolve Chopped", ChiselTargetMode.RevolveChopped);

        /// <summary>Creates a revolved target in the scene and selects it.</summary>
        [Instructions(
            title: "Create Revolved",
            description: "Creates a revolved target in the scene and selects it."
        )]
        internal void UserCreateChiselRevolvedExtrudeTarget() => UserCreateChiselTarget("Revolve Extrude", ChiselTargetMode.RevolveExtrude);

        /// <summary>Creates a scaled target in the scene and selects it.</summary>
        [Instructions(
            title: "Create Pyramid",
            description: "Creates a pyramid target in the scene and selects it."
        )]
        internal void UserCreateChiselScaledExtrudeTarget() => UserCreateChiselTarget("Scaled Extrude", ChiselTargetMode.ScaledExtrude);

        /// <summary>Creates a slope in the scene using a linear staircase target and selects it. It does this by checking the sloped checkbox.</summary>
        [Instructions(
            title: "Create Slope",
            description: "Creates a slope in the scene using a linear staircase target and selects it. It does this by checking the sloped checkbox."
        )]
        internal void UserCreateChiselSlopeExtrudeTarget() => UserCreateChiselTarget("Slope Extrude", ChiselTargetMode.LinearStaircase, (g, t) => t.linearStaircaseSloped = true);

        /// <summary>Creates a spiral in the scene using a revolve extrude target and selects it. It does this by checking the sloped checkbox and setting the spiral height to 0.75.</summary>
        [Instructions(
            title: "Create Spiral",
            description: "Creates a spiral in the scene using a revolve extrude target and selects it. It does this by checking the sloped checkbox and setting the spiral height to 0.75."
        )]
        internal void UserCreateChiselSpiralExtrudeTarget() => UserCreateChiselTarget("Spiral Extrude", ChiselTargetMode.RevolveExtrude, (g, t) => { t.revolveExtrudeHeight = 0.75f; t.revolveExtrudeSloped = true; });

        /// <summary>Creates a curved staircase in the scene using a revolve extrude target and selects it. It does this by setting the spiral height to 0.75.</summary>
        [Instructions(
            title: "Create Curved Staircase",
            description: "Creates a curved staircase in the scene using a revolve extrude target and selects it. It does this by setting the spiral height to 0.75."
        )]
        internal void UserCreateCurvedStaircaseExtrudeTarget() => UserCreateShapeEditorTarget("Curved Staircase", ShapeEditorTargetMode.RevolveExtrude, (g, t) => t.revolveExtrudeHeight = 0.75f);

        /// <summary>Creates a fixed distance extrude target in the scene and selects it.</summary>
        [Instructions(
            title: "Create Extruded",
            description: "Creates a fixed distance extrude target in the scene and selects it."
        )]
        internal void UserCreateFixedExtrudeTarget() => UserCreateShapeEditorTarget("Fixed Extrude", ShapeEditorTargetMode.FixedExtrude);

        /// <summary>Creates a linear staircase target in the scene and selects it.</summary>
        [Instructions(
            title: "Create Linear Staircase",
            description: "Creates a linear staircase target in the scene and selects it."
        )]
        internal void UserCreateLinearStaircaseExtrudeTarget() => UserCreateShapeEditorTarget("Linear Staircase", ShapeEditorTargetMode.LinearStaircase);

        /// <summary>Creates a polygon target in the scene and selects it.</summary>
        [Instructions(
            title: "Create Polygon",
            description: "Creates a polygon target in the scene and selects it."
        )]
        internal void UserCreatePolygonTarget() => UserCreateShapeEditorTarget("Polygon", ShapeEditorTargetMode.Polygon);

        /// <summary>Creates a bevel in the scene using a scaled extrude target and selects it. It does this by setting the back scale to 0.5.</summary>
        [Instructions(
            title: "Create Bevel",
            description: "Creates a bevel in the scene using a scaled extrude target and selects it. It does this by setting the back scale to 0.5."
        )]
        internal void UserCreateRealtimeCsgBevelExtrudeTarget() => UserCreateRealtimeCSGTarget("Bevel Extrude", RealtimeCSGTargetMode.ScaledExtrude, (g, t) => t.scaledExtrudeBackScale = 0.5f);

        /// <summary>Creates a curved staircase in the scene using a revolve extrude target and selects it. It does this by setting the spiral height to 0.75.</summary>
        [Instructions(
            title: "Create Curved Staircase",
            description: "Creates a curved staircase in the scene using a revolve extrude target and selects it. It does this by setting the spiral height to 0.75."
        )]
        internal void UserCreateRealtimeCsgCurvedStaircaseExtrudeTarget() => UserCreateRealtimeCSGTarget("Curved Staircase", RealtimeCSGTargetMode.RevolveExtrude, (g, t) => t.revolveExtrudeHeight = 0.75f);

        /// <summary>Creates a fixed distance extrude target in the scene and selects it.</summary>
        [Instructions(
            title: "Create Extruded",
            description: "Creates a fixed distance extrude target in the scene and selects it."
        )]
        internal void UserCreateRealtimeCsgFixedExtrudeTarget() => UserCreateRealtimeCSGTarget("Fixed Extrude", RealtimeCSGTargetMode.FixedExtrude);

        /// <summary>Creates a linear staircase target in the scene and selects it.</summary>
        [Instructions(
            title: "Create Linear Staircase",
            description: "Creates a linear staircase target in the scene and selects it."
        )]
        internal void UserCreateRealtimeCsgLinearStaircaseExtrudeTarget() => UserCreateRealtimeCSGTarget("Linear Staircase", RealtimeCSGTargetMode.LinearStaircase);

        /// <summary>Creates a revolve chopped target in the scene and selects it.</summary>
        [Instructions(
            title: "Create Revolve Chopped",
            description: "Creates a revolve chopped target in the scene and selects it."
        )]
        internal void UserCreateRealtimeCsgRevolveChoppedExtrudeTarget() => UserCreateRealtimeCSGTarget("Revolve Chopped", RealtimeCSGTargetMode.RevolveChopped);

        /// <summary>Creates a revolved target in the scene and selects it.</summary>
        [Instructions(
            title: "Create Revolved",
            description: "Creates a revolved target in the scene and selects it."
        )]
        internal void UserCreateRealtimeCsgRevolvedExtrudeTarget() => UserCreateRealtimeCSGTarget("Revolve Extrude", RealtimeCSGTargetMode.RevolveExtrude);

        /// <summary>Creates a scaled target in the scene and selects it.</summary>
        [Instructions(
            title: "Create Pyramid",
            description: "Creates a pyramid target in the scene and selects it."
        )]
        internal void UserCreateRealtimeCsgScaledExtrudeTarget() => UserCreateRealtimeCSGTarget("Scaled Extrude", RealtimeCSGTargetMode.ScaledExtrude);

        /// <summary>Creates a slope in the scene using a linear staircase target and selects it. It does this by checking the sloped checkbox.</summary>
        [Instructions(
            title: "Create Slope",
            description: "Creates a slope in the scene using a linear staircase target and selects it. It does this by checking the sloped checkbox."
        )]
        internal void UserCreateRealtimeCsgSlopeExtrudeTarget() => UserCreateRealtimeCSGTarget("Slope Extrude", RealtimeCSGTargetMode.LinearStaircase, (g, t) => t.linearStaircaseSloped = true);

        /// <summary>Creates a spiral in the scene using a revolve extrude target and selects it. It does this by checking the sloped checkbox and setting the spiral height to 0.75.</summary>
        [Instructions(
            title: "Create Spiral",
            description: "Creates a spiral in the scene using a revolve extrude target and selects it. It does this by checking the sloped checkbox and setting the spiral height to 0.75."
        )]
        internal void UserCreateRealtimeCsgSpiralExtrudeTarget() => UserCreateRealtimeCSGTarget("Spiral Extrude", RealtimeCSGTargetMode.RevolveExtrude, (g, t) => { t.revolveExtrudeHeight = 0.75f; t.revolveExtrudeSloped = true; });

        /// <summary>Creates a spline target in the scene and selects it.</summary>
        [Instructions(
            title: "Create Spline (Experimental!)",
            description: "Creates a spline target in the scene and selects it."
        )]
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

        /// <summary>Creates a revolve chopped target in the scene and selects it.</summary>
        [Instructions(
            title: "Create Revolve Chopped",
            description: "Creates a revolve chopped target in the scene and selects it."
        )]
        internal void UserCreateRevolveChoppedExtrudeTarget() => UserCreateShapeEditorTarget("Revolve Chopped", ShapeEditorTargetMode.RevolveChopped);

        /// <summary>Creates a revolved target in the scene and selects it.</summary>
        [Instructions(
            title: "Create Revolved",
            description: "Creates a revolved target in the scene and selects it."
        )]
        internal void UserCreateRevolvedExtrudeTarget() => UserCreateShapeEditorTarget("Revolve Extrude", ShapeEditorTargetMode.RevolveExtrude);

        /// <summary>Creates a scaled target in the scene and selects it.</summary>
        [Instructions(
            title: "Create Pyramid",
            description: "Creates a pyramid target in the scene and selects it."
        )]
        internal void UserCreateScaledExtrudeTarget() => UserCreateShapeEditorTarget("Scaled Extrude", ShapeEditorTargetMode.ScaledExtrude);

        /// <summary>Creates a slope in the scene using a linear staircase target and selects it. It does this by checking the sloped checkbox.</summary>
        [Instructions(
            title: "Create Slope",
            description: "Creates a slope in the scene using a linear staircase target and selects it. It does this by checking the sloped checkbox."
        )]
        internal void UserCreateSlopeExtrudeTarget() => UserCreateShapeEditorTarget("Slope Extrude", ShapeEditorTargetMode.LinearStaircase, (g, t) => t.linearStaircaseSloped = true);

        /// <summary>Creates a spiral in the scene using a revolve extrude target and selects it. It does this by checking the sloped checkbox and setting the spiral height to 0.75.</summary>
        [Instructions(
            title: "Create Spiral",
            description: "Creates a spiral in the scene using a revolve extrude target and selects it. It does this by checking the sloped checkbox and setting the spiral height to 0.75."
        )]
        internal void UserCreateSpiralExtrudeTarget() => UserCreateShapeEditorTarget("Spiral Extrude", ShapeEditorTargetMode.RevolveExtrude, (g, t) => { t.revolveExtrudeHeight = 0.75f; t.revolveExtrudeSloped = true; });

        /// <summary>Creates a spline target in the scene and selects it.</summary>
        [Instructions(
            title: "Create Spline",
            description: "Creates a spline target in the scene and selects it."
        )]
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

        /// <summary>Deletes the selected objects from the project.</summary>
        [Instructions(
            title: "Delete selection",
            description: "Deletes the selected objects from the project.",
            shortcut: "Delete or X key"
        )]
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

        /// <summary>
        /// Finds all fully selected shapes, clears the current selection, adds a copy of the shapes
        /// to the project, selects the new shapes and switches to a single-use translation tool.
        /// </summary>
        [Instructions(
            title: "Duplicate selected shapes",
            description: "Finds all fully selected shapes, clears the current selection, adds a copy of the shapes to the project, selects the new shapes and switches to a single-use translation tool.",
            shortcut: "Ctrl + D"
        )]
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

        /// <summary>Closes the shape editor window.</summary>
        [Instructions(
            title: "Exit",
            description: "Closes the shape editor window."
        )]
        internal void UserExitShapeEditor()
        {
            Close();
        }

        /// <summary>
        /// Flips the selection horizontally against the leftmost position of all selected elements.
        /// </summary>
        [Instructions(
            title: "Flip selection horizontally",
            description: "Flips the selection horizontally against the leftmost position of all selected elements.",
            shortcut: "Shift + H"
        )]
        internal void UserFlipSelectionHorizonally()
        {
            RegisterUndo("Flip Selection Horizonally");

            var left = float.MaxValue;
            var right = float.MinValue;
            foreach (var selectable in ForEachSelectedObject())
            {
                if (selectable.position.x < left)
                    left = selectable.position.x;
                if (selectable.position.x > right)
                    right = selectable.position.x;
            }

            foreach (var selectable in ForEachSelectedObject())
            {
                selectable.position = new float2(-selectable.position.x + left + right, selectable.position.y);
                if (selectable is Segment segment && segment.next.selected)
                    segment.generator.FlipDirection();
            }
        }

        /// <summary>
        /// Flips the selection vertically against the topmost position of all selected elements.
        /// </summary>
        [Instructions(
            title: "Flip selection vertically",
            description: "Flips the selection vertically against the topmost position of all selected elements.",
            shortcut: "Shift + V"
        )]
        internal void UserFlipSelectionVertically()
        {
            RegisterUndo("Flip Selection Vertically");

            var top = float.MaxValue;
            var bottom = float.MinValue;
            foreach (var selectable in ForEachSelectedObject())
            {
                if (selectable.position.y < top)
                    top = selectable.position.y;
                if (selectable.position.y > bottom)
                    bottom = selectable.position.y;
            }

            foreach (var selectable in ForEachSelectedObject())
            {
                selectable.position = new float2(selectable.position.x, -selectable.position.y + top + bottom);
                if (selectable is Segment segment && segment.next.selected)
                    segment.generator.FlipDirection();
            }
        }

        /// <summary>Inverts the selection all of the selectable objects in the project.</summary>
        [Instructions(
            title: "Invert selection",
            description: "Inverts the selection all of the selectable objects in the project.",
            shortcut: "Ctrl + I"
        )]
        internal void UserInvertSelection()
        {
            project.InvertSelection();
        }

        /// <summary>
        /// Pushes the current project to the undo stack and starts a new project containing only a
        /// default cube.
        /// </summary>
        [Instructions(
            title: "New project",
            description: "Pushes the current project to the undo stack and starts a new project containing only a default cube."
        )]
        internal void UserNewProject()
        {
            NewProject();
        }

        /// <summary>Opens the GitHub repository in a browser window.</summary>
        [Instructions(
            title: "Open GitHub repository",
            description: "Opens the GitHub repository in a browser window."
        )]
        internal void UserOpenGitHubRepository()
        {
            Application.OpenURL("https://github.com/Henry00IS/ShapeEditor");
        }

        /// <summary>Opens the GitHub repository wiki in a browser window.</summary>
        [Instructions(
            title: "Open online manual",
            description: "Opens the GitHub repository wiki in a browser window."
        )]
        internal void UserOpenOnlineManual()
        {
            Application.OpenURL("https://github.com/Henry00IS/ShapeEditor/wiki");
        }

        /// <summary>Displays a file open dialog to load a project file.</summary>
        [Instructions(
            title: "Open project",
            description: "Displays a file open dialog to load a project file."
        )]
        internal void UserOpenProject()
        {
            string path = EditorUtility.OpenFilePanel("Load 2D Shape Editor Project", "", "s2d,sabre2d");
            if (path.Length != 0)
            {
                OpenProject(path);
            }
        }

        /// <summary>Clears the current selection, pastes shapes from the clipboard into the project and selects them.</summary>
        [Instructions(
            title: "Paste shapes",
            description: "Clears the current selection, pastes shapes from the clipboard into the project and selects them.",
            shortcut: "Ctrl + V"
        )]
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

        /// <summary>Pushes the selected shapes to the back (for the order of boolean operations).</summary>
        [Instructions(
            title: "Push selected shapes to the back",
            description: "Pushes the selected shapes to the back. This changes the order of Boolean operations, since the objects that are behind others will be processed first."
        )]
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

        /// <summary>Pushes the selected shapes to the front (for boolean operations).</summary>
        [Instructions(
            title: "Push selected shapes to the front",
            description: "Pushes the selected shapes to the front. This changes the order of Boolean operations, since the objects that are in front of others will be processed last."
        )]
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

        /// <summary>Redoes a previously undone action. Called on CTRL+Y when the editor window has focus.</summary>
        [Instructions(
            title: "Redo",
            description: "Redoes a previously undone action.",
            shortcut: "Ctrl + Y"
        )]
        internal void UserRedo()
        {
            OnRedo();
        }

        /// <summary>Centers the camera by resetting the grid offset and zoom.</summary>
        [Instructions(
            title: "Reset camera",
            description: "Centers the camera by resetting the grid offset and zoom.",
            shortcut: "H key"
        )]
        internal void UserResetCamera()
        {
            GridResetOffset();
            GridResetZoom();
        }

        /// <summary>Resets the segment generators to be linear for all selected edges.</summary>
        [Instructions(
            title: "Reset edge to linear",
            description: "Resets the segment generators to be linear for all selected edges."
        )]
        internal void UserResetSegmentGeneratorForSelectedEdges()
        {
            RegisterUndo("Reset Edge To Linear");

            foreach (var segment in ForEachSelectedEdge())
                segment.generator = new SegmentGenerator(segment);
        }

        /// <summary>Displays a file save dialog to save a project file.</summary>
        [Instructions(
            title: "Save project as",
            description: "Displays a file save dialog to save a project file.",
            shortcut: "Ctrl + S"
        )]
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

        /// <summary>Selects all of the selectable objects in the project.</summary>
        [Instructions(
            title: "Select all",
            description: "Selects all of the selectable objects in the project.",
            shortcut: "Ctrl + A"
        )]
        internal void UserSelectAll()
        {
            project.SelectAll();
        }

        /// <summary>Marks the selected shapes as additive. This changes the behaviour of Boolean operations.</summary>
        [Instructions(
            title: "Set selected shapes additive",
            description: "Marks the selected shapes as additive. This changes the behaviour of Boolean operations."
        )]
        internal void UserSetSelectedShapesAdditive()
        {
            foreach (var shape in ForEachSelectedShape())
                shape.booleanOperator = PolygonBooleanOperator.Union;
        }

        /// <summary>Marks the selected shapes as subtractive. This changes the behaviour of Boolean operations.</summary>
        [Instructions(
            title: "Set selected shapes subtractive",
            description: "Marks the selected shapes as subtractive. This changes the behaviour of Boolean operations."
        )]
        internal void UserSetSelectedShapesSubtractive()
        {
            foreach (var shape in ForEachSelectedShape())
                shape.booleanOperator = PolygonBooleanOperator.Difference;
        }

        /// <summary>Displays the about window.</summary>
        [Instructions(
            title: "About",
            description: "Displays the about window."
        )]
        internal void UserShowAboutWindow()
        {
            OpenWindow(new AboutWindow());
        }

        /// <summary>Displays the arch inspector window.</summary>
        [Instructions(
            title: "Arch inspector",
            description: "Displays the arch inspector window. Allows for modification of arch segment generator parameters."
        )]
        internal void UserShowArchInspectorWindow()
        {
            OpenWindow(new ArchInspectorWindow());
        }

        /// <summary>Displays the background settings window.</summary>
        [Instructions(
            title: "Background settings",
            description: "Displays the background settings window. Can load a background image that will be displayed behind the grid."
        )]
        internal void UserShowBackgroundSettingsWindow()
        {
            OpenWindow(new BackgroundSettingsWindow());
        }

        /// <summary>Displays the bezier inspector window.</summary>
        [Instructions(
            title: "Bezier inspector",
            description: "Displays the bezier inspector window. Allows for modification of bezier segment generator parameters."
        )]
        internal void UserShowBezierInspectorWindow()
        {
            OpenWindow(new BezierInspectorWindow());
        }

        /// <summary>Displays the circle generator window.</summary>
        [Instructions(
            title: "Circle generator",
            description: "Displays the circle generator window. Provides the ability to create circular shapes."
        )]
        internal void UserShowCircleGeneratorWindow()
        {
            OpenWindow(new CircleGeneratorWindow());
        }

        /// <summary>Displays the repeat inspector window.</summary>
        [Instructions(
            title: "Repeat inspector",
            description: "Displays the repeat inspector window. Allows for modification of repeat segment generator parameters."
        )]
        internal void UserShowRepeatInspectorWindow()
        {
            OpenWindow(new RepeatInspectorWindow());
        }

        /// <summary>Displays the shape inspector window.</summary>
        [Instructions(
            title: "Shape inspector",
            description: "Displays the shape inspector window. Provides additional behavior such as automatic symmetry on a per shape basis."
        )]
        internal void UserShowShapeInspectorWindow()
        {
            OpenWindow(new ShapeInspectorWindow());
        }

        /// <summary>Displays the sine inspector window.</summary>
        [Instructions(
            title: "Sine inspector",
            description: "Displays the sine inspector window. Allows for modification of sine segment generator parameters."
        )]
        internal void UserShowSineInspectorWindow()
        {
            OpenWindow(new SineInspectorWindow());
        }

        /// <summary>Snaps the selected objects to the grid.</summary>
        [Instructions(
            title: "Snap selection to grid",
            description: "Snaps the selected objects to the grid.",
            shortcut: "Shift + S"
        )]
        internal void UserSnapSelectionToGrid()
        {
            RegisterUndo("Snap Selection To Grid");
            foreach (var selectable in ForEachSelectedObject())
                selectable.position = selectable.position.Snap(gridSnap);
        }

        /// <summary>Switches to the box select tool unless already active.</summary>
        [Instructions(
            title: "Box Select Tool",
            description: "Switches to the box select tool. You can draw a rectangle while holding down the left mouse button. Any element that lies within this rectangle will be selected. Hold down the Shift key to select more elements. Hold down the Ctrl key to deselect elements.",
            shortcut: "Q key"
        )]
        internal void UserSwitchToBoxSelectTool() => SwitchTool(boxSelectTool);

        /// <summary>Switches to the cut tool unless already active.</summary>
        [Instructions(
            title: "Cut Tool",
            description: "Switches to the cutting tool. When you hover the mouse over a segment, you will see a small yellow cutting caret. When you click the left mouse button, the segment is split at this point and a new vertex is inserted.",
            shortcut: "C key single-use"
        )]
        internal void UserSwitchToCutTool() => SwitchTool(cutTool);

        /// <summary>Switches to the edge select mode.</summary>
        [Instructions(
            title: "Edge select mode",
            description: "Switches to edge select mode. Clicking the mouse near an edge will select it.",
            shortcut: "2 key"
        )]
        internal void UserSwitchToEdgeSelectMode() => shapeSelectMode = ShapeSelectMode.Edge;

        /// <summary>Switches to the face select mode.</summary>
        [Instructions(
            title: "Face select mode",
            description: "Switches to face select mode. Clicking the mouse inside of a shape will select it.",
            shortcut: "3 key"
        )]
        internal void UserSwitchToFaceSelectMode() => shapeSelectMode = ShapeSelectMode.Face;

        /// <summary>Switches to the rotate tool unless already active.</summary>
        [Instructions(
            title: "Rotate Tool",
            description: "Switches to the rotate tool. Once you have selected one or more items, a rotation gizmo appears. You can spin it around to rotate the selection.\n\nMost of the other tools allow you to rotate the selection with the R key as well.",
            shortcut: "R key"
        )]
        internal void UserSwitchToRotateTool() => SwitchTool(rotateTool);

        /// <summary>Switches to the scale tool unless already active.</summary>
        [Instructions(
            title: "Scale Tool",
            description: "Switches to the scale tool. Once you have selected one or more items, a scaling gizmo appears. You can use it to scale the selection.\n\nMost of the other tools allow you to scale the selection with the S key as well. Once activated, you can press X or Y to scale along that axis accordingly.",
            shortcut: "S key"
        )]
        internal void UserSwitchToScaleTool() => SwitchTool(scaleTool);

        /// <summary>Switches to the translate tool unless already active.</summary>
        [Instructions(
            title: "Translate Tool",
            description: "Switches to the translate tool. Once you have selected one or more items, a translation gizmo appears. You can drag it around to translate the selection.\n\nMost of the other tools allow you to translate the selection with the G key. Once activated, you can press X or Y to move along that axis accordingly.",
            shortcut: "W key"
        )]
        internal void UserSwitchToTranslateTool() => SwitchTool(translateTool);

        /// <summary>Switches to the vertex select mode.</summary>
        [Instructions(
            title: "Vertex select mode",
            description: "Switches to vertex select mode. Clicking the mouse near a vertex will select it.",
            shortcut: "1 key"
        )]
        internal void UserSwitchToVertexSelectMode() => shapeSelectMode = ShapeSelectMode.Vertex;

        /// <summary>
        /// Switches between the bezier and linear segment generator for all selected edges.
        /// </summary>
        [Instructions(
            title: "Toggle bezier generator",
            description: "Switches between the bezier and linear segment generator for all selected edges.",
            shortcut: "B key"
        )]
        internal void UserToggleBezierSegmentGeneratorForSelectedEdges()
        {
            RegisterUndo("Toggle Bezier Generator");

            foreach (var segment in ForEachSelectedEdge())
                segment.generator = new SegmentGenerator(segment, segment.generator.type != SegmentGeneratorType.Bezier ? SegmentGeneratorType.Bezier : SegmentGeneratorType.Linear);
        }

        /// <summary>Toggles whether grid snapping is enabled by default.</summary>
        [Instructions(
            title: "Toggle grid snapping",
            description: "Toggles whether grid snapping is enabled by default. Holding down the Ctrl key temporarily inverts this behavior."
        )]
        internal void UserToggleGridSnapping()
        {
            snapEnabled = !snapEnabled;
        }

        /// <summary>
        /// Switches between the arch and linear segment generator for all selected edges.
        /// </summary>
        [Instructions(
            title: "Toggle arch generator",
            description: "Switches between the arch and linear segment generator for all selected edges."
        )]
        internal void UserToggleArchSegmentGeneratorForSelectedEdges()
        {
            RegisterUndo("Toggle Arch Generator");

            foreach (var segment in ForEachSelectedEdge())
                segment.generator = new SegmentGenerator(segment, segment.generator.type != SegmentGeneratorType.Arch ? SegmentGeneratorType.Arch : SegmentGeneratorType.Linear);
        }

        /// <summary>
        /// Switches between the repeat and linear segment generator for all selected edges.
        /// </summary>
        [Instructions(
            title: "Toggle repeat generator",
            description: "Switches between the repeat and linear segment generator for all selected edges.",
            shortcut: "M key"
        )]
        internal void UserToggleRepeatSegmentGeneratorForSelectedEdges()
        {
            RegisterUndo("Toggle Repeat Generator");

            foreach (var segment in ForEachSelectedEdge())
                segment.generator = new SegmentGenerator(segment, segment.generator.type != SegmentGeneratorType.Repeat ? SegmentGeneratorType.Repeat : SegmentGeneratorType.Linear);
        }

        /// <summary>
        /// Switches between the sine and linear segment generator for all selected edges.
        /// </summary>
        [Instructions(
            title: "Toggle sine generator",
            description: "Switches between the sine and linear segment generator for all selected edges.",
            shortcut: "N key"
        )]
        internal void UserToggleSineSegmentGeneratorForSelectedEdges()
        {
            RegisterUndo("Toggle Sine Generator");

            foreach (var segment in ForEachSelectedEdge())
                segment.generator = new SegmentGenerator(segment, segment.generator.type != SegmentGeneratorType.Sine ? SegmentGeneratorType.Sine : SegmentGeneratorType.Linear);
        }

        /// <summary>Undoes a previous action. Called on CTRL+Z when the editor window has focus.</summary>
        [Instructions(
            title: "Undo",
            description: "Undoes a previous action.",
            shortcut: "Ctrl + Z"
        )]
        internal void UserUndo()
        {
            OnUndo();
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
            Undo.RegisterCreatedObjectUndo(go, "Create Chisel Target");
            go.transform.SetSiblingOfActiveTransform();

            var target = go.AddComponent<ChiselTarget>();
            target.targetMode = mode;
            init?.Invoke(go, target);
            target.OnShapeEditorUpdateProject(project);

            Selection.activeGameObject = go;
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
            Undo.RegisterCreatedObjectUndo(go, "Create RealtimeCSG Target");
            go.transform.SetSiblingOfActiveTransform();

            var target = go.AddComponent<RealtimeCSGTarget>();
            target.targetMode = mode;
            init?.Invoke(go, target);
            target.OnShapeEditorUpdateProject(project);

            Selection.activeGameObject = go;
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
            Undo.RegisterCreatedObjectUndo(go, "Create Shape Editor Target");
            go.transform.SetSiblingOfActiveTransform();

            var target = go.AddComponent<ShapeEditorTarget>();
            target.targetMode = mode;
            init?.Invoke(go, target);
            target.OnShapeEditorUpdateProject(project);

            Selection.activeGameObject = go;
        }
    }
}

#endif