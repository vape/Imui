using System;
using Imui.Core;
using Imui.Controls.Styling;
using UnityEngine;

namespace Imui.Controls
{
    public struct ImDropdownState
    {
        public bool Open;
    }
    
    public static class ImDropdown
    {
        public static ImDropdownStyle Style = ImDropdownStyle.Default;
        
        public static void BeginDropdown(this ImGui gui, in ReadOnlySpan<char> label, out bool open, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rect = ImControls.GetRowRect(gui, size);
            BeginDropdown(gui, in label, out open, rect);
        }
        
        public static void BeginDropdown(this ImGui gui, in ReadOnlySpan<char> label, out bool open, in ImRect rect)
        {
            gui.PushId(label);
            
            var id = gui.GetNextControlId();
            ref var state = ref gui.Storage.Get<ImDropdownState>(id);

            var clicked = DropdownButton(gui, id, in label, rect);
            if (clicked)
            {
                state.Open = !state.Open;
            }

            open = state.Open;
        }

        public static void EndDropdown(this ImGui gui)
        {
            gui.PopId();
        }
        
        public static bool Dropdown(this ImGui gui, ref int selected, in ReadOnlySpan<string> options, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rect = ImControls.GetRowRect(gui, size);
            return Dropdown(gui, ref selected, in options, in rect);
        }
        
        public static bool Dropdown(this ImGui gui, ref int selected, in ReadOnlySpan<string> options, in ImRect rect)
        {
            var id = gui.GetNextControlId();
            return Dropdown(gui, id, ref selected, in options, in rect);
        }
        
        public static bool Dropdown(this ImGui gui, uint id, ref int selected, in ReadOnlySpan<string> options, in ImRect rect)
        {
            ref var state = ref gui.Storage.Get<ImDropdownState>(id);

            var text = selected < 0 || selected >= options.Length ? string.Empty : options[selected];
            var clicked = DropdownButton(gui, id, text, rect);
            var changed = false;
            var closed = false;
            
            if (state.Open)
            {
                var position = new Vector2(rect.X, rect.Y);
                var width = rect.W;

                DropdownOptionsPanel(gui, id, ref selected, options, position, width, out closed, out changed);
            }
            
            if (clicked || closed || changed)
            {
                state.Open = !state.Open;
            }

            return changed;
        }

        public static void DropdownOptionsPanel(ImGui gui, uint id, ref int selected, ReadOnlySpan<string> options, Vector2 position, float width, out bool closed, out bool changed)
        {
            changed = false;
            
            var optionButtonHeight = gui.GetRowHeight();
            var totalSpacingHeight = Style.OptionsButtonsSpacing * (options.Length - 1);
            var panelHeight = ImPanel.GetEnclosingHeight(Mathf.Min(Style.MaxPanelHeight, options.Length * optionButtonHeight + totalSpacingHeight));
            var panelRect = new ImRect(position.x, position.y - panelHeight, width, panelHeight);

            gui.Canvas.PushNoClipRect();
            gui.Canvas.PushNoRectMask();

            gui.PushId(id);
            gui.BeginPopup();
            gui.BeginPanel(panelRect);

            var optionButtonWidth = gui.Layout.GetAvailableWidth();

            using (new ImStyleScope<ImControlsStyle>(ref ImControls.Style))
            {
                ImControls.Style.ControlsSpacing = Style.OptionsButtonsSpacing;
                    
                for (int i = 0; i < options.Length; ++i)
                {
                    var isSelected = selected == i;
                    var style = isSelected ? Style.OptionButtonSelected : Style.OptionButton;

                    using (new ImStyleScope<ImButtonStyle>(ref ImButton.Style, style))
                    {
                        if (gui.Button(options[i], (optionButtonWidth, optionButtonHeight)))
                        {
                            selected = i;
                            changed = true;
                        }
                    }
                }
            }
                
            gui.EndPanel();
            gui.EndPopup(out closed);
            gui.PopId();
                
            gui.Canvas.PopRectMask();
            gui.Canvas.PopClipRect();
        }

        public static bool DropdownButton(ImGui gui, uint id, in ReadOnlySpan<char> label, ImRect rect)
        {
            var arrowRect = ImButton.Style.GetContentRect(rect);
            arrowRect.W = arrowRect.H * Style.ArrowOuterScale;
            
            using var _ = new ImStyleScope<ImButtonStyle>(ref ImButton.Style);
            
            ImButton.Style.Alignment = Style.Alignment;
            ImButton.Style.AdditionalPadding.Left += arrowRect.W + ImControls.Style.InnerSpacing;

            var clicked = gui.Button(id, label, rect, out var state);

            DrawArrow(gui, arrowRect, ImButton.Style.GetStateStyle(state).FrontColor);

            return clicked;
        }

        public static void DrawArrow(ImGui gui, ImRect rect, Color32 color)
        {
            rect = rect.WithAspect(1.0f).ScaleFromCenter(Style.ArrowInnerScale).WithAspect(1.1547f);
            
            Span<Vector2> points = stackalloc Vector2[3]
            {
                new Vector2(rect.X + rect.W * 0.5f, rect.Y),
                new Vector2(rect.X + rect.W, rect.Y + rect.H),
                new Vector2(rect.X, rect.Y + rect.H),
            };
        
            gui.Canvas.ConvexFill(points, color);
        }
    }

    public struct ImDropdownStyle
    {
        public const float DEFAULT_MAX_PANEL_HEIGHT = 300;

        public static readonly ImDropdownStyle Default = CreateDefaultStyle();
        
        private static ImDropdownStyle CreateDefaultStyle()
        {
            var style = new ImDropdownStyle()
            {
                Alignment = new ImTextAlignment(0.0f, 0.5f),
                ArrowInnerScale = 0.7f,
                ArrowOuterScale = 0.7f,
                MaxPanelHeight = DEFAULT_MAX_PANEL_HEIGHT,
                OptionsButtonsSpacing = 0
            };
            
            style.OptionButton = ImButtonStyle.Default;
            style.OptionButton.Normal.BackColor = ImColors.Blue.WithAlpha(0);
            style.OptionButton.Hovered.BackColor = ImColors.Blue.WithAlpha(32);
            style.OptionButton.Pressed.BackColor = ImColors.Blue.WithAlpha(48);
            style.OptionButton.AdditionalPadding += 4;
            style.OptionButton.Alignment.X = 0;
            style.OptionButton.SetBorderWidth(0);
            
            style.OptionButtonSelected = ImButtonStyle.Default;
            style.OptionButtonSelected.Normal.BackColor = ImColors.Blue;
            style.OptionButtonSelected.Normal.FrontColor = ImColors.White;
            style.OptionButtonSelected.Hovered.BackColor = ImColors.LightBlue;
            style.OptionButtonSelected.Hovered.FrontColor = ImColors.White;
            style.OptionButtonSelected.Pressed.BackColor = ImColors.DarkBlue;
            style.OptionButtonSelected.Pressed.FrontColor = ImColors.White;
            style.OptionButtonSelected.AdditionalPadding += 4;
            style.OptionButtonSelected.Alignment.X = 0;
            style.OptionButtonSelected.SetBorderWidth(0);

            return style;
        }

        public float ArrowInnerScale;
        public float ArrowOuterScale;
        public ImTextAlignment Alignment;
        public float MaxPanelHeight;
        public ImButtonStyle OptionButton;
        public ImButtonStyle OptionButtonSelected;
        public float OptionsButtonsSpacing;
    }
}