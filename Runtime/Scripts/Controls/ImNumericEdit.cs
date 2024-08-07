using System;
using System.Globalization;
using Imui.Controls.Styling;
using Imui.Core;
using Imui.IO.Utility;

namespace Imui.Controls
{
    public static class ImNumericEdit
    {
        public static readonly IntegerFilter IntegerFilterAllowEmptyString = new(true);
        public static readonly FloatFilter FloatFilterAllowEmptyString = new(true);

        public static void IntEdit(this ImGui gui, ref int value, ImSize size = default, ReadOnlySpan<char> format = default)
        {
            TextEditNumeric(gui, ref value, IntegerFilterAllowEmptyString, format, size);
        }

        public static void FloatEdit(this ImGui gui, ref float value, ImSize size = default, ReadOnlySpan<char> format = default)
        {
            TextEditNumeric(gui, ref value, FloatFilterAllowEmptyString, format, size);
        }

        private static void TextEditNumeric<T>(ImGui gui, ref T value, NumericFilter<T> filter, ReadOnlySpan<char> format, ImSize size)
        {
            var buffer = new ImTextEditBuffer();
            buffer.MakeMutable();
            if (filter.TryFormat(buffer.Buffer, value, out var length, format))
            {
                buffer.Length = length;
            }
            else
            {
                buffer.Insert(0, filter.GetFallbackString());
            }

            gui.AddSpacingIfLayoutFrameNotEmpty();
            var rect = ImTextEdit.GetRect(gui, size);
            var changed = ImTextEdit.TextEdit(gui, ref buffer, rect, filter, multiline: false);
            if (changed && filter.TryParse(buffer, out var newValue))
            {
                value = newValue;
            }
        }

        public abstract class NumericFilter<T> : ImTextEditFilter
        {
            public override ImTouchKeyboardType KeyboardType => ImTouchKeyboardType.Numeric;
            protected bool emptyStringIsValid;

            public NumericFilter(bool emptyStringIsValid)
            {
                this.emptyStringIsValid = emptyStringIsValid;
            }

            public abstract bool TryParse(ReadOnlySpan<char> buffer, out T value);
            public abstract bool TryFormat(Span<char> buffer, T value, out int length, ReadOnlySpan<char> format);
        }

        public sealed class IntegerFilter : NumericFilter<int>
        {
            public IntegerFilter(bool emptyStringIsValid = false) : base(emptyStringIsValid) { }

            public override bool IsValid(ReadOnlySpan<char> buffer) => TryParse(buffer, out _);
            public override string GetFallbackString() => "0";

            public override bool TryParse(ReadOnlySpan<char> buffer, out int value)
            {
                if (emptyStringIsValid && buffer.IsEmpty)
                {
                    value = 0;
                    return true;
                }

                return int.TryParse(buffer, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
            }

            public override bool TryFormat(Span<char> buffer, int value, out int length, ReadOnlySpan<char> format) =>
                value.TryFormat(buffer, out length, format);
        }

        public sealed class FloatFilter : NumericFilter<float>
        {
            // allow to use comma as decimal separator
            private static readonly CultureInfo DeCulture = new("de");

            public FloatFilter(bool emptyStringIsValid = false) : base(emptyStringIsValid) { }

            public override bool IsValid(ReadOnlySpan<char> buffer) => TryParse(buffer, out _);
            public override string GetFallbackString() => "0.0";

            public override bool TryParse(ReadOnlySpan<char> buffer, out float value)
            {
                if (emptyStringIsValid && buffer.IsEmpty)
                {
                    value = 0.0f;
                    return true;
                }

                return float.TryParse(buffer, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
                       float.TryParse(buffer, NumberStyles.Float, DeCulture, out value);
            }

            public override bool TryFormat(Span<char> buffer, float value, out int length, ReadOnlySpan<char> format) =>
                value.TryFormat(buffer, out length, format);
        }
    }
}