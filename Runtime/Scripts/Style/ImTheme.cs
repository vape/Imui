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
        public float ScrollBarSize;
        public float WindowBorderRadius;
        public float WindowBorderThickness;
        public float BorderRadius;
        public float BorderThickness;
        public float ReadOnlyColorMultiplier;

        public Color Background;
        public Color Foreground;
        public Color Control;
        public Color Accent;
        public float Variance;
    }

    public static class ImThemeBuiltin
    {
        public static ImTheme Light()
        {
            return new ImTheme
            {
                TextSize = 20f,
                Spacing = 3f,
                InnerSpacing = 5f,
                Indent = 12f,
                ExtraRowHeight = 4f,
                ScrollBarSize = 13f,
                WindowBorderRadius = 8f,
                WindowBorderThickness = 1f,
                BorderRadius = 5f,
                BorderThickness = 1f,
                ReadOnlyColorMultiplier = 0.9f,
                Background = new Color32(241, 241, 241, 255),
                Foreground = new Color32(30, 30, 30, 255),
                Accent = new Color32(0, 120, 202, 255),
                Control = new Color32(0, 0, 0, 0),
                Variance = 0.05f
            };
        }

        public static ImTheme Dark()
        {
            return new ImTheme
            {
                TextSize = 20f,
                Spacing = 3f,
                InnerSpacing = 5f,
                Indent = 12f,
                ExtraRowHeight = 4f,
                ScrollBarSize = 13f,
                WindowBorderRadius = 8f,
                WindowBorderThickness = 1f,
                BorderRadius = 5f,
                BorderThickness = 1f,
                ReadOnlyColorMultiplier = 0.7f,
                Background = new Color32(58, 58, 58, 255),
                Foreground = new Color32(224, 224, 224, 255),
                Accent = new Color32(0, 125, 219, 255),
                Control = new Color32(255, 255, 255, 8),
                Variance = 0.15f,
            };
        }

        public static ImTheme Dear()
        {
            return new ImTheme
            {
                TextSize = 20f,
                Spacing = 3f,
                InnerSpacing = 3f,
                Indent = 8f,
                ExtraRowHeight = 4f,
                ScrollBarSize = 12f,
                WindowBorderRadius = 0f,
                WindowBorderThickness = 1f,
                BorderRadius = 0f,
                BorderThickness = 0f,
                ReadOnlyColorMultiplier = 0.7f,
                Background = new Color32(10, 10, 10, 242),
                Foreground = new Color32(255, 255, 255, 255),
                Accent = new Color32(89, 148, 243, 255),
                Control = new Color32(75, 114, 200, 118),
                Variance = 0.2f,
            };
        }

        public static ImTheme Orange()
        {
            return new ImTheme
            {
                TextSize = 20f,
                Spacing = 3f,
                InnerSpacing = 5f,
                Indent = 12f,
                ExtraRowHeight = 4f,
                ScrollBarSize = 13f,
                WindowBorderRadius = 8f,
                WindowBorderThickness = 1f,
                BorderRadius = 5f,
                BorderThickness = 1f,
                ReadOnlyColorMultiplier = 0.7f,
                Background = new Color32(17, 18, 18, 245),
                Foreground = new Color32(224, 224, 224, 255),
                Accent = new Color32(211, 85, 12, 255),
                Control = new Color32(0, 121, 255, 11),
                Variance = 0.22f,
            };
        }

        public static ImTheme Terminal()
        {
            return new ImTheme
            {
                TextSize = 18f,
                Spacing = 1f,
                InnerSpacing = 2f,
                Indent = 8f,
                ExtraRowHeight = 0f,
                ScrollBarSize = 15f,
                WindowBorderRadius = 0f,
                WindowBorderThickness = 1f,
                BorderRadius = 0f,
                BorderThickness = 1f,
                ReadOnlyColorMultiplier = 0.7f,
                Background = new Color32(0, 0, 0, 240),
                Foreground = new Color32(18, 255, 0, 255),
                Accent = new Color32(52, 224, 0, 255),
                Control = new Color32(22, 78, 0, 255),
                Variance = 0.2f,
            };
        }
    }
}