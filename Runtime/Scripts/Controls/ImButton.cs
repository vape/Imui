using System;
using Imui.Core;
using Imui.IO.Events;
using Imui.Controls.Styling;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImButton
    {
        public static ImButtonStyle Style = ImButtonStyle.Default;

        public static ImRect GetRect(ImGui gui, ImSize size, ReadOnlySpan<char> label)
        {
            return size.Type switch
            {
                ImSizeType.FixedSize => gui.Layout.AddRect(size.Width, size.Height),
                ImSizeType.AutoFit => gui.Layout.AddRect(Style.GetButtonSize(gui.MeasureTextSize(label, GetTextSettings()))),
                _ => gui.Layout.AddRect(gui.GetAvailableWidth(), Style.GetButtonHeight(gui.GetRowHeight()))
            };

        }
        
        public static bool Button(this ImGui gui, in ReadOnlySpan<char> label, ImSize size = default)
        {
            gui.AddControlSpacing();

            var rect = GetRect(gui, size, label);
            return Button(gui, label, in rect);
        }
        
        public static bool Button(this ImGui gui, in ReadOnlySpan<char> label, in ImRect rect)
        {
            return Button(gui, gui.GetNextControlId(), label, in rect, out _);
        }

        public static bool Button(this ImGui gui, in ImRect rect, out ImButtonState state)
        {
            return Button(gui, gui.GetNextControlId(), in rect, out state);
        }

        public static bool Button(this ImGui gui, uint id, in ReadOnlySpan<char> label, in ImRect rect, out ImButtonState state)
        {
            var clicked = Button(gui, id, in rect, out state);
            var textSettings = GetTextSettings();
            var textColor = Style.GetStyle(state).FrontColor;
            gui.Canvas.Text(in label, textColor, Style.GetContentRect(rect), in textSettings);
            return clicked;
        }
        
        public static bool Button(this ImGui gui, uint id, in ImRect rect, out ImButtonState state)
        {
            var hovered = gui.IsControlHovered(id);
            var pressed = gui.IsControlActive(id);
            var clicked = false;
            
            state = pressed ? ImButtonState.Pressed : hovered ? ImButtonState.Hovered : ImButtonState.Normal;
            gui.Box(in rect, Style.GetStyle(state));

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
            return new ImTextSettings(ImControls.Style.TextSize, Style.Alignment, Style.TextWrap);
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
            TextWrap = false,
            Normal = new ImBoxStyle()
            {
                BackColor = new Color32(230, 230, 230, 255),
                BorderColor = ImColors.Black,
                FrontColor = ImColors.Black,
                BorderRadius = 3,
                BorderWidth = 1
            },
            Hovered = new ImBoxStyle()
            {
                BackColor = new Color32(235, 235, 235, 255),
                BorderColor = ImColors.Gray1,
                FrontColor = ImColors.Gray1,
                BorderRadius = 3,
                BorderWidth = 1
            },
            Pressed = new ImBoxStyle()
            {
                BackColor = new Color32(220, 220, 220, 255),
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
        public bool TextWrap;

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