#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A window title control that allows the user to drag the window around.</summary>
    public class GuiWindowTitle : GuiControl
    {
        private const float height = 20f;
        private static Color titleBackgroundColor = new Color(0.235f, 0.235f, 0.235f, 0.75f);

        /// <summary>The window title.</summary>
        public string title;

        private bool isDragging;

        private GuiButton closeButton;

        public GuiWindowTitle(string title) : base(new float2(1f, 1f), new float2(1f, height))
        {
            this.title = title;
        }

        public override void OnActivate()
        {
            base.OnActivate();

            Add(closeButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorDelete, 20, OnCloseButtonClicked));
        }

        public override void OnRender()
        {
            size = new float2(parent.size.x - 2f, height);
            closeButton.position = new float2(size.x - 20f, 0f);

            GLUtilities.DrawGui(() =>
            {
                GL.Color(titleBackgroundColor);
                GLUtilities.DrawRectangle(drawPosition.x, drawPosition.y, size.x, 20f);
            });

            GLUtilities.DrawGuiText(ShapeEditorResources.fontSegoeUI14, title, drawPosition + new float2(3f, 3f));

            if ((isMouseHoverEffectApplicable || isDragging) && !closeButton.isMouseOver)
            {
                parent.editor.SetMouseCursor(UnityEditor.MouseCursor.MoveArrow);
            }

            base.OnRender();
        }

        public override void OnMouseDown(int button)
        {
            if (button == 0 && !closeButton.isMouseOver)
            {
                isDragging = true;
            }

            base.OnMouseDown(button);
        }

        public override void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (editor.isLeftMousePressed && isDragging)
            {
                parent.position += screenDelta;
            }

            base.OnMouseDrag(button, screenDelta, gridDelta);
        }

        public override void OnGlobalMouseUp(int button)
        {
            if (button == 0)
            {
                isDragging = false;
            }

            base.OnGlobalMouseUp(button);
        }

        private void OnCloseButtonClicked()
        {
            if (parent is GuiWindow window)
            {
                window.Close();
            }
        }

        public override bool IsBusy()
        {
            return isDragging;
        }
    }
}

#endif