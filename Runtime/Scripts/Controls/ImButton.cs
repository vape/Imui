using System;
using Imui.Core;
using Imui.Core.Input;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImButton
    {
        public static bool Button(this ImGui gui, ImRect rect, in Style style, ReadOnlySpan<char> text)
        {
            var clicked = Button(gui, rect, in style, out var content, out var state);
            gui.Canvas.Text(text, state.FrontColor, content, in style.Text);
            return clicked;
        }
        
        public static bool Button(this ImGui gui, ImRect rect, in Style style, out ImRect content, out StateStyle state)
        {
            const float FRAME_WIDTH = 1.0f;
            const float CORNER_RADIUS = 15.0f;
            
            var id = gui.GetNextControlId();
            var hovered = gui.IsControlHovered(id);
            var pressed = gui.ActiveControl == id;
            state = pressed ? style.Pressed : hovered ? style.Hovered : style.Normal;
            
            gui.Canvas.Rect(rect, state.BackColor, CORNER_RADIUS);
            gui.Canvas.RectOutline(rect, state.FrameColor, FRAME_WIDTH, CORNER_RADIUS);

            content = rect.AddPadding(FRAME_WIDTH);
            
            var clicked = false;

            switch (gui.Input.MouseEvent.Type)
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
            public ImTextLayoutSettings Text;
        }
    }
}