using Imui.Core;
using UnityEngine;

namespace Imui.Controls.Styling.Themes
{
    public static class ImDefaultTheme
    {
        public static ImTheme Create()
        {
            return new ImTheme()
            {
                Window = CreateWindowStyle(),
                Text = CreateTextStyle(),
                Button = CreateButtonStyle(),
                Checkbox = CreateCheckboxStyle(),
                Foldout = CreateFoldoutStyle(),
                Panel = CreatePanelStyle(),
                Scroll = CreateScrollStyle(),
                TextEdit = CreateTextEditStyle(),
                Dropdown = CreateDropdownStyle(),
                Slider = CreateSliderStyle(),
                Controls = CreateControlsStyle()
            };
        }

        public static ImWindowStyle CreateWindowStyle()
        {
            return new ImWindowStyle()
            {
                Box = new ImBoxStyle()
                {
                    BackColor = ImColors.White,
                    BorderColor = ImColors.Black,
                    BorderWidth = 1.0f,
                    BorderRadius = 8.0f
                },
                ResizeHandleColor = ImColors.Gray2.WithAlpha(196),
                ResizeHandleSize = 30.0f,
                ContentPadding = 4.0f,
                TitleBar = new ImWindowTitleBarStyle()
                {
                    BackColor = ImColors.Gray3,
                    FrontColor = ImColors.White,
                    AdditionalPadding = 2.0f,
                    Alignment = new ImTextAlignment(0.5f, 0.5f)
                }
            };
        }

        public static ImTextStyle CreateTextStyle()
        {
            return new ImTextStyle()
            {
                Color = ImColors.Black,
                Alignment = new ImTextAlignment(0.0f, 0.0f)
            };
        }

        public static ImButtonStyle CreateButtonStyle()
        {
            return new ImButtonStyle()
            {
                AdditionalPadding = 0.0f,
                Alignment = new ImTextAlignment(0.5f, 0.5f),
                TextWrap = false,
                BorderRadius = 4.0f,
                BorderWidth = 1.0f,
                Normal = new ImButtonStateStyle()
                {
                    BackColor = new Color32(230, 230, 230, 255),
                    BorderColor = ImColors.Black,
                    FrontColor = ImColors.Black
                },
                Hovered = new ImButtonStateStyle()
                {
                    BackColor = new Color32(235, 235, 235, 255),
                    BorderColor = ImColors.Gray1,
                    FrontColor = ImColors.Gray1
                },
                Pressed = new ImButtonStateStyle()
                {
                    BackColor = new Color32(220, 220, 220, 255),
                    BorderColor = ImColors.Black,
                    FrontColor = ImColors.Black
                }
            };
        }

        public static ImCheckboxStyle CreateCheckboxStyle()
        {
            return new ImCheckboxStyle()
            {
                CheckmarkScale = 0.6f,
                TextAlignment = new ImTextAlignment(0.0f, 0.5f),
                WrapText = false
            };
        }

        public static ImFoldoutStyle CreateFoldoutStyle()
        {
            var style = new ImFoldoutStyle()
            {
                ArrowInnerScale = 0.6f,
                ArrowOuterScale = 0.7f,
                BorderWidth = 0.0f,
                TextAlignment = new ImTextAlignment(0.0f, 0.5f)
            };

            return style;
        }

        public static ImPanelStyle CreatePanelStyle()
        {
            return new ImPanelStyle()
            {
                Box = new ImBoxStyle
                {
                    BackColor = ImColors.White,
                    BorderColor = ImColors.Black,
                    BorderWidth = 1.0f,
                    BorderRadius = 4.0f
                }
            };
        }

        public static ImScrollStyle CreateScrollStyle()
        {
            return new ImScrollStyle()
            {
                Size = 20.0f,
                Margin = 2.0f,
                Padding = 1.0f,
                BorderRadius = 4.0f,
                NormalState = new ImScrollBarStateStyle()
                {
                    BackColor = ImColors.Black,
                    FrontColor = ImColors.Gray7
                },
                HoveredState = new ImScrollBarStateStyle()
                {
                    BackColor = ImColors.Black,
                    FrontColor = ImColors.Gray8
                },
                PressedState = new ImScrollBarStateStyle()
                {
                    BackColor  = ImColors.Black,
                    FrontColor = ImColors.Gray6
                }
            };
        }

        public static ImTextEditStyle CreateTextEditStyle()
        {
            return new ImTextEditStyle()
            {
                Normal = new ImTextEditStateStyle()
                {
                    Box = new ImBoxStyle()
                    {
                        BackColor = ImColors.Gray7,
                        FrontColor = ImColors.Black,
                        BorderColor = ImColors.Gray1,
                        BorderRadius = 4.0f,
                        BorderWidth = 1.0f
                    },
                    SelectionColor = ImColors.Black.WithAlpha(32)
                },
                Selected = new ImTextEditStateStyle()
                {
                    Box = new ImBoxStyle()
                    {
                        BackColor = ImColors.White,
                        FrontColor = ImColors.Black,
                        BorderColor = ImColors.Black,
                        BorderRadius = 4.0f,
                        BorderWidth = 1.0f
                    },
                    SelectionColor = ImColors.Black.WithAlpha(64)
                },
                CaretWidth = 2.0f,
                Alignment = new ImTextAlignment(0.0f, 0.0f),
                TextWrap = false
            };
        }
        
        private static ImDropdownStyle CreateDropdownStyle()
        {
            var style = new ImDropdownStyle()
            {
                Alignment = new ImTextAlignment(0.0f, 0.5f),
                ArrowInnerScale = 0.6f,
                ArrowOuterScale = 0.7f,
                MaxPanelHeight = 300.0f,
                OptionsButtonsSpacing = 2.0f
            };

            style.OptionButton = CreateButtonStyle();
            style.OptionButton.Normal.BackColor = ImColors.Blue.WithAlpha(0);
            style.OptionButton.Hovered.BackColor = ImColors.Blue.WithAlpha(32);
            style.OptionButton.Pressed.BackColor = ImColors.Blue.WithAlpha(48);
            style.OptionButton.AdditionalPadding = 0.0f;
            style.OptionButton.AdditionalPadding.Left = 4.0f;
            style.OptionButton.Alignment.X = 0.0f;
            style.OptionButton.BorderWidth = 0.0f;

            style.OptionButtonSelected = style.OptionButton;
            style.OptionButtonSelected.Normal.BackColor = ImColors.DarkBlue.WithAlpha(240);
            style.OptionButtonSelected.Normal.FrontColor = ImColors.White;
            style.OptionButtonSelected.Hovered.BackColor = ImColors.DarkBlue.WithAlpha(224);
            style.OptionButtonSelected.Hovered.FrontColor = ImColors.White;
            style.OptionButtonSelected.Pressed.BackColor = ImColors.DarkBlue.WithAlpha(255);
            style.OptionButtonSelected.Pressed.FrontColor = ImColors.White;

            return style;
        }

        private static ImSliderStyle CreateSliderStyle()
        {
            var style = new ImSliderStyle()
            {
                Box = new ImBoxStyle()
                {
                    BackColor = ImColors.White,
                    BorderWidth = 1.0f,
                    BorderColor = ImColors.Black,
                    BorderRadius = 4.0f
                },
                Handle = CreateButtonStyle(),
                Padding = 1.0f,
                HandleAspectRatio = 1.0f
            };
            
            style.Handle.Normal.BackColor = ImColors.Black;
            style.Handle.Hovered.BackColor = ImColors.Gray1;
            style.Handle.Pressed.BackColor = ImColors.Black;
            style.Handle.BorderRadius = (style.Box.BorderRadius - style.Box.BorderWidth);
            style.Handle.BorderWidth = 0.0f;
            
            return style;
        }

        private static ImControlsStyle CreateControlsStyle()
        {
            return new ImControlsStyle()
            {
                Padding = 4.0f,
                TextSize = 22.0f,
                ControlsSpacing = 2.0f,
                InnerSpacing = 2.0f,
                ScrollSpeedScale = 6.0f,
                Indent = 20.0f
            };
        }
    }
}