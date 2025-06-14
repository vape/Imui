using Imui.Rendering;
using UnityEngine;

namespace Imui.Style
{
    public static class ImStyleSheetBuilder
    {
        public static ImPalette GetPalette(ImTheme theme)
        {
            float GetBrightness(Color color)
            {
                return 0.2125f * color.r + 0.7152f * color.g + 0.0722f * color.b;
            }

            Color ChangeBrightness(Color col, float delta)
            {
                Color.RGBToHSV(col, out var h, out var s, out var v);
                v *= 1 + delta;
                return Color.HSVToRGB(h, s, v).WithAlpha(col.a);
            }

            Color Lerp(Color c0, Color c1, float value, float alpha = 1.0f) => Color.Lerp(c0, c1, value).WithAlpha(alpha);
            Color BlendAccent(Color baseColor, Color accent) => Color.Lerp(baseColor, accent.WithAlpha(1.0f), accent.a).WithAlpha(1.0f);

            var variance = theme.Variance;
            var borderVariance = theme.Variance * 2.3f;

            var palette = new ImPalette();
            var accentDark = GetBrightness(theme.Accent) < 0.5f;
            var bgBrighter = GetBrightness(theme.Background) > GetBrightness(theme.Foreground);
            var white = bgBrighter ? theme.Background : theme.Foreground;
            var black = bgBrighter ? theme.Foreground : theme.Background;

            palette.BackPrimary = theme.Background;
            palette.BackSecondary = ChangeBrightness(theme.Background, -variance * 0.5f);

            palette.FrontDarker = ChangeBrightness(theme.Foreground, -variance);
            palette.FrontLighter = ChangeBrightness(theme.Foreground, +variance);
            palette.Front = theme.Foreground;

            palette.Accent = theme.Accent;
            palette.AccentFront = accentDark ? white : black;
            palette.AccentDarker = ChangeBrightness(theme.Accent, -variance);
            palette.AccentLighter = ChangeBrightness(theme.Accent, +variance);

            palette.Control = BlendAccent(Lerp(theme.Background, theme.Foreground, 0.1f), theme.Control);
            palette.ControlBorder = ChangeBrightness(palette.Control, -borderVariance);

            palette.ControlAlt = ChangeBrightness(palette.Control, -0.1f);
            palette.ControlAltBorder = ChangeBrightness(palette.ControlAlt, -borderVariance);

            palette.ControlDark = ChangeBrightness(palette.Control, -variance);
            palette.ControlDarkBorder = ChangeBrightness(palette.ControlDark, -borderVariance);

            palette.ControlLight = ChangeBrightness(palette.Control, +variance);
            palette.ControlLightBorder = ChangeBrightness(palette.ControlLight, -borderVariance);

            palette.FrontAlt = Lerp(palette.Front, palette.Control, 0.4f);

            return palette;
        }

        public static ImStyleSheet Build(ImTheme theme)
        {
            return Build(theme, GetPalette(theme));
        }

        public static ImStyleSheet Build(ImTheme theme, ImPalette palette)
        {
            var sheet = new ImStyleSheet() { Theme = theme, Palette = palette };

            // Text
            sheet.Text.Color = palette.Front;

            // Layout
            sheet.Layout.ExtraRowHeight = theme.ExtraRowHeight;
            sheet.Layout.Spacing = theme.Spacing;
            sheet.Layout.InnerSpacing = theme.InnerSpacing;
            sheet.Layout.TextSize = theme.TextSize;
            sheet.Layout.Indent = theme.Indent;

            // Button
            sheet.Button.Alignment = new ImAlignment(0.5f, 0.5f);
            sheet.Button.BorderRadius = theme.BorderRadius;
            sheet.Button.BorderThickness = theme.BorderThickness;

            sheet.Button.Normal.BackColor = palette.Control;
            sheet.Button.Normal.FrontColor = palette.Front;
            sheet.Button.Normal.BorderColor = palette.ControlBorder;

            sheet.Button.Hovered.BackColor = palette.ControlLight;
            sheet.Button.Hovered.FrontColor = palette.FrontLighter;
            sheet.Button.Hovered.BorderColor = palette.ControlLightBorder;

            sheet.Button.Pressed.BackColor = palette.ControlDark;
            sheet.Button.Pressed.FrontColor = palette.FrontDarker;
            sheet.Button.Pressed.BorderColor = palette.ControlDarkBorder;

            // Window
            sheet.Window.ContentPadding = theme.Spacing;

            sheet.Window.ResizeHandleNormalColor = palette.ControlBorder;
            sheet.Window.ResizeHandleActiveColor = palette.Accent;
            sheet.Window.ResizeHandleSize = Mathf.Max(theme.BorderRadius * 2.0f, (theme.TextSize + theme.ExtraRowHeight) * 1.25f);

            sheet.Window.Box.BackColor = palette.BackPrimary;
            sheet.Window.Box.FrontColor = palette.Front;
            sheet.Window.Box.BorderRadius = theme.WindowBorderRadius;
            sheet.Window.Box.BorderThickness = theme.WindowBorderThickness;
            sheet.Window.Box.BorderColor = palette.ControlBorder;

            sheet.Window.TitleBar.BackColor = palette.BackSecondary.WithAlpha(1.0f);
            sheet.Window.TitleBar.FrontColor = palette.Front;
            sheet.Window.TitleBar.Alignment = new ImAlignment(0.5f, 0.5f);
            sheet.Window.TitleBar.Overflow = ImTextOverflow.Ellipsis;

            sheet.Window.TitleBar.CloseButton = sheet.Button;
            sheet.Window.TitleBar.CloseButton.BorderRadius = 999.9f;

            sheet.Window.TitleBar.CloseButton.Hovered.BackColor = palette.Accent;
            sheet.Window.TitleBar.CloseButton.Hovered.FrontColor = palette.AccentFront;
            sheet.Window.TitleBar.CloseButton.Hovered.BorderColor = sheet.Window.Box.BorderColor;

            sheet.Window.TitleBar.CloseButton.Pressed.BackColor = palette.AccentDarker;
            sheet.Window.TitleBar.CloseButton.Pressed.FrontColor = palette.AccentFront;
            sheet.Window.TitleBar.CloseButton.Pressed.BorderColor = sheet.Window.Box.BorderColor;

            // Text Edit
            sheet.TextEdit.Normal.SelectionColor = palette.Accent.WithAlpha(0.25f);
            sheet.TextEdit.Normal.Box.BackColor = palette.ControlAlt;
            sheet.TextEdit.Normal.Box.FrontColor = palette.Front;
            sheet.TextEdit.Normal.Box.BorderColor = palette.ControlAltBorder;
            sheet.TextEdit.Normal.Box.BorderRadius = theme.BorderRadius;
            sheet.TextEdit.Normal.Box.BorderThickness = theme.BorderThickness;

            sheet.TextEdit.Selected.SelectionColor = sheet.TextEdit.Normal.SelectionColor;
            sheet.TextEdit.Selected.Box.BackColor = palette.Control;
            sheet.TextEdit.Selected.Box.FrontColor = palette.Front;
            sheet.TextEdit.Selected.Box.BorderColor = palette.Accent;
            sheet.TextEdit.Selected.Box.BorderRadius = theme.BorderRadius;
            sheet.TextEdit.Selected.Box.BorderThickness = theme.BorderThickness;

            sheet.TextEdit.CaretWidth = 2.0f;
            sheet.TextEdit.Alignment = new ImAlignment(0.0f, 0.0f);
            sheet.TextEdit.TextWrap = false;

            // Scroll
            sheet.Scroll.Size = (int)theme.ScrollBarSize;
            sheet.Scroll.BorderThickness = Mathf.Max(1.0f, theme.BorderThickness);
            sheet.Scroll.BorderRadius = theme.BorderRadius;
            sheet.Scroll.VMargin = new ImPadding(theme.Spacing, 0, 0, 0);
            sheet.Scroll.HMargin = new ImPadding(0, 0, theme.Spacing, 0);

            sheet.Scroll.NormalState.BackColor = palette.ControlDarkBorder;
            sheet.Scroll.NormalState.FrontColor = palette.ControlDark;

            sheet.Scroll.HoveredState.BackColor = palette.ControlDarkBorder;
            sheet.Scroll.HoveredState.FrontColor = palette.Accent;

            sheet.Scroll.PressedState.BackColor = palette.ControlDarkBorder;
            sheet.Scroll.PressedState.FrontColor = palette.AccentLighter;

            // Checkbox
            sheet.Checkbox.Normal = sheet.Button;
            sheet.Checkbox.CheckmarkScale = 0.6f;

            sheet.Checkbox.Checked = sheet.Button;
            sheet.Checkbox.Checked.Normal.BackColor = palette.Accent;
            sheet.Checkbox.Checked.Normal.FrontColor = palette.AccentFront;
            sheet.Checkbox.Checked.Normal.BorderColor = palette.AccentDarker;

            sheet.Checkbox.Checked.Hovered.BackColor = palette.AccentLighter;
            sheet.Checkbox.Checked.Hovered.FrontColor = palette.AccentFront;
            sheet.Checkbox.Checked.Hovered.BorderColor = palette.AccentDarker;

            sheet.Checkbox.Checked.Pressed.BackColor = palette.AccentDarker;
            sheet.Checkbox.Checked.Pressed.FrontColor = palette.AccentFront;
            sheet.Checkbox.Checked.Pressed.BorderColor = palette.AccentDarker;

            // Radiobox
            sheet.Radiobox.KnobScale = 0.5f;

            sheet.Radiobox.Normal = sheet.Checkbox.Normal;
            sheet.Radiobox.Normal.BorderRadius = 999.9f;
            sheet.Radiobox.Checked = sheet.Checkbox.Checked;
            sheet.Radiobox.Checked.BorderRadius = 999.9f;

            // Slider
            var sliderRadius = theme.BorderRadius;

            sheet.Slider.BarThickness = 0.45f;
            sheet.Slider.TextOverflow = ImTextOverflow.Ellipsis;
            sheet.Slider.HeaderScale = 0.75f;

            sheet.Slider.Normal.BackColor = palette.ControlDark;
            sheet.Slider.Normal.BorderColor = palette.ControlDarkBorder;
            sheet.Slider.Normal.BorderThickness = theme.BorderThickness;
            sheet.Slider.Normal.BorderRadius = sliderRadius;
            sheet.Slider.Normal.FrontColor = palette.Front;

            sheet.Slider.Selected.BackColor = palette.ControlDark;
            sheet.Slider.Selected.BorderColor = palette.Accent;
            sheet.Slider.Selected.BorderThickness = theme.BorderThickness;
            sheet.Slider.Selected.BorderRadius = sliderRadius;
            sheet.Slider.Selected.FrontColor = palette.Front;

            sheet.Slider.Fill = sheet.Slider.Normal;
            sheet.Slider.Fill.BackColor = palette.Accent;
            sheet.Slider.Fill.BorderColor = palette.AccentDarker;

            sheet.Slider.Handle.BorderThickness = theme.BorderThickness;
            sheet.Slider.Handle.BorderRadius = theme.BorderRadius >= 1.0f ? 999.9f : 0.0f;
            sheet.Slider.HandleThickness = 1.0f;

            sheet.Slider.Handle.Normal.BackColor = palette.ControlLight;
            sheet.Slider.Handle.Normal.BorderColor = palette.ControlBorder;

            sheet.Slider.Handle.Hovered = sheet.Slider.Handle.Normal;
            sheet.Slider.Handle.Hovered.BorderColor = palette.Accent;

            sheet.Slider.Handle.Pressed = sheet.Radiobox.Checked.Pressed;

            // List
            sheet.List.Box.BorderColor = palette.ControlDarkBorder;
            sheet.List.Box.BackColor = palette.ControlDark;
            sheet.List.Box.BorderRadius = theme.BorderRadius;
            sheet.List.Box.BorderThickness = theme.BorderThickness;
            sheet.List.Box.FrontColor = default;
            sheet.List.Padding = theme.Spacing;

            sheet.List.ItemNormal.BorderThickness = 0.0f;
            sheet.List.ItemNormal.BorderRadius = theme.BorderRadius;
            sheet.List.ItemNormal.Alignment = new ImAlignment(0.0f, 0.5f);

            sheet.List.ItemNormal.Normal.BackColor = palette.Front.WithAlpha(0.03f);
            sheet.List.ItemNormal.Normal.FrontColor = palette.Front;
            sheet.List.ItemNormal.Normal.BorderColor = default;

            sheet.List.ItemNormal.Hovered.BackColor = palette.Front.WithAlpha(0.094f);
            sheet.List.ItemNormal.Hovered.FrontColor = palette.Front;
            sheet.List.ItemNormal.Hovered.BorderColor = default;

            sheet.List.ItemNormal.Pressed.BackColor = palette.Front.WithAlpha(0.063f);
            sheet.List.ItemNormal.Pressed.FrontColor = palette.Front;
            sheet.List.ItemNormal.Pressed.BorderColor = default;

            sheet.List.ItemSelected.BorderThickness = 0.0f;
            sheet.List.ItemSelected.BorderRadius = theme.BorderRadius;
            sheet.List.ItemSelected.Alignment = new ImAlignment(0.0f, 0.5f);

            sheet.List.ItemSelected.Normal.BackColor = palette.Accent;
            sheet.List.ItemSelected.Normal.FrontColor = palette.AccentFront;
            sheet.List.ItemSelected.Normal.BorderColor = default;

            sheet.List.ItemSelected.Hovered.BackColor = palette.AccentDarker;
            sheet.List.ItemSelected.Hovered.FrontColor = palette.AccentFront;
            sheet.List.ItemSelected.Hovered.BorderColor = default;

            sheet.List.ItemSelected.Pressed.BackColor = palette.AccentLighter;
            sheet.List.ItemSelected.Pressed.FrontColor = palette.AccentFront;
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
            sheet.Dropdown.Button = sheet.Button;
            sheet.Dropdown.Button.Alignment = new ImAlignment(0.0f, 0.5f);
            sheet.Dropdown.Button.Overflow = ImTextOverflow.Ellipsis;

            // Separator
            sheet.Separator.Thickness = Mathf.Max(1, theme.BorderThickness);
            sheet.Separator.Color = palette.FrontAlt;
            sheet.Separator.TextColor = palette.FrontAlt;
            sheet.Separator.TextAlignment = new ImAlignment(0.1f, 0.5f);
            sheet.Separator.TextOverflow = ImTextOverflow.Ellipsis;
            sheet.Separator.TextMargin = new ImPadding(theme.Spacing, theme.Spacing, 0, 0);

            // Tooltip
            sheet.Tooltip.Offset = new Vector2(10, 10);
            sheet.Tooltip.Padding = theme.InnerSpacing;
            sheet.Tooltip.Box.BackColor = palette.BackSecondary;
            sheet.Tooltip.Box.BorderColor = palette.ControlBorder;
            sheet.Tooltip.Box.BorderRadius = theme.BorderRadius;
            sheet.Tooltip.Box.BorderThickness = theme.BorderThickness;
            sheet.Tooltip.Box.FrontColor = palette.Front;

            // Menu
            sheet.Menu.Box = sheet.List.Box;
            sheet.Menu.Box.BackColor = palette.BackSecondary;
            sheet.Menu.Padding = sheet.List.Padding;
            sheet.Menu.ItemNormal = sheet.List.ItemNormal;
            sheet.Menu.ItemNormal.Normal.BackColor = Color.clear;
            sheet.Menu.ItemActive = sheet.List.ItemSelected;
            sheet.Menu.ItemActive.Normal.BackColor.SetAlpha(0.8f);
            sheet.Menu.ArrowScale = 0.6f;
            sheet.Menu.CheckmarkScale = 0.6f;
            sheet.Menu.MinWidth = 50.0f;
            sheet.Menu.MinHeight = 10.0f;

            // Menu Bar
            sheet.MenuBar.ItemExtraWidth = theme.InnerSpacing * 6.0f;
            sheet.MenuBar.Box = sheet.Menu.Box;
            sheet.MenuBar.Box.BackColor = palette.Control;
            sheet.MenuBar.Box.BorderRadius = 0.0f;
            sheet.MenuBar.Box.BorderThickness = sheet.Window.Box.BorderThickness;
            sheet.MenuBar.Box.BorderColor = sheet.Window.Box.BorderColor;
            sheet.MenuBar.ItemNormal = sheet.Menu.ItemNormal;
            sheet.MenuBar.ItemNormal.Alignment = new ImAlignment(0.5f, 0.5f);
            sheet.MenuBar.ItemNormal.BorderRadius = 0.0f;
            sheet.MenuBar.ItemNormal.BorderThickness = 0.0f;
            sheet.MenuBar.ItemActive = sheet.Menu.ItemActive;
            sheet.MenuBar.ItemActive.BorderRadius = 0.0f;
            sheet.MenuBar.ItemActive.BorderThickness = 0.0f;
            sheet.MenuBar.ItemActive.Alignment = new ImAlignment(0.5f, 0.5f);
            sheet.MenuBar.ItemActive.Normal.BackColor.SetAlpha(1.0f);

            // Color Picker
            sheet.ColorPicker.BorderColor = palette.ControlBorder;
            sheet.ColorPicker.BorderThickness = theme.BorderThickness;
            sheet.ColorPicker.PreviewCircleScale = 0.5f;

            // Tab
            sheet.Tabs.IndicatorColor = palette.Accent;
            sheet.Tabs.Normal = sheet.Button;
            sheet.Tabs.Selected = sheet.Button;
            sheet.Tabs.Selected.Normal.BackColor = palette.BackSecondary;
            sheet.Tabs.Selected.Hovered.BackColor = sheet.Tabs.Selected.Normal.BackColor;

            sheet.Tabs.ContainerBox.BackColor = sheet.Tabs.Selected.Normal.BackColor;
            sheet.Tabs.ContainerBox.BorderColor = sheet.Tabs.Selected.Normal.BorderColor;
            sheet.Tabs.ContainerBox.BorderRadius = sheet.Tabs.Selected.BorderRadius;
            sheet.Tabs.ContainerBox.BorderRadius.TopLeft = 0;
            sheet.Tabs.ContainerBox.BorderRadius.TopRight = 0;
            sheet.Tabs.ContainerBox.BorderThickness = sheet.Tabs.Selected.BorderThickness;

            // Table
            sheet.Table.CellPadding = theme.InnerSpacing;
            sheet.Table.BorderColor = sheet.Separator.Color;
            sheet.Table.SelectedColumnColor = palette.Accent;
            sheet.Table.BorderThickness = sheet.Separator.Thickness;
            sheet.Table.SelectedColumnThickness = sheet.Table.BorderThickness * 2;

            return sheet;
        }
    }
}