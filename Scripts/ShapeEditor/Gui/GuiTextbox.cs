#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a textbox control inside of a window.</summary>
    public class GuiTextbox : GuiControl
    {
        /// <summary>The current user-modifiable text.</summary>
        public string text = "";
        /// <summary>The placeholder text when empty.</summary>
        public string placeholder = "";
        /// <summary>Whether this textbox hides all characters.</summary>
        public bool isPassword;
        /// <summary>The maximum amount of characters allowed.</summary>
        public int maxLength = 32767;
        /// <summary>Whether this textbox accepts text input.</summary>
        public bool isReadonly;
        /// <summary>The textbox font.</summary>
        public BmFont font;

        /// <summary>The caret character position in the text string.</summary>
        private int caretCharPosition = 0;
        /// <summary>The virtual scroll of text inside the textbox.</summary>
        private int scrollX;
        /// <summary>The caret blink timer used for drawing.</summary>
        private int timerCaretBlink;
        /// <summary>The character position where a selection begins.</summary>
        private int selectionBegin = 0;
        /// <summary>The character position where a selection ends.</summary>
        private int selectionEnd = 0;
        /// <summary>Used to vertical align text in the middle of the textbox.</summary>
        private float2 textVerticalAlign = new float2(0f, 3f);

        private Color backgroundColorDefault = new Color(255 / 255f, 255 / 255f, 255 / 255f);
        private Color backgroundColorReadonly = new Color(180 / 255f, 180 / 255f, 180 / 255f);
        private Color borderColorDefault = new Color(122 / 255f, 122 / 255f, 122 / 255f);
        private Color borderColorFocus = new Color(0 / 255f, 120 / 255f, 215 / 255f);
        private Color textColorDefault = new Color(0 / 255f, 0 / 255f, 0 / 255f);
        private Color textColorPlaceholder = new Color(167 / 255f, 167 / 255f, 167 / 255f);
        private Color textColorSelected = new Color(255 / 255f, 255 / 255f, 255 / 255f);
        private Color caretColorDefault = new Color(0 / 255f, 0 / 255f, 0 / 255f);
        private Color selectionColorDefault = new Color(0 / 255f, 120 / 255f, 215 / 255f);

        private Rect drawRect => new Rect(drawPosition, size);

        public GuiTextbox(float2 position, float2 size) : base(position, size)
        {
            font = ShapeEditorResources.fontSegoeUI14;
        }

        public override void OnMouseDown(int button)
        {
            if (button == 0)
            {
                // set the caret position.
                caretCharPosition = TextXToCaretCharPosition(Mathf.RoundToInt(parent.parent.mousePosition.x));

                // set the selection position.
                SelectionSet(caretCharPosition, caretCharPosition);

                // reset the caret blink timer.
                CaretResetBlink();

                // set the selection end position.
                SelectionSetEnd(caretCharPosition);
            }
        }

        public override void OnMouseDrag(int button)
        {
            if (button == 0)
            {
                // automatic scrolling when the mouse moves (for the edges on long text).

                // set the caret position.
                caretCharPosition = TextXToCaretCharPosition(Mathf.RoundToInt(parent.parent.mousePosition.x));

                // reset the caret blink timer.
                CaretResetBlink();

                // set the selection end position.
                SelectionSetEnd(caretCharPosition);
            }
        }

        public override void OnRender()
        {
            // process the caret blinking timer.
            timerCaretBlink++;
            timerCaretBlink %= 120;

            // handle scrolling within the textbox.
            // ensure that the caret is visible by scrolling the textbox.
            ScrollToCaret();

            // draw the textbox background color.
            var x1 = drawPosition.x;
            var y1 = drawPosition.y;
            var x2 = drawPosition.x + size.x;
            var y2 = drawPosition.y + size.y;
            var textDrawPosition = drawPosition + textVerticalAlign;

            var textboxBackgroundColor = isReadonly ? backgroundColorReadonly : backgroundColorDefault;

            GLUtilities.DrawGui(() =>
            {
                GL.Color(textboxBackgroundColor);
                GLUtilities.DrawRectangle(drawPosition.x, drawPosition.y, size.x, size.y);
            });

            // draw outside border that indicates whether the textbox has input focus.
            GLUtilities.DrawGui(() =>
            {
                GLUtilities.DrawRectangleOutline(drawPosition.x, drawPosition.y, size.x, size.y, isActive ? borderColorFocus : borderColorDefault);
            });

            // draw the placeholder inside.
            if (!isActive && placeholder.Length > 0 && text.Length == 0)
            {
                GLUtilities.DrawGuiClippedText(drawRect, font, placeholder, textDrawPosition, textColorPlaceholder);
            }

            // draw the text inside.
            GLUtilities.DrawGuiClippedText(drawRect, font, TextGetVisualString(), textDrawPosition + new float2(scrollX, 0f), textColorDefault);

            // draw the selection.
            if (isActive && HasSelection())
            {
                var selectionStringBegin = SelectionGetBegin();
                var selectionStringEnd = SelectionGetEnd();

                var selectionBeginX = scrollX + CharacterPositionGetDrawX(selectionStringBegin);
                var selectionEndX = scrollX + CharacterPositionGetDrawX(selectionStringEnd);

                // highlight the selection.
                GLUtilities.DrawGuiClipped(drawRect, () =>
                {
                    GL.Color(selectionColorDefault);
                    var r = MathEx.RectXYXY(drawPosition.x + selectionBeginX, drawPosition.y + 2, drawPosition.x + selectionEndX, drawPosition.y + size.y - 2);
                    GLUtilities.DrawRectangle(r.x, r.y, r.width, r.height);
                });

                // draw the selected text in a different color.
                GLUtilities.DrawGuiClippedText(drawRect, font, TextGetVisualString().Substring(selectionStringBegin, selectionStringEnd - selectionStringBegin), new float2(drawPosition.x + selectionBeginX, drawPosition.y + textVerticalAlign.y), textColorSelected);
            }

            // draw the caret.
            if (isActive)
            {
                var xcaret = drawPosition.x + scrollX + CharacterPositionGetDrawX(caretCharPosition);

                if (timerCaretBlink < 60)
                {
                    GLUtilities.DrawGuiClipped(drawRect, () =>
                    {
                        GLUtilities.DrawLine(1.0f, xcaret, drawPosition.y + 2, xcaret, drawPosition.y + size.y - 2, caretColorDefault, caretColorDefault);
                    });
                }
            }
        }

        public override bool OnKeyDown(KeyCode keyCode)
        {
            // detect valid keyboard character inputs.

            var character = Event.current.character;
            if (character != 0)
                CaretTypeCharacter(character);

            if (keyCode == KeyCode.Backspace) CaretBackspace();
            if (keyCode == KeyCode.Delete) CaretDelete();
            if (keyCode == KeyCode.LeftArrow) CaretLeft(parent.parent.isShiftPressed);
            if (keyCode == KeyCode.RightArrow) CaretRight(parent.parent.isShiftPressed);
            if (keyCode == KeyCode.Home) CaretFirst(parent.parent.isShiftPressed);
            if (keyCode == KeyCode.UpArrow) CaretFirst(parent.parent.isShiftPressed);
            if (keyCode == KeyCode.DownArrow) CaretLast(parent.parent.isShiftPressed);
            if (keyCode == KeyCode.End) CaretLast(parent.parent.isShiftPressed);

            if (parent.parent.isCtrlPressed && keyCode == KeyCode.A) CaretSelectAll();
            if (parent.parent.isCtrlPressed && keyCode == KeyCode.C) CaretCopy();
            if (parent.parent.isCtrlPressed && keyCode == KeyCode.V) CaretPaste();
            if (parent.parent.isCtrlPressed && keyCode == KeyCode.X) CaretCut();

            return true; // true everything, we are typing!
        }

        /// <summary>Gets the text the user sees (e.g. passwords use different characters).</summary>
        private string TextGetVisualString()
        {
            if (isPassword)
                return new string('*', text.Length);
            return text;
        }

        /// <summary>Type a character at the caret position.</summary>
        private void CaretTypeCharacter(char character)
        {
            if (isReadonly) return;
            if (character == 0) return;
            if (!font.HasCharacter(character)) return;
            if (text.Length >= maxLength) return;

            // remove any selected text.
            SelectionRemove();

            text = text.Insert(caretCharPosition, character.ToString());
            caretCharPosition++;

            // reset any selection.
            SelectionReset();

            // reset the caret blink timer.
            CaretResetBlink();
        }

        /// <summary>Removes a character to the left of the caret position.</summary>
        private void CaretBackspace()
        {
            if (isReadonly) return;

            // when there is a selection we remove that instead.
            if (HasSelection()) { SelectionRemove(); return; }

            if (caretCharPosition == 0) return;
            if (text.Length == 0) return;

            text = text.Remove(caretCharPosition - 1, 1);
            caretCharPosition--;

            // reset the caret blink timer.
            CaretResetBlink();
        }

        /// <summary>Removes a character to the right of the caret position.</summary>
        private void CaretDelete()
        {
            if (isReadonly) return;

            // when there is a selection we remove that instead.
            if (HasSelection()) { SelectionRemove(); return; }

            if (caretCharPosition == text.Length) return;
            if (text.Length == 0) return;

            text = text.Remove(caretCharPosition, 1);

            // reset the caret blink timer.
            CaretResetBlink();
        }

        /// <summary>Move the caret left.</summary>
        private void CaretLeft(bool shift)
        {
            if (caretCharPosition == 0) return;
            caretCharPosition--;

            // drag selection to caret or reset selection.
            if (shift) SelectionToCaret(); else SelectionReset();

            // reset the caret blink timer.
            CaretResetBlink();
        }

        /// <summary>Move the caret right.</summary>
        private void CaretRight(bool shift)
        {
            if (caretCharPosition == text.Length) return;
            caretCharPosition++;

            // drag selection to caret or reset selection.
            if (shift) SelectionToCaret(); else SelectionReset();

            // reset the caret blink timer.
            CaretResetBlink();
        }

        /// <summary>Move the caret to the first position.</summary>
        private void CaretFirst(bool shift)
        {
            caretCharPosition = 0;

            // drag selection to caret or reset selection.
            if (shift) SelectionToCaret(); else SelectionReset();

            // reset the caret blink timer.
            CaretResetBlink();
        }

        /// <summary>
        /// Move the caret to the last position.
        /// </summary>
        private void CaretLast(bool shift)
        {
            caretCharPosition = text.Length;

            // drag selection to caret or reset selection.
            if (shift) SelectionToCaret(); else SelectionReset();

            // reset the caret blink timer.
            CaretResetBlink();
        }

        /// <summary>Copy the selected text into the clipboard contents.</summary>
        private void CaretCopy()
        {
            if (isPassword) return;
            if (!HasSelection()) return;

            var selectionStringBegin = SelectionGetBegin();
            var selectionStringEnd = SelectionGetEnd();

            EditorGUIUtility.systemCopyBuffer = text.Substring(selectionStringBegin, selectionStringEnd - selectionStringBegin);
        }

        /// <summary>Paste the clipboard contents at the caret position.</summary>
        private void CaretPaste()
        {
            if (isReadonly) return;
            var text = EditorGUIUtility.systemCopyBuffer;
            if (text.Length == 0) return;

            for (var i = 0; i < text.Length; i++)
                CaretTypeCharacter(text[i]);
        }

        /// <summary>Cuts the selected text into the clipboard contents.</summary>
        private void CaretCut()
        {
            if (isReadonly) return;
            if (isPassword) return;
            if (!HasSelection()) return;

            CaretCopy();
            SelectionRemove();
        }

        /// <summary>Selects all of the text and puts the caret on the right.</summary>
        private void CaretSelectAll()
        {
            // move the caret to the last position.
            CaretLast(true);

            // select all of the text.
            SelectionSet(0, caretCharPosition);
        }

        /// <summary>Reset the caret blink timer, ensures the caret is visible.</summary>
        private void CaretResetBlink()
        {
            timerCaretBlink = 0;
        }

        /// <summary>
        /// Get the x draw position of the caret within the text.
        /// </summary>
        private int CharacterPositionGetDrawX(int charPosition)
        {
            string vs = TextGetVisualString();
            if (vs.Length == 0) return 0;
            return font.StringWidth(vs.Substring(0, charPosition));
        }

        /// <summary>
        /// Takes a shape editor mouse coordinate and transforms it to virtual textbox coordinates.
        /// </summary>
        private int ToVirtualX(int xpos)
        {
            return Mathf.RoundToInt(xpos - drawPosition.x) - scrollX;
        }

        /// <summary>
        /// Get the x caret character position within the text for the given x draw position.
        /// </summary>
        private int TextXToCaretCharPosition(int xpos)
        {
            var textLength = text.Length;
            if (textLength == 0) return 0;

            // transform to local coordinates inside the textbox.
            xpos = ToVirtualX(xpos);

            // iterate over every character in the text string.
            var visualText = TextGetVisualString();
            var textTemp = "";
            var textTempWidthPrevious = 0;
            for (var i = 0; i < textLength; i++)
            {
                // add a character to the temp text.
                textTemp += visualText[i];

                // get the temp text width after modifications.
                var textTempWidth = font.StringWidth(textTemp);

                // get half of the difference between the widths.
                // we use this to accept clicks slightly left and right
                // of the middle of a character.
                var half = (textTempWidth - textTempWidthPrevious) / 2;
                if (textTempWidth >= xpos + half)
                    return i;

                textTempWidthPrevious = textTempWidth;
            }

            return textLength;
        }

        /// <summary>Ensures that the caret is visible by scrolling the textbox content.</summary>
        private void ScrollToCaret()
        {
            var sizex = Mathf.RoundToInt(size.x);

            var busy = true;
            while (busy)
            {
                var xcaret = scrollX + CharacterPositionGetDrawX(caretCharPosition);

                if (xcaret <= 10)
                {
                    scrollX += 10;
                }
                else if (xcaret >= sizex - 10)
                {
                    scrollX -= 10;
                }
                else
                {
                    busy = false;
                }
            }

            // the textbox can never scroll right (which would leave a gap on the left).
            if (scrollX > 0) scrollX = 0;

            // the textbox can never scroll beyond the size of the text (which would leave a gap on the right).
            var visualText = TextGetVisualString();
            var textWidth = font.StringWidth(visualText);
            var rightmostScroll = -(textWidth - sizex) - 1;
            if (scrollX < rightmostScroll)
                scrollX = rightmostScroll;

            // if the full text fits in the textbox, always reset the scroll.
            if (textWidth <= sizex) scrollX = 0;
        }

        /// <summary>Sets the selection.</summary>
        private void SelectionSet(int beginCharPosition, int endCharPosition)
        {
            selectionBegin = beginCharPosition;
            selectionEnd = endCharPosition;
        }

        /// <summary>Sets the selection end.</summary>
        private void SelectionSetEnd(int endCharPosition)
        {
            SelectionSet(selectionBegin, endCharPosition);
        }

        /// <summary>Resets the current selection.</summary>
        private void SelectionReset()
        {
            selectionBegin = caretCharPosition;
            selectionEnd = caretCharPosition;
        }

        /// <summary>Removes the selected text.</summary>
        private void SelectionRemove()
        {
            if (isReadonly) return;
            if (!HasSelection()) return;

            var selectionStringBegin = SelectionGetBegin();
            var selectionStringEnd = SelectionGetEnd();

            // delete the selected characters.
            text = text.Remove(selectionStringBegin, selectionStringEnd - selectionStringBegin);

            // move the caret to the selection beginning.
            caretCharPosition = selectionStringBegin;

            // reset the selection.
            SelectionReset();
        }

        /// <summary>Returns the beginning of the selection sorted lowest to highest.</summary>
        private int SelectionGetBegin()
        {
            return selectionBegin < selectionEnd ? selectionBegin : selectionEnd;
        }

        /// <summary>Returns the ending of the selection sorted lowest to highest.</summary>
        private int SelectionGetEnd()
        {
            return selectionEnd > selectionBegin ? selectionEnd : selectionBegin;
        }

        /// <summary>Returns whether anything is selected.</summary>
        private bool HasSelection()
        {
            return (selectionBegin != selectionEnd);
        }

        /// <summary>Drags the selection towards the caret position.</summary>
        private void SelectionToCaret()
        {
            selectionEnd = caretCharPosition;
        }
    }
}

#endif