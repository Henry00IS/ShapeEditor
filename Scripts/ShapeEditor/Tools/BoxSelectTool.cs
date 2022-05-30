#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class BoxSelectTool : Tool
    {
        private bool isMarqueeActive;
        private bool isMarqueeSubtractive;
        private static readonly Color marqueeColor = new Color(1.0f, 0.5f, 0.0f);

        /// <summary>Used to cycle through selectable objects when the click position is unchanged.</summary>
        private float2 lastMouseUpPosition;
        /// <summary>The last selected shape in face select mode for cycling through shapes.</summary>
        private Shape lastSelectedShape;

        public override void OnActivate()
        {
            isMarqueeActive = false;
        }

        public override void OnRender()
        {
            GLUtilities.DrawGui(() =>
            {
                if (isMarqueeActive)
                {
                    // draw the marquee selection if active.
                    var marqueeBegin = editor.GridPointToScreen(editor.mouseGridInitialPosition);
                    var marqueeEnd = editor.GridPointToScreen(editor.mouseGridPosition);
                    var marqueeRect = MathEx.RectXYXY(marqueeBegin, marqueeEnd);

                    GLUtilities.DrawRectangleOutline(marqueeRect.x, marqueeRect.y, marqueeRect.width, marqueeRect.height, marqueeColor);
                }
            });
        }

        public override void OnGlobalMouseUp(int button)
        {
            if (button == 0)
            {
                // unless the shift key is held down we clear the selection.
                if (!editor.isShiftPressed && !isMarqueeSubtractive)
                    editor.project.ClearSelection();

                if (isMarqueeActive)
                {
                    // iterate over all segments within the marquee.
                    var marqueeRect = MathEx.RectXYXY(editor.mouseGridInitialPosition, editor.mouseGridPosition);
                    foreach (var segment in editor.ForEachSelectableInGridRect(marqueeRect))
                        segment.selected = !isMarqueeSubtractive;

                    // todo, check for edges in edge mode.
                    // todo, check for shapes in shape mode.
                }
                else
                {
                    switch (editor.shapeSelectMode)
                    {
                        case ShapeSelectMode.Vertex:
                            // find the closest segment to the click position.
                            var segment = editor.FindSegmentAtScreenPosition(editor.mousePosition, 60.0f);
                            if (segment != null)
                                segment.selected = !segment.selected;
                            break;

                        case ShapeSelectMode.Edge:
                            // find the closest edge to the click position.
                            var lineResult = editor.FindSegmentLineAtScreenPosition(editor.mousePosition, 60.0f);
                            if (lineResult != null)
                            {
                                if (lineResult.selected && lineResult.next.selected)
                                {
                                    lineResult.selected = false;
                                    lineResult.next.selected = false;
                                }
                                else
                                {
                                    lineResult.selected = true;
                                    lineResult.next.selected = true;
                                }
                            }
                            break;

                        case ShapeSelectMode.Face:

                            // when the mouse position changed we reset the selection cycle.
                            if (!editor.mousePosition.Equals(lastMouseUpPosition))
                                lastSelectedShape = null;

                            // find what shape the click position is inside of.
                            lastSelectedShape = editor.FindShapeAtGridPosition(editor.mouseGridPosition, lastSelectedShape);
                            if (lastSelectedShape != null)
                            {
                                if (lastSelectedShape.IsSelected())
                                    lastSelectedShape.ClearSelection();
                                else
                                    lastSelectedShape.SelectAll();
                            }
                            break;
                    }
                }

                lastMouseUpPosition = editor.mousePosition;
                isMarqueeActive = false;
            }
        }

        public override void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (button == 0)
            {
                // once the mouse moves a bit from the initial position the marquee activates.
                if (!isMarqueeActive)
                {
                    isMarqueeActive = (math.distance(editor.mouseInitialPosition, editor.mousePosition) > 3.0f);
                    isMarqueeSubtractive = editor.isCtrlPressed;
                }
            }
        }

        public override bool OnKeyDown(KeyCode keyCode)
        {
            // these single-use tools are not available while in single-use mode.
            if (isSingleUse) return false;

            switch (keyCode)
            {
                case KeyCode.G:
                    if (editor.selectedSegmentsCount > 0)
                    {
                        editor.UseTool(new TranslateTool());
                        return true;
                    }
                    return false;

                case KeyCode.S:
                    if (!editor.isModifierPressed && editor.selectedSegmentsCount > 1)
                    {
                        editor.UseTool(new ScaleTool());
                        return true;
                    }
                    return false;

                case KeyCode.R:
                    if (editor.selectedSegmentsCount > 1)
                    {
                        editor.UseTool(new RotateTool());
                        return true;
                    }
                    return false;

                case KeyCode.E:
                    if (editor.selectedSegmentsCount > 1)
                    {
                        editor.UseTool(new ExtrudeTool());
                        return true;
                    }
                    return false;

                case KeyCode.C:
                    editor.UseTool(new CutTool());
                    return true;

                case KeyCode.B:
                    if (editor.selectedSegmentsCount > 0)
                    {
                        editor.UserToggleBezierSegmentGeneratorForSelectedEdges();
                        return true;
                    }
                    return false;

                case KeyCode.N:
                    if (editor.selectedSegmentsCount > 0)
                    {
                        editor.UserToggleSineSegmentGeneratorForSelectedEdges();
                        return true;
                    }
                    return false;

                case KeyCode.M:
                    if (editor.selectedSegmentsCount > 0)
                    {
                        editor.UserToggleRepeatSegmentGeneratorForSelectedEdges();
                        return true;
                    }
                    return false;

                case KeyCode.V:
                    if (editor.selectedSegmentsCount > 0)
                    {
                        editor.UserApplyGeneratorForSelectedEdges();
                        return true;
                    }
                    return false;
            }
            return false;
        }
    }
}

#endif