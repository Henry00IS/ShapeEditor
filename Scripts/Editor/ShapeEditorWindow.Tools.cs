#if UNITY_EDITOR

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorWindow
    {
        /// <summary>The currently active viewport tool.</summary>
        internal Tool activeTool;

        private BoxSelectTool boxSelectTool;
        private TranslateTool translateTool;
        private RotateTool rotateTool;

        /// <summary>Ensures that a valid tools always exists, to handle C# reloads.</summary>
        private void ValidateTools()
        {
            if (boxSelectTool == null)
            {
                boxSelectTool = new BoxSelectTool();
                translateTool = new TranslateTool();
                rotateTool = new RotateTool();
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

            // switch to the new tool and activate it.
            tool.editor = this;
            activeTool = tool;
            activeTool.OnActivate();
        }

        /// <summary>Switches to the box select tool unless already active.</summary>
        internal void SwitchToBoxSelectTool() => SwitchTool(boxSelectTool);

        /// <summary>Switches to the translate tool unless already active.</summary>
        internal void SwitchToTranslateTool() => SwitchTool(translateTool);

        /// <summary>Switches to the rotate tool unless already active.</summary>
        internal void SwitchToRotateTool() => SwitchTool(rotateTool);

        /// <summary>Draws the active tool.</summary>
        private void DrawTool()
        {
            if (activeTool != null)
                activeTool.OnRender();
        }
    }
}

#endif