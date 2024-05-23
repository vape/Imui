using System;
using Imui.Core;
using Imui.IO.Events;
using Imui.Styling;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImSlider
    {
        public static ImSliderStyle Style = ImSliderStyle.Default;

        public static bool Slider(this ImGui gui, ref float value, float min, float max)
        {
            var width = gui.Layout.GetAvailableSize().x;
            return Slider(gui, ref value, min, max, width); 
        }
        
        public static bool Slider(this ImGui gui, ref float value, float min, float max, float width)
        {
            var rect = gui.Layout.AddRect(width, Style.Height);
            return Slider(gui, ref value, min, max, rect);
        }
        
        public static bool Slider(this ImGui gui, ref float value, float min, float max, ImRect rect)
        {
            var prevValue = value;
            value = Mathf.InverseLerp(min, max, value);

            gui.Canvas.RectWithOutline(rect, Style.BackColor, Style.FrameColor, Style.FrameWidth, Style.CornerRadius);

            var rectPadded = rect.WithPadding(Style.FrameWidth).WithPadding(Style.Padding);

            var handleW = rectPadded.H * 1.5f;
            var handleH = rectPadded.H;
            
            var xmin = rectPadded.X + handleW / 2.0f;
            var xmax = rectPadded.X + rectPadded.W - handleW / 2.0f;
            
            var handleX = Mathf.Lerp(xmin, xmax, value) - (handleW / 2.0f);
            var handleY = rectPadded.Y + (rectPadded.H / 2.0f) - (handleH / 2.0f);
            var handleRect = new ImRect(handleX, handleY, handleW, handleH);

            var id = gui.GetNextControlId();

            using (new ImStyleScope<ImButtonStyle>(ref ImButton.Style, Style.Handle))
            {
                gui.Button(id, in handleRect, out _, out _);
            }
            
            ref readonly var evt = ref gui.Input.MouseEvent;
            
            if (gui.ActiveControl == id)
            {
                if (evt.Type == ImMouseEventType.Drag)
                {
                    value = Mathf.InverseLerp(xmin, xmax, Mathf.Lerp(xmin, xmax, value) + evt.Delta.x);
                    gui.Input.UseMouseEvent();
                }
                else if (evt.Type == ImMouseEventType.Up)
                {
                    gui.ActiveControl = default;
                }
            }
            else
            {
                if (evt.Type == ImMouseEventType.Down && gui.IsControlHovered(id))
                {
                    gui.ActiveControl = id;
                    gui.Input.UseMouseEvent();
                }
            }
            
            gui.HandleControl(id, rect);
            
            value = Mathf.Lerp(min, max, value);
            
            return Mathf.Abs(value - prevValue) > 0.000001f;
        }
    }

    public struct ImSliderStyle
    {
        public static readonly ImSliderStyle Default = CreateDefaultStyle();

        public static ImSliderStyle CreateDefaultStyle()
        {
            var style = new ImSliderStyle()
            {
                Height = 24,
                BackColor = ImColors.White,
                FrameWidth = 1,
                FrameColor = ImColors.Black,
                CornerRadius = 4,
                Handle = ImButtonStyle.Default,
                Padding = 1
            };

            style.Handle.CornerRadius = style.CornerRadius - style.FrameWidth;
            style.Handle.FrameWidth = 0;
            style.Handle.Normal.BackColor = ImColors.Black;
            style.Handle.Hovered.BackColor = ImColors.Gray1;
            style.Handle.Pressed.BackColor = ImColors.Black;
            
            return style;
        }

        public float Height;
        public ImButtonStyle Handle;
        public Color32 BackColor;
        public float FrameWidth;
        public Color32 FrameColor;
        public float CornerRadius;
        public ImPadding Padding;
    }
}