#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A window title control that allows the user to drag the window around.</summary>
    public class GuiWindowTitle : GuiControl
    {
        private const float height = 20f;

        /// <summary>The window title.</summary>
        public string title;

        private bool isDragging;

        public GuiWindowTitle(string title) : base(new float2(1f, 1f), new float2(1f, height))
        {
            this.title = title;
        }

        public override void OnRender()
        {
            size = new float2(parent.size.x - 2f, height);

            GLUtilities.DrawGui(() =>
            {
                GL.Color(new Color(0.235f, 0.235f, 0.235f, 0.75f));
                GLUtilities.DrawRectangle(drawPosition.x, drawPosition.y, size.x, 20f);
            });

            GLUtilities.DrawGuiText(ShapeEditorResources.fontSegoeUI14, title, drawPosition + new float2(3f, 3f));

            if (isMouseHoverEffectApplicable || isDragging)
            {
                parent.editor.SetMouseCursor(UnityEditor.MouseCursor.MoveArrow);
            }
        }

        public override void OnMouseDown(int button)
        {
            if (button == 0)
            {
                isDragging = true;
            }
        }

        public override void OnMouseDrag(int button, float2 screenDelta)
        {
            if (button == 0)
            {
                parent.position += screenDelta;
            }
        }

        public override void OnGlobalMouseUp(int button)
        {
            if (button == 0)
            {
                isDragging = false;
            }
        }
    }
}

#endif