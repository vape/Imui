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
        
        public static bool ButtonFitted(this ImGui gui, in ReadOnlySpan<char> label)
        {
            gui.AddControlSpacing();

            var textSettings = GetTextSettings();
            var textSize = gui.MeasureTextSize(label, in textSettings);
            var rect = gui.Layout.AddRect(Style.GetButtonSize(textSize));
            return Button(gui, label, in rect);
        }

        public static bool Button(this ImGui gui, in ReadOnlySpan<char> label)
        {
            gui.AddControlSpacing();

            var width = gui.Layout.GetAvailableWidth();
            var height = Style.GetButtonHeight(gui.GetRowHeight());
            var rect = gui.Layout.AddRect(width, height);
            return Button(gui, label, in rect);
        }

        public static bool Button(this ImGui gui, in ReadOnlySpan<char> label, float width, float height)
        {
            gui.AddControlSpacing();
            
            var rect = gui.Layout.AddRect(width, height);
            return Button(gui, label, in rect);
        }
        
        public static bool Button(this ImGui gui, in ReadOnlySpan<char> label, Vector2 size)
        {
            gui.AddControlSpacing();
            
            var rect = gui.Layout.AddRect(size);
            return Button(gui, label, in rect);
        }
        
        public static bool Button(this ImGui gui, in ReadOnlySpan<char> label, in ImRect rect)
        {
            var clicked = Button(gui, in rect, out var state);
            var textSettings = GetTextSettings();
            var textColor = Style.GetStyle(state).FrontColor;
            gui.Canvas.Text(in label, textColor, Style.GetContentRect(rect), in textSettings);
            return clicked;
        }

        public static bool Button(this ImGui gui, in ImRect rect, out ImButtonState state)
        {
            var id = gui.GetNextControlId();
            var clicked = Button(gui, id, in rect, out state);
            return clicked;
        }
        
        public static bool Button(this ImGui gui, uint id, in ImRect rect, out ImButtonState state)
        {
            var hovered = gui.IsControlHovered(id);
            var pressed = gui.IsControlActive(id);
            var clicked = false;
            
            state = pressed ? ImButtonState.Pressed : hovered ? ImButtonState.Hovered : ImButtonState.Normal;
            gui.DrawBox(in rect, Style.GetStyle(state));

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down:
                    if (!pressed && hovered)
                    {
                        gui.SetActiveControl(id);
                        gui.Input.UseMouseEvent();
                    }
                    break;
                
                case ImMouseEventType.Up:
                    if (pressed)
                    {
                        gui.ResetActiveControl();
                        clicked = hovered;
                        
                        if (clicked)
                        {
                            gui.Input.UseMouseEvent();
                        }
                    }
                    break;
            }
            
            gui.RegisterControl(id, rect);

            return clicked;
        }

        public static bool InvisibleButton(this ImGui gui, ImRect rect, bool actOnPress = false)
        {
            var id = gui.GetNextControlId();

            return InvisibleButton(gui, id, rect);
        }
        
        public static bool InvisibleButton(this ImGui gui, uint id, ImRect rect, bool actOnPress = false)
        {
            var hovered = gui.IsControlHovered(id);
            var pressed = gui.IsControlActive(id);
            var clicked = false;
            
            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down when !pressed && hovered && actOnPress:
                    clicked = true;
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Down when !pressed && hovered:
                    gui.SetActiveControl(id);
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Up when pressed:
                    gui.ResetActiveControl();
                    clicked = hovered;
                        
                    if (clicked)
                    {
                        gui.Input.UseMouseEvent();
                    }
                    break;
            }
            
            gui.RegisterControl(id, rect);

            return clicked;
        }

        public static ImTextSettings GetTextSettings()
        {
            return new ImTextSettings(ImControls.Style.TextSize, Style.Alignment);
        }
    }

    public enum ImButtonState
    {
        Normal,
        Hovered,
        Pressed
    }
    
    [Serializable]
    public struct ImButtonStyle
    {
        public static readonly ImButtonStyle Default = new ImButtonStyle()
        {
            Padding = 2.0f,
            Alignment = new ImTextAlignment(0.5f, 0.5f),
            Normal = new ImBoxStyle()
            {
                BackColor = ImColors.Gray7,
                BorderColor = ImColors.Black,
                FrontColor = ImColors.Black,
                BorderRadius = 3,
                BorderWidth = 1
            },
            Hovered = new ImBoxStyle()
            {
                BackColor = ImColors.Gray8,
                BorderColor = ImColors.Gray1,
                FrontColor = ImColors.Gray1,
                BorderRadius = 3,
                BorderWidth = 1
            },
            Pressed = new ImBoxStyle()
            {
                BackColor = ImColors.Gray6,
                BorderColor = ImColors.Black,
                FrontColor = ImColors.Black,
                BorderRadius = 3,
                BorderWidth = 1
            }
        };
        
        public ImBoxStyle Normal;
        public ImBoxStyle Hovered;
        public ImBoxStyle Pressed;
        public ImPadding Padding;
        public ImTextAlignment Alignment;

        public Vector2 GetButtonSize(Vector2 contentSize)
        {
            return new Vector2(
                contentSize.x + Padding.Horizontal, 
                contentSize.y + Padding.Vertical);
        }

        public float GetButtonHeight(float contentHeight)
        {
            return contentHeight + Padding.Vertical;
        }

        public ImRect GetContentRect(ImRect buttonRect)
        {
            return buttonRect.WithPadding(Padding);
        }
        
        public ImBoxStyle GetStyle(ImButtonState state)
        {
            switch (state)
            {
                case ImButtonState.Hovered:
                    return Hovered;
                case ImButtonState.Pressed:
                    return Pressed;
                default:
                    return Normal;
            }
        }

        public void SetBorderRadius(ImRectRadius radius)
        {
            Normal.BorderRadius = radius;
            Hovered.BorderRadius = radius;
            Pressed.BorderRadius = radius;
        }

        public void SetBorderWidth(float width)
        {
            Normal.BorderWidth = width;
            Hovered.BorderWidth = width;
            Pressed.BorderWidth = width;
        }

        public void SetTint(Color32 backColor, Color32 frontColor)
        {
            Color.RGBToHSV(backColor, out var h, out var s, out var v);
            
            Normal.BackColor = backColor;
            Normal.FrontColor = frontColor;

            Hovered.BackColor = Color.HSVToRGB(h, s, v * 1.1f);
            Hovered.FrontColor = frontColor;

            Pressed.BackColor = Color.HSVToRGB(h, s, v * 0.9f);
            Pressed.FrontColor = frontColor;
        }
    }
}