using System;
using Imui.Core;
using Imui.IO.Events;
using Imui.Controls.Styling;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImButton
    {
        public static ImRect GetRect(ImGui gui, ImSize size, ReadOnlySpan<char> label)
        {
            if (size.Type == ImSizeType.Fit)
            {
                ref readonly var style = ref ImTheme.Active.Button;

                var textSettings = GetTextSettings();
                var textSize = gui.MeasureTextSize(label, in textSettings);
                ImRectExt.AddPaddingToSize(ref textSize, ImTheme.Active.Controls.Padding);
                ImRectExt.AddPaddingToSize(ref textSize, style.AdditionalPadding);

                return gui.Layout.AddRect(textSize);
            }

            return ImControls.GetRowRect(gui, size);
        }
        
        public static bool Button(this ImGui gui, ReadOnlySpan<char> label, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rect = GetRect(gui, size, label);
            return Button(gui, label, rect);
        }
        
        public static bool Button(this ImGui gui, ReadOnlySpan<char> label, ImRect rect)
        {
            return Button(gui, gui.GetNextControlId(), label, rect, out _);
        }

        public static bool Button(this ImGui gui, ImRect rect, out ImButtonState state)
        {
            return Button(gui, gui.GetNextControlId(), rect, out state);
        }

        public static bool Button(this ImGui gui, uint id, ReadOnlySpan<char> label, ImRect rect, out ImButtonState state)
        {
            var clicked = Button(gui, id, rect, out state);
            var textSettings = GetTextSettings();
            var textColor = GetStateFontColor(state);
            var textRect = GetContentRect(rect);
            
            gui.Canvas.Text(label, textColor, textRect, in textSettings);
            
            return clicked;
        }
        
        public static bool Button(this ImGui gui, uint id, ImRect rect, out ImButtonState state)
        {
            var hovered = gui.IsControlHovered(id);
            var pressed = gui.IsControlActive(id);
            var clicked = false;
            
            gui.RegisterControl(id, rect);
            
            state = pressed ? ImButtonState.Pressed : hovered ? ImButtonState.Hovered : ImButtonState.Normal;
            gui.Box(rect, GetStateBoxStyle(state));

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

        public static ImRect GetContentRect(ImRect rect)
        {
            ref readonly var style = ref ImTheme.Active.Button;
            
            rect.AddPadding(style.AdditionalPadding);
            rect.AddPadding(ImTheme.Active.Controls.Padding);

            return rect;
        }

        public static Color32 GetStateFontColor(ImButtonState state)
        {
            ref readonly var stateStyle = ref GetStateStyle(state);
            return stateStyle.FrontColor;
        }

        public static ImBoxStyle GetStateBoxStyle(ImButtonState state)
        {
            ref readonly var style = ref ImTheme.Active.Button;
            ref readonly var stateStyle = ref GetStateStyle(state);
            
            return new ImBoxStyle
            {
                BackColor = stateStyle.BackColor,
                FrontColor = stateStyle.FrontColor,
                BorderColor = stateStyle.BorderColor,
                BorderWidth = style.BorderWidth,
                BorderRadius = style.BorderRadius
            };
        }

        public static ref readonly ImButtonStateStyle GetStateStyle(ImButtonState state)
        {
            ref readonly var style = ref ImTheme.Active.Button;
            
            switch (state)
            {
                case ImButtonState.Hovered:
                    return ref style.Hovered;
                case ImButtonState.Pressed:
                    return ref style.Pressed;
                default:
                    return ref style.Normal;
            }
        }
        
        public static ImTextSettings GetTextSettings()
        {
            ref readonly var style = ref ImTheme.Active.Button;

            return new ImTextSettings(ImTheme.Active.Controls.TextSize, style.Alignment, style.TextWrap);
        }
    }

    public enum ImButtonState
    {
        Normal,
        Hovered,
        Pressed
    }

    [Serializable]
    public struct ImButtonStateStyle
    {
        public Color32 BackColor;
        public Color32 FrontColor;
        public Color32 BorderColor;
    }
    
    [Serializable]
    public struct ImButtonStyle
    {
        public ImButtonStateStyle Normal;
        public ImButtonStateStyle Hovered;
        public ImButtonStateStyle Pressed;
        public float BorderWidth;
        public ImRectRadius BorderRadius;
        public ImPadding AdditionalPadding;
        public ImTextAlignment Alignment;
        public bool TextWrap;
    }
}