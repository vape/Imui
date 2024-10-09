using Imui.Core;
using UnityEngine;

namespace Imui.Controls.Styling.Themes
{
    public static class ImLightTheme
    {
        public const string NAME = "Light";

        public static ImTheme Create()
        {
            var theme = new ImTheme()
            {
                Name = NAME,
                Window = CreateWindowStyle(),
                Text = CreateTextStyle(),
                Button = CreateButtonStyle(),
                Checkbox = CreateCheckboxStyle(),
                Foldout = CreateFoldoutStyle(),
                Scroll = CreateScrollStyle(),
                TextEdit = CreateTextEditStyle(),
                Dropdown = CreateDropdownStyle(),
                Slider = CreateSliderStyle(),
                Controls = CreateControlsStyle(),
                List = CreateListStyle(),
                Radio = CreateRadioStyle(),
                Separator = CreateSeparatorStyle()
            };

            theme.Foldout.ArrowScale = 1.0f;
            theme.Tree.ArrowScale = 1.0f;

            return theme;
        }

        private static ImSeparatorStyle CreateSeparatorStyle()
        {
            return new ImSeparatorStyle { Thickness = 4.0f, FrontColor = new Color32(153, 153, 153, 255) };
        }

        private static ImRadioStyle CreateRadioStyle()
        {
            return new ImRadioStyle
            {
                IndicatorScale = 0.6f,
                TextAlignment = new ImTextAlignment(0.0f, 0.5f),
                WrapText = false
            };
        }

        private static ImListStyle CreateListStyle()
        {
            var style = new ImListStyle()
            {
                Box = new ImBoxStyle
                {
                    BackColor = new Color32(202, 202, 202, 255),
                    BorderColor = new Color32(128, 128, 128, 255),
                    BorderWidth = 1.0f,
                    BorderRadius = 4.0f
                },
                Padding = 4.0f
            };

            style.ItemNormal = CreateButtonStyle();
            style.ItemNormal.HorizontalPadding = 4.0f;
            style.ItemNormal.Alignment.X = 0.0f;
            style.ItemNormal.BorderThickness = 0.0f;

            style.ItemSelected = style.ItemNormal;

            style.ItemNormal.Normal.BackColor = new Color32(255, 255, 255, 64);
            style.ItemNormal.Normal.FrontColor = new Color32(38, 38, 38, 255);
            style.ItemNormal.Normal.BorderColor = new Color32(0, 0, 0, 0);
            style.ItemNormal.Hovered.BackColor = new Color32(255, 255, 255, 102);
            style.ItemNormal.Hovered.FrontColor = new Color32(51, 51, 51, 255);
            style.ItemNormal.Hovered.BorderColor = new Color32(0, 0, 0, 0);
            style.ItemNormal.Pressed.BackColor = new Color32(255, 255, 255, 153);
            style.ItemNormal.Pressed.FrontColor = new Color32(13, 13, 13, 255);
            style.ItemNormal.Pressed.BorderColor = new Color32(0, 0, 0, 0);
            style.ItemSelected.Normal.BackColor = new Color32(0, 122, 204, 255);
            style.ItemSelected.Normal.BorderColor = new Color32(0, 0, 0, 0);
            style.ItemSelected.Normal.FrontColor = new Color32(255, 255, 255, 255);
            style.ItemSelected.Hovered.BackColor = new Color32(0, 137, 230, 255);
            style.ItemSelected.Hovered.BorderColor = new Color32(0, 0, 0, 0);
            style.ItemSelected.Hovered.FrontColor = new Color32(255, 255, 255, 255);
            style.ItemSelected.Pressed.BackColor = new Color32(0, 107, 179, 255);
            style.ItemSelected.Pressed.BorderColor = new Color32(0, 0, 0, 0);
            style.ItemSelected.Pressed.FrontColor = new Color32(255, 255, 255, 255);

            return style;
        }

        public static ImWindowStyle CreateWindowStyle()
        {
            return new ImWindowStyle()
            {
                Box = new ImBoxStyle()
                {
                    BackColor = new Color32(232, 232, 232, 255),
                    BorderColor = new Color32(51, 51, 51, 255),
                    BorderWidth = 1.0f,
                    BorderRadius = 8.0f
                },
                ResizeHandleColor = new Color32(51, 51, 51, 128),
                ResizeHandleSize = 30.0f,
                ContentPadding = 4.0f,
                TitleBar = new ImWindowTitleBarStyle()
                {
                    BackColor = new Color32(209, 209, 209, 255),
                    FrontColor = new Color32(46, 46, 46, 255),
                    AdditionalPadding = 2.0f,
                    Alignment = new ImTextAlignment(0.5f, 0.5f)
                }
            };
        }

        public static ImTextStyle CreateTextStyle()
        {
            return new ImTextStyle() { Color = new Color32(13, 13, 13, 255), Alignment = new ImTextAlignment(0.0f, 0.0f) };
        }

        public static ImButtonStyle CreateButtonStyle()
        {
            return new ImButtonStyle()
            {
                HorizontalPadding = 4.0f,
                Alignment = new ImTextAlignment(0.5f, 0.5f),
                BorderRadius = 4.0f,
                BorderThickness = 1.0f,
                Normal = new ImButtonStateStyle()
                {
                    BackColor = new Color32(215, 215, 215, 255),
                    FrontColor = new Color32(13, 13, 13, 255),
                    BorderColor = new Color32(153, 153, 153, 255),
                },
                Hovered = new ImButtonStateStyle()
                {
                    BackColor = new Color32(219, 219, 219, 255),
                    FrontColor = new Color32(64, 64, 64, 255),
                    BorderColor = new Color32(166, 166, 166, 255),
                },
                Pressed = new ImButtonStateStyle()
                {
                    BackColor = new Color32(202, 202, 202, 255),
                    FrontColor = new Color32(13, 13, 13, 255),
                    BorderColor = new Color32(128, 128, 128, 255),
                }
            };
        }

        public static ImCheckboxStyle CreateCheckboxStyle()
        {
            return new ImCheckboxStyle()
            {
                CheckmarkScale = 0.6f,
                // TextAlignment = new ImTextAlignment(0.0f, 0.5f),
                // WrapText = false
            };
        }

        public static ImFoldoutStyle CreateFoldoutStyle()
        {
            var style = new ImFoldoutStyle();
            // {
            //     ArrowInnerScale = 0.6f,
            //     ArrowOuterScale = 0.7f,
            //     BorderWidth = 0.0f,
            //     TextAlignment = new ImTextAlignment(0.0f, 0.5f)
            // };

            return style;
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
                    BackColor = new Color32(51, 51, 51, 255), FrontColor = new Color32(255, 255, 255, 192),
                },
                HoveredState = new ImScrollBarStateStyle()
                {
                    BackColor = new Color32(51, 51, 51, 255), FrontColor = new Color32(255, 255, 255, 230),
                },
                PressedState = new ImScrollBarStateStyle()
                {
                    BackColor = new Color32(51, 51, 51, 255), FrontColor = new Color32(255, 255, 255, 205),
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
                        BackColor = new Color32(202, 202, 202, 255),
                        FrontColor = new Color32(13, 13, 13, 255),
                        BorderColor = new Color32(128, 128, 128, 255),
                        BorderRadius = 4.0f,
                        BorderWidth = 1.0f
                    },
                    SelectionColor = new Color32(0, 115, 190, 102),
                },
                Selected = new ImTextEditStateStyle()
                {
                    Box = new ImBoxStyle()
                    {
                        BackColor = new Color32(230, 230, 230, 255),
                        FrontColor = new Color32(13, 13, 13, 255),
                        BorderColor = new Color32(53, 53, 53, 255),
                        BorderRadius = 4.0f,
                        BorderWidth = 1.0f
                    },
                    SelectionColor = new Color32(0, 115, 190, 102),
                },
                Padding = 4.0f,
                CaretWidth = 2.0f,
                Alignment = new ImTextAlignment(0.0f, 0.0f),
                TextWrap = false
            };
        }

        private static ImDropdownStyle CreateDropdownStyle()
        {
            var style = new ImDropdownStyle()
            {
                ArrowScale = 0.4f,
                MinListWidth = 100.0f,
                MaxListHeight = 300.0f
            };

            return style;
        }

        private static ImSliderStyle CreateSliderStyle()
        {
            var style = new ImSliderStyle()
            {
                Box = new ImBoxStyle()
                {
                    BackColor = new Color32(202, 202, 202, 255),
                    BorderColor = new Color32(128, 128, 128, 255),
                    BorderWidth = 1.0f,
                    BorderRadius = 4.0f,
                    FrontColor = new Color32(0, 0, 0, 255)
                },
                Handle = CreateButtonStyle(),
                Padding = 1.0f,
                HandleAspectRatio = 1.0f,
            };

            style.Handle.Normal.BackColor = new Color32(110, 110, 110, 255);
            style.Handle.Normal.BorderColor = new Color32(0, 0, 0, 0);
            style.Handle.Hovered.BackColor = new Color32(115, 115, 115, 255);
            style.Handle.Hovered.BorderColor = new Color32(0, 0, 0, 0);
            style.Handle.Pressed.BackColor = new Color32(100, 100, 100, 255);
            style.Handle.Pressed.BorderColor = new Color32(0, 0, 0, 0);

            return style;
        }

        private static ImControlsStyle CreateControlsStyle()
        {
            return new ImControlsStyle()
            {
                ExtraRowHeight = 8.0f,
                TextSize = 22.0f,
                ControlsSpacing = 2.0f,
                InnerSpacing = 2.0f,
                ScrollSpeedScale = 6.0f,
                Indent = 8.0f
            };
        }
    }
}