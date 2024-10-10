using UnityEngine;

namespace Imui.Style.Themes
{
    public static class ImDarkTheme
    {
        public static ImTheme Create()
        {
            var style = new ImStyle();
            style.IsDark = true;
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
            style.ReadOnlyColorMultiplier = 0.7f;
            style.Background = new Color32(64, 64, 64, 255);
            style.Foreground = new Color32(217, 217, 217, 255);
            style.AccentBackground = new Color32(0, 116, 204, 255);
            style.AccentForeground = new Color32(230, 230, 230, 255);
            style.BorderColor = new Color32(36, 36, 36, 255);
            style.ButtonColor = new Color32(75, 75, 75, 255);
            style.FieldColor = new Color32(48, 48, 48, 255);
            return ImStyleBuilder.Build(style);
        }
    }
}