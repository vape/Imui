using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Imui.Core;
using Imui.IO.Events;
using Imui.IO.Touch;
using Imui.Rendering;
using Imui.Style;
using UnityEngine;

// ReSharper disable BuiltInTypeReferenceStyle
namespace Imui.Controls
{
    [Flags]
    public enum ImNumericEditFlag
    {
        None = 0,
        PlusMinus = 1 << 0,
        Slider = 1 << 1
    }

    public static class ImNumericEdit
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(this ImGui gui,
                                       ref byte value,
                                       ImSize size = default,
                                       ReadOnlySpan<char> format = default,
                                       byte step = 1,
                                       byte min = byte.MinValue,
                                       byte max = byte.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            var rect = ImTextEdit.AddRect(gui, size, false, out _);
            return NumericEdit(gui, ref value, rect, format, step, min, max, flags, hint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(this ImGui gui,
                                       ref short value,
                                       ImSize size = default,
                                       ReadOnlySpan<char> format = default,
                                       short step = 1,
                                       short min = short.MinValue,
                                       short max = short.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            var rect = ImTextEdit.AddRect(gui, size, false, out _);
            return NumericEdit(gui, ref value, rect, format, step, min, max, flags, hint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(this ImGui gui,
                                       ref int value,
                                       ImSize size = default,
                                       ReadOnlySpan<char> format = default,
                                       int step = 1,
                                       int min = int.MinValue,
                                       int max = int.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            var rect = ImTextEdit.AddRect(gui, size, false, out _);
            return NumericEdit(gui, ref value, rect, format, step, min, max, flags, hint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(this ImGui gui,
                                       ref long value,
                                       ImSize size = default,
                                       ReadOnlySpan<char> format = default,
                                       long step = 1L,
                                       long min = long.MinValue,
                                       long max = long.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            var rect = ImTextEdit.AddRect(gui, size, false, out _);
            return NumericEdit(gui, ref value, rect, format, step, min, max, flags, hint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(this ImGui gui,
                                       ref float value,
                                       ImSize size = default,
                                       ReadOnlySpan<char> format = default,
                                       float step = 0.1f,
                                       float min = float.MinValue,
                                       float max = float.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            var rect = ImTextEdit.AddRect(gui, size, false, out _);
            return NumericEdit(gui, ref value, rect, format, step, min, max, flags, hint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(this ImGui gui,
                                       ref double value,
                                       ImSize size = default,
                                       ReadOnlySpan<char> format = default,
                                       double step = 0.1d,
                                       double min = double.MinValue,
                                       double max = double.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            var rect = ImTextEdit.AddRect(gui, size, false, out _);
            return NumericEdit(gui, ref value, rect, format, step, min, max, flags, hint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(ImGui gui,
                                       ref byte value,
                                       ImRect rect,
                                       ReadOnlySpan<char> format = default,
                                       byte step = 1,
                                       byte min = byte.MinValue,
                                       byte max = byte.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            var id = gui.GetNextControlId();

            return NumericEdit(gui, id, ref value, rect, format, step, min, max, flags, hint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(ImGui gui,
                                       ref short value,
                                       ImRect rect,
                                       ReadOnlySpan<char> format = default,
                                       short step = 1,
                                       short min = short.MinValue,
                                       short max = short.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            var id = gui.GetNextControlId();

            return NumericEdit(gui, id, ref value, rect, format, step, min, max, flags, hint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(ImGui gui,
                                       ref int value,
                                       ImRect rect,
                                       ReadOnlySpan<char> format = default,
                                       int step = 1,
                                       int min = int.MinValue,
                                       int max = int.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            var id = gui.GetNextControlId();

            return NumericEdit(gui, id, ref value, rect, format, step, min, max, flags, hint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(ImGui gui,
                                       ref long value,
                                       ImRect rect,
                                       ReadOnlySpan<char> format = default,
                                       long step = 1L,
                                       long min = long.MinValue,
                                       long max = long.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            var id = gui.GetNextControlId();

            return NumericEdit(gui, id, ref value, rect, format, step, min, max, flags, hint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(ImGui gui,
                                       ref float value,
                                       ImRect rect,
                                       ReadOnlySpan<char> format = default,
                                       float step = 0.1f,
                                       float min = float.MinValue,
                                       float max = float.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            var id = gui.GetNextControlId();

            return NumericEdit(gui, id, ref value, rect, format, step, min, max, flags, hint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(ImGui gui,
                                       ref double value,
                                       ImRect rect,
                                       ReadOnlySpan<char> format = default,
                                       double step = 0.1d,
                                       double min = double.MinValue,
                                       double max = double.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            var id = gui.GetNextControlId();

            return NumericEdit(gui, id, ref value, rect, format, step, min, max, flags, hint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(ImGui gui,
                                       uint id,
                                       ref byte value,
                                       ImRect rect,
                                       ReadOnlySpan<char> format = default,
                                       byte step = 1,
                                       byte min = byte.MinValue,
                                       byte max = byte.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            var numberValue = new NumberValue(value);
            var changed = NumericEditControl(gui, id, ref numberValue, rect, format, step, min, max, flags, hint);
            if (changed)
            {
                value = numberValue.ValueByte;
            }

            return changed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(ImGui gui,
                                       uint id,
                                       ref short value,
                                       ImRect rect,
                                       ReadOnlySpan<char> format = default,
                                       short step = 1,
                                       short min = short.MinValue,
                                       short max = short.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            var numberValue = new NumberValue(value);
            var changed = NumericEditControl(gui, id, ref numberValue, rect, format, step, min, max, flags, hint);
            if (changed)
            {
                value = numberValue.ValueInt16;
            }

            return changed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(ImGui gui,
                                       uint id,
                                       ref int value,
                                       ImRect rect,
                                       ReadOnlySpan<char> format = default,
                                       int step = 1,
                                       int min = int.MinValue,
                                       int max = int.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            var numberValue = new NumberValue(value);
            var changed = NumericEditControl(gui, id, ref numberValue, rect, format, step, min, max, flags, hint);
            if (changed)
            {
                value = numberValue.ValueInt32;
            }

            return changed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(ImGui gui,
                                       uint id,
                                       ref long value,
                                       ImRect rect,
                                       ReadOnlySpan<char> format = default,
                                       long step = 1L,
                                       long min = long.MinValue,
                                       long max = long.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            var numberValue = new NumberValue(value);
            var changed = NumericEditControl(gui, id, ref numberValue, rect, format, step, min, max, flags, hint);
            if (changed)
            {
                value = numberValue.ValueInt64;
            }

            return changed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(ImGui gui,
                                       uint id,
                                       ref float value,
                                       ImRect rect,
                                       ReadOnlySpan<char> format = default,
                                       float step = 0.1f,
                                       float min = float.MinValue,
                                       float max = float.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            var numberValue = new NumberValue(value);
            var changed = NumericEditControl(gui, id, ref numberValue, rect, format, step, min, max, flags, hint);
            if (changed)
            {
                value = numberValue.ValueSingle;
            }

            return changed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(ImGui gui,
                                       uint id,
                                       ref double value,
                                       ImRect rect,
                                       ReadOnlySpan<char> format = default,
                                       double step = 0.1d,
                                       double min = double.MinValue,
                                       double max = double.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            var numberValue = new NumberValue(value);
            var changed = NumericEditControl(gui, id, ref numberValue, rect, format, step, min, max, flags, hint);
            if (changed)
            {
                value = numberValue.ValueDouble;
            }

            return changed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(this ref ImGroup group,
                                       uint id,
                                       ref byte value,
                                       ReadOnlySpan<char> format = default,
                                       byte step = 1,
                                       byte min = byte.MinValue,
                                       byte max = byte.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            var item = group.GetNext();
            bool changed;
            
            var normalBorder = group.Gui.Style.TextEdit.Normal.Box.BorderRadius;
            var selectedBorder = group.Gui.Style.TextEdit.Selected.Box.BorderRadius;
            
            try
            {
                group.Gui.Style.TextEdit.Normal.Box.BorderRadius.Apply(item.Flags);
                group.Gui.Style.TextEdit.Selected.Box.BorderRadius.Apply(item.Flags);
            
                changed = NumericEdit(group.Gui, id, ref value, item.Rect, format, step, min, max, flags, hint);
            }
            finally
            {
                group.Gui.Style.TextEdit.Normal.Box.BorderRadius = normalBorder;
                group.Gui.Style.TextEdit.Selected.Box.BorderRadius = selectedBorder;
            }
            
            return changed;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(this ref ImGroup group,
                                       uint id,
                                       ref short value,
                                       ReadOnlySpan<char> format = default,
                                       short step = 1,
                                       short min = short.MinValue,
                                       short max = short.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            var item = group.GetNext();
            bool changed;
            
            var normalBorder = group.Gui.Style.TextEdit.Normal.Box.BorderRadius;
            var selectedBorder = group.Gui.Style.TextEdit.Selected.Box.BorderRadius;
            
            try
            {
                group.Gui.Style.TextEdit.Normal.Box.BorderRadius.Apply(item.Flags);
                group.Gui.Style.TextEdit.Selected.Box.BorderRadius.Apply(item.Flags);
            
                changed = NumericEdit(group.Gui, id, ref value, item.Rect, format, step, min, max, flags, hint);
            }
            finally
            {
                group.Gui.Style.TextEdit.Normal.Box.BorderRadius = normalBorder;
                group.Gui.Style.TextEdit.Selected.Box.BorderRadius = selectedBorder;
            }
            
            return changed;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(this ref ImGroup group,
                                       uint id,
                                       ref int value,
                                       ReadOnlySpan<char> format = default,
                                       int step = 1,
                                       int min = int.MinValue,
                                       int max = int.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            var item = group.GetNext();
            bool changed;
            
            var normalBorder = group.Gui.Style.TextEdit.Normal.Box.BorderRadius;
            var selectedBorder = group.Gui.Style.TextEdit.Selected.Box.BorderRadius;
            
            try
            {
                group.Gui.Style.TextEdit.Normal.Box.BorderRadius.Apply(item.Flags);
                group.Gui.Style.TextEdit.Selected.Box.BorderRadius.Apply(item.Flags);
            
                changed = NumericEdit(group.Gui, id, ref value, item.Rect, format, step, min, max, flags, hint);
            }
            finally
            {
                group.Gui.Style.TextEdit.Normal.Box.BorderRadius = normalBorder;
                group.Gui.Style.TextEdit.Selected.Box.BorderRadius = selectedBorder;
            }
            
            return changed;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(this ref ImGroup group,
                                       uint id,
                                       ref long value,
                                       ReadOnlySpan<char> format = default,
                                       long step = 1,
                                       long min = long.MinValue,
                                       long max = long.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            var item = group.GetNext();
            bool changed;
            
            var normalBorder = group.Gui.Style.TextEdit.Normal.Box.BorderRadius;
            var selectedBorder = group.Gui.Style.TextEdit.Selected.Box.BorderRadius;
            
            try
            {
                group.Gui.Style.TextEdit.Normal.Box.BorderRadius.Apply(item.Flags);
                group.Gui.Style.TextEdit.Selected.Box.BorderRadius.Apply(item.Flags);
            
                changed = NumericEdit(group.Gui, id, ref value, item.Rect, format, step, min, max, flags, hint);
            }
            finally
            {
                group.Gui.Style.TextEdit.Normal.Box.BorderRadius = normalBorder;
                group.Gui.Style.TextEdit.Selected.Box.BorderRadius = selectedBorder;
            }
            
            return changed;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(this ref ImGroup group,
                                       uint id,
                                       ref float value,
                                       ReadOnlySpan<char> format = default,
                                       float step = 0.1f,
                                       float min = float.MinValue,
                                       float max = float.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            var item = group.GetNext();
            bool changed;
            
            var normalBorder = group.Gui.Style.TextEdit.Normal.Box.BorderRadius;
            var selectedBorder = group.Gui.Style.TextEdit.Selected.Box.BorderRadius;
            
            try
            {
                group.Gui.Style.TextEdit.Normal.Box.BorderRadius.Apply(item.Flags);
                group.Gui.Style.TextEdit.Selected.Box.BorderRadius.Apply(item.Flags);
            
                changed = NumericEdit(group.Gui, id, ref value, item.Rect, format, step, min, max, flags, hint);
            }
            finally
            {
                group.Gui.Style.TextEdit.Normal.Box.BorderRadius = normalBorder;
                group.Gui.Style.TextEdit.Selected.Box.BorderRadius = selectedBorder;
            }
            
            return changed;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NumericEdit(this ref ImGroup group,
                                       uint id,
                                       ref double value,
                                       ReadOnlySpan<char> format = default,
                                       double step = 0.1d,
                                       double min = double.MinValue,
                                       double max = double.MaxValue,
                                       ImNumericEditFlag flags = default,
                                       ReadOnlySpan<char> hint = default)
        {
            var item = group.GetNext();
            bool changed;
            
            var normalBorder = group.Gui.Style.TextEdit.Normal.Box.BorderRadius;
            var selectedBorder = group.Gui.Style.TextEdit.Selected.Box.BorderRadius;
            
            try
            {
                group.Gui.Style.TextEdit.Normal.Box.BorderRadius.Apply(item.Flags);
                group.Gui.Style.TextEdit.Selected.Box.BorderRadius.Apply(item.Flags);
            
                changed = NumericEdit(group.Gui, id, ref value, item.Rect, format, step, min, max, flags, hint);
            }
            finally
            {
                group.Gui.Style.TextEdit.Normal.Box.BorderRadius = normalBorder;
                group.Gui.Style.TextEdit.Selected.Box.BorderRadius = selectedBorder;
            }
            
            return changed;
        }

        public static bool NumericEditControl(ImGui gui,
                                              uint id,
                                              ref NumberValue value,
                                              ImRect rect,
                                              ReadOnlySpan<char> format,
                                              NumberValue step,
                                              NumberValue min,
                                              NumberValue max,
                                              ImNumericEditFlag flags,
                                              ReadOnlySpan<char> hint)
        {
            ImAssert.IsTrue(value.Type == min.Type && value.Type == max.Type && value.Type == step.Type,
                            "value.Type == min.Type && value.Type == max.Type && value.Type == step.Type");

            var delta = 0.0d;
            var hovered = gui.IsControlHovered(id);
            var active = gui.IsControlActive(id);
            var useSlider = (flags & ImNumericEditFlag.Slider) != 0;
            var usePlusMinusButtons = !useSlider && (flags & ImNumericEditFlag.PlusMinus) != 0;

            ref readonly var evt = ref gui.Input.MouseEvent;

            gui.PushId(id);

            var bufferId = gui.GetNextControlId();
            var sliderId = gui.GetNextControlId();

            uint plusId = 0;
            uint minusId = 0;
            ImRect plusMinusRect = default;

            if (usePlusMinusButtons)
            {
                plusId = gui.GetNextControlId();
                minusId = gui.GetNextControlId();
                plusMinusRect = rect.TakeRight(rect.H * 2);
                plusMinusRect.X += gui.Style.Global.EmbeddedButtonPadding;
                plusMinusRect.W -= gui.Style.Global.EmbeddedButtonPadding;
            }

            if (!active && useSlider)
            {
                // (artem-s): when double-clicking, pass control to the text editor
                if (evt.LeftButton && (evt.Type != ImMouseEventType.Down || evt.Count < 2))
                {
                    var sliderMin = min.AsDouble();
                    var sliderMax = max.AsDouble();
                    var sliderStep = step.AsDouble();
                    var sliderDelta = NumericSlider(gui, id, sliderId, sliderMin, sliderMax, sliderStep, rect);

                    if (value.Type.IsInteger())
                    {
                        sliderDelta = Math.Round(sliderDelta);
                    }

                    delta += sliderDelta;
                }
            }

            gui.PopId();

            ImTextEditBuffer textBuffer;

            if (active)
            {
                ref readonly var editBuffer = ref gui.Storage.Get<EditBuffer>(bufferId);
                var tempBuffer = gui.Arena.AllocArray<char>(editBuffer.Count);
                EditBuffer.Copy(in editBuffer, tempBuffer);
                
                textBuffer = new ImTextEditBuffer(tempBuffer, tempBuffer.Length, gui.Arena, EditBuffer.BUFFER_LENGTH);
            }
            else
            {
                textBuffer = new ImTextEditBuffer(value.Format(gui.Formatter, format), gui.Arena, 0);
            }
            
            var changed = false;

            if (!active && useSlider)
            {
                ref readonly var style = ref gui.Style.TextEdit.Normal.Box;

                var align = gui.Style.TextEdit.Alignment;
                var halfVertPadding = Mathf.Max(rect.H - gui.TextDrawer.GetLineHeightFromFontSize(gui.Style.Layout.TextSize), 0.0f) / 2.0f;
                var textRect = rect.WithPadding(left: gui.Style.Layout.InnerSpacing,
                                                right: gui.Style.Layout.InnerSpacing,
                                                top: halfVertPadding,
                                                bottom: halfVertPadding);

                gui.Canvas.RectWithOutline(rect, style.BackColor, style.BorderColor, style.BorderThickness, style.BorderRadius);
                gui.Canvas.Text(textBuffer,
                                style.FrontColor,
                                textRect,
                                gui.Style.Layout.TextSize,
                                alignX: align.X,
                                alignY: align.Y,
                                false,
                                ImTextOverflow.Ellipsis);

                gui.RegisterControl(id, rect);

                if (hovered && evt is { Type: ImMouseEventType.Down, LeftButton: true, Count: >= 2 })
                {
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                }
            }
            else
            {
                using (gui.StyleScope(ref gui.Style.TextEdit.Padding.Right, plusMinusRect.W))
                {
                    changed = gui.TextEdit(id, ref textBuffer, rect, false, ImTouchKeyboardType.Numeric, hint);
                }
            }

            if (usePlusMinusButtons)
            {
                var group = new ImGroup(gui, plusMinusRect.WithPadding(gui.Style.Global.EmbeddedButtonPadding), 2);
                
                if (group.Button(minusId,
                               "-",
                               gui.Style.AccentButton,
                               out _,
                               ImButtonFlag.ReactToHeldDown))
                {
                    delta -= step.AsDouble();
                }

                if (group.Button(plusId,
                               "+",
                               gui.Style.AccentButton,
                               out _,
                               ImButtonFlag.ReactToHeldDown))
                {
                    delta += step.AsDouble();
                }
            }

            var justActivated = !active && gui.IsControlActive(id);
            if (justActivated)
            {
                ref var editBuffer = ref gui.Storage.Get<EditBuffer>(bufferId);
                editBuffer.Populate(value.Format(gui.Formatter));
                ImTextEdit.SelectAll(gui, id, editBuffer.Count);
            }

            if (delta != 0)
            {
                value.Add(delta);
                changed = true;
            }

            if (changed)
            {
                if (active)
                {
                    ref var editBuffer = ref gui.Storage.Get<EditBuffer>(bufferId);
                    editBuffer.Populate(textBuffer);

                    if (NumberValue.TryParse(value.Type, textBuffer, out var newValue))
                    {
                        value = newValue;
                    }
                }

                value.Clamp(in min, in max);
            }

            return changed;
        }

        private static void HandleDrag(in ImMouseEvent evt, ref double delta, double step, double min, double max, in ImRect rect)
        {
            if (evt.Delta.x == 0)
            {
                return;
            }

            delta = step == 0 ? Math.Min(max - min, rect.W) * evt.Delta.x / rect.W : step * Math.Sign(evt.Delta.x);
        }

        public static double NumericSlider(ImGui gui, uint hoveredId, uint id, double min, double max, double step, ImRect rect)
        {
            var hovered = gui.IsControlHovered(hoveredId);
            var active = gui.IsControlActive(id);
            var delta = 0.0d;

            gui.RegisterControl(id, rect);

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when evt.LeftButton && hovered:
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                    HandleDrag(in evt, ref delta, step, min, max, in rect);
                    gui.Input.UseMouseEvent();
                    break;

                case ImMouseEventType.Drag when active:
                    HandleDrag(in evt, ref delta, step, min, max, in rect);
                    gui.Input.UseMouseEvent();
                    break;

                case ImMouseEventType.Up when active:
                    gui.ResetActiveControl();
                    break;
            }

            return delta;
        }

        private static bool IsInteger(this NumberType type)
        {
            return type is NumberType.Byte or NumberType.Int16 or NumberType.Int32 or NumberType.Int64;
        }

        public enum NumberType
        {
            Byte,
            Int16,
            Int32,
            Int64,
            Single,
            Double
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct EditBuffer
        {
            public const int BUFFER_LENGTH = 126;

            public int Count => count;

            private int count;
            private fixed char fixedBuffer[BUFFER_LENGTH];

            public void Populate(ReadOnlySpan<char> str)
            {
                var stringToCopy = str[..Math.Min(BUFFER_LENGTH, str.Length)];

                fixed (char* buf = fixedBuffer)
                {
                    stringToCopy.CopyTo(new Span<char>(buf, BUFFER_LENGTH));
                    count = stringToCopy.Length;
                }
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Copy(in EditBuffer from, Span<char> to)
            {
                fixed (char* buf = from.fixedBuffer)
                {
                    var toCopy = Math.Min(from.count, to.Length);
                    new Span<char>(buf, toCopy).CopyTo(to);
                    return toCopy;
                }
            }
        }

        // (artem-s): if only we could generalize working with numbers... Oh, we can! But not in Unity with their shit-tier support for new c# features
        public struct NumberValue
        {
            // (artem-s): allow using comma as a decimal separator
            private static readonly CultureInfo CultureDeutsch = new("de");
            private static readonly CultureInfo CultureInvariant = CultureInfo.InvariantCulture;

            private const NumberStyles INTEGER_STYLE = NumberStyles.Integer | NumberStyles.AllowExponent;
            private const NumberStyles FLOAT_STYLE = NumberStyles.Float;

            public readonly NumberType Type;

            public Byte ValueByte;
            public Int16 ValueInt16;
            public Int32 ValueInt32;
            public Int64 ValueInt64;
            public Single ValueSingle;
            public Double ValueDouble;

            public NumberValue(Byte value)
            {
                Type = NumberType.Byte;
                ValueByte = value;
                ValueInt16 = 0;
                ValueInt32 = 0;
                ValueInt64 = 0;
                ValueSingle = 0;
                ValueDouble = 0;
            }

            public NumberValue(Int16 value)
            {
                Type = NumberType.Int16;
                ValueByte = 0;
                ValueInt16 = value;
                ValueInt32 = 0;
                ValueInt64 = 0;
                ValueSingle = 0;
                ValueDouble = 0;
            }

            public NumberValue(Int32 value)
            {
                Type = NumberType.Int32;
                ValueByte = 0;
                ValueInt16 = 0;
                ValueInt32 = value;
                ValueInt64 = 0;
                ValueSingle = 0;
                ValueDouble = 0;
            }

            public NumberValue(Int64 value)
            {
                Type = NumberType.Int64;
                ValueByte = 0;
                ValueInt16 = 0;
                ValueInt32 = 0;
                ValueInt64 = value;
                ValueSingle = 0;
                ValueDouble = 0;
            }

            public NumberValue(Single value)
            {
                Type = NumberType.Single;
                ValueByte = 0;
                ValueInt16 = 0;
                ValueInt32 = 0;
                ValueInt64 = 0;
                ValueSingle = value;
                ValueDouble = 0;
            }

            public NumberValue(Double value)
            {
                Type = NumberType.Double;
                ValueByte = 0;
                ValueInt16 = 0;
                ValueInt32 = 0;
                ValueInt64 = 0;
                ValueSingle = 0;
                ValueDouble = value;
            }

            private NumberValue(NumberType type)
            {
                Type = type;
                ValueByte = 0;
                ValueInt16 = 0;
                ValueInt32 = 0;
                ValueInt64 = 0;
                ValueSingle = 0;
                ValueDouble = 0;
            }

            public double AsDouble()
            {
                return Type switch
                {
                    NumberType.Byte => ValueByte,
                    NumberType.Int16 => ValueInt16,
                    NumberType.Int32 => ValueInt32,
                    NumberType.Int64 => ValueInt64,
                    NumberType.Single => ValueSingle,
                    NumberType.Double => ValueDouble,
                    _ => throw new NotImplementedException()
                };
            }

            public ReadOnlySpan<char> Format(ImFormatter formatter, ReadOnlySpan<char> format = default)
            {
                return Type switch
                {
                    NumberType.Byte => formatter.Format(ValueByte, format),
                    NumberType.Int16 => formatter.Format(ValueInt16, format),
                    NumberType.Int32 => formatter.Format(ValueInt32, format),
                    NumberType.Int64 => formatter.Format(ValueInt64, format),
                    NumberType.Single => formatter.Format(ValueSingle, format == default ? "G9" : format),
                    NumberType.Double => formatter.Format(ValueDouble, format == default ? "G17" : format),
                    _ => throw new NotImplementedException()
                };
            }

            public void Add(Double value)
            {
                switch (Type)
                {
                    case NumberType.Byte:
                    {
                        Int32 result = ValueByte + (Int32)value;
                        if (result > Byte.MaxValue)
                        {
                            result = Byte.MaxValue;
                        }
                        else if (result < Byte.MinValue)
                        {
                            result = Byte.MinValue;
                        }
                        ValueByte = (Byte)result;
                        break;
                    }
                    case NumberType.Int16:
                    {
                        Int32 result = ValueInt16 + (Int32)value;
                        if (result > Int16.MaxValue)
                        {
                            result = Int16.MaxValue;
                        }
                        else if (result < Int16.MinValue)
                        {
                            result = Int16.MinValue;
                        }
                        ValueInt16 = (Int16)result;
                        break;
                    }
                    case NumberType.Int32:
                    {
                        Int64 result = (Int64)ValueInt32 + (Int64)value;
                        if (result > Int32.MaxValue)
                        {
                            result = Int32.MaxValue;
                        }
                        else if (result < Int32.MinValue)
                        {
                            result = Int32.MinValue;
                        }
                        ValueInt32 = (Int32)result;
                        break;
                    }
                    case NumberType.Int64:
                    {
                        if (value > 0 && ValueInt64 > Int64.MaxValue - (Int64)value)
                        {
                            ValueInt64 = Int64.MaxValue;
                        }
                        else if (value < 0 && ValueInt64 < Int64.MinValue - (Int64)value)
                        {
                            ValueInt64 = Int64.MinValue;
                        }
                        else
                        {
                            ValueInt64 += (Int64)value;
                        }
                        break;
                    }
                    case NumberType.Single:
                    {
                        Double result = (Double)ValueSingle + value;
                        if (Double.IsPositiveInfinity(result))
                        {
                            ValueSingle = Single.MaxValue;
                        }
                        else if (Double.IsNegativeInfinity(result))
                        {
                            ValueSingle = Single.MinValue;
                        }
                        else
                        {
                            ValueSingle = (Single)result;
                        }
                        break;
                    }
                    case NumberType.Double:
                    {
                        Double result = ValueDouble + value;
                        if (Double.IsPositiveInfinity(result))
                        {
                            ValueDouble = Double.MaxValue;
                        }
                        else if (Double.IsNegativeInfinity(result))
                        {
                            ValueDouble = Double.MinValue;
                        }
                        else
                        {
                            ValueDouble = result;
                        }
                        break;
                    }
                    default:
                        throw new NotImplementedException();
                }
            }

            public void Clamp(in NumberValue min, in NumberValue max)
            {
                Clamp(ref this, in min, in max);
            }

            public static implicit operator NumberValue(Byte value) => new(value);
            public static implicit operator NumberValue(Int16 value) => new(value);
            public static implicit operator NumberValue(Int32 value) => new(value);
            public static implicit operator NumberValue(Int64 value) => new(value);
            public static implicit operator NumberValue(Single value) => new(value);
            public static implicit operator NumberValue(Double value) => new(value);

            public static bool TryParse(NumberType type, ReadOnlySpan<char> span, out NumberValue value)
            {
                if (span.Length == 0)
                {
                    value = new NumberValue(type);
                    return true;
                }

                switch (type)
                {
                    case NumberType.Byte:
                        if (Byte.TryParse(span, INTEGER_STYLE, CultureInvariant, out var valueByte))
                        {
                            value = new NumberValue(valueByte);
                            return true;
                        }
                        break;
                    case NumberType.Int16:
                        if (Int16.TryParse(span, INTEGER_STYLE, CultureInvariant, out var valueInt16))
                        {
                            value = new NumberValue(valueInt16);
                            return true;
                        }
                        break;
                    case NumberType.Int32:
                        if (Int32.TryParse(span, INTEGER_STYLE, CultureInvariant, out var valueInt32))
                        {
                            value = new NumberValue(valueInt32);
                            return true;
                        }
                        break;
                    case NumberType.Int64:
                        if (Int64.TryParse(span, INTEGER_STYLE, CultureInvariant, out var valueInt64))
                        {
                            value = new NumberValue(valueInt64);
                            return true;
                        }
                        break;
                    case NumberType.Single:
                        if (Single.TryParse(span, FLOAT_STYLE, CultureDeutsch, out var valueSingle) ||
                            Single.TryParse(span, FLOAT_STYLE, CultureInvariant, out valueSingle))
                        {
                            value = new NumberValue(valueSingle);
                            return true;
                        }
                        break;
                    case NumberType.Double:
                        if (Double.TryParse(span, FLOAT_STYLE, CultureDeutsch, out var valueDouble) ||
                            Double.TryParse(span, FLOAT_STYLE, CultureInvariant, out valueDouble))
                        {
                            value = new NumberValue(valueDouble);
                            return true;
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }

                value = default;
                return false;
            }

            public static void Clamp(ref NumberValue value, in NumberValue min, in NumberValue max)
            {
                ImAssert.IsTrue(value.Type == min.Type && value.Type == max.Type, "value.Type == min.Type && value.Type == max.Type");

                switch (value.Type)
                {
                    case NumberType.Byte:
                        value.ValueByte = Math.Clamp(value.ValueByte, min.ValueByte, max.ValueByte);
                        break;
                    case NumberType.Int16:
                        value.ValueInt16 = Math.Clamp(value.ValueInt16, min.ValueInt16, max.ValueInt16);
                        break;
                    case NumberType.Int32:
                        value.ValueInt32 = Math.Clamp(value.ValueInt32, min.ValueInt32, max.ValueInt32);
                        break;
                    case NumberType.Int64:
                        value.ValueInt64 = Math.Clamp(value.ValueInt64, min.ValueInt64, max.ValueInt64);
                        break;
                    case NumberType.Single:
                        value.ValueSingle = Math.Clamp(value.ValueSingle, min.ValueSingle, max.ValueSingle);
                        break;
                    case NumberType.Double:
                        value.ValueDouble = Math.Clamp(value.ValueDouble, min.ValueDouble, max.ValueDouble);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}