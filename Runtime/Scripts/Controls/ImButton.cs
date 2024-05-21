using System;
using Imui.Core;
using Imui.IO.Events;
using Imui.Styling;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImButton
    {
        public static ImButtonStyle Style = ImButtonStyle.Default;
        
        public static bool Button(this ImGui gui, in ReadOnlySpan<char> label)
        {
            var size = MeasureSize(gui, in label);
            var rect = gui.Layout.AddRect(size);

            return Button(gui, in rect, label);
        }
        
        public static bool Button(this ImGui gui, Vector2 size, in ReadOnlySpan<char> label)
        {
            return Button(gui, gui.Layout.AddRect(size), label);
        }
        
        public static bool Button(this ImGui gui, in ImRect rect, in ReadOnlySpan<char> label)
        {
            var clicked = Button(gui, in rect, out var content, out var state);
            gui.Canvas.Text(in label, state.FrontColor, content, in Style.Text);
            return clicked;
        }

        public static bool Button(this ImGui gui, in ImRect rect, out ImRect content, out ImButtonStateStyle state)
        {
            var id = gui.GetNextControlId();
            var clicked = Button(gui, id, in rect, out content, out state);
            return clicked;
        }

        public static bool Button(this ImGui gui, uint id, in ImRect rect, out ImRect content, out ImButtonStateStyle state)
        {
            return Button(gui, id, in rect, out content, out state, in rect);
        }
        
        public static bool Button(this ImGui gui, uint id, in ImRect rect, out ImRect content, out ImButtonStateStyle state, in ImRect clickable)
        {
            var hovered = gui.IsControlHovered(id);
            var pressed = gui.ActiveControl == id;
            state = pressed ? Style.Pressed : hovered ? Style.Hovered : Style.Normal;
            
            gui.Canvas.RectWithOutline(rect, state.BackColor, state.FrameColor, Style.FrameWidth, Style.CornerRadius);
            
            content = rect.WithPadding(Style.FrameWidth).WithPadding(Style.Padding);
            
            var clicked = false;

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down:
                    if (!pressed && hovered)
                    {
                        gui.ActiveControl = id;
                        gui.Input.UseMouseEvent();
                    }
                    break;
                
                case ImMouseEventType.Up:
                    if (pressed)
                    {
                        gui.ActiveControl = 0;
                        clicked = hovered;
                        
                        if (clicked)
                        {
                            gui.Input.UseMouseEvent();
                        }
                    }
                    break;
            }
            
            gui.HandleControl(id, clickable);

            return clicked;
        }

        public static bool InvisibleButton(this ImGui gui, ImRect rect)
        {
            var id = gui.GetNextControlId();

            return InvisibleButton(gui, id, rect);
        }
        
        public static bool InvisibleButton(this ImGui gui, uint id, ImRect rect)
        {
            var hovered = gui.IsControlHovered(id);
            var pressed = gui.ActiveControl == id;
            var clicked = false;
            
            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down:
                    if (!pressed && hovered)
                    {
                        gui.ActiveControl = id;
                        gui.Input.UseMouseEvent();
                    }
                    break;
                
                case ImMouseEventType.Up:
                    if (pressed)
                    {
                        gui.ActiveControl = 0;
                        clicked = hovered;
                        
                        if (clicked)
                        {
                            gui.Input.UseMouseEvent();
                        }
                    }
                    break;
            }
            
            gui.HandleControl(id, rect);

            return clicked;
        }

        public static Vector2 MeasureSize(ImGui gui, in ReadOnlySpan<char> text)
        {
            ref readonly var textLayout = ref gui.TextDrawer.BuildTempLayout(in text, 0, 0, Style.Text.AlignX,
                Style.Text.AlignY, Style.Text.Size);

            return new Vector2(
                textLayout.Width + Style.Padding.Left + Style.Padding.Right + (Style.FrameWidth * 2) + 0.1f,
                textLayout.Height + Style.Padding.Top + Style.Padding.Bottom + (Style.FrameWidth * 2) + 0.1f);
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
            Padding = 1.0f,
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
        public ImPadding Padding;
        public float FrameWidth;
        public float CornerRadius;
    }
}