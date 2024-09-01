using System;
using Imui.Core;
using Imui.IO.Events;
using Imui.Controls.Styling;
using UnityEngine;

namespace Imui.Controls
{
    [Flags]
    public enum ImButtonFlag
    {
        None = 0,
        ActOnPress = 1,
        ReactToHeldDown = 2
    }
    
    public static class ImButton
    {
        public static ImRect GetRect(ImGui gui, ImSize size, ReadOnlySpan<char> label)
        {
            if (size.Type == ImSizeType.Fit || (size.Type == ImSizeType.Auto && gui.Layout.Axis == ImAxis.Horizontal))
            {
                var textSettings = GetTextSettings();
                var textSize = gui.MeasureTextSize(label, in textSettings);
                var rectSize = textSize;

                rectSize.x += ImTheme.Active.Button.Padding.Horizontal;
                rectSize.y += ImTheme.Active.Controls.ExtraRowHeight;

                return gui.Layout.AddRect(rectSize);
            }

            return ImControls.AddRowRect(gui, size);
        }
        
        public static bool Button(this ImGui gui, ReadOnlySpan<char> label, ImSize size = default, ImButtonFlag flag = ImButtonFlag.None)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rect = GetRect(gui, size, label);
            return Button(gui, label, rect, flag);
        }
        
        public static bool Button(this ImGui gui, ReadOnlySpan<char> label, ImRect rect, ImButtonFlag flag = ImButtonFlag.None)
        {
            return Button(gui, gui.GetNextControlId(), label, rect, out _, flag);
        }

        public static bool Button(this ImGui gui, ImRect rect, out ImButtonState state, ImButtonFlag flag = ImButtonFlag.None)
        {
            return Button(gui, gui.GetNextControlId(), rect, out state, flag);
        }

        public static bool Button(this ImGui gui, uint id, ReadOnlySpan<char> label, ImRect rect, ImButtonFlag flag = ImButtonFlag.None)
        {
            return Button(gui, id, label, rect, out _, flag);
        }

        public static bool Button(this ImGui gui, uint id, ReadOnlySpan<char> label, ImRect rect, out ImButtonState state, ImButtonFlag flag = ImButtonFlag.None)
        {
            var clicked = Button(gui, id, rect, out state, flag);
            var textSettings = GetTextSettings();
            var textColor = GetStateFontColor(state);
            var textRect = GetContentRect(rect);
            
            gui.Canvas.Text(label, textColor, textRect, in textSettings);
            
            return clicked;
        }
        
        public static bool Button(this ImGui gui, uint id, ImRect rect, out ImButtonState state, ImButtonFlag flag = ImButtonFlag.None)
        {
            var hovered = gui.IsControlHovered(id);
            var pressed = gui.IsControlActive(id);
            var clicked = false;
            var adjacency = gui.GetNextControlSettings().Adjacency;
            
            gui.RegisterControl(id, rect);
            
            state = pressed ? ImButtonState.Pressed : hovered ? ImButtonState.Hovered : ImButtonState.Normal;

            gui.Box(rect, GetStateBoxStyle(state).Apply(adjacency));

            if (gui.IsReadOnly)
            {
                return false;
            }

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down when hovered:
                    if ((flag & ImButtonFlag.ActOnPress) != 0)
                    {
                        clicked = true;
                        gui.Input.UseMouseEvent();
                    }
                    else if (!pressed)
                    {
                        gui.SetActiveControl(id);
                        gui.Input.UseMouseEvent();
                    }
                    break;
                
                case ImMouseEventType.Up when pressed:
                    gui.ResetActiveControl();
                    clicked = hovered;
                        
                    if (clicked & (flag & ImButtonFlag.ActOnPress) == 0)
                    {
                        gui.Input.UseMouseEvent();
                    }
                    break;
                
                case ImMouseEventType.Held when pressed && (flag & ImButtonFlag.ReactToHeldDown) != 0:
                    clicked = true;
                    gui.Input.UseMouseEvent();
                    break;
            }

            return clicked;
        }

        public static bool InvisibleButton(this ImGui gui, ImRect rect, ImButtonFlag flag = ImButtonFlag.None)
        {
            var id = gui.GetNextControlId();

            return InvisibleButton(gui, id, rect, flag);
        }
        
        public static bool InvisibleButton(this ImGui gui, ImRect rect, out ImButtonState state, ImButtonFlag flag = ImButtonFlag.None)
        {
            var id = gui.GetNextControlId();

            return InvisibleButton(gui, id, rect, out state, flag);
        }

        public static bool InvisibleButton(this ImGui gui, uint id, ImRect rect, ImButtonFlag flag = ImButtonFlag.None)
        {
            return InvisibleButton(gui, id, rect, out _, flag);
        }
        
        public static bool InvisibleButton(this ImGui gui, uint id, ImRect rect, out ImButtonState state, ImButtonFlag flag = ImButtonFlag.None)
        {
            var hovered = gui.IsControlHovered(id);
            var pressed = gui.IsControlActive(id);
            var clicked = false;

            state = pressed ? ImButtonState.Pressed : hovered ? ImButtonState.Hovered : ImButtonState.Normal;
            
            gui.RegisterControl(id, rect);

            if (gui.IsReadOnly)
            {
                return false;
            }
            
            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down when !pressed && hovered && (flag & ImButtonFlag.ActOnPress) != 0:
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
            return rect.WithPadding(ImTheme.Active.Button.Padding);
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
        
        public static ref readonly ImButtonStateStyle GetStateStyle(in ImButtonStyle style, ImButtonState state)
        {
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
        public ImPadding Padding;
        public ImTextAlignment Alignment;
        public bool TextWrap;
    }
}