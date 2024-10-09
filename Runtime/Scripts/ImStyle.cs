using System;
using Imui.Controls.Styling;
using Imui.Controls.Styling.Themes;
using Imui.Core;
using UnityEngine;
using UnityEngine.Profiling;

namespace Imui
{
    [Serializable]
    public class ImStyle
    {
        public bool IsDark;

        public float TextSize;
        public float Spacing;
        public float InnerSpacing;

        public float WindowBorderRadius;
        public float WindowBorderThickness;
        
        public float BorderRadius;
        public float BorderWidth;

        public Color32 Background;
        public Color32 Foreground;

        public Color32 AccentBackground;
        public Color32 AccentForeground;
        
        public Color32 BorderColor;
        public Color32 ButtonColor;
        public Color32 FieldColor;
    }

    public static class ImThemeBuilder
    {
        public static ImTheme Build(ImStyle style)
        {
            Profiler.BeginSample("ImThemeBuilder.Build");
            
            var theme = style.IsDark ? ImDarkTheme.Create() : ImLightTheme.Create();

            theme.Text.Color = style.Foreground;

            var black = new Color32(0, 0, 0, 255);
            var white = new Color32(255, 255, 255, 255);
            
            // Base
            theme.Controls.ControlsSpacing = style.Spacing;
            theme.Controls.InnerSpacing = style.InnerSpacing;
            theme.Controls.TextSize = style.TextSize;
            
            // Window
            theme.Window.ContentPadding = style.Spacing;
            theme.Window.Box.BackColor = style.Background;
            theme.Window.Box.FrontColor = style.Foreground;
            theme.Window.TitleBar.BackColor = D(style.Background, 0.15f);
            theme.Window.TitleBar.FrontColor = style.Foreground;
            theme.Window.ResizeHandleColor = D(style.Background, 0.15f);
            theme.Window.Box.BorderRadius = style.WindowBorderRadius;
            theme.Window.Box.BorderWidth = style.WindowBorderThickness;
            theme.Window.Box.BorderColor = style.BorderColor;

            // Button
            theme.Button.BorderRadius = style.BorderRadius;
            theme.Button.BorderThickness = style.BorderWidth;
            
            theme.Button.Normal.BackColor = style.ButtonColor;
            theme.Button.Normal.FrontColor = style.Foreground;
            theme.Button.Normal.BorderColor = style.BorderColor;

            theme.Button.Hovered.BackColor = L(style.ButtonColor, 0.05f);
            theme.Button.Hovered.FrontColor = L(style.Foreground, 0.05f);
            theme.Button.Hovered.BorderColor = L(style.BorderColor, 0.05f);

            theme.Button.Pressed.BackColor = D(style.ButtonColor, 0.05f);
            theme.Button.Pressed.FrontColor = D(style.Foreground, 0.05f);
            theme.Button.Pressed.BorderColor = D(style.BorderColor, 0.05f);
            
            // Text Edit
            var textSelection = style.AccentBackground.WithAlpha((byte)(255 * 0.25f));

            theme.TextEdit.Normal.Box.BorderRadius = style.BorderRadius;
            theme.TextEdit.Normal.Box.BorderWidth = style.BorderWidth;
            
            theme.TextEdit.Normal.SelectionColor = textSelection;
            theme.TextEdit.Normal.Box.BackColor = style.FieldColor;
            theme.TextEdit.Normal.Box.FrontColor = style.Foreground;
            theme.TextEdit.Normal.Box.BorderColor = style.BorderColor;
            theme.TextEdit.Normal.Box.BorderRadius = style.BorderRadius;
            theme.TextEdit.Normal.Box.BorderWidth = style.BorderWidth;

            theme.TextEdit.Selected.SelectionColor = textSelection;
            theme.TextEdit.Selected.Box.BackColor = L(style.FieldColor, 0.05f);
            theme.TextEdit.Selected.Box.FrontColor = style.Foreground;
            theme.TextEdit.Selected.Box.BorderColor = style.AccentBackground;
            theme.TextEdit.Selected.Box.BorderRadius = style.BorderRadius;
            theme.TextEdit.Selected.Box.BorderWidth = style.BorderWidth;
            
            // Scroll
            theme.Scroll.BorderRadius = style.BorderRadius;
            theme.Scroll.NormalState.BackColor = style.ButtonColor.WithAlpha(196);
            theme.Scroll.NormalState.FrontColor = style.Foreground.WithAlpha(64);
            theme.Scroll.HoveredState.BackColor = style.ButtonColor.WithAlpha(255);
            theme.Scroll.HoveredState.FrontColor = style.Foreground.WithAlpha(128);
            theme.Scroll.PressedState.BackColor = style.ButtonColor.WithAlpha(255);
            theme.Scroll.PressedState.FrontColor = style.Foreground;
            
            // Slider
            theme.Slider.Box.BackColor = style.FieldColor;
            theme.Slider.Box.BorderColor = style.BorderColor;
            theme.Slider.Box.BorderWidth = style.BorderWidth;
            theme.Slider.Box.BorderRadius = style.BorderRadius;
            theme.Slider.Box.FrontColor = style.Foreground;
            theme.Slider.Handle.BorderRadius = style.BorderRadius - style.BorderWidth;
            
            theme.Slider.Handle.Normal.BackColor = D(style.Background, 0.4f);
            theme.Slider.Handle.Hovered.BackColor = D(style.Background, 0.3f);
            theme.Slider.Handle.Pressed.BackColor = D(style.Background, 0.5f);
            
            // List
            theme.List.Box.BorderColor = style.BorderColor;
            theme.List.Box.BackColor = style.FieldColor;
            theme.List.Box.BorderRadius = style.BorderRadius;
            theme.List.Box.BorderWidth = style.BorderWidth;
            theme.List.Box.FrontColor = default;
            theme.List.Padding = style.Spacing;

            theme.List.ItemNormal.BorderThickness = 0.0f;
            theme.List.ItemNormal.BorderRadius = style.BorderRadius;
            theme.List.ItemNormal.Alignment = new ImTextAlignment(0.0f, 0.5f);
            
            theme.List.ItemNormal.Normal.BackColor = style.Foreground.WithAlpha(8);
            theme.List.ItemNormal.Normal.FrontColor = style.Foreground;
            theme.List.ItemNormal.Normal.BorderColor = default;

            theme.List.ItemNormal.Hovered.BackColor = style.Foreground.WithAlpha(24);
            theme.List.ItemNormal.Hovered.FrontColor = style.Foreground;
            theme.List.ItemNormal.Hovered.BorderColor = default;

            theme.List.ItemNormal.Pressed.BackColor = style.Foreground.WithAlpha(48);
            theme.List.ItemNormal.Pressed.FrontColor = style.Foreground;
            theme.List.ItemNormal.Pressed.BorderColor = default;
            
            theme.List.ItemSelected.BorderThickness = 0.0f;
            theme.List.ItemSelected.BorderRadius = style.BorderRadius;
            theme.List.ItemSelected.Alignment = new ImTextAlignment(0.0f, 0.5f);

            theme.List.ItemSelected.Normal.BackColor = style.AccentBackground;
            theme.List.ItemSelected.Normal.FrontColor = style.AccentForeground;
            theme.List.ItemSelected.Normal.BorderColor = default;

            theme.List.ItemSelected.Hovered.BackColor = L(style.AccentBackground, 0.1f);
            theme.List.ItemSelected.Hovered.FrontColor = style.AccentForeground;
            theme.List.ItemSelected.Hovered.BorderColor = default;

            theme.List.ItemSelected.Pressed.BackColor = D(style.AccentBackground, 0.05f);
            theme.List.ItemSelected.Pressed.FrontColor = style.AccentForeground;
            theme.List.ItemSelected.Pressed.BorderColor = default;
            
            // Foldout
            theme.Foldout.ArrowScale = 0.6f;
            theme.Foldout.Button = theme.Button;
            theme.Foldout.Button.BorderThickness = 0.0f;
            theme.Foldout.Button.Alignment = new ImTextAlignment(0.0f, 0.5f);
            
            // Tree
            theme.Tree.ArrowScale = 0.6f;
            theme.Tree.ItemNormal = theme.List.ItemNormal;
            theme.Tree.ItemNormal.Normal.BackColor = default;
            theme.Tree.ItemSelected = theme.List.ItemSelected;
            
            // Dropdown
            theme.Dropdown.Button = theme.Button;
            theme.Dropdown.Button.Alignment = new ImTextAlignment(0.0f, 0.5f);
            
            // Checkbox
            theme.Checkbox.Normal = theme.Button;
            theme.Checkbox.CheckmarkScale = 0.6f;
            
            theme.Checkbox.Checked = theme.Button;
            theme.Checkbox.Checked.Normal.BackColor = style.AccentBackground;
            theme.Checkbox.Checked.Normal.FrontColor = style.AccentForeground;
            theme.Checkbox.Checked.Normal.BorderColor = D(style.AccentBackground, 0.1f);

            theme.Checkbox.Checked.Hovered.BackColor = L(style.AccentBackground, 0.1f);
            theme.Checkbox.Checked.Hovered.FrontColor = style.AccentForeground;
            theme.Checkbox.Checked.Hovered.BorderColor = D(style.AccentBackground, 0.05f);

            theme.Checkbox.Checked.Pressed.BackColor = D(style.AccentBackground, 0.05f);
            theme.Checkbox.Checked.Pressed.FrontColor = style.AccentForeground;
            theme.Checkbox.Checked.Pressed.BorderColor = D(style.AccentBackground, 0.15f);
            
            // Radiobox
            theme.Radiobox.KnobScale = 0.5f;
            theme.Radiobox.Normal = theme.Checkbox.Normal;
            theme.Radiobox.Normal.BorderRadius = 999.9f;
            theme.Radiobox.Checked = theme.Checkbox.Checked;
            theme.Radiobox.Checked.BorderRadius = 999.9f;
            
            // Separator
            theme.Separator.Thickness = style.TextSize * 0.15f;
            theme.Separator.Color = style.BorderColor;
            
            Profiler.EndSample();
            
            return theme;
        }

        public static Color32 D(Color32 color, float value) => ScaleValue(color, 1.0f - value);
        public static Color32 L(Color32 color, float value) => ScaleValue(color, 1.0f + value);
                
        public static Color32 ScaleValue(Color32 color, float scale)
        {
            Color.RGBToHSV(color, out var h, out var s, out var v);
            v = Mathf.Clamp01(v * scale);
            var result = (Color32)Color.HSVToRGB(h, s, v);
            result.a = color.a;
            return result;
        }
    }
}