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
		public string placeholder;
		/// <summary>Whether this textbox hides all characters.</summary>
		public bool password;
		/// <summary>The maximum amount of characters allowed.</summary>
		public int max_length = 32767;
		/// <summary>Whether this textbox accepts text input.</summary>
		public bool isReadonly;
		/// <summary>The textbox font.</summary>
		public BmFont font;

		/// <summary>The caret character position in the text string.</summary>
		private int caret_char_position = 0;
		/// <summary>The virtual scroll of text inside the textbox.</summary>
		private int scroll_x;
		/// <summary>The caret blink timer used for drawing.</summary>
		private int timer_caret_blink;
		/// <summary>Determine whether the mouse moved.</summary>
		private int mouse_xprevious;
		/// <summary>Determine whether the mouse moved.</summary>
		private int mouse_yprevious;
		/// <summary>The character position where a selection begins.</summary>
		private int selection_begin = 0;
		/// <summary>The character position where a selection ends.</summary>
		private int selection_end = 0;
		/// <summary>Used to vertical align text in the middle of the textbox.</summary>
		private float2 text_valign = new float2(0f, 3f);

		private Color c_background_default = new Color(255 / 255f, 255 / 255f, 255 / 255f);
		private Color c_background_readonly = new Color(180 / 255f, 180 / 255f, 180 / 255f);
		private Color c_border_default = new Color(122 / 255f, 122 / 255f, 122 / 255f);
		private Color c_border_focus = new Color(0 / 255f, 120 / 255f, 215 / 255f);
		private Color c_text_default = new Color(0 / 255f, 0 / 255f, 0 / 255f);
		private Color c_text_placeholder = new Color(167 / 255f, 167 / 255f, 167 / 255f);
		private Color c_text_selected = new Color(255 / 255f, 255 / 255f, 255 / 255f);
		private Color c_caret_default = new Color(0 / 255f, 0 / 255f, 0 / 255f);
		private Color c_selection_default = new Color(0 / 255f, 120 / 255f, 215 / 255f);

		private Rect drawRect => new Rect(drawPosition, size);

		public GuiTextbox(float2 position, float2 size) : base(position, size)
		{
			font = ShapeEditorResources.fontSegoeUI14;
			text = "";
			placeholder = "Enter a string...";
		}

        public override void OnRender()
        {
			/*
			///////////////////////////////////////////////////////////////////////////////
			// detect mouse position and clicks to determine whether the textbox has focus.
			///////////////////////////////////////////////////////////////////////////////

			var x1 = x;
			var y1 = y;
			var x2 = x + width;
			var y2 = y + height;

			// if the mouse is hovering over the textbox:
			var mouse_x_mrc = device_mouse_x_mrc();
			var mouse_y_mrc = device_mouse_y_mrc();
			if (point_in_rectangle(mouse_x_mrc, mouse_y_mrc, x1, y1, x2, y2))
			{
				// and the left mouse button was clicked:
				if (mouse_check_button_pressed(mb_left))
				{
					// deselect all other textboxes.
					with (object_index) { focus = false; }
		
					// select this textbox.
					focus = true;
		
					// set the caret position.
					caret_char_position = _text_x_to_caret_char_position(mouse_x_mrc);
		
					// set the selection position.
					_selection_set(caret_char_position, caret_char_position);
				}
	
				// automatic scrolling when the mouse moves (for the edges on long text).
				if
				(
					mouse_check_button(mb_left)
					&& (mouse_x_mrc != mouse_xprevious || mouse_y_mrc != mouse_yprevious)
					&& !keyboard_check(vk_anykey)
				)
				{
					// set the caret position.
					caret_char_position = _text_x_to_caret_char_position(mouse_x_mrc);
				}
	
				if (mouse_check_button(mb_left))
				{
					// reset the caret blink timer.
					_caret_reset_blink();
		
					// set the selection end position.
					_selection_set_end(caret_char_position);
				}
	
				// set the mouse cursor.
				window_set_cursor_once(cr_beam);
			}
			// else if the mouse is not hovering over the textbox:
			else
			{
				// and the mouse has just been clicked:
				if (mouse_check_button_pressed(mb_left))
				{
					focus = false;
				}
			}

			// remember the previous mouse position.
			mouse_xprevious = mouse_x_mrc;
			mouse_yprevious = mouse_y_mrc;

			///////////////////////////////////////////////////////////////////////////////
			// if the textbox does not have focus- stop processing here.
			///////////////////////////////////////////////////////////////////////////////
			if (!focus) return;
			*/


			///////////////////////////////////////////////////////////////////////////////
			// process the caret blinking timer.
			///////////////////////////////////////////////////////////////////////////////

			timer_caret_blink++;
			timer_caret_blink %= 120;

			/*


			///////////////////////////////////////////////////////////////////////////////
			// handle scrolling within the textbox.
			///////////////////////////////////////////////////////////////////////////////

			// ensure that the caret is visible by scrolling the textbox.
			_scroll_to_caret();
			 */

            {
				///////////////////////////////////////////////////////////////////////////////
				// draw the textbox background color.
				///////////////////////////////////////////////////////////////////////////////

				var x1 = drawPosition.x;
				var y1 = drawPosition.y;
				var x2 = drawPosition.x + size.x;
				var y2 = drawPosition.y + size.y;
				var textDrawPosition = drawPosition + text_valign;

				var textbox_background_color = isReadonly ? c_background_readonly : c_background_default;

				GLUtilities.DrawGui(() => {
					GL.Color(textbox_background_color);
					GLUtilities.DrawRectangle(drawPosition.x, drawPosition.y, size.x, size.y);
				});

				///////////////////////////////////////////////////////////////////////////////
				// draw outside border that indicates whether the textbox has input focus.
				///////////////////////////////////////////////////////////////////////////////

				GLUtilities.DrawGui(() => {
					GLUtilities.DrawRectangleOutline(drawPosition.x, drawPosition.y, size.x, size.y, isActive ? c_border_focus : c_border_default);
				});

				///////////////////////////////////////////////////////////////////////////////
				// draw the placeholder inside.
				///////////////////////////////////////////////////////////////////////////////

				if (!isActive && placeholder.Length > 0 && text.Length == 0)
				{
					GLUtilities.DrawGuiClippedText(drawRect, font, placeholder, textDrawPosition, c_text_placeholder);
				}

				///////////////////////////////////////////////////////////////////////////////
				// draw the text inside.
				///////////////////////////////////////////////////////////////////////////////

				GLUtilities.DrawGuiClippedText(drawRect, font, _text_get_visual_string(), textDrawPosition + new float2(scroll_x, 0f), c_text_default);

				///////////////////////////////////////////////////////////////////////////////
				// draw the selection.
				///////////////////////////////////////////////////////////////////////////////

				if (isActive && _has_selection())
				{
					var selection_string_begin = _selection_get_begin();
					var selection_string_end = _selection_get_end();

					var selection_begin_x = scroll_x + _character_position_get_draw_x(selection_string_begin);
					var selection_end_x = scroll_x + _character_position_get_draw_x(selection_string_end);

					// highlight the selection.
					GLUtilities.DrawGuiClipped(drawRect, () => {
						GL.Color(c_selection_default);
						var r = MathEx.RectXYXY(drawPosition.x + selection_begin_x, drawPosition.y + 2, drawPosition.x + selection_end_x, drawPosition.y + size.y - 2);
						GLUtilities.DrawRectangle(r.x, r.y, r.width, r.height);
					});

					// draw the selected text in a different color.
					GLUtilities.DrawGuiClippedText(drawRect, font, _text_get_visual_string().Substring(selection_string_begin, selection_string_end - selection_string_begin), new float2(drawPosition.x + selection_begin_x, drawPosition.y + text_valign.y), c_text_selected);
				}

				///////////////////////////////////////////////////////////////////////////////
				// draw the caret.
				///////////////////////////////////////////////////////////////////////////////

				if (isActive)
				{
					var xcaret = drawPosition.x + scroll_x + _character_position_get_draw_x(caret_char_position);

					if (timer_caret_blink < 60)
					{
						GLUtilities.DrawGuiClipped(drawRect, () => {
							GLUtilities.DrawLine(1.0f, xcaret, drawPosition.y + 2, xcaret, drawPosition.y + size.y - 2, c_caret_default, c_caret_default);
						});
					}
				}
			}
		}

        public override bool OnKeyDown(KeyCode keyCode)
        {
			///////////////////////////////////////////////////////////////////////////////
			// detect valid keyboard character inputs.
			///////////////////////////////////////////////////////////////////////////////
			
			var character = Event.current.character;
			if (character != 0)
				_caret_type_character(character);

            if (keyCode == KeyCode.Backspace) _caret_backspace();
			if (keyCode == KeyCode.Delete) _caret_delete();
            if (keyCode == KeyCode.LeftArrow) _caret_left(parent.parent.isShiftPressed);
            if (keyCode == KeyCode.RightArrow) _caret_right(parent.parent.isShiftPressed);
			if (keyCode == KeyCode.Home) _caret_first(parent.parent.isShiftPressed);
			if (keyCode == KeyCode.UpArrow) _caret_first(parent.parent.isShiftPressed);
			if (keyCode == KeyCode.DownArrow) _caret_last(parent.parent.isShiftPressed);
			if (keyCode == KeyCode.End) _caret_last(parent.parent.isShiftPressed);

            if (parent.parent.isCtrlPressed && keyCode == KeyCode.A) _caret_select_all();
            if (parent.parent.isCtrlPressed && keyCode == KeyCode.C) _caret_copy();
            if (parent.parent.isCtrlPressed && keyCode == KeyCode.V) _caret_paste();
			if (parent.parent.isCtrlPressed && keyCode == KeyCode.X) _caret_cut();

			return true; // true everything, we are typing!
        }

        ///////////////////////////////////////////////////////////////////////////////
        
		private string _text_get_visual_string()
		{
			if (password)
				return new string('*', text.Length);
			return text;
		}

		/// <summary>Type a character at the caret position.</summary>
		private void _caret_type_character(char character)
		{
			if (isReadonly) return;
			if (character == 0) return;
			if (!font.HasCharacter(character)) return;
			if (text.Length >= max_length) return;

			// remove any selected text.
			_selection_remove();

			text = text.Insert(caret_char_position, character.ToString());
			caret_char_position++;

			// reset any selection.
			_selection_reset();

			// reset the caret blink timer.
			_caret_reset_blink();
		}

		/// <summary>Removes a character to the left of the caret position.</summary>
		private void _caret_backspace()
		{
			if (isReadonly) return;

			// when there is a selection we remove that instead.
			if (_has_selection()) { _selection_remove(); return; }

			if (caret_char_position == 0) return;
			if (text.Length == 0) return;

			text = text.Remove(caret_char_position - 1, 1);
			caret_char_position--;

			// reset the caret blink timer.
			_caret_reset_blink();
		}

		/// <summary>Removes a character to the right of the caret position.</summary>
		private void _caret_delete()
		{
			if (isReadonly) return;

			// when there is a selection we remove that instead.
			if (_has_selection()) { _selection_remove(); return; }

			if (caret_char_position == text.Length) return;
			if (text.Length == 0) return;

			text = text.Remove(caret_char_position, 1);

			// reset the caret blink timer.
			_caret_reset_blink();
		}


		/// <summary>Move the caret left.</summary>
		private void _caret_left(bool shift)
		{
			if (caret_char_position == 0) return;
			caret_char_position--;

			// drag selection to caret or reset selection.
			if (shift) _selection_to_caret(); else _selection_reset();

			// reset the caret blink timer.
			_caret_reset_blink();
		}

		/// <summary>Move the caret right.</summary>
		private void _caret_right(bool shift)
		{
			if (caret_char_position == text.Length) return;
			caret_char_position++;

			// drag selection to caret or reset selection.
			if (shift) _selection_to_caret(); else _selection_reset();

			// reset the caret blink timer.
			_caret_reset_blink();
		}

		/// <summary>Move the caret to the first position.</summary>
		private void _caret_first(bool shift)
		{
			caret_char_position = 0;

			// drag selection to caret or reset selection.
			if (shift) _selection_to_caret(); else _selection_reset();

			// reset the caret blink timer.
			_caret_reset_blink();
		}

		/// <summary>
		/// Move the caret to the last position.
		/// </summary>
		private void _caret_last(bool shift)
		{
			caret_char_position = text.Length;

			// drag selection to caret or reset selection.
			if (shift) _selection_to_caret(); else _selection_reset();

			// reset the caret blink timer.
			_caret_reset_blink();
		}

		/// <summary>Copy the selected text into the clipboard contents.</summary>
		private void _caret_copy()
		{
			if (password) return;
			if (!_has_selection()) return;

			var selection_string_begin = _selection_get_begin();
			var selection_string_end = _selection_get_end();

			EditorGUIUtility.systemCopyBuffer = text.Substring(selection_string_begin, selection_string_end - selection_string_begin);
		}

		/// <summary>Paste the clipboard contents at the caret position.</summary>
		private void _caret_paste()
		{
			if (isReadonly) return;
			var text = EditorGUIUtility.systemCopyBuffer;
			if (text.Length == 0) return;

			for (var i = 0; i < text.Length; i++)
				_caret_type_character(text[i]);
		}

		/// <summary>Cuts the selected text into the clipboard contents.</summary>
		private void _caret_cut()
		{
			if (isReadonly) return;
			if (password) return;
			if (!_has_selection()) return;

			_caret_copy();
			_selection_remove();
		}

		/// <summary>Selects all of the text and puts the caret on the right.</summary>
		private void _caret_select_all()
		{
			// move the caret to the last position.
			_caret_last(true);

			// select all of the text.
			_selection_set(0, caret_char_position);
		}

		/// <summary>Reset the caret blink timer, ensures the caret is visible.</summary>
		private void _caret_reset_blink()
		{
			timer_caret_blink = 0;
		}

		/// <summary>
		/// Get the x draw position of the caret within the text.
		/// </summary>
		private int _character_position_get_draw_x(int char_position)
		{
			string vs = _text_get_visual_string();
			if (vs.Length == 0) return 0;
			return font.StringWidth(vs.Substring(0, char_position));
		}

		/*
		// takes a room coordinate and transforms it to virtual textbox coordinates.
		function _to_virtual_x(xpos)
		{
			return (xpos - x) - scroll_x;
		}

		// get the x caret character position within the text for the given x draw position.
		function _text_x_to_caret_char_position(xpos)
		{
			var text_length = string_length(text);
			if (text_length == 0) return 1;

			// transform to local coordinates inside the textbox.
			xpos = _to_virtual_x(xpos);

			// iterate over every character in the text string.
			var visual_text = _text_get_visual_string();
			var text_temp = "";
			var text_temp_width_previous = 0;
			for (var i = 1; i <= text_length; i++)
			{
				// add a character to the temp text.
				text_temp += string_char_at(visual_text, i);

				// get the temp text width after modifications.
				var text_temp_width = font_string_width(font, text_temp);

				// get half of the difference between the widths.
				// we use this to accept clicks slightly left and right
				// of the middle of a character.
				var half = (text_temp_width - text_temp_width_previous) / 2;
				if (text_temp_width >= xpos + half)
					return i;

				text_temp_width_previous = text_temp_width;
			}

			return text_length + 1;
		}

		// ensures that the caret is visible by scrolling the textbox content.
		function _scroll_to_caret()
		{
			var busy = true;
			while (busy)
			{
				var xcaret = scroll_x + _character_position_get_draw_x(caret_char_position);

				if (xcaret <= 10)
				{
					scroll_x += 10;
				}
				else if (xcaret >= width - 10)
				{
					scroll_x -= 10;
				}
				else
				{
					busy = false;
				}
			}

			// the textbox can never scroll right (which would leave a gap on the left).
			if (scroll_x > 0) scroll_x = 0;

			// the textbox can never scroll beyond the size of the text (which would leave a gap on the right).
			var visual_text = _text_get_visual_string();
			var text_width = font_string_width(font, visual_text);
			var rightmost_scroll = -(text_width - width) - 1;
			if (scroll_x < rightmost_scroll)
				scroll_x = rightmost_scroll;

			// if the full text fits in the textbox, always reset the scroll.
			if (text_width <= width) scroll_x = 0;
		}*/

		/// <summary>Sets the selection.</summary>
		private void _selection_set(int begin_char_position, int end_char_position)
		{
			selection_begin = begin_char_position;
			selection_end = end_char_position;
		}

		/// <summary>Sets the selection end.</summary>
		private void _selection_set_end(int end_char_position)
		{
			_selection_set(selection_begin, end_char_position);
		}

		/// <summary>Resets the current selection.</summary>
		private void _selection_reset()
		{
			selection_begin = caret_char_position;
			selection_end = caret_char_position;
		}

		/// <summary>Removes the selected text.</summary>
		private void _selection_remove()
		{
			if (isReadonly) return;
			if (!_has_selection()) return;

			var selection_string_begin = _selection_get_begin();
			var selection_string_end = _selection_get_end();

			// delete the selected characters.
			text = text.Remove(selection_string_begin, selection_string_end - selection_string_begin);

			// move the caret to the selection beginning.
			caret_char_position = selection_string_begin;

			// reset the selection.
			_selection_reset();
		}

		/// <summary>Returns the beginning of the selection sorted lowest to highest.</summary>
		private int _selection_get_begin()
		{
			return selection_begin < selection_end ? selection_begin : selection_end;
		}

		/// <summary>Returns the ending of the selection sorted lowest to highest.</summary>
		private int _selection_get_end()
		{
			return selection_end > selection_begin ? selection_end : selection_begin;
		}

		/// <summary>Returns whether anything is selected.</summary>
		private bool _has_selection()
		{
			return (selection_begin != selection_end);
		}

		/// <summary>Drags the selection towards the caret position.</summary>
		private void _selection_to_caret()
		{
			selection_end = caret_char_position;
		}
	}
}

#endif