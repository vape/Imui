using UnityEngine;
using UnityEngine.Profiling;

namespace Imui.Style
{
    public static class ImStyleBuilder
    {
        public static ImTheme Build(ImStyle style)
        {
            Profiler.BeginSample("ImStyleBuilder.Build");

            var theme = new ImTheme();
            
            // Text
            theme.Text.Color = style.Foreground;
            theme.Text.Alignment = default;
            
            // Base
            theme.Layout.ExtraRowHeight = style.ExtraRowSize;
            theme.Layout.ControlsSpacing = style.Spacing;
            theme.Layout.InnerSpacing = style.InnerSpacing;
            theme.Layout.TextSize = style.TextSize;
            theme.Layout.ReadOnlyColorMultiplier = style.ReadOnlyColorMultiplier;
            theme.Layout.Indent = style.Indent;
            theme.Layout.ScrollSpeedScale = style.ScrollSpeed;
            
            // Window
            theme.Window.ContentPadding = style.Spacing;
            theme.Window.Box.BackColor = style.Background;
            theme.Window.Box.FrontColor = style.Foreground;
            theme.Window.TitleBar.BackColor = D(style.Background, 0.15f);
            theme.Window.TitleBar.FrontColor = style.Foreground;
            theme.Window.TitleBar.Alignment = new ImAlignment(0.5f, 0.5f);
            theme.Window.ResizeHandleColor = D(style.Background, 0.15f);
            theme.Window.ResizeHandleSize = style.ScrollBarSize * 1.3f;
            theme.Window.Box.BorderRadius = style.WindowBorderRadius;
            theme.Window.Box.BorderWidth = style.WindowBorderThickness;
            theme.Window.Box.BorderColor = style.BorderColor;

            // Button
            theme.Button.Alignment = new ImAlignment(0.5f, 0.5f);
            theme.Button.BorderRadius = style.BorderRadius;
            theme.Button.BorderThickness = style.BorderWidth;
            
            theme.Button.Normal.BackColor = style.ButtonColor;
            theme.Button.Normal.FrontColor = style.Foreground;
            theme.Button.Normal.BorderColor = style.BorderColor;

            theme.Button.Hovered.BackColor = style.IsDark ? L(style.ButtonColor, 0.3f) : L(style.ButtonColor, 0.05f);
            theme.Button.Hovered.FrontColor = L(style.Foreground, 0.05f);
            theme.Button.Hovered.BorderColor = L(style.BorderColor, 0.05f);

            theme.Button.Pressed.BackColor = style.IsDark ? D(theme.Button.Hovered.BackColor, 0.4f) : D(theme.Button.Hovered.BackColor, 0.1f);
            theme.Button.Pressed.FrontColor = D(style.Foreground, 0.05f);
            theme.Button.Pressed.BorderColor = D(style.BorderColor, 0.05f);
            
            // Text Edit
            var textSelection = style.AccentBackground.WithAlpha((byte)(255 * 0.25f));

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

            theme.TextEdit.CaretWidth = 2.0f;
            theme.TextEdit.Alignment = new ImAlignment(0.0f, 0.0f);
            theme.TextEdit.TextWrap = false;
            
            // Scroll
            theme.Scroll.Size = (int)style.ScrollBarSize;
            theme.Scroll.Padding = style.BorderWidth;
            theme.Scroll.VMargin = new ImPadding(style.Spacing, 0, 0, 0);
            theme.Scroll.HMargin = new ImPadding(0, 0, style.Spacing, 0);
            theme.Scroll.BorderRadius = style.BorderRadius;

            theme.Scroll.NormalState.BackColor = Color32.Lerp(style.Background, style.Foreground, 0.15f);
            theme.Scroll.NormalState.FrontColor = Color32.Lerp(style.Background, style.Foreground, 0.25f);
            theme.Scroll.HoveredState.BackColor = theme.Scroll.NormalState.BackColor;
            theme.Scroll.HoveredState.FrontColor = style.IsDark ? L(theme.Scroll.NormalState.FrontColor, 0.1f) : D(theme.Scroll.NormalState.FrontColor, 0.1f);
            theme.Scroll.PressedState.BackColor = theme.Scroll.HoveredState.BackColor;
            theme.Scroll.PressedState.FrontColor = style.IsDark ? style.AccentBackground : D(style.AccentBackground, 0.1f);
            
            // Slider
            theme.Slider.Box.BackColor = style.FieldColor;
            theme.Slider.Box.BorderColor = style.BorderColor;
            theme.Slider.Box.BorderWidth = style.BorderWidth;
            theme.Slider.Box.BorderRadius = style.BorderRadius;
            theme.Slider.Box.FrontColor = style.Foreground;
            theme.Slider.Handle.BorderRadius = Mathf.Max(0, style.BorderRadius - style.BorderWidth);

            theme.Slider.Handle.Normal.BackColor = style.IsDark ? D(style.Foreground, 0.4f) : L(style.Foreground, 3.0f); 
            theme.Slider.Handle.Hovered.BackColor = L(theme.Slider.Handle.Normal.BackColor, 0.2f);
            theme.Slider.Handle.Pressed.BackColor = D(theme.Slider.Handle.Normal.BackColor, 0.1f);
            
            // List
            theme.List.Box.BorderColor = style.BorderColor;
            theme.List.Box.BackColor = style.FieldColor;
            theme.List.Box.BorderRadius = style.BorderRadius;
            theme.List.Box.BorderWidth = style.BorderWidth;
            theme.List.Box.FrontColor = default;
            theme.List.Padding = style.Spacing;

            theme.List.ItemNormal.BorderThickness = 0.0f;
            theme.List.ItemNormal.BorderRadius = style.BorderRadius;
            theme.List.ItemNormal.Alignment = new ImAlignment(0.0f, 0.5f);
            
            theme.List.ItemNormal.Normal.BackColor = style.Foreground.WithAlpha(8);
            theme.List.ItemNormal.Normal.FrontColor = style.Foreground;
            theme.List.ItemNormal.Normal.BorderColor = default;

            theme.List.ItemNormal.Hovered.BackColor = style.Foreground.WithAlpha(24);
            theme.List.ItemNormal.Hovered.FrontColor = style.Foreground;
            theme.List.ItemNormal.Hovered.BorderColor = default;

            theme.List.ItemNormal.Pressed.BackColor = style.IsDark ? style.Foreground.WithAlpha(16) : style.Foreground.WithAlpha(48);
            theme.List.ItemNormal.Pressed.FrontColor = style.Foreground;
            theme.List.ItemNormal.Pressed.BorderColor = default;
            
            theme.List.ItemSelected.BorderThickness = 0.0f;
            theme.List.ItemSelected.BorderRadius = style.BorderRadius;
            theme.List.ItemSelected.Alignment = new ImAlignment(0.0f, 0.5f);

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
            theme.Foldout.Button.Normal.BackColor = Color.Lerp(theme.Window.Box.BackColor, theme.Button.Normal.BackColor, 0.5f);
            theme.Foldout.Button.Hovered.BackColor = Color.Lerp(theme.Window.Box.BackColor, theme.Button.Hovered.BackColor, 0.5f);
            theme.Foldout.Button.Pressed.BackColor = Color.Lerp(theme.Window.Box.BackColor, theme.Button.Pressed.BackColor, 0.5f);
            theme.Foldout.Button.BorderThickness = 0.0f;
            theme.Foldout.Button.Alignment = new ImAlignment(0.0f, 0.5f);
            
            // Tree
            theme.Tree.ArrowScale = 0.6f;
            theme.Tree.ItemNormal = theme.List.ItemNormal;
            theme.Tree.ItemNormal.Normal.BackColor = default;
            theme.Tree.ItemSelected = theme.List.ItemSelected;
            
            // Dropdown
            theme.Dropdown.ArrowScale = 0.6f;
            theme.Dropdown.MaxListHeight = 300.0f;
            theme.Dropdown.MinListWidth = 150.0f;
            theme.Dropdown.Button = theme.Button;
            theme.Dropdown.Button.Alignment = new ImAlignment(0.0f, 0.5f);
            
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
            theme.Radiobox.KnobScale = 0.6f;
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