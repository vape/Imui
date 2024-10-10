using UnityEngine;

namespace Imui.Style.Themes
{
    public static class ImLightTheme
    {
        public static ImStyleSheet Create()
        {
            var style = new ImTheme();
            style.IsDark = false;
            style.TextSize = 22f;
            style.Spacing = 4f;
            style.InnerSpacing = 4f;
            style.Indent = 8f;
            style.ExtraRowSize = 8f;
            style.ScrollSpeed = 2f;
            style.ScrollBarSize = 22f;
            style.WindowBorderRadius = 12f;
            style.WindowBorderThickness = 1f;
            style.BorderRadius = 6f;
            style.BorderWidth = 1f;
            style.ReadOnlyColorMultiplier = 0.9f;
            style.Background = new Color32(241, 241, 241, 255);
            style.Foreground = new Color32(30, 30, 30, 255);
            style.AccentBackground = new Color32(0, 120, 202, 255);
            style.AccentForeground = new Color32(241, 241, 241, 255);
            style.BorderColor = new Color32(179, 179, 179, 255);
            style.ButtonColor = new Color32(222, 222, 222, 255);
            style.FieldColor = new Color32(214, 214, 214, 255);
            return ImStyleSheetBuilder.Build(style);
        }
    }
}