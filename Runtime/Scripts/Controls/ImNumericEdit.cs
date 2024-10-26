using System;
using System.Globalization;
using Imui.Core;
using Imui.IO.Utility;
using Imui.Style;

// ReSharper disable BuiltInTypeReferenceStyle

namespace Imui.Controls
{
    [Flags]
    public enum ImNumericEditFlag
    {
        None = 0
    }
    
    public static class ImNumericEdit
    {
        public static readonly ByteFilter FilterByte = new();
        public static readonly Int16Filter FilterInt16 = new();
        public static readonly Int32Filter FilterInt32 = new();
        public static readonly Int64Filter FilterInt64 = new();
        public static readonly SingleFilter FilterSingle = new();
        public static readonly DoubleFilter FilterDouble = new();

        private static void GetIdAndRect(ImGui gui, ImSize size, out uint id, out ImRect rect)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            id = gui.GetNextControlId();
            rect = ImTextEdit.GetRect(gui, size, false, out _);
        }
        
        public static bool NumericEdit(this ImGui gui,
                                       ref byte value,
                                       ImSize size = default,
                                       ReadOnlySpan<char> format = default,
                                       byte step = 0,
                                       byte min = byte.MinValue,
                                       byte max = byte.MaxValue,
                                       ImNumericEditFlag flags = default)
        {
            GetIdAndRect(gui, size, out var id, out var rect);

            return NumericEdit(gui, id, ref value, rect, format, step, min, max, flags);
        }
        
        public static bool NumericEdit(this ImGui gui,
                                       ref short value,
                                       ImSize size = default,
                                       ReadOnlySpan<char> format = default,
                                       short step = 0,
                                       short min = short.MinValue,
                                       short max = short.MaxValue,
                                       ImNumericEditFlag flags = default)
        {
            GetIdAndRect(gui, size, out var id, out var rect);

            return NumericEdit(gui, id, ref value, rect, format, step, min, max, flags);
        }

        public static bool NumericEdit(this ImGui gui,
                                       ref int value,
                                       ImSize size = default,
                                       ReadOnlySpan<char> format = default,
                                       int step = 0,
                                       int min = int.MinValue,
                                       int max = int.MaxValue,
                                       ImNumericEditFlag flags = default)
        {
            GetIdAndRect(gui, size, out var id, out var rect);

            return NumericEdit(gui, id, ref value, rect, format, step, min, max, flags);
        }

        public static bool NumericEdit(this ImGui gui,
                                       ref long value,
                                       ImSize size = default,
                                       ReadOnlySpan<char> format = default,
                                       long step = 0L,
                                       long min = long.MinValue,
                                       long max = long.MaxValue,
                                       ImNumericEditFlag flags = default)
        {
            GetIdAndRect(gui, size, out var id, out var rect);

            return NumericEdit(gui, id, ref value, rect, format, step, min, max, flags);
        }

        public static bool NumericEdit(this ImGui gui,
                                       ref float value,
                                       ImSize size = default,
                                       ReadOnlySpan<char> format = default,
                                       float step = 0.0f,
                                       float min = float.MinValue,
                                       float max = float.MaxValue,
                                       ImNumericEditFlag flags = default)
        {
            GetIdAndRect(gui, size, out var id, out var rect);

            return NumericEdit(gui, id, ref value, rect, format, step, min, max, flags);
        }
        
        public static bool NumericEdit(this ImGui gui,
                                       ref double value,
                                       ImSize size = default,
                                       ReadOnlySpan<char> format = default,
                                       double step = 0.0f,
                                       double min = double.MinValue,
                                       double max = double.MaxValue,
                                       ImNumericEditFlag flags = default)
        {
            GetIdAndRect(gui, size, out var id, out var rect);
            
            return NumericEdit(gui, id, ref value, rect, format, step, min, max, flags);
        }

        public static bool NumericEdit(ImGui gui,
                                       uint id,
                                       ref byte value,
                                       ImRect rect,
                                       ReadOnlySpan<char> format = default,
                                       byte step = 0,
                                       byte min = byte.MinValue,
                                       byte max = byte.MaxValue,
                                       ImNumericEditFlag flags = default)
        {
            return NumericEditControl(gui, id, ref value, FilterByte, rect, format, step, min, max, flags);
        }

        public static bool NumericEdit(ImGui gui,
                                       uint id,
                                       ref short value,
                                       ImRect rect,
                                       ReadOnlySpan<char> format = default,
                                       short step = 0,
                                       short min = short.MinValue,
                                       short max = short.MaxValue,
                                       ImNumericEditFlag flags = default)
        {
            return NumericEditControl(gui, id, ref value, FilterInt16, rect, format, step, min, max, flags);
        }

        public static bool NumericEdit(ImGui gui,
                                       uint id,
                                       ref int value,
                                       ImRect rect,
                                       ReadOnlySpan<char> format = default,
                                       int step = 0,
                                       int min = int.MinValue,
                                       int max = int.MaxValue,
                                       ImNumericEditFlag flags = default)
        {
            return NumericEditControl(gui, id, ref value, FilterInt32, rect, format, step, min, max, flags);
        }

        public static bool NumericEdit(ImGui gui,
                                       uint id,
                                       ref long value,
                                       ImRect rect,
                                       ReadOnlySpan<char> format = default,
                                       long step = 0L,
                                       long min = long.MinValue,
                                       long max = long.MaxValue,
                                       ImNumericEditFlag flags = default)
        {
            return NumericEditControl(gui, id, ref value, FilterInt64, rect, format, step, min, max, flags);
        }

        public static bool NumericEdit(ImGui gui,
                                       uint id,
                                       ref float value,
                                       ImRect rect,
                                       ReadOnlySpan<char> format = default,
                                       float step = 0.1f,
                                       float min = float.MinValue,
                                       float max = float.MaxValue,
                                       ImNumericEditFlag flags = default)
        {
            return NumericEditControl(gui, id, ref value, FilterSingle, rect, format, step, min, max, flags);
        }

        public static bool NumericEdit(ImGui gui,
                                       uint id,
                                       ref double value,
                                       ImRect rect,
                                       ReadOnlySpan<char> format = default,
                                       double step = 0.1d,
                                       double min = double.MinValue,
                                       double max = double.MaxValue,
                                       ImNumericEditFlag flags = default)
        {
            return NumericEditControl(gui, id, ref value, FilterDouble, rect, format, step, min, max, flags);
        }

        public static bool NumericEditControl<T>(ImGui gui,
                                                 uint id,
                                                 ref T value,
                                                 NumericFilter<T> filter,
                                                 ImRect rect,
                                                 ReadOnlySpan<char> format,
                                                 double step,
                                                 T min,
                                                 T max,
                                                 ImNumericEditFlag flags)
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

            var delta = 0.0d;

            if (step != 0)
            {
                delta = PlusMinusButtons(gui, ref rect) * step;
                gui.SetNextAdjacency(ImAdjacency.Left);
            }

            var changed = gui.TextEdit(id, ref buffer, rect, false, filter);
            if (changed && filter.TryParse(buffer, out var newValue))
            {
                value = newValue;
            }

            if (delta != 0)
            {
                value = filter.Add(value, delta);
            }

            if (changed)
            {
                value = filter.Clamp(value, min, max);
            }

            return changed;
        }

        private static int PlusMinusButtons(ImGui gui, ref ImRect rect)
        {
            var border = gui.Style.Button.BorderThickness;
            var height = rect.H;
            var width = height;

            var plusBtnRect = rect.SplitRight(width, -border, out rect);
            var minusBtnRect = rect.SplitRight(width, -border, out rect);
            var delta = 0;

            gui.SetNextAdjacency(ImAdjacency.Middle);
            if (gui.Button("-", minusBtnRect, flags: ImButtonFlag.ReactToHeldDown))
            {
                delta--;
            }

            gui.SetNextAdjacency(ImAdjacency.Right);
            if (gui.Button("+", plusBtnRect, flags: ImButtonFlag.ReactToHeldDown))
            {
                delta++;
            }

            return delta;
        }

        public abstract class NumericFilter<T> : ImTextEditFilter
        {
            // allow to use comma as decimal separator
            protected static readonly CultureInfo DeutschCulture = new("de");

            public override ImTouchKeyboardType KeyboardType => ImTouchKeyboardType.Numeric;

            public override string GetFallbackString()
            {
                return "0";
            }

            public override bool IsValid(ReadOnlySpan<char> buffer)
            {
                return TryParse(buffer, out _);
            }

            public virtual bool TryParse(ReadOnlySpan<char> buffer, out T value)
            {
                if (buffer.IsEmpty)
                {
                    value = default;
                    return true;
                }

                return TryParseNonEmpty(in buffer, out value);
            }

            public abstract T Add(T value0, double value1);
            public abstract T Clamp(T value, T min, T max);
            public abstract bool TryParseNonEmpty(in ReadOnlySpan<char> buffer, out T value);
            public abstract bool TryFormat(Span<char> buffer, T value, out int length, ReadOnlySpan<char> format);

            protected double Add(double value0, double value1, double min, double max)
            {
                var result = value0 + value1;
                if (result > max)
                {
                    result = max;
                }
                else if (result < min)
                {
                    result = min;
                }

                return result;
            }
        }

        public sealed class ByteFilter : NumericFilter<Byte>
        {
            public override Byte Add(Byte value0, double value1) => (Byte)Add(value0, value1, Byte.MinValue, Byte.MaxValue);
            public override Byte Clamp(Byte value, Byte min, Byte max) => value > max ? max : value < min ? min : value;

            public override bool TryParseNonEmpty(in ReadOnlySpan<char> buffer, out Byte value) =>
                Byte.TryParse(buffer, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

            public override bool TryFormat(Span<char> buffer, Byte value, out int length, ReadOnlySpan<char> format) =>
                value.TryFormat(buffer, out length, format);
        }

        public sealed class Int16Filter : NumericFilter<Int16>
        {
            public override Int16 Add(Int16 value0, double value1) => (Int16)Add(value0, value1, byte.MinValue, byte.MaxValue);
            public override Int16 Clamp(Int16 value, Int16 min, Int16 max) => value > max ? max : value < min ? min : value;

            public override bool TryParseNonEmpty(in ReadOnlySpan<char> buffer, out Int16 value) =>
                Int16.TryParse(buffer, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

            public override bool TryFormat(Span<char> buffer, Int16 value, out int length, ReadOnlySpan<char> format) =>
                value.TryFormat(buffer, out length, format);
        }

        public sealed class Int32Filter : NumericFilter<Int32>
        {
            public override Int32 Add(Int32 value0, double value1) => (Int32)Add(value0, value1, Int32.MinValue, Int32.MaxValue);
            public override Int32 Clamp(Int32 value, Int32 min, Int32 max) => value > max ? max : value < min ? min : value;

            public override bool TryParseNonEmpty(in ReadOnlySpan<char> buffer, out Int32 value) =>
                Int32.TryParse(buffer, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

            public override bool TryFormat(Span<char> buffer, Int32 value, out int length, ReadOnlySpan<char> format) =>
                value.TryFormat(buffer, out length, format);
        }

        public sealed class Int64Filter : NumericFilter<Int64>
        {
            public override Int64 Add(Int64 value0, double value1) => (Int64)Add(value0, value1, Int64.MinValue, Int64.MaxValue);
            public override Int64 Clamp(Int64 value, Int64 min, Int64 max) => value > max ? max : value < min ? min : value;

            public override bool TryParseNonEmpty(in ReadOnlySpan<char> buffer, out Int64 value) =>
                Int64.TryParse(buffer, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

            public override bool TryFormat(Span<char> buffer, Int64 value, out int length, ReadOnlySpan<char> format) =>
                value.TryFormat(buffer, out length, format);
        }

        public sealed class SingleFilter : NumericFilter<Single>
        {
            public override Single Add(Single value0, double value1) => (Single)Add(value0, value1, Single.MinValue, Single.MaxValue);
            public override Single Clamp(Single value, Single min, Single max) => value > max ? max : value < min ? min : value;

            public override bool TryParseNonEmpty(in ReadOnlySpan<char> buffer, out Single value)
            {
                return Single.TryParse(buffer, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
                       Single.TryParse(buffer, NumberStyles.Float, DeutschCulture, out value);
            }

            public override bool TryFormat(Span<char> buffer, Single value, out int length, ReadOnlySpan<char> format) =>
                value.TryFormat(buffer, out length, format);
        }

        public sealed class DoubleFilter : NumericFilter<Double>
        {
            public override Double Add(Double value0, double value1) => (Double)Add(value0, value1, Double.MinValue, Double.MaxValue);
            public override Double Clamp(Double value, Double min, Double max) => value > max ? max : value < min ? min : value;

            public override bool TryParseNonEmpty(in ReadOnlySpan<char> buffer, out Double value)
            {
                return Double.TryParse(buffer, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
                       Double.TryParse(buffer, NumberStyles.Float, DeutschCulture, out value);
            }

            public override bool TryFormat(Span<char> buffer, Double value, out int length, ReadOnlySpan<char> format) =>
                value.TryFormat(buffer, out length, format);
        }
    }
}