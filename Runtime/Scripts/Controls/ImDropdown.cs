using System;
using Imui.Core;
using Imui.Styling;
using UnityEngine;

namespace Imui.Controls
{
    public struct ImDropdownState
    {
        public bool Open;
    }
    
    // TODO (artem-s): add optional search field
    // TODO (artem-s): maybe move panel drawing into separate control for reusing
    public static class ImDropdown
    {
        public static ImDropdownStyle Style = ImDropdownStyle.Default;
        
        public static bool Dropdown(this ImGui gui, ref int selected, in ReadOnlySpan<string> options)
        {
            gui.AddControlSpacing();

            var rect = gui.Layout.AddRect(gui.Layout.GetAvailableWidth(), GetControlHeight(gui));
            return Dropdown(gui, ref selected, in options, in rect);
        }
        
        public static bool Dropdown(this ImGui gui, ref int selected, in ReadOnlySpan<string> options, float width, float height)
        {
            gui.AddControlSpacing();
            
            var rect = gui.Layout.AddRect(width, height);
            return Dropdown(gui, ref selected, in options, in rect);
        }
        
        public static bool Dropdown(this ImGui gui, ref int selected, in ReadOnlySpan<string> options, Vector2 size)
        {
            gui.AddControlSpacing();
            
            var rect = gui.Layout.AddRect(size);
            return Dropdown(gui, ref selected, in options, in rect);
        }
        
        public static bool Dropdown(this ImGui gui, ref int selected, in ReadOnlySpan<string> options, in ImRect rect)
        {
            var id = gui.GetNextControlId();
            ref var state = ref gui.Storage.Get<ImDropdownState>(id);

            var text = selected < 0 || selected >= options.Length ? string.Empty : options[selected];
            var clicked = gui.Select(text, rect);
            var changed = false;
            var closeClicked = false;
            
            if (state.Open)
            {
                var contentHeight = gui.GetRowHeight();
                var optionButtonHeight = Style.OptionButton.GetButtonHeight(contentHeight);
                var totalSpacingHeight = Style.OptionsButtonsSpacing * (options.Length - 1);
                var panelHeight = ImPanel.Style.GetHeight(Mathf.Min(Style.MaxPanelHeight, options.Length * optionButtonHeight + totalSpacingHeight));
                var panelRect = new ImRect(rect.X, rect.Y - panelHeight, rect.W, panelHeight);

                gui.Canvas.PushNoClipRect();
                gui.Canvas.PushNoRectMask();
                
                gui.BeginPopup();
                gui.BeginPanel(in panelRect);

                var optionButtonWidth = gui.Layout.GetAvailableWidth();

                using (new ImStyleScope<ImControlsStyle>(ref ImControls.Style))
                {
                    ImControls.Style.Spacing = Style.OptionsButtonsSpacing;
                    
                    for (int i = 0; i < options.Length; ++i)
                    {
                        var isSelected = selected == i;
                        var style = isSelected ? Style.OptionButtonSelected : Style.OptionButton;

                        using (new ImStyleScope<ImButtonStyle>(ref ImButton.Style, style))
                        {
                            if (gui.Button(options[i], optionButtonWidth, optionButtonHeight))
                            {
                                selected = i;
                                changed = true;
                            }
                        }
                    }
                }
                
                gui.EndPanel();
                gui.EndPopup(out closeClicked);
                
                gui.Canvas.PopRectMask();
                gui.Canvas.PopClipRect();
            }
            
            if (clicked || closeClicked || changed)
            {
                state.Open = !state.Open;
            }

            return changed;
        }

        public static float GetControlHeight(ImGui gui)
        {
            return ImSelect.Style.Button.GetButtonHeight(gui.GetRowHeight());
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
                MaxPanelHeight = DEFAULT_MAX_PANEL_HEIGHT,
                OptionsButtonsSpacing = 0
            };
            
            style.OptionButton = ImButtonStyle.Default;
            style.OptionButton.Normal.BackColor = ImColors.Blue.WithAlpha(0);
            style.OptionButton.Hovered.BackColor = ImColors.Blue.WithAlpha(32);
            style.OptionButton.Pressed.BackColor = ImColors.Blue.WithAlpha(48);
            style.OptionButton.Padding += 4;
            style.OptionButton.Alignment.X = 0;
            style.OptionButton.SetBorderWidth(0);
            
            style.OptionButtonSelected = ImButtonStyle.Default;
            style.OptionButtonSelected.Normal.BackColor = ImColors.Blue;
            style.OptionButtonSelected.Normal.FrontColor = ImColors.White;
            style.OptionButtonSelected.Hovered.BackColor = ImColors.LightBlue;
            style.OptionButtonSelected.Hovered.FrontColor = ImColors.White;
            style.OptionButtonSelected.Pressed.BackColor = ImColors.DarkBlue;
            style.OptionButtonSelected.Pressed.FrontColor = ImColors.White;
            style.OptionButtonSelected.Padding += 4;
            style.OptionButtonSelected.Alignment.X = 0;
            style.OptionButtonSelected.SetBorderWidth(0);

            return style;
        }
        
        public float MaxPanelHeight;
        public ImButtonStyle OptionButton;
        public ImButtonStyle OptionButtonSelected;
        public float OptionsButtonsSpacing;
    }
}