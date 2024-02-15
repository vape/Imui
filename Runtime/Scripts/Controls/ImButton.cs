using System;
using Imui.Core;
using Imui.Core.Input;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImButton
    {
        public static bool Button(this ImGui gui, in Style style, in ReadOnlySpan<char> text)
        {
            ref readonly var textLayout = ref gui.TextDrawer.BuildTempLayout(in text, 0, 0, style.Text.AlignX,
                style.Text.AlignY, style.Text.Size);

            var size = new Vector2(
                textLayout.Width + (style.Padding + style.FrameWidth) * 2 + 0.1f,
                textLayout.Height + (style.Padding + style.FrameWidth) * 2 + 0.1f);
            var rect = gui.Layout.AddRect(size);

            return Button(gui, rect, in style, text);
        }
        
        public static bool Button(this ImGui gui, Vector2 size, in Style style, in ReadOnlySpan<char> text)
        {
            return Button(gui, gui.Layout.AddRect(size), in style, in text);
        }
        
        public static bool Button(this ImGui gui, ImRect rect, in Style style, in ReadOnlySpan<char> text)
        {
            var clicked = Button(gui, rect, in style, out var content, out var state);
            gui.Canvas.Text(in text, state.FrontColor, content, in style.Text);
            return clicked;
        }
        
        public static bool Button(this ImGui gui, ImRect rect, in Style style, out ImRect content, out StateStyle state)
        {
            var id = gui.GetNextControlId();
            var hovered = gui.IsControlHovered(id);
            var pressed = gui.ActiveControl == id;
            state = pressed ? style.Pressed : hovered ? style.Hovered : style.Normal;
            
            gui.Canvas.Rect(rect, state.BackColor, style.CornerRadius);
            gui.Canvas.RectOutline(rect, state.FrameColor, style.FrameWidth, style.CornerRadius);

            content = rect.AddPadding(style.FrameWidth + style.Padding);
            
            var clicked = false;

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImInputEventMouseType.Down:
                    if (!pressed && hovered)
                    {
                        gui.ActiveControl = id;
                        gui.Input.UseMouse();
                    }
                    break;
                
                case ImInputEventMouseType.Up:
                    if (pressed)
                    {
                        gui.ActiveControl = 0;
                        clicked = hovered;
                        
                        if (clicked)
                        {
                            gui.Input.UseMouse();
                        }
                    }
                    break;
            }
            
            gui.HandleControl(id, rect);

            return clicked;
        }
        
        [Serializable]
        public struct StateStyle
        {
            public Color32 BackColor;
            public Color32 FrontColor;
            public Color32 FrameColor;
        }
        
        [Serializable]
        public struct Style
        {
            public StateStyle Normal;
            public StateStyle Hovered;
            public StateStyle Pressed;
            public ImTextSettings Text;
            public float Padding;
            public float FrameWidth;
            public float CornerRadius;
        }
    }
}