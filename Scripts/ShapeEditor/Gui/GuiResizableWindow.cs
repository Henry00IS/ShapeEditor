#if UNITY_EDITOR

using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a resizable floating window inside of the 2D Shape Editor.</summary>
    public class GuiResizableWindow : GuiWindow
    {
        /// <summary>Whether the user is currently resizing the window.</summary>
        protected bool isResizing = false;
        /// <summary>The direction to resize towards (depending on the border location clicked).</summary>
        protected CompassRose resizingDirection;
        /// <summary>The virtual rectangle that is getting resized to keep mouse positions consistent.</summary>
        protected float4 resizeRect;
        /// <summary>The initial window position for anchoring.</summary>
        protected float2 resizeInitialPosition;

        /// <summary>Gets or sets the minimum size of the window.</summary>
        public float2 minSize { get; set; }

        /// <summary>Creates a new window at the specified position of the specified size.</summary>
        /// <param name="position">The window position in screen coordinates.</param>
        /// <param name="size">The window size in screen coordinates.</param>
        public GuiResizableWindow(float2 position, float2 size) : base(position, size)
        {
            // use the initial window size as the minimum size by default.
            minSize = size;
        }

        public override bool IsBusy()
        {
            // always busy while resizing.
            return isResizing;
        }

        /// <summary>Called when the window is rendered.</summary>
        public override void OnRender()
        {
            if (isResizing)
            {
                editor.SetMouseCursor(resizingDirection);
            }
            else if (isMouseHoverEffectApplicable)
            {
                if (IsMouseOnWindowBorder(out var direction))
                {
                    editor.SetMouseCursor(direction);
                }
            }

            // render the window and child containers.
            base.OnRender();
        }

        public override void OnMouseDown(int button)
        {
            if (button == 0 && IsMouseOnWindowBorder(out resizingDirection))
            {
                isResizing = true;
                resizeRect = new float4(position.x, position.y, size.x, size.y);

                // calculate anchor based on direction:
                resizeInitialPosition = position;

                // if resizing from the left (w, nw, sw), the anchor x must shift:
                if (resizingDirection == CompassRose.W || resizingDirection == CompassRose.NW || resizingDirection == CompassRose.SW)
                    resizeInitialPosition.x += size.x - minSize.x;

                // if resizing from the top (n, ne, nw), the anchor y must shift:
                if (resizingDirection == CompassRose.N || resizingDirection == CompassRose.NE || resizingDirection == CompassRose.NW)
                    resizeInitialPosition.y += size.y - minSize.y;
            }
            else
            {
                base.OnMouseDown(button);
            }
        }

        public override void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (!isResizing || button != 0)
            {
                base.OnMouseDrag(button, screenDelta, gridDelta);
                return;
            }

            switch (resizingDirection)
            {
                case CompassRose.N:
                    resizeRect.xy += new float2(0f, screenDelta.y);
                    resizeRect.zw -= new float2(0f, screenDelta.y);
                    break;

                case CompassRose.NE:
                    resizeRect.xy += new float2(0f, screenDelta.y);
                    resizeRect.zw += new float2(screenDelta.x, -screenDelta.y);
                    break;

                case CompassRose.E:
                    resizeRect.zw += new float2(screenDelta.x, 0f);
                    break;

                case CompassRose.SE:
                    resizeRect.zw += screenDelta;
                    break;

                case CompassRose.S:
                    resizeRect.zw += new float2(0f, screenDelta.y);
                    break;

                case CompassRose.SW:
                    resizeRect.xy += new float2(screenDelta.x, 0f);
                    resizeRect.zw += new float2(-screenDelta.x, screenDelta.y);
                    break;

                case CompassRose.W:
                    resizeRect.xy += new float2(screenDelta.x, 0f);
                    resizeRect.zw -= new float2(screenDelta.x, 0f);
                    break;

                case CompassRose.NW:
                    resizeRect.xy += new float2(screenDelta.x, screenDelta.y);
                    resizeRect.zw -= new float2(screenDelta.x, screenDelta.y);
                    break;
            }

            var finalRect = resizeRect;
            if (finalRect.z < minSize.x)
            {
                finalRect.x = resizeInitialPosition.x;
                finalRect.z = minSize.x;
            }
            if (finalRect.w < minSize.y)
            {
                finalRect.y = resizeInitialPosition.y;
                finalRect.w = minSize.y;
            }

            position = finalRect.xy;
            size = finalRect.zw;

            OnResize();
        }

        public override void OnGlobalMouseUp(int button)
        {
            if (button == 0)
            {
                isResizing = false;
            }
        }

        /// <summary>Called when the window is resized.</summary>
        protected virtual void OnResize()
        {
        }

        /// <summary>Gets whether the mouse cursor is currently on the window border.</summary>
        /// <param name="direction">The resizing direction for the mouse position.</param>
        /// <returns>True when the mouse is on the border else false.</returns>
        protected bool IsMouseOnWindowBorder(out CompassRose direction)
        {
            return MathEx.GetBorderDirection(sizeRect, 1f, mousePosition, out direction);
        }
    }
}

#endif