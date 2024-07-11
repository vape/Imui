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
                    BorderRadius = 4.0f
                },
                ResizeHandleColor = ImColors.Gray2.WithAlpha(196),
                ResizeHandleSize = 24,
                ContentPadding = 4,
                TitleBar = new ImWindowTitleBarStyle()
                {
                    BackColor = ImColors.Gray3,
                    FrontColor = ImColors.White,
                    AdditionalPadding = 2,
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
                ArrowInnerScale = 0.7f,
                ArrowOuterScale = 0.7f,
                Button = CreateButtonStyle(),
                TextAlignment = new ImTextAlignment(0.0f, 0.5f)
            };

            style.Button.Normal.BorderWidth = 0.0f;
            style.Button.Hovered.BorderWidth = 0.0f;
            style.Button.Pressed.BorderWidth = 0.0f;

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
                    BorderWidth = 1,
                    BorderRadius = 3
                }
            };
        }

        public static ImScrollStyle CreateScrollStyle()
        {
            return new ImScrollStyle()
            {
                Size = 20,
                Margin = 1,
                Padding = 1,
                BorderRadius = 3,
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
                        BorderRadius = 3.0f,
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
                        BorderRadius = 3.0f,
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
                ArrowInnerScale = 0.7f,
                ArrowOuterScale = 0.7f,
                MaxPanelHeight = 300,
                OptionsButtonsSpacing = 0
            };

            style.OptionButton = CreateButtonStyle();
            style.OptionButton.Normal.BackColor = ImColors.Blue.WithAlpha(0);
            style.OptionButton.Hovered.BackColor = ImColors.Blue.WithAlpha(32);
            style.OptionButton.Pressed.BackColor = ImColors.Blue.WithAlpha(48);
            style.OptionButton.AdditionalPadding += 4;
            style.OptionButton.Alignment.X = 0;
            style.OptionButton.Normal.BorderWidth = 0;
            style.OptionButton.Hovered.BorderWidth = 0;
            style.OptionButton.Pressed.BorderWidth = 0;

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
                    BorderWidth = 1,
                    BorderColor = ImColors.Black,
                    BorderRadius = 4
                },
                Handle = CreateButtonStyle(),
                Padding = 1,
                HandleAspectRatio = 1.5f
            };
            
            style.Handle.Normal.BackColor = ImColors.Black;
            style.Handle.Normal.BorderRadius = (style.Box.BorderRadius - style.Box.BorderWidth);
            style.Handle.Normal.BorderWidth = 0.0f;
            style.Handle.Hovered.BackColor = ImColors.Gray1;
            style.Handle.Hovered.BorderRadius = style.Handle.Normal.BorderRadius;
            style.Handle.Hovered.BorderWidth = 0.0f;
            style.Handle.Pressed.BackColor = ImColors.Black;
            style.Handle.Pressed.BorderRadius = style.Handle.Normal.BorderRadius;
            style.Handle.Pressed.BorderWidth = 0.0f;
            
            return style;
        }

        private static ImControlsStyle CreateControlsStyle()
        {
            return new ImControlsStyle()
            {
                Padding = 2,
                TextSize = 26,
                ControlsSpacing = 4,
                InnerSpacing = 2,
                ScrollSpeedScale = 6,
                Indent = 20
            };
        }
    }
}