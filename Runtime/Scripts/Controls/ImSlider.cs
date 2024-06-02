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
            gui.AddControlSpacing();

            var rect = gui.Layout.AddRect(gui.Layout.GetAvailableWidth(), gui.GetRowHeight());
            return Slider(gui, ref value, min, max, in rect); 
        }
        
        public static bool Slider(this ImGui gui, ref float value, float min, float max, float width, float height)
        {
            gui.AddControlSpacing();

            var rect = gui.Layout.AddRect(width, height);
            return Slider(gui, ref value, min, max, in rect);
        }
        
        public static bool Slider(this ImGui gui, ref float value, float min, float max, Vector2 size)
        {
            gui.AddControlSpacing();

            var rect = gui.Layout.AddRect(size);
            return Slider(gui, ref value, min, max, in rect);
        }
        
        public static bool Slider(this ImGui gui, ref float value, float min, float max, in ImRect rect)
        {
            const float EPSILON = 0.000001f;
            
            var prevValue = value;
            value = Mathf.InverseLerp(min, max, value);

            gui.DrawBox(in rect, in Style.Box);

            var rectPadded = rect.WithPadding(Style.Padding);

            var handleW = rectPadded.H * Style.HandleAspectRatio;
            var handleH = rectPadded.H;
            
            var xmin = rectPadded.X + handleW / 2.0f;
            var xmax = rectPadded.X + rectPadded.W - handleW / 2.0f;
            
            var handleX = Mathf.Lerp(xmin, xmax, value) - (handleW / 2.0f);
            var handleY = rectPadded.Y + (rectPadded.H / 2.0f) - (handleH / 2.0f);
            var handleRect = new ImRect(handleX, handleY, handleW, handleH);

            var id = gui.GetNextControlId();
            var hovered = gui.IsControlHovered(id);
            var active = gui.IsControlActive(id);

            using (new ImStyleScope<ImButtonStyle>(ref ImButton.Style, Style.Handle))
            {
                gui.Button(id, in handleRect, out _);
            }
            
            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when hovered:
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Drag when active:
                    value = Mathf.InverseLerp(xmin, xmax, Mathf.Lerp(xmin, xmax, value) + evt.Delta.x);
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Up when active:
                    gui.ResetActiveControl();
                    break;
            }
            
            gui.RegisterControl(id, rect);
            
            value = Mathf.Lerp(min, max, value);
            
            return Mathf.Abs(value - prevValue) > EPSILON;
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
            style.Handle.SetBorderRadius(style.Box.BorderRadius);
            style.Handle.SetBorderWidth(0);
            
            return style;
        }

        public ImBoxStyle Box;
        public ImButtonStyle Handle;
        public ImPadding Padding;
        public float HandleAspectRatio;
    }
}