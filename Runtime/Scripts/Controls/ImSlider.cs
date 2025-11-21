using System;
using System.Runtime.CompilerServices;
using Imui.Core;
using Imui.IO.Events;
using Imui.Rendering;
using UnityEngine;

namespace Imui.Controls
{
    [Flags]
    public enum ImSliderFlag
    {
        None = 0,
        DynamicHandleWidth = 1 << 0,
        NoFill = 1 << 1,
        FillRightSegment = 1 << 2
    }

    [Flags]
    public enum ImSliderRangeFlag
    {
        None = 0,
        NoFill = 1 << 1
    }

    public static class ImSlider
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SliderHeader(this ImGui gui,
                                        ReadOnlySpan<char> label,
                                        float value,
                                        ReadOnlySpan<char> valueFormat = default)
        {
            var valueFormatted = gui.Formatter.Format(value, valueFormat);
            SliderHeader(gui, label, valueFormatted);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SliderHeader(this ImGui gui,
                                        ReadOnlySpan<char> label,
                                        float valueLeft,
                                        float valueRight,
                                        ReadOnlySpan<char> valueFormat = default)
        {
            var valueLeftFormatted = gui.Formatter.Format(valueLeft, valueFormat);
            var valueRightFormatted = gui.Formatter.Format(valueRight, valueFormat);
            var valueFormatted = gui.Formatter.Concat(valueLeftFormatted, " - ", valueRightFormatted);
            
            SliderHeader(gui, label, valueFormatted);
        }

        public static void SliderHeader(this ImGui gui,
                                        ReadOnlySpan<char> label,
                                        ReadOnlySpan<char> valueLabel)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            gui.BeginHorizontal();

            var rowHeight = gui.GetRowHeight();
            var height = rowHeight * gui.Style.Slider.HeaderScale;
            var rect = gui.AddLayoutRect(gui.GetLayoutWidth(), height);
            var barHeight = gui.Style.Slider.BarThickness * rowHeight;
            var padding = (rowHeight - barHeight) * 0.5f;
            var fontSize = gui.TextDrawer.GetFontSizeFromLineHeight(height);

            // (artem-s): align with slider's bar
            rect.X += padding;
            rect.W -= padding * 2;

            // (artem-s): shift rect down by spacing value so there is no gap between header and slider itself
            rect.Y -= gui.Style.Layout.Spacing;

            var textSettings = new ImTextSettings(fontSize, 0.0f, 1.0f, overflow: ImTextOverflow.Ellipsis);
            gui.Text(label, textSettings, rect);

            textSettings.Align.X = 1.0f;
            gui.Text(valueLabel, textSettings, rect);

            gui.EndHorizontal();
        }

        public static int Slider(this ImGui gui,
                                 int value,
                                 int min,
                                 int max,
                                 ImSize size = default,
                                 int step = 1,
                                 ImSliderFlag flags = ImSliderFlag.None)
        {
            Slider(gui, ref value, min, max, size, step, flags);
            return value;
        }

        public static bool Slider(this ImGui gui,
                                  ref int value,
                                  int min,
                                  int max,
                                  ImSize size = default,
                                  int step = 1,
                                  ImSliderFlag flags = ImSliderFlag.None)
        {
            var floatValue = (float)value;
            var changed = Slider(gui, ref floatValue, min, max, size, step, flags);
            value = (int)floatValue;
            return changed;
        }

        public static float Slider(this ImGui gui,
                                   float value,
                                   float min,
                                   float max,
                                   ImSize size = default,
                                   float step = 0,
                                   ImSliderFlag flags = ImSliderFlag.None)
        {
            Slider(gui, ref value, min, max, size, step, flags);
            return value;
        }

        public static bool Slider(this ImGui gui,
                                  ref float value,
                                  float min,
                                  float max,
                                  ImSize size = default,
                                  float step = 0,
                                  ImSliderFlag flags = ImSliderFlag.None)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            return Slider(gui, ref value, min, max, gui.AddSingleRowRect(size, minWidth: gui.GetRowHeight() * 2), step, flags);
        }

        public static bool Slider(this ImGui gui,
                                  ref float value,
                                  float min,
                                  float max,
                                  ImRect rect,
                                  float step = 0,
                                  ImSliderFlag flags = ImSliderFlag.None)
        {
            var id = gui.GetNextControlId();
            var hovered = gui.IsControlHovered(id);
            var active = gui.IsControlActive(id);

            step = Mathf.Abs(step);

            var normValue = Mathf.InverseLerp(min, max, value);
            var changed = false;

            var barRect = rect;
            barRect.H *= gui.Style.Slider.BarThickness;
            barRect.Y += (rect.H - barRect.H) * 0.5f;
            barRect.W -= (rect.H - barRect.H);
            barRect.X += (rect.H - barRect.H) * 0.5f;

            ref readonly var style = ref (active ? ref gui.Style.Slider.Selected : ref gui.Style.Slider.Normal);
            gui.Box(barRect, in style);

            if ((flags & ImSliderFlag.NoFill) == 0)
            {
                var fillRect = barRect;

                if ((flags & ImSliderFlag.FillRightSegment) != 0)
                {
                    fillRect.X += barRect.W * normValue;
                    fillRect.W *= 1.0f - normValue;
                }
                else
                {
                    fillRect.W *= normValue;
                }

                gui.Box(fillRect, gui.Style.Slider.Fill);
            }

            var handleBounds = rect;

            var handleW = (flags & ImSliderFlag.DynamicHandleWidth) == 0 ? handleBounds.H : Mathf.Max(handleBounds.H, handleBounds.W / ((max - min) / step));
            var handleH = handleBounds.H * gui.Style.Slider.HandleThickness;

            var xmin = handleBounds.X + handleW / 2.0f;
            var xmax = handleBounds.X + handleBounds.W - handleW / 2.0f;

            var handleX = Mathf.Lerp(xmin, xmax, normValue) - (handleW / 2.0f);
            var handleY = handleBounds.Y + (handleBounds.H / 2.0f) - (handleH / 2.0f);
            var handleRect = new ImRect(handleX, handleY, handleW, handleH);

            ref readonly var evt = ref gui.Input.MouseEvent;
            ref readonly var buttonStyle = ref gui.Style.Slider.Handle;

            var type = evt.Type;
            var device = evt.Device;

            if (gui.Button(id, handleRect, in buttonStyle, out _, ImButtonFlag.ActOnPressMouse))
            {
                normValue = Mathf.InverseLerp(xmin, xmax, Mathf.Lerp(xmin, xmax, (gui.Input.MousePosition.x - xmin) / (xmax - xmin)));
                changed = true;

                if (type == ImMouseEventType.Down && device == ImMouseDevice.Mouse)
                {
                    // if button is activated on press, select control, so we can continue to scroll while mouse is down
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                }
            }

            gui.RegisterControl(id, rect);

            if (gui.IsReadOnly)
            {
                return false;
            }

            switch (evt.Type)
            {
                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when
                    evt.LeftButton &&
                    hovered &&
                    IsScrollingHorizontally(in evt) &&
                    !gui.ActiveControlIs(ImControlFlag.Draggable):

                    normValue = Mathf.InverseLerp(xmin, xmax, Mathf.Lerp(xmin, xmax, (gui.Input.MousePosition.x - xmin) / (xmax - xmin)));
                    changed = true;
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                    gui.Input.UseMouseEvent();
                    break;

                case ImMouseEventType.Drag when active:
                    normValue = Mathf.InverseLerp(xmin, xmax, Mathf.Lerp(xmin, xmax, (gui.Input.MousePosition.x - xmin) / (xmax - xmin)));
                    changed = true;
                    gui.Input.UseMouseEvent();
                    break;

                case ImMouseEventType.Up when active:
                    gui.ResetActiveControl();
                    break;
            }

            if (!changed)
            {
                return false;
            }

            value = Mathf.Lerp(min, max, normValue);

            if (step > 0)
            {
                var precision = 1.0f / step;
                value = Mathf.Clamp(Mathf.Round(value * precision) / precision, min, max);
            }

            return true;
        }

        public static (int valueLeft, int valueRight) SliderRange(this ImGui gui,
                                                                  int valueLeft,
                                                                  int valueRight,
                                                                  int min,
                                                                  int max,
                                                                  ImSize size = default,
                                                                  int step = 1,
                                                                  ImSliderRangeFlag flags = ImSliderRangeFlag.None)
        {
            SliderRange(gui, ref valueLeft, ref valueRight, min, max, size, step, flags);
            return (valueLeft, valueRight);
        }

        public static bool SliderRange(this ImGui gui,
                                       ref int valueLeft,
                                       ref int valueRight,
                                       int min,
                                       int max,
                                       ImSize size = default,
                                       int step = 1,
                                       ImSliderRangeFlag flags = ImSliderRangeFlag.None)
        {
            var floatValueLeft = (float)valueLeft;
            var floatValueRight = (float)valueRight;
            var changed = SliderRange(gui, ref floatValueLeft, ref floatValueRight, min, max, size, step, flags);
            (valueLeft, valueRight) = ((int)floatValueLeft, (int)floatValueRight);
            return changed;
        }

        public static (float valueLeft, float valueRight) SliderRange(this ImGui gui,
                                                                      float valueLeft,
                                                                      float valueRight,
                                                                      float min,
                                                                      float max,
                                                                      ImSize size = default,
                                                                      float step = 0,
                                                                      ImSliderRangeFlag flags = ImSliderRangeFlag.None)
        {
            SliderRange(gui, ref valueLeft, ref valueRight, min, max, size, step, flags);
            return (valueLeft, valueRight);
        }

        public static bool SliderRange(this ImGui gui,
                                       ref float valueLeft,
                                       ref float valueRight,
                                       float min,
                                       float max,
                                       ImSize size = default,
                                       float step = 0,
                                       ImSliderRangeFlag flags = ImSliderRangeFlag.None)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            return SliderRange(gui, ref valueLeft, ref valueRight, min, max, gui.AddSingleRowRect(size, minWidth: gui.GetRowHeight() * 2), step, flags);
        }

        public static bool SliderRange(this ImGui gui,
                                       ref float valueLeft,
                                       ref float valueRight,
                                       float min,
                                       float max,
                                       ImRect rect,
                                       float step = 0,
                                       ImSliderRangeFlag flags = ImSliderRangeFlag.None)
        {
            static bool Handle(ImGui gui, uint id, ref float normValue, in ImRect handleBounds, bool left)
            {
                var changed = false;
                var hovered = gui.IsControlHovered(id);
                var active = gui.IsControlActive(id);

                ref readonly var evt = ref gui.Input.MouseEvent;
                ref readonly var buttonStyle = ref gui.Style.Slider.Handle;

                var handleW = handleBounds.H;
                var handleH = handleBounds.H * gui.Style.Slider.HandleThickness;

                var xmin = handleBounds.X + handleW / 2.0f;
                var xmax = handleBounds.X + handleBounds.W - handleW / 2.0f;

                if (left)
                {
                    xmax -= handleW;
                }
                else
                {
                    xmin += handleW;
                }

                var handleX = Mathf.Lerp(xmin, xmax, normValue) - (handleW / 2.0f);
                var handleY = handleBounds.Y + (handleBounds.H / 2.0f) - (handleH / 2.0f);
                var handleRect = new ImRect(handleX, handleY, handleW, handleH);

                var type = evt.Type;
                var device = evt.Device;

                gui.RegisterControl(id, handleRect);

                if (gui.Button(id, handleRect, in buttonStyle, out _, ImButtonFlag.ActOnPressMouse) && type == ImMouseEventType.Down &&
                    device == ImMouseDevice.Mouse)
                {
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                }

                if (!gui.IsReadOnly)
                {
                    switch (evt.Type)
                    {
                        case ImMouseEventType.Down or ImMouseEventType.BeginDrag when
                            evt.LeftButton &&
                            hovered &&
                            IsScrollingHorizontally(in evt) &&
                            !gui.ActiveControlIs(ImControlFlag.Draggable):

                            normValue = Mathf.InverseLerp(xmin, xmax, Mathf.Lerp(xmin, xmax, (gui.Input.MousePosition.x - xmin) / (xmax - xmin)));
                            changed = true;
                            gui.SetActiveControl(id, ImControlFlag.Draggable);
                            gui.Input.UseMouseEvent();
                            break;

                        case ImMouseEventType.Drag when active:
                            normValue = Mathf.InverseLerp(xmin, xmax, Mathf.Lerp(xmin, xmax, (gui.Input.MousePosition.x - xmin) / (xmax - xmin)));
                            changed = true;
                            gui.Input.UseMouseEvent();
                            break;

                        case ImMouseEventType.Up when active:
                            gui.ResetActiveControl();
                            break;
                    }
                }

                return changed;
            }

            var idLeft = gui.GetNextControlId();
            var idRight = gui.GetNextControlId();
            var active = gui.IsControlActive(idRight) || gui.IsControlActive(idRight);

            step = Mathf.Abs(step);

            var normValueLeft = Mathf.InverseLerp(min, max, valueLeft);
            var normValueRight = Mathf.InverseLerp(min, max, valueRight);
            var changed = false;

            var barRect = rect;
            barRect.H *= gui.Style.Slider.BarThickness;
            barRect.Y += (rect.H - barRect.H) * 0.5f;
            barRect.W -= (rect.H - barRect.H);
            barRect.X += (rect.H - barRect.H) * 0.5f;

            var handleW = rect.H;

            ref readonly var style = ref (active ? ref gui.Style.Slider.Selected : ref gui.Style.Slider.Normal);
            gui.Box(barRect, in style);

            if ((flags & ImSliderRangeFlag.NoFill) == 0)
            {
                var fillRect = barRect;

                var left = Mathf.Lerp(handleW / 2.0f, fillRect.W - handleW, normValueLeft);
                var right = Mathf.Lerp(handleW, fillRect.W - handleW / 2.0f, normValueRight);

                fillRect.X += left;
                fillRect.W = right - left;

                if (fillRect.W > 0)
                {
                    gui.Box(fillRect, gui.Style.Slider.Fill);
                }
            }

            changed |= Handle(gui, idLeft, ref normValueLeft, in rect, left: true);
            changed |= Handle(gui, idRight, ref normValueRight, in rect, left: false);

            if (gui.IsReadOnly)
            {
                return false;
            }

            if (!changed)
            {
                return false;
            }

            valueLeft = Math.Min(Mathf.Lerp(min, max, normValueLeft), valueRight);
            valueRight = Math.Max(Mathf.Lerp(min, max, normValueRight), valueLeft);

            if (step > 0)
            {
                var precision = 1.0f / step;
                valueLeft = Mathf.Clamp(Mathf.Round(valueLeft * precision) / precision, min, max);
                valueRight = Mathf.Clamp(Mathf.Round(valueRight * precision) / precision, min, max);
            }

            return true;
        }

        private static bool IsScrollingHorizontally(in ImMouseEvent e)
        {
            return e.Device == ImMouseDevice.Mouse || Mathf.Abs(e.Delta.x) > Mathf.Abs(e.Delta.y);
        }
    }
}