using Imui.Core;
using Imui.Core.Input;
using Imui.Styling;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImSlider
    {
        public static ImSliderStyle Style = ImSliderStyle.Default;
        
        public static void Slider(this ImGui gui, ref float value, float min, float max, ImRect rect)
        {
            value = Mathf.InverseLerp(min, max, value);
            
            gui.Canvas.Rect(rect, Style.BackColor, Style.CornerRadius);
            gui.Canvas.RectOutline(rect, Style.FrameColor, Style.FrameWidth, Style.CornerRadius);

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
                if (evt.Type == ImInputEventMouseType.Drag)
                {
                    value = Mathf.InverseLerp(xmin, xmax, Mathf.Lerp(xmin, xmax, value) + evt.Delta.x);
                    gui.Input.UseMouse();
                }
                else if (evt.Type == ImInputEventMouseType.Up)
                {
                    gui.ActiveControl = default;
                }
            }
            else
            {
                if (evt.Type == ImInputEventMouseType.Down && gui.IsControlHovered(id))
                {
                    gui.ActiveControl = id;
                    gui.Input.UseMouse();
                }
            }
            
            gui.HandleControl(id, rect);
            
            value = Mathf.Lerp(min, max, value);
        }
    }

    public struct ImSliderStyle
    {
        public static readonly ImSliderStyle Default = CreateDefaultStyle();

        public static ImSliderStyle CreateDefaultStyle()
        {
            var style = new ImSliderStyle()
            {
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
        
        public ImButtonStyle Handle;
        public Color32 BackColor;
        public float FrameWidth;
        public Color32 FrameColor;
        public float CornerRadius;
        public ImPadding Padding;
    }
}