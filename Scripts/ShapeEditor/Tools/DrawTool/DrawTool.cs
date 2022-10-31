#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class DrawTool : Tool
    {
        private static readonly Color drawIndicatorColor = new Color(1.0f, 0.5f, 0.0f);
        private Segment selectedSegment1 = null;
        private Segment selectedSegment2 = null;
        private State state = new State();

        /// <summary>Represents a state the tool can be in.</summary>
        private class State
        {
            /// <summary>The tool that uses this state.</summary>
            public DrawTool tool;

            /// <summary>The shape editor window.</summary>
            public ShapeEditorWindow editor;

            /// <summary>
            /// Gets whether the tool is busy and has to maintain the input focus, making it
            /// impossible to switch to another object.
            /// </summary>
            public virtual bool IsBusy()
            {
                return false;
            }

            /// <summary>Called when the tool enters this state.</summary>
            public virtual void OnStateEnter()
            {
            }

            /// <summary>Called when the tool exits this state.</summary>
            public virtual void OnStateExit()
            {
            }

            /// <summary>Called when the tool is rendered.</summary>
            public virtual void OnRender()
            {
            }

            /// <summary>Called when the tool receives a mouse down event.</summary>
            public virtual void OnMouseDown(int button)
            {
            }

            /// <summary>Called when the tool receives a mouse move event.</summary>
            public virtual void OnMouseMove(float2 screenDelta, float2 gridDelta)
            {
            }

            /// <summary>Called when the tool receives a key down event.</summary>
            public virtual bool OnKeyDown(KeyCode keyCode)
            {
                return false;
            }

            /// <summary>
            /// Exits the tool (only call this method when the tool is in single-use mode).
            /// </summary>
            public void ExitSingleUseTool()
            {
                // switch to a dummy state that is not busy.
                tool.GotoState(new State());

                // exit the single-use tool.
                editor.SwitchTool(tool.parent);
            }
        }

        /// <summary>Switches the tool to the specified next state.</summary>
        /// <param name="next">The next state the tool will switch to.</param>
        private void GotoState(State next)
        {
            // provide the next state with important tool details.
            next.tool = this;
            next.editor = editor;

            // exit the current state.
            state.OnStateExit();

            // switch to the next state.
            state = next;

            // enter the next state.
            state.OnStateEnter();
        }

        public override bool IsBusy()
        {
            return state.IsBusy();
        }

        public override void OnActivate()
        {
            if (isSingleUse)
            {
                // find a single selected edge in the project and if found:
                if (FindSingleSelectedEdgeInProject())
                {
                    editor.project.ClearSelection();
                    GotoState(new EdgeDrawState());
                }
                // without such an edge enter free-draw mode:
                else
                {
                    editor.project.ClearSelection();
                    GotoState(new FreeDrawState());
                }
            }
            // have the user select an edge first:
            else
            {
                editor.project.ClearSelection();
                GotoState(new FindEdgeState());
            }
        }

        public override void OnRender()
        {
            editor.SetMouseCursor(ShapeEditorResources.Instance.shapeEditorMouseCursorPencil);

            state.OnRender();
        }

        public override void OnMouseMove(float2 screenDelta, float2 gridDelta)
        {
            state.OnMouseMove(screenDelta, gridDelta);
        }

        public override void OnMouseDown(int button)
        {
            state.OnMouseDown(button);
        }

        public override bool OnKeyDown(KeyCode keyCode)
        {
            return state.OnKeyDown(keyCode);
        }

        /// <summary>
        /// Attempts to find a single selected edge in the project. If found <see
        /// cref="selectedSegment1"/> and <see cref="selectedSegment2"/> will be set accordingly and
        /// if no edge could be found they are set to null.
        /// </summary>
        /// <returns>Returns false if none (or more that one) are found else true.</returns>
        private bool FindSingleSelectedEdgeInProject()
        {
            selectedSegment1 = null;
            selectedSegment2 = null;

            // find the selected edge that can be drawn on.
            if (editor.selectedSegmentsCount == 2)
            {
                foreach (var edge in editor.ForEachSelectedEdge())
                {
                    selectedSegment1 = edge;
                    selectedSegment2 = edge.next;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the <see cref="ShapeEditorWindow.mouseGridPosition"/> with optional grid snapping.
        /// </summary>
        private float2 mouseSnappedGridPosition
        {
            get
            {
                var mouseGridPosition = editor.mouseGridPosition;

                // optionally snap to the grid.
                if (editor.isSnapping)
                {
                    mouseGridPosition = mouseGridPosition.Snap(editor.gridSnap);
                }

                return mouseGridPosition;
            }
        }

        /// <summary>
        /// Gets the <see cref="ShapeEditorWindow.mousePosition"/> with optional grid snapping.
        /// </summary>
        private float2 mouseSnappedPosition
        {
            get
            {
                var mousePosition = editor.mousePosition;

                // optionally snap to the grid.
                if (editor.isSnapping)
                {
                    mousePosition = editor.GridPointToScreen(editor.mouseGridPosition.Snap(editor.gridSnap));
                }

                return mousePosition;
            }
        }
    }
}

#endif