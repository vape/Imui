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

        public static bool IntEdit(this ImGui gui,
                                   ref int value,
                                   ImSize size = default,
                                   ReadOnlySpan<char> format = default,
                                   int step = 1)
        {
            long longValue = value;
            var changed = TextEditNumeric(gui, ref longValue, Int64FilterAllowEmptyString, format, size, step, out var delta);
            longValue += (long)delta;
            value = longValue > int.MaxValue ? int.MaxValue : longValue < int.MinValue ? int.MinValue : (int)longValue;
            return changed;
        }

        public static bool LongEdit(this ImGui gui,
                                    ref long value,
                                    ImSize size = default,
                                    ReadOnlySpan<char> format = default,
                                    long step = 0)
        {
            var changed = TextEditNumeric(gui, ref value, Int64FilterAllowEmptyString, format, size, step, out var delta);
            value += (long)delta;
            return changed;
        }

        public static bool FloatEdit(this ImGui gui,
                                     ref float value,
                                     ImSize size = default,
                                     ReadOnlySpan<char> format = default,
                                     float step = 0.1f)
        {
            double doubleValue = value;
            var changed = TextEditNumeric(gui, ref doubleValue, DoubleFilterAllowEmptyString, format, size, step, out var delta);
            value = (float)(doubleValue + delta);
            return changed;
        }

        private static bool TextEditNumeric<T>(ImGui gui,
                                               ref T value,
                                               NumericFilter<T> filter,
                                               ReadOnlySpan<char> format,
                                               ImSize size,
                                               double step,
                                               out double delta)
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
            
            delta = 0;
            
            var rect = ImTextEdit.GetRect(gui, size);
            if (step != 0)
            {
                delta = PlusMinusButtons(gui, ref rect) * step;
                gui.SetNextAdjacency(ImControlAdjacency.Left);
            }

            var changed = gui.TextEdit(ref buffer, rect, filter, multiline: false);
            if (changed && filter.TryParse(buffer, out var newValue))
            {
                value = newValue;
            }

            return delta != 0 || changed;
        }

        private static int PlusMinusButtons(ImGui gui, ref ImRect rect)
        {
            var height = rect.H;
            var width = height;

            var plusBtnRect = rect.SplitRight(width, out rect);
            var minusBtnRect = rect.SplitRight(width, out rect);
            var delta = 0;

            gui.SetNextAdjacency(ImControlAdjacency.Middle);
            if (gui.Button("-", minusBtnRect, flag: ImButtonFlag.ReactToHeldDown))
            {
                delta--;
            }

            gui.SetNextAdjacency(ImControlAdjacency.Right);
            if (gui.Button("+", plusBtnRect, flag: ImButtonFlag.ReactToHeldDown))
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