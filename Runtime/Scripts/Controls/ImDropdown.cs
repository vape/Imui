using System;
using Imui.Core;
using Imui.Styling;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImDropdown
    {
        public static ImDropdownStyle Style = ImDropdownStyle.Default;
        
        public static bool Dropdown(this ImGui gui, ref int selected, in ReadOnlySpan<string> options)
        {
            gui.AddControlSpacing();

            var height = gui.GetRowHeight() + ImSelect.Style.Button.Padding.Vertical;
            var rect = gui.Layout.AddRect(gui.Layout.GetAvailableWidth(), height);
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
            ref var state = ref gui.Storage.Get<State>(id);

            var selectedLabel = selected < 0 || selected >= options.Length ? string.Empty : options[selected];
            var clicked = gui.Select(selectedLabel, rect);
            var changed = false;
            var closeClicked = false;
            
            if (state.Open)
            {
                var buttonHeight = gui.GetRowHeight() + Style.OptionButton.Padding.Vertical;
                var totalSpacingHeight = ImControls.Style.Spacing * (options.Length - 1);
                var popupHeight = Mathf.Min(Style.MaxPopupHeight, options.Length * buttonHeight + totalSpacingHeight);
                var popupRectSize = ImPanel.PanelSizeFromContentSize(new Vector2(rect.W, popupHeight));
                var popupRect = new ImRect(rect.X, rect.Y - popupRectSize.y, rect.W, popupRectSize.y);
                
                gui.Canvas.PushNoClipRect();
                gui.Canvas.PushNoRectMask();
                
                gui.BeginPopup();
                gui.BeginPanel(popupRect);

                var buttonWidth = gui.Layout.GetAvailableWidth();
                var currentlySelected = selected;
                
                for (int i = 0; i < options.Length; ++i)
                {
                    var isSelected = currentlySelected == i;
                    var style = isSelected ? Style.OptionButtonSelected : Style.OptionButton;
                    
                    using var _ = new ImStyleScope<ImButtonStyle>(ref ImButton.Style, style);
                    
                    if (gui.Button(options[i], buttonWidth, buttonHeight))
                    {
                        selected = i;
                        changed = true;
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

        public struct State
        {
            public bool Open;
        }
    }

    public struct ImDropdownStyle
    {
        public const float DEFAULT_MAX_POPUP_HEIGHT = 300;

        public static readonly ImDropdownStyle Default = CreateDefaultStyle();
        
        private static ImDropdownStyle CreateDefaultStyle()
        {
            var style = new ImDropdownStyle()
            {
                MaxPopupHeight = DEFAULT_MAX_POPUP_HEIGHT
            };
            
            style.OptionButton = ImButtonStyle.Default;
            style.OptionButton.Normal.BackColor = ImColors.Blue.WithAlpha(0);
            style.OptionButton.Hovered.BackColor = ImColors.Blue.WithAlpha(32);
            style.OptionButton.Pressed.BackColor = ImColors.Blue.WithAlpha(48);
            style.OptionButton.Alignment.Hor = 0;
            style.OptionButton.SetBorderWidth(0);
            
            style.OptionButtonSelected = ImButtonStyle.Default;
            style.OptionButtonSelected.Normal.BackColor = ImColors.Blue;
            style.OptionButtonSelected.Normal.FrontColor = ImColors.White;
            style.OptionButtonSelected.Hovered.BackColor = ImColors.LightBlue;
            style.OptionButtonSelected.Hovered.FrontColor = ImColors.White;
            style.OptionButtonSelected.Pressed.BackColor = ImColors.DarkBlue;
            style.OptionButtonSelected.Pressed.FrontColor = ImColors.White;
            style.OptionButtonSelected.Alignment.Hor = 0;
            style.OptionButtonSelected.SetBorderWidth(0);

            return style;
        }
        
        public float MaxPopupHeight;
        public ImButtonStyle OptionButton;
        public ImButtonStyle OptionButtonSelected;
    }
}