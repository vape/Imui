using System;
using UnityEngine;

namespace Imui.Style
{
    [Serializable]
    public struct ImTheme
    {
        public float TextSize;
        public float Spacing;
        public float InnerSpacing;
        public float Indent;
        public float ExtraRowHeight;
        // TODO (artem-s): this does not belong here
        public float ScrollSpeed;
        public float ScrollBarSize;

        public float WindowBorderRadius;
        public float WindowBorderThickness;
        
        public float BorderRadius;
        public float BorderThickness;

        public float ReadOnlyColorMultiplier;
        
        public Color32 Background;
        public Color32 Foreground;

        public Color32 AccentBackground;
        public Color32 AccentForeground;
        
        public Color32 BorderColor;
        public Color32 ButtonColor;
        public Color32 FieldColor;
    }

    public static class ImThemeBuiltin
    {
        public static ImTheme Light()
        {
            return new ImTheme { 
                TextSize = 22f, 
                Spacing = 4f,
                InnerSpacing = 4f,
                Indent = 8f,
                ExtraRowHeight = 8f,
                ScrollSpeed = 2f,
                ScrollBarSize = 22f,
                WindowBorderRadius = 12f,
                WindowBorderThickness = 1f,
                BorderRadius = 6f,
                BorderThickness = 1f,
                ReadOnlyColorMultiplier = 0.9f,
                Background = new Color32(241, 241, 241, 255),
                Foreground = new Color32(30, 30, 30, 255),
                AccentBackground = new Color32(0, 120, 202, 255),
                AccentForeground = new Color32(241, 241, 241, 255),
                BorderColor = new Color32(179, 179, 179, 255),
                ButtonColor = new Color32(222, 222, 222, 255),
                FieldColor = new Color32(214, 214, 214, 255)
            };
        }
        
        public static ImTheme Dark()
        {
            return new ImTheme { 
                TextSize = 22f, 
                Spacing = 4f,
                InnerSpacing = 4f,
                Indent = 8f,
                ExtraRowHeight = 8f,
                ScrollSpeed = 2f,
                ScrollBarSize = 22f,
                WindowBorderRadius = 12f,
                WindowBorderThickness = 1f,
                BorderRadius = 6f,
                BorderThickness = 1f,
                ReadOnlyColorMultiplier = 0.7f,
                Background = new Color32(64, 64, 64, 255),
                Foreground = new Color32(217, 217, 217, 255),
                AccentBackground = new Color32(0, 116, 204, 255),
                AccentForeground = new Color32(230, 230, 230, 255),
                BorderColor = new Color32(36, 36, 36, 255),
                ButtonColor = new Color32(75, 75, 75, 255),
                FieldColor = new Color32(48, 48, 48, 255)
            };
        }
    }
}