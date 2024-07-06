using Imui.Core;
using Imui.IO.Events;
using Imui.Controls.Styling;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImSlider
    {
        public static ImSliderStyle Style = ImSliderStyle.Default;

        public static ImRect GetRect(ImGui gui, ImSize size)
        {
            return size.Type switch
            {
                ImSizeType.FixedSize => gui.Layout.AddRect(size.Width, size.Height),
                _ => gui.Layout.AddRect(gui.Layout.GetAvailableWidth(), gui.GetRowHeight())
            };
        }
        
        public static bool Slider(this ImGui gui, ref float value, float min, float max, ImSize size = default)
        {
            gui.AddControlSpacing();

            var rect = GetRect(gui, size);
            return Slider(gui, ref value, min, max, in rect); 
        }
        
        public static bool Slider(this ImGui gui, ref float value, float min, float max, in ImRect rect)
        {
            const float EPSILON = 0.000001f;
            
            var normValue = Mathf.InverseLerp(min, max, value);

            gui.Box(in rect, in Style.Box);

            var rectPadded = rect.WithPadding(Style.Padding);

            var handleW = rectPadded.H * Style.HandleAspectRatio;
            var handleH = rectPadded.H;
            
            var xmin = rectPadded.X + handleW / 2.0f;
            var xmax = rectPadded.X + rectPadded.W - handleW / 2.0f;
            
            var handleX = Mathf.Lerp(xmin, xmax, normValue) - (handleW / 2.0f);
            var handleY = rectPadded.Y + (rectPadded.H / 2.0f) - (handleH / 2.0f);
            var handleRect = new ImRect(handleX, handleY, handleW, handleH);

            var id = gui.GetNextControlId();
            var hovered = gui.IsControlHovered(id);
            var active = gui.IsControlActive(id);

            using (new ImStyleScope<ImButtonStyle>(ref ImButton.Style, Style.Handle))
            {
                gui.Button(id, in handleRect, out _);
            }
            
            gui.RegisterControl(id, rect);

            if (gui.IsReadOnly)
            {
                return false;
            }
            
            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when hovered:
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Drag when active:
                    normValue = Mathf.InverseLerp(xmin, xmax, Mathf.Lerp(xmin, xmax, normValue) + evt.Delta.x);
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Up when active:
                    gui.ResetActiveControl();
                    break;
            }
            
            var newValue = Mathf.Lerp(min, max, normValue);
            if (Mathf.Abs(newValue - value) > EPSILON)
            {
                value = newValue;
                return true;
            }

            return false;
        }
    }

    public struct ImSliderStyle
    {
        public static readonly ImSliderStyle Default = CreateDefaultStyle();

        public static ImSliderStyle CreateDefaultStyle()
        {
            var style = new ImSliderStyle()
            {
                Box = new ImBoxStyle()
                {
                    BackColor = ImColors.White,
                    BorderWidth = 1,
                    BorderColor = ImColors.Black,
                    BorderRadius = 4
                },
                Handle = ImButtonStyle.Default,
                Padding = 1,
                HandleAspectRatio = 1.5f
            };
            
            style.Handle.Normal.BackColor = ImColors.Black;
            style.Handle.Hovered.BackColor = ImColors.Gray1;
            style.Handle.Pressed.BackColor = ImColors.Black;
            style.Handle.SetBorderRadius(style.Box.BorderRadius - style.Box.BorderWidth);
            style.Handle.SetBorderWidth(0);
            
            return style;
        }

        public ImBoxStyle Box;
        public ImButtonStyle Handle;
        public ImPadding Padding;
        public float HandleAspectRatio;
    }
}