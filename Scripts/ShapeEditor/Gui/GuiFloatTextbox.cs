#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a float textbox control inside of a window.</summary>
    public class GuiFloatTextbox : GuiTextbox
    {
        private float _value;
        private bool _ignoreTextChange = false;

        /// <summary>
        /// The last correct value written in the textbox. When this property is assigned, the
        /// number will be validated and modified according to the rules of this textbox.
        /// </summary>
        public float value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = ValidateNumber(value);
                // do not bother the user while editing.
                if (!isActive)
                    SetTextToValue();
            }
        }

        /// <summary>Whether negative numbers are supported.</summary>
        public bool allowNegativeNumbers = true;
        /// <summary>The value must be at least this number.</summary>
        public float minValue = float.MinValue;
        /// <summary>The value must be at most this number.</summary>
        public float maxValue = float.MaxValue;

        public GuiFloatTextbox(float2 position, float2 size) : base(position, size)
        {
        }

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
                    return true;

                case '-':
                    return allowNegativeNumbers && !text.Contains("-");

                case '.':
                    return !text.Contains(".");
            }
            return false;
        }

        protected override void OnTextChanged()
        {
            if (_ignoreTextChange) return;

            float lastValue = _value;

            // try parsing the text as a float.
            if (float.TryParse(text, out _value))
            {
                value = _value;
            }
            else
            {
                // not valid.
                _value = lastValue;
            }
        }

        /// <summary>Modifies the number so that it's valid for the rules of this textbox.</summary>
        /// <param name="number">The number to be checked.</param>
        /// <returns>The corrected number or the same number.</returns>
        private float ValidateNumber(float number)
        {
            // if the number is negative but that's not allowed:
            if (!allowNegativeNumbers && number < 0f)
                number = 0f;

            // clamp the number between the min and max values.
            number = math.clamp(number, minValue, maxValue);

            return number;
        }

        /// <summary>Sets the text to the value and tries to keep it nice.</summary>
        private void SetTextToValue()
        {
            _ignoreTextChange = true;
            SetText(_value.ToString("0.00000000").TrimEnd('0').TrimEnd('.'));
            _ignoreTextChange = false;
        }

        public override void OnFocusLost()
        {
            SetTextToValue();
        }

        public override bool OnKeyDown(KeyCode keyCode)
        {
            if (keyCode == KeyCode.Return)
            {
                SetTextToValue();
            }

            return base.OnKeyDown(keyCode);
        }

        public override void OnRender()
        {
            // set the default value when the textbox is empty.
            if (!isActive && text.Length == 0)
                SetTextToValue();

            base.OnRender();
        }
    }
}

#endif