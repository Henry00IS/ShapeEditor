﻿#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorWindow
    {
        /// <summary>The currently active viewport tool.</summary>
        internal Tool activeTool;

        /// <summary>The shape select mode (e.g. vertex or edge or face select).</summary>
        internal ShapeSelectMode shapeSelectMode = ShapeSelectMode.Vertex;

        private BoxSelectTool boxSelectTool;
        private TranslateTool translateTool;
        private RotateTool rotateTool;
        private ScaleTool scaleTool;
        private DrawTool drawTool;
        private CutTool cutTool;
        private MeasuringTapeTool measuringTapeTool;

        /// <summary>Ensures that a valid tools always exists, to handle C# reloads.</summary>
        private void ValidateTools()
        {
            if (boxSelectTool == null)
            {
                boxSelectTool = new BoxSelectTool();
                translateTool = new TranslateTool();
                rotateTool = new RotateTool();
                scaleTool = new ScaleTool();
                drawTool = new DrawTool();
                cutTool = new CutTool();
                measuringTapeTool = new MeasuringTapeTool();
            }

            if (activeTool == null)
                SwitchTool(boxSelectTool);
        }

        /// <summary>Switches the from current tool to the specified tool.</summary>
        /// <param name="tool">The tool to switch to.</param>
        internal void SwitchTool(Tool tool)
        {
            // if the tool is unchanged we ignore this call.
            if (activeTool == tool) return;

            // if there was an active tool we deactivate it.
            if (activeTool != null)
                activeTool.OnDeactivate();

            // clear any widgets the tool was using.
            ClearWidgets();

            // switch to the new tool and activate it.
            tool.editor = this;
            activeTool = tool;
            activeTool.OnActivate();

            // todo: this needs to be checked first.
            TrySwitchActiveEventReceiver(tool);
        }

        /// <summary>
        /// This function switches to the specified single-use tool and returns to the current tool
        /// when it's done. This is useful for single-use tools that are instantiated with a
        /// keyboard binding.
        /// </summary>
        /// <param name="tool">The single-use tool to switch to.</param>
        internal void UseTool(Tool tool)
        {
            // prevent accidental errors.
            if (activeTool == tool) { Debug.LogWarning("Cannot UseTool() the already active tool!"); return; }

            // set the parent of the tool to be the currently active tool.
            tool.parent = activeTool;

            // switch to the new tool.
            SwitchTool(tool);
        }

        /// <summary>Draws the active tool.</summary>
        private void DrawTool()
        {
            if (activeTool != null)
                activeTool.OnRender();
        }
    }
}

#endif