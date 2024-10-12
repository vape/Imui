using System;
using Imui.Core;
using Imui.IO.Events;
using Imui.Style;
using UnityEngine;

namespace Imui.Controls
{
    public enum ImButtonState
    {
        Normal,
        Hovered,
        Pressed
    }

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
            if (size.Mode == ImSizeMode.Fit || (size.Mode == ImSizeMode.Auto && gui.Layout.Axis == ImAxis.Horizontal))
            {
                var textSettings = CreateTextSettings(gui);
                var textSize = gui.MeasureTextSize(label, in textSettings);
                var rectSize = textSize;

                rectSize.x += gui.Style.Layout.InnerSpacing * 2;
                rectSize.y += gui.Style.Layout.ExtraRowHeight;

                return gui.Layout.AddRect(rectSize);
            }

            return ImControls.AddRowRect(gui, size);
        }

        public static bool Button(this ImGui gui, ReadOnlySpan<char> label, ImSize size = default, ImButtonFlag flags = ImButtonFlag.None)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rect = GetRect(gui, size, label);
            return Button(gui, label, rect, flags);
        }

        public static bool Button(this ImGui gui, ReadOnlySpan<char> label, ImRect rect, ImButtonFlag flags = ImButtonFlag.None)
        {
            return Button(gui, gui.GetNextControlId(), label, rect, out _, flags);
        }

        public static bool Button(this ImGui gui, ImRect rect, out ImButtonState state, ImButtonFlag flags = ImButtonFlag.None)
        {
            return Button(gui, gui.GetNextControlId(), rect, out state, flags);
        }

        public static bool Button(this ImGui gui, uint id, ReadOnlySpan<char> label, ImRect rect, ImButtonFlag flag = ImButtonFlag.None)
        {
            return Button(gui, id, label, rect, out _, flag);
        }

        public static bool Button(this ImGui gui,
                                  uint id,
                                  ReadOnlySpan<char> label,
                                  ImRect rect,
                                  out ImButtonState state,
                                  ImButtonFlag flag = ImButtonFlag.None)
        {
            var clicked = Button(gui, id, rect, out state, flag);
            var textSettings = CreateTextSettings(gui);
            var textColor = GetStateFrontColor(gui, state);
            var textRect = CalculateContentRect(gui, rect);

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

            gui.Box(rect, GetStateBoxStyle(gui, state).Apply(adjacency));

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

                case ImMouseEventType.Hold when pressed && (flag & ImButtonFlag.ReactToHeldDown) != 0:
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

        public static Color32 GetStateFrontColor(ImGui gui, ImButtonState state) => GetStateFrontColor(in gui.Style.Button, state);

        public static Color32 GetStateFrontColor(in ImStyleButton style, ImButtonState state)
        {
            ref readonly var stateStyle = ref GetStateStyle(in style, state);
            return stateStyle.FrontColor;
        }

        public static ImTextSettings CreateTextSettings(ImGui gui) => CreateTextSettings(gui, in gui.Style.Button);

        public static ImTextSettings CreateTextSettings(ImGui gui, in ImStyleButton style)
        {
            return new ImTextSettings(gui.Style.Layout.TextSize, style.Alignment, false);
        }

        public static ImRect CalculateContentRect(ImGui gui, ImRect buttonRect)
        {
            buttonRect.X += gui.Style.Layout.InnerSpacing;
            buttonRect.W -= gui.Style.Layout.InnerSpacing * 2;

            return buttonRect;
        }

        public static ImStyleBox GetStateBoxStyle(ImGui gui, ImButtonState state) => GetStateBoxStyle(in gui.Style.Button, state);

        public static ImStyleBox GetStateBoxStyle(in ImStyleButton style, ImButtonState state)
        {
            ref readonly var stateStyle = ref GetStateStyle(in style, state);

            return new ImStyleBox
            {
                BackColor = stateStyle.BackColor,
                FrontColor = stateStyle.FrontColor,
                BorderColor = stateStyle.BorderColor,
                BorderThickness = style.BorderThickness,
                BorderRadius = style.BorderRadius
            };
        }

        public static ref readonly ImStyleButtonState GetStateStyle(ImGui gui, ImButtonState state) => ref GetStateStyle(in gui.Style.Button, state);

        public static ref readonly ImStyleButtonState GetStateStyle(in ImStyleButton style, ImButtonState state)
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
    }
}