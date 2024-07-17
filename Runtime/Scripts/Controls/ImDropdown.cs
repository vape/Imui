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
        public static void BeginDropdown(this ImGui gui, ReadOnlySpan<char> label, out bool open, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rect = ImControls.GetRowRect(gui, size);
            BeginDropdown(gui, label, out open, rect);
        }
        
        public static void BeginDropdown(this ImGui gui, ReadOnlySpan<char> label, out bool open, ImRect rect)
        {
            gui.PushId(label);
            
            var id = gui.GetNextControlId();
            ref var state = ref gui.Storage.Get<ImDropdownState>(id);

            var clicked = DropdownButton(gui, id, label, rect);
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
        
        public static bool Dropdown(this ImGui gui, ref int selected, ReadOnlySpan<string> options, ImSize size = default, ReadOnlySpan<char> defaultLabel = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rect = ImControls.GetRowRect(gui, size);
            return Dropdown(gui, ref selected, options, rect, defaultLabel);
        }
        
        public static bool Dropdown(this ImGui gui, ref int selected, ReadOnlySpan<string> options, ImRect rect, ReadOnlySpan<char> defaultLabel = default)
        {
            var id = gui.GetNextControlId();
            return Dropdown(gui, id, ref selected, options, rect, defaultLabel);
        }
        
        public static bool Dropdown(this ImGui gui, uint id, ref int selected, ReadOnlySpan<string> options, ImRect rect, ReadOnlySpan<char> defaultLabel = default)
        {
            ref var state = ref gui.Storage.Get<ImDropdownState>(id);

            var text = selected < 0 || selected >= options.Length ? defaultLabel : options[selected];
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

            ref readonly var style = ref ImTheme.Active.Dropdown;
            
            var optionButtonHeight = gui.GetRowHeight();
            var totalSpacingHeight = style.OptionsButtonsSpacing * (options.Length - 1);
            var panelHeight = ImPanel.GetEnclosingHeight(Mathf.Min(style.MaxPanelHeight, options.Length * optionButtonHeight + totalSpacingHeight));
            var panelRect = new ImRect(position.x, position.y - panelHeight, width, panelHeight);

            gui.Canvas.PushNoClipRect();
            gui.Canvas.PushNoRectMask();

            gui.PushId(id);
            gui.BeginPopup();
            gui.BeginPanel(panelRect);
            gui.BeginScrollable();

            var optionButtonWidth = gui.Layout.GetAvailableWidth();

            using (new ImStyleScope<ImControlsStyle>(ref ImTheme.Active.Controls))
            {
                ImTheme.Active.Controls.ControlsSpacing = style.OptionsButtonsSpacing;
                    
                for (int i = 0; i < options.Length; ++i)
                {
                    var isSelected = selected == i;
                    var optionStyle = isSelected ? style.OptionButtonSelected : style.OptionButton;

                    using (new ImStyleScope<ImButtonStyle>(ref ImTheme.Active.Button, optionStyle))
                    {
                        if (gui.Button(options[i], (optionButtonWidth, optionButtonHeight)))
                        {
                            selected = i;
                            changed = true;
                        }
                    }
                }
            }
            
            gui.EndScrollable();
            gui.EndPanel();
            gui.EndPopup(out closed);
            gui.PopId();
                
            gui.Canvas.PopRectMask();
            gui.Canvas.PopClipRect();
        }

        public static bool DropdownButton(ImGui gui, uint id, ReadOnlySpan<char> label, ImRect rect)
        {
            var arrowRect = ImButton.GetContentRect(rect);
            var arrowSize = (arrowRect.H - ImTheme.Active.Controls.ExtraRowHeight) * ImTheme.Active.Dropdown.ArrowOuterScale;
            arrowRect.X += arrowRect.W - arrowSize;
            arrowRect.W = arrowSize;
            
            using var _ = new ImStyleScope<ImButtonStyle>(ref ImTheme.Active.Button);
            
            ImTheme.Active.Button.Alignment = ImTheme.Active.Dropdown.Alignment;
            ImTheme.Active.Button.Padding.Right += arrowRect.W + ImTheme.Active.Controls.InnerSpacing;

            var clicked = gui.Button(id, label, rect, out var state);

            DrawArrow(gui, arrowRect, ImButton.GetStateFontColor(state));

            return clicked;
        }

        public static void DrawArrow(ImGui gui, ImRect rect, Color32 color)
        {
            rect = rect.WithAspect(1.0f).ScaleFromCenter(ImTheme.Active.Dropdown.ArrowInnerScale).WithAspect(1.1547f);
            
            Span<Vector2> points = stackalloc Vector2[3]
            {
                new Vector2(rect.X + rect.W * 0.5f, rect.Y),
                new Vector2(rect.X + rect.W, rect.Y + rect.H),
                new Vector2(rect.X, rect.Y + rect.H),
            };
        
            gui.Canvas.ConvexFill(points, color);
        }
    }

    [Serializable]
    public struct ImDropdownStyle
    {
        public float ArrowInnerScale;
        public float ArrowOuterScale;
        public ImTextAlignment Alignment;
        public float MaxPanelHeight;
        public ImButtonStyle OptionButton;
        public ImButtonStyle OptionButtonSelected;
        public float OptionsButtonsSpacing;
    }
}