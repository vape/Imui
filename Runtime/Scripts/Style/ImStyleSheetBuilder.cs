using UnityEngine;

namespace Imui.Style
{
    public static class ImStyleSheetBuilder
    {
        public static ImStyleSheet Build(ImTheme theme)
        {
            var isDark = GetBrightness(theme.Background) <= 128;

            var sheet = new ImStyleSheet() { Theme = theme };
            
            // Text
            sheet.Text.Color = theme.Foreground;
            sheet.Text.Alignment = new ImAlignment(0.0f, 0.0f);
            
            // Layout
            sheet.Layout.ExtraRowHeight = theme.ExtraRowHeight;
            sheet.Layout.Spacing = theme.Spacing;
            sheet.Layout.InnerSpacing = theme.InnerSpacing;
            sheet.Layout.TextSize = theme.TextSize;
            sheet.Layout.Indent = theme.Indent;
            sheet.Layout.ScrollSpeedScale = theme.ScrollSpeed;
            
            // Window
            sheet.Window.ContentPadding = theme.Spacing;
            
            sheet.Window.TitleBar.BackColor = D(theme.Background, 0.15f);
            sheet.Window.TitleBar.FrontColor = theme.Foreground;
            sheet.Window.TitleBar.Alignment = new ImAlignment(0.5f, 0.5f);

            sheet.Window.ResizeHandleColor = isDark ? D(theme.Foreground, 0.2f) : L(theme.Foreground, 0.3f);
            sheet.Window.ResizeHandleSize = Mathf.Max(theme.BorderRadius * 1.2f, theme.TextSize);
            
            sheet.Window.Box.BackColor = theme.Background;
            sheet.Window.Box.FrontColor = theme.Foreground;
            sheet.Window.Box.BorderRadius = theme.WindowBorderRadius;
            sheet.Window.Box.BorderThickness = theme.WindowBorderThickness;
            sheet.Window.Box.BorderColor = theme.BorderColor;

            // Button
            sheet.Button.Alignment = new ImAlignment(0.5f, 0.5f);
            sheet.Button.BorderRadius = theme.BorderRadius;
            sheet.Button.BorderThickness = theme.BorderThickness;
            
            sheet.Button.Normal.BackColor = theme.ButtonColor;
            sheet.Button.Normal.FrontColor = theme.Foreground;
            sheet.Button.Normal.BorderColor = theme.BorderColor;

            sheet.Button.Hovered.BackColor = isDark ? L(theme.ButtonColor, 0.3f) : L(theme.ButtonColor, 0.05f);
            sheet.Button.Hovered.FrontColor = L(theme.Foreground, 0.05f);
            sheet.Button.Hovered.BorderColor = L(theme.BorderColor, 0.05f);

            sheet.Button.Pressed.BackColor = isDark ? D(sheet.Button.Hovered.BackColor, 0.4f) : D(sheet.Button.Hovered.BackColor, 0.1f);
            sheet.Button.Pressed.FrontColor = D(theme.Foreground, 0.05f);
            sheet.Button.Pressed.BorderColor = D(theme.BorderColor, 0.05f);
            
            // Text Edit
            sheet.TextEdit.Normal.SelectionColor = theme.AccentBackground.WithAlpha((byte)(255 * 0.25f));;
            sheet.TextEdit.Normal.Box.BackColor = theme.FieldColor;
            sheet.TextEdit.Normal.Box.FrontColor = theme.Foreground;
            sheet.TextEdit.Normal.Box.BorderColor = theme.BorderColor;
            sheet.TextEdit.Normal.Box.BorderRadius = theme.BorderRadius;
            sheet.TextEdit.Normal.Box.BorderThickness = theme.BorderThickness;

            sheet.TextEdit.Selected.SelectionColor = sheet.TextEdit.Normal.SelectionColor;
            sheet.TextEdit.Selected.Box.BackColor = L(theme.FieldColor, 0.05f);
            sheet.TextEdit.Selected.Box.FrontColor = theme.Foreground;
            sheet.TextEdit.Selected.Box.BorderColor = theme.AccentBackground;
            sheet.TextEdit.Selected.Box.BorderRadius = theme.BorderRadius;
            sheet.TextEdit.Selected.Box.BorderThickness = theme.BorderThickness;

            sheet.TextEdit.CaretWidth = 2.0f;
            sheet.TextEdit.Alignment = new ImAlignment(0.0f, 0.0f);
            sheet.TextEdit.TextWrap = false;
            
            // Scroll
            sheet.Scroll.Size = (int)theme.ScrollBarSize;
            sheet.Scroll.BorderThickness = theme.BorderThickness;
            sheet.Scroll.BorderRadius = theme.BorderRadius;
            sheet.Scroll.VMargin = new ImPadding(theme.Spacing, 0, 0, 0);
            sheet.Scroll.HMargin = new ImPadding(0, 0, theme.Spacing, 0);

            sheet.Scroll.NormalState.BackColor = Color32.Lerp(theme.Background, theme.Foreground, 0.15f);
            sheet.Scroll.NormalState.FrontColor = Color32.Lerp(theme.Background, theme.Foreground, 0.25f);
            
            sheet.Scroll.HoveredState.BackColor = sheet.Scroll.NormalState.BackColor;
            sheet.Scroll.HoveredState.FrontColor = isDark ? L(sheet.Scroll.NormalState.FrontColor, 0.1f) : D(sheet.Scroll.NormalState.FrontColor, 0.1f);
            
            sheet.Scroll.PressedState.BackColor = sheet.Scroll.HoveredState.BackColor;
            sheet.Scroll.PressedState.FrontColor = isDark ? theme.AccentBackground : D(theme.AccentBackground, 0.1f);
            
            // Slider
            sheet.Slider.Box.BackColor = theme.FieldColor;
            sheet.Slider.Box.BorderColor = theme.BorderColor;
            sheet.Slider.Box.BorderThickness = theme.BorderThickness;
            sheet.Slider.Box.BorderRadius = theme.BorderRadius;
            sheet.Slider.Box.FrontColor = theme.Foreground;

            sheet.Slider.Handle.BorderThickness = 0;
            sheet.Slider.Handle.BorderRadius = Mathf.Max(0, theme.BorderRadius - theme.BorderThickness);
            
            sheet.Slider.Handle.Normal.BackColor = isDark ? D(theme.Foreground, 0.4f) : L(theme.Foreground, 3.0f); 
            sheet.Slider.Handle.Hovered.BackColor = L(sheet.Slider.Handle.Normal.BackColor, 0.2f);
            sheet.Slider.Handle.Pressed.BackColor = D(sheet.Slider.Handle.Normal.BackColor, 0.1f);
            
            // List
            sheet.List.Box.BorderColor = theme.BorderColor;
            sheet.List.Box.BackColor = theme.FieldColor;
            sheet.List.Box.BorderRadius = theme.BorderRadius;
            sheet.List.Box.BorderThickness = theme.BorderThickness;
            sheet.List.Box.FrontColor = default;
            sheet.List.Padding = theme.Spacing;

            sheet.List.ItemNormal.BorderThickness = 0.0f;
            sheet.List.ItemNormal.BorderRadius = theme.BorderRadius;
            sheet.List.ItemNormal.Alignment = new ImAlignment(0.0f, 0.5f);
            
            sheet.List.ItemNormal.Normal.BackColor = theme.Foreground.WithAlpha(8);
            sheet.List.ItemNormal.Normal.FrontColor = theme.Foreground;
            sheet.List.ItemNormal.Normal.BorderColor = default;

            sheet.List.ItemNormal.Hovered.BackColor = theme.Foreground.WithAlpha(24);
            sheet.List.ItemNormal.Hovered.FrontColor = theme.Foreground;
            sheet.List.ItemNormal.Hovered.BorderColor = default;

            sheet.List.ItemNormal.Pressed.BackColor = isDark ? theme.Foreground.WithAlpha(16) : theme.Foreground.WithAlpha(48);
            sheet.List.ItemNormal.Pressed.FrontColor = theme.Foreground;
            sheet.List.ItemNormal.Pressed.BorderColor = default;
            
            sheet.List.ItemSelected.BorderThickness = 0.0f;
            sheet.List.ItemSelected.BorderRadius = theme.BorderRadius;
            sheet.List.ItemSelected.Alignment = new ImAlignment(0.0f, 0.5f);

            sheet.List.ItemSelected.Normal.BackColor = theme.AccentBackground;
            sheet.List.ItemSelected.Normal.FrontColor = theme.AccentForeground;
            sheet.List.ItemSelected.Normal.BorderColor = default;

            sheet.List.ItemSelected.Hovered.BackColor = L(theme.AccentBackground, 0.1f);
            sheet.List.ItemSelected.Hovered.FrontColor = theme.AccentForeground;
            sheet.List.ItemSelected.Hovered.BorderColor = default;

            sheet.List.ItemSelected.Pressed.BackColor = D(theme.AccentBackground, 0.05f);
            sheet.List.ItemSelected.Pressed.FrontColor = theme.AccentForeground;
            sheet.List.ItemSelected.Pressed.BorderColor = default;
            
            // Foldout
            sheet.Foldout.ArrowScale = 0.6f;
            sheet.Foldout.Button = sheet.Button;
            sheet.Foldout.Button.Normal.BackColor = Color.Lerp(sheet.Window.Box.BackColor, sheet.Button.Normal.BackColor, 0.5f);
            sheet.Foldout.Button.Hovered.BackColor = Color.Lerp(sheet.Window.Box.BackColor, sheet.Button.Hovered.BackColor, 0.5f);
            sheet.Foldout.Button.Pressed.BackColor = Color.Lerp(sheet.Window.Box.BackColor, sheet.Button.Pressed.BackColor, 0.5f);
            sheet.Foldout.Button.BorderThickness = 0.0f;
            sheet.Foldout.Button.Alignment = new ImAlignment(0.0f, 0.5f);
            
            // Tree
            sheet.Tree.ArrowScale = 0.6f;
            sheet.Tree.ItemNormal = sheet.List.ItemNormal;
            sheet.Tree.ItemNormal.Normal.BackColor = default;
            sheet.Tree.ItemSelected = sheet.List.ItemSelected;
            
            // Dropdown
            sheet.Dropdown.ArrowScale = 0.6f;
            sheet.Dropdown.MaxListHeight = 300.0f;
            sheet.Dropdown.MinListWidth = 150.0f;
            sheet.Dropdown.Button = sheet.Button;
            sheet.Dropdown.Button.Alignment = new ImAlignment(0.0f, 0.5f);
            
            // Checkbox
            sheet.Checkbox.Normal = sheet.Button;
            sheet.Checkbox.CheckmarkScale = 0.6f;
            
            sheet.Checkbox.Checked = sheet.Button;
            sheet.Checkbox.Checked.Normal.BackColor = theme.AccentBackground;
            sheet.Checkbox.Checked.Normal.FrontColor = theme.AccentForeground;
            sheet.Checkbox.Checked.Normal.BorderColor = D(theme.AccentBackground, 0.1f);

            sheet.Checkbox.Checked.Hovered.BackColor = L(theme.AccentBackground, 0.1f);
            sheet.Checkbox.Checked.Hovered.FrontColor = theme.AccentForeground;
            sheet.Checkbox.Checked.Hovered.BorderColor = D(theme.AccentBackground, 0.05f);

            sheet.Checkbox.Checked.Pressed.BackColor = D(theme.AccentBackground, 0.05f);
            sheet.Checkbox.Checked.Pressed.FrontColor = theme.AccentForeground;
            sheet.Checkbox.Checked.Pressed.BorderColor = D(theme.AccentBackground, 0.15f);
            
            // Radiobox
            sheet.Radiobox.KnobScale = 0.5f;
            
            sheet.Radiobox.Normal = sheet.Checkbox.Normal;
            sheet.Radiobox.Normal.BorderRadius = 999.9f;
            sheet.Radiobox.Checked = sheet.Checkbox.Checked;
            sheet.Radiobox.Checked.BorderRadius = 999.9f;
            
            // Separator
            sheet.Separator.Thickness = Mathf.Max(1, theme.TextSize * 0.15f);
            sheet.Separator.Color = theme.BorderColor;
            
            return sheet;
        }

        private static Color32 D(Color32 color, float value) => ScaleValue(color, 1.0f - value);
        private static Color32 L(Color32 color, float value) => ScaleValue(color, 1.0f + value);
                
        private static Color32 ScaleValue(Color32 color, float scale)
        {
            Color.RGBToHSV(color, out var h, out var s, out var v);
            v = Mathf.Clamp01(v * scale);
            var result = (Color32)Color.HSVToRGB(h, s, v);
            result.a = color.a;
            return result;
        }

        public static byte GetBrightness(Color32 color)
        {
            return (byte)(0.2125 * color.r + 0.7152 * color.g + 0.0722 * color.b);
        }
    }
}