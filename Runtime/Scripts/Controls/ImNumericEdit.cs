using System;
using System.Globalization;
using Imui.Controls.Styling;
using Imui.Core;
using Imui.IO.Utility;

// ReSharper disable BuiltInTypeReferenceStyle

namespace Imui.Controls
{
    public static class ImNumericEdit
    {
        public static readonly Int64Filter Int64FilterAllowEmptyString = new(true);
        public static readonly DoubleFilter DoubleFilterAllowEmptyString = new(true);

        public static void IntEdit(this ImGui gui,
                                   ref int value,
                                   ImSize size = default,
                                   ReadOnlySpan<char> format = default,
                                   int step = 1)
        {
            long longValue = value;
            var delta = TextEditNumeric(gui, ref longValue, Int64FilterAllowEmptyString, format, size, step);
            longValue += (long)delta;
            value = longValue > int.MaxValue ? int.MaxValue : longValue < int.MinValue ? int.MinValue : (int)longValue;
        }

        public static void LongEdit(this ImGui gui,
                                    ref long value,
                                    ImSize size = default,
                                    ReadOnlySpan<char> format = default,
                                    long step = 0)
        {
            var delta = TextEditNumeric(gui, ref value, Int64FilterAllowEmptyString, format, size, step);
            value += (long)delta;
        }

        public static void FloatEdit(this ImGui gui,
                                     ref float value,
                                     ImSize size = default,
                                     ReadOnlySpan<char> format = default,
                                     float step = 0.1f)
        {
            double doubleValue = value;
            var delta = TextEditNumeric(gui, ref doubleValue, DoubleFilterAllowEmptyString, format, size, step);
            value = (float)(doubleValue + delta);
        }

        private static double TextEditNumeric<T>(ImGui gui,
                                                 ref T value,
                                                 NumericFilter<T> filter,
                                                 ReadOnlySpan<char> format,
                                                 ImSize size,
                                                 double step)
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

            ImStyleScope<ImTextEditStyle> scope = default;
            
            var delta = 0;
            var rect = ImTextEdit.GetRect(gui, size);
            if (step != 0)
            {
                scope = new ImStyleScope<ImTextEditStyle>(ref ImTheme.Active.TextEdit);
                
                // TODO (artem-s): this looks awful
                ImTheme.Active.TextEdit.Normal.Box.BorderRadius.TopRight = 0;
                ImTheme.Active.TextEdit.Normal.Box.BorderRadius.BottomRight = 0;
                ImTheme.Active.TextEdit.Selected.Box.BorderRadius.TopRight = 0;
                ImTheme.Active.TextEdit.Selected.Box.BorderRadius.BottomRight = 0;
                
                delta = PlusMinusButtons(gui, ref rect);
            }

            var changed = gui.TextEdit(ref buffer, rect, filter, multiline: false);
            if (changed && filter.TryParse(buffer, out var newValue))
            {
                value = newValue;
            }

            if (scope.IsValid)
            {
                scope.Dispose();
            }

            return step * delta;
        }

        private static int PlusMinusButtons(ImGui gui, ref ImRect rect)
        {
            var height = rect.H;
            var width = height;

            var plusBtnRect = rect.SplitRight(width, out rect);
            var minusBtnRect = rect.SplitRight(width, out rect);
            var delta = 0;

            if (gui.Button("-", minusBtnRect, flag: ImButtonFlag.ReactToHeldDown | ImButtonFlag.NoRoundCornersLeft | ImButtonFlag.NoRoundCornersRight))
            {
                delta--;
            }

            if (gui.Button("+", plusBtnRect, flag: ImButtonFlag.ReactToHeldDown | ImButtonFlag.NoRoundCornersLeft))
            {
                delta++;
            }

            return delta;
        }

        public abstract class NumericFilter<T> : ImTextEditFilter
        {
            public override ImTouchKeyboardType KeyboardType => ImTouchKeyboardType.Numeric;

            protected readonly bool EmptyStringIsValid;

            public NumericFilter(bool emptyStringIsValid)
            {
                EmptyStringIsValid = emptyStringIsValid;
            }

            public abstract bool TryParse(ReadOnlySpan<char> buffer, out T value);
            public abstract bool TryFormat(Span<char> buffer, T value, out int length, ReadOnlySpan<char> format);
        }

        public sealed class Int64Filter : NumericFilter<Int64>
        {
            public Int64Filter(bool emptyStringIsValid = false) : base(emptyStringIsValid) { }

            public override bool IsValid(ReadOnlySpan<char> buffer) => TryParse(buffer, out _);
            public override string GetFallbackString() => "0";

            public override bool TryParse(ReadOnlySpan<char> buffer, out Int64 value)
            {
                if (EmptyStringIsValid && buffer.IsEmpty)
                {
                    value = 0;
                    return true;
                }

                return Int64.TryParse(buffer, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
            }

            public override bool TryFormat(Span<char> buffer, Int64 value, out int length, ReadOnlySpan<char> format) =>
                value.TryFormat(buffer, out length, format);
        }

        public sealed class DoubleFilter : NumericFilter<Double>
        {
            // allow to use comma as decimal separator
            private static readonly CultureInfo DeCulture = new("de");

            public DoubleFilter(bool emptyStringIsValid = false) : base(emptyStringIsValid) { }

            public override bool IsValid(ReadOnlySpan<char> buffer) => TryParse(buffer, out _);
            public override string GetFallbackString() => "0.0";

            public override bool TryParse(ReadOnlySpan<char> buffer, out Double value)
            {
                if (EmptyStringIsValid && buffer.IsEmpty)
                {
                    value = 0.0;
                    return true;
                }

                return Double.TryParse(buffer, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
                       Double.TryParse(buffer, NumberStyles.Float, DeCulture, out value);
            }

            public override bool TryFormat(Span<char> buffer, Double value, out int length, ReadOnlySpan<char> format) =>
                value.TryFormat(buffer, out length, format);
        }
    }
}