using System;
using Imui.Core;
using Imui.Core.Input;
using Imui.Styling;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImButton
    {
        public static ImButtonStyle Style = ImButtonStyle.Default;
        
        public static bool Button(this ImGui gui, in ReadOnlySpan<char> text)
        {
            ref readonly var textLayout = ref gui.TextDrawer.BuildTempLayout(in text, 0, 0, Style.Text.AlignX,
                Style.Text.AlignY, Style.Text.Size);

            var size = new Vector2(
                textLayout.Width + (Style.Padding + Style.FrameWidth) * 2 + 0.1f,
                textLayout.Height + (Style.Padding + Style.FrameWidth) * 2 + 0.1f);
            var rect = gui.Layout.AddRect(size);

            return Button(gui, rect, text);
        }
        
        public static bool Button(this ImGui gui, Vector2 size, in ReadOnlySpan<char> text)
        {
            return Button(gui, gui.Layout.AddRect(size), in text);
        }
        
        public static bool Button(this ImGui gui, ImRect rect, in ReadOnlySpan<char> text)
        {
            var id = gui.GetControlId(text);
            var clicked = Button(gui, id, rect, out var content, out var state);
            gui.Canvas.Text(in text, state.FrontColor, content, in Style.Text);
            return clicked;
        }
        
        public static bool Button(this ImGui gui, uint id, ImRect rect, out ImRect content, out ImButtonStateStyle state)
        {
            var hovered = gui.IsControlHovered(id);
            var pressed = gui.ActiveControl == id;
            state = pressed ? Style.Pressed : hovered ? Style.Hovered : Style.Normal;
            
            gui.Canvas.Rect(rect, state.BackColor, Style.CornerRadius);
            gui.Canvas.RectOutline(rect, state.FrameColor, Style.FrameWidth, Style.CornerRadius);

            content = rect.WithPadding(Style.FrameWidth + Style.Padding);
            
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
    }
    
    [Serializable]
    public struct ImButtonStateStyle
    {
        public Color32 BackColor;
        public Color32 FrontColor;
        public Color32 FrameColor;
    }
        
    [Serializable]
    public struct ImButtonStyle
    {
        public static readonly ImButtonStyle Default = new ImButtonStyle()
        {
            Padding = 1,
            FrameWidth = 1,
            CornerRadius = 3,
            Text = new ImTextSettings()
            {
                AlignX = 0.5f,
                AlignY = 0.5f,
                Size = ImText.DEFAULT_TEXT_SIZE
            },
            Normal = new ImButtonStateStyle()
            {
                BackColor = ImColors.Gray7,
                FrameColor = ImColors.Black,
                FrontColor = ImColors.Black
            },
            Hovered = new ImButtonStateStyle()
            {
                BackColor = ImColors.Gray8,
                FrameColor = ImColors.Gray1,
                FrontColor = ImColors.Gray1
            },
            Pressed = new ImButtonStateStyle()
            {
                BackColor = ImColors.Gray6,
                FrameColor = ImColors.Black,
                FrontColor = ImColors.Black
            }
        };
        
        public ImButtonStateStyle Normal;
        public ImButtonStateStyle Hovered;
        public ImButtonStateStyle Pressed;
        public ImTextSettings Text;
        public float Padding;
        public float FrameWidth;
        public float CornerRadius;
    }
}