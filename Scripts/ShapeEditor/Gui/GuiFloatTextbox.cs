#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a float textbox control inside of a window.</summary>
    public class GuiFloatTextbox : GuiTextbox
    {
        // by design this textbox does not actually store the number. The caller is reponsible for
        // storing the number and filtering it through this textbox using the UpdateValue(input)
        // function. This makes it easy to have textboxes that update their numbers rapidly be
        // editable by the user, without their temporary edits affecting the current value in any
        // way. From a programming standpoint this is a much simpler approach and mimics what unity
        // does with their imgui implementation.

        /// <summary>Whether negative numbers are supported.</summary>
        public bool allowNegativeNumbers = true;
        /// <summary>The value must be at least this number.</summary>
        public float minValue = float.MinValue;
        /// <summary>The value must be at most this number.</summary>
        public float maxValue = float.MaxValue;

        /// <summary>The text before the user made an edit.</summary>
        private string textBeforeEdit;

        /// <summary>Represents a new number written by the user.</summary>
        private float newNumber;

        /// <summary>Whether the user has written a new number.</summary>
        private bool hasNewNumber;

        public GuiFloatTextbox(float2 position, float2 size) : base(position, size)
        {
        }

        public GuiFloatTextbox(float2 size) : base(float2.zero, size)
        {
        }

        /// <summary>
        /// Updates the textbox value and when the user makes an edit returns the new value once.
        /// <para>Intended to be used like: value = textbox.UpdateValue(value);</para>
        /// </summary>
        /// <param name="input">The input value to display.</param>
        /// <returns>The input value or a new value if there was an edit.</returns>
        public float UpdateValue(float input)
        {
            // unless a new number is available we return the input value.
            if (!hasNewNumber)
            {
                // if the textbox does not have input focus, we can safely update the text.
                if (!isActive)
                {
                    SetText(input.ToString());
                }

                return input;
            }

            // a new number is available so we return that instead.
            hasNewNumber = false;
            textBeforeEdit = newNumber.ToString();

            SetText(textBeforeEdit, true);

            return newNumber;
        }

        // whitelist the input characters related to floating point numbers and math.
        protected override bool ValidateCharacter(char character)
        {
            switch (character)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '.':
                case '-':
                case '+':
                case '*':
                case '/':
                case '%':
                case '^':
                case '(':
                case ')':
                case 'e':
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Parses and modifies the number so that it's valid for the rules of this textbox.
        /// </summary>
        /// <param name="number">The number to be checked.</param>
        /// <returns>The corrected number or the same number.</returns>
        private float ParseAndValidateNumber(string text)
        {
            // have unity parse and evaluate mathematical expressions.
            ExpressionEvaluator.Evaluate(text, out float number);

            // if the number is negative but that's not allowed:
            if (!allowNegativeNumbers && number < 0f)
                number = 0f;

            // clamp the number between the min and max values.
            number = math.clamp(number, minValue, maxValue);

            return number;
        }

        /// <summary>Called when the user finishes editing the textbox.</summary>
        private void OnFinishEdit()
        {
            // if the text is unchanged don't do anything.
            if (text == textBeforeEdit) return;

            // parse and validate the number that was written by the user.
            newNumber = ParseAndValidateNumber(text);
            hasNewNumber = true;
        }

        public override void OnFocus()
        {
            // store a copy of the text before the edit.
            textBeforeEdit = text;
        }

        public override void OnFocusLost()
        {
            // losing the input focus could mean the user finished an edit.
            OnFinishEdit();
        }

        public override bool OnKeyDown(KeyCode keyCode)
        {
            // pressing the enter key could mean the user finished an edit.
            if (keyCode == KeyCode.Return)
            {
                OnFinishEdit();
            }

            return base.OnKeyDown(keyCode);
        }
    }
}

#endif