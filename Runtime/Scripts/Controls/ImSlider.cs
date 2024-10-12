using System;
using Imui.Core;
using Imui.IO.Events;
using Imui.Style;
using UnityEngine;

namespace Imui.Controls
{
    [Flags]
    public enum ImSliderFlag
    {
        None,
        DynamicHandle
    }

    public static class ImSlider
    {
        public static ImRect GetRect(ImGui gui, ImSize size)
        {
            return size.Mode switch
            {
                ImSizeMode.Fixed => gui.Layout.AddRect(size.Width, size.Height),
                _ => gui.Layout.AddRect(gui.Layout.GetAvailableWidth(), gui.GetRowHeight())
            };
        }

        public static int Slider(this ImGui gui,
                                 int value,
                                 int min,
                                 int max,
                                 ImSize size = default,
                                 ReadOnlySpan<char> format = default,
                                 int step = 1,
                                 ImSliderFlag flags = ImSliderFlag.None)
        {
            Slider(gui, ref value, min, max, size, format, step, flags);
            return value;
        }

        public static bool Slider(this ImGui gui,
                                  ref int value,
                                  int min,
                                  int max,
                                  ImSize size = default,
                                  ReadOnlySpan<char> format = default,
                                  int step = 1,
                                  ImSliderFlag flags = ImSliderFlag.None)
        {
            var floatValue = (float)value;
            var changed = Slider(gui, ref floatValue, min, max, size, format, step, flags);
            value = (int)floatValue;
            return changed;
        }

        public static float Slider(this ImGui gui,
                                   float value,
                                   float min,
                                   float max,
                                   ImSize size = default,
                                   ReadOnlySpan<char> format = default,
                                   float step = 0,
                                   ImSliderFlag flags = ImSliderFlag.None)
        {
            Slider(gui, ref value, min, max, size, format, step, flags);
            return value;
        }

        public static bool Slider(this ImGui gui,
                                  ref float value,
                                  float min,
                                  float max,
                                  ImSize size = default,
                                  ReadOnlySpan<char> format = default,
                                  float step = 0,
                                  ImSliderFlag flags = ImSliderFlag.None)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            return Slider(gui, ref value, min, max, GetRect(gui, size), format, step, flags);
        }

        public static bool Slider(this ImGui gui,
                                  ref float value,
                                  float min,
                                  float max,
                                  ImRect rect,
                                  ReadOnlySpan<char> format = default,
                                  float step = 0,
                                  ImSliderFlag flags = ImSliderFlag.None)
        {
            const float EPSILON = 0.000001f;

            var id = gui.GetNextControlId();
            var hovered = gui.IsControlHovered(id);
            var active = gui.IsControlActive(id);
            
            step = Mathf.Abs(step);

            var normValue = Mathf.InverseLerp(min, max, value);

            var backgroundRect = rect;
            backgroundRect.H *= gui.Style.Slider.BackScale;
            backgroundRect.Y += (rect.H - backgroundRect.H) * 0.5f;
            backgroundRect.W -= (rect.H - backgroundRect.H);
            backgroundRect.X += (rect.H - backgroundRect.H) * 0.5f;
            
            ref readonly var style = ref (active ? ref gui.Style.Slider.Selected : ref gui.Style.Slider.Normal);
            gui.Box(backgroundRect, in style);

            var handleBounds = rect;

            var handleW = (flags & ImSliderFlag.DynamicHandle) == 0 ? handleBounds.H : Mathf.Max(handleBounds.H, handleBounds.W / ((max - min) / step));
            var handleH = handleBounds.H;

            var xmin = handleBounds.X + handleW / 2.0f;
            var xmax = handleBounds.X + handleBounds.W - handleW / 2.0f;

            var handleX = Mathf.Lerp(xmin, xmax, normValue) - (handleW / 2.0f);
            var handleY = handleBounds.Y + (handleBounds.H / 2.0f) - (handleH / 2.0f);
            var handleRect = new ImRect(handleX, handleY, handleW, handleH);

            using (new ImStyleScope<ImStyleButton>(ref gui.Style.Button, gui.Style.Slider.Handle))
            {
                if (gui.Button(id, handleRect, out _, ImButtonFlag.ActOnPress))
                {
                    normValue = Mathf.InverseLerp(xmin, xmax, Mathf.Lerp(xmin, xmax, (gui.Input.MousePosition.x - rect.Position.x) / rect.W));
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                }
            }

            gui.RegisterControl(id, rect);

            if (format.IsEmpty)
            {
                format = GetFormatForStep(gui, step);
            }

            var textSize = gui.TextDrawer.GetTextSize(backgroundRect.H);
            var textSettings = new ImTextSettings(textSize, 0.5f, 0.5f);
            
            gui.Text(gui.Formatter.Format(value, format), in textSettings, gui.Style.Slider.Normal.FrontColor, backgroundRect);

            if (gui.IsReadOnly)
            {
                return false;
            }

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when hovered:
                    normValue = Mathf.InverseLerp(xmin, xmax, Mathf.Lerp(xmin, xmax, (gui.Input.MousePosition.x - rect.Position.x) / rect.W));
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                    gui.Input.UseMouseEvent();
                    break;

                case ImMouseEventType.Drag when active:
                    normValue = Mathf.InverseLerp(xmin, xmax, Mathf.Lerp(xmin, xmax, (gui.Input.MousePosition.x - rect.Position.x) / rect.W));
                    gui.Input.UseMouseEvent();
                    break;

                case ImMouseEventType.Up when active:
                    gui.ResetActiveControl();
                    break;
            }

            var newValue = Mathf.Lerp(min, max, normValue);
            if (step > 0)
            {
                var precision = 1.0f / step;
                newValue = Mathf.Round(newValue * precision) / precision;
            }

            if (Mathf.Abs(newValue - value) > EPSILON)
            {
                value = newValue;
                return true;
            }

            return false;
        }

        public static ReadOnlySpan<char> GetFormatForStep(ImGui gui, float step)
        {
            if (step == 0)
            {
                return "0.00";
            }

            return gui.Formatter.JoinDuplicate("0.", "0", Mathf.CeilToInt(Mathf.Log10(1.0f / Mathf.Abs(step - (int)step))));
        }
    }
}