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
            if (size.Type == ImSizeType.Fit)
            {
                var textSettings = Style.GetTextSettings();
                var textSize = gui.MeasureTextSize(label, in textSettings);
                ImRectExt.AddPaddingToSize(ref textSize, ImControls.Padding);
                ImRectExt.AddPaddingToSize(ref textSize, Style.AdditionalPadding);

                return gui.Layout.AddRect(textSize);
            }

            return ImControls.GetRowRect(gui, size);
        }
        
        public static bool Button(this ImGui gui, in ReadOnlySpan<char> label, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

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
            var textSettings = Style.GetTextSettings();
            var textColor = Style.GetStateStyle(state).FrontColor;
            var textRect = Style.GetContentRect(rect);
            
            gui.Canvas.Text(in label, textColor, textRect, in textSettings);
            
            return clicked;
        }
        
        public static bool Button(this ImGui gui, uint id, in ImRect rect, out ImButtonState state)
        {
            var hovered = gui.IsControlHovered(id);
            var pressed = gui.IsControlActive(id);
            var clicked = false;
            
            gui.RegisterControl(id, rect);
            
            state = pressed ? ImButtonState.Pressed : hovered ? ImButtonState.Hovered : ImButtonState.Normal;
            gui.Box(in rect, Style.GetStateStyle(state));

            if (gui.IsReadOnly)
            {
                return false;
            }

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

            return clicked;
        }

        public static bool InvisibleButton(this ImGui gui, ImRect rect, bool actOnPress = false)
        {
            var id = gui.GetNextControlId();

            return InvisibleButton(gui, id, rect, actOnPress);
        }
        
        public static bool InvisibleButton(this ImGui gui, uint id, ImRect rect, bool actOnPress = false)
        {
            var hovered = gui.IsControlHovered(id);
            var pressed = gui.IsControlActive(id);
            var clicked = false;
            
            gui.RegisterControl(id, rect);

            if (gui.IsReadOnly)
            {
                return false;
            }
            
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

            return clicked;
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
            AdditionalPadding = 0.0f,
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
        public ImPadding AdditionalPadding;
        public ImTextAlignment Alignment;
        public bool TextWrap;

        public ImRect GetContentRect(ImRect buttonRect)
        {
            buttonRect.AddPadding(AdditionalPadding);
            buttonRect.AddPadding(ImControls.Padding);
            
            return buttonRect;
        }
        
        public ImBoxStyle GetStateStyle(ImButtonState state)
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
        
        public ImTextSettings GetTextSettings()
        {
            return new ImTextSettings(ImControls.Style.TextSize, Alignment, TextWrap);
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