using Color = UnityEngine.Color;
using Color32 = UnityEngine.Color32;

namespace Imui.Style
{
    [System.Serializable]
    public struct ImTheme : System.IEquatable<ImTheme>
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

        #region Equality
        public bool Equals(ImTheme other) => Equals(this, in other);
        public override bool Equals(object obj) => obj is ImTheme other && Equals(this, other);
        public static bool operator ==(in ImTheme left, in ImTheme right) => Equals(left, right);
        public static bool operator !=(in ImTheme left, in ImTheme right) => !Equals(left, right);

        /// <summary>
        /// This method exists to allow equality operations to proceed without copying this massive struct every time
        /// </summary>
        /// <returns>True if both are equal</returns>
        /// <remarks>Should this just be a GetHashCode call?</remarks>
        [System.Diagnostics.Contracts.Pure]
        private static bool Equals(in ImTheme first, in ImTheme second)
        {
            return first.TextSize.Equals(second.TextSize) && 
                   first.Spacing.Equals(second.Spacing) &&
                   first.InnerSpacing.Equals(second.InnerSpacing) && 
                   first.Indent.Equals(second.Indent) &&
                   first.ExtraRowHeight.Equals(second.ExtraRowHeight) &&
                   first.ScrollBarSize.Equals(second.ScrollBarSize) &&
                   first.WindowBorderRadius.Equals(second.WindowBorderRadius) &&
                   first.WindowBorderThickness.Equals(second.WindowBorderThickness) &&
                   first.BorderRadius.Equals(second.BorderRadius) &&
                   first.BorderThickness.Equals(second.BorderThickness) &&
                   first.ReadOnlyColorMultiplier.Equals(second.ReadOnlyColorMultiplier) &&
                   first.Background.Equals(second.Background) && 
                   first.Foreground.Equals(second.Foreground) &&
                   first.Control.Equals(second.Control) && 
                   first.Accent.Equals(second.Accent) &&
                   first.Variance.Equals(second.Variance);
        }


        public override int GetHashCode()
        {
            var hashCode = new System.HashCode();
            hashCode.Add(TextSize);
            hashCode.Add(Spacing);
            hashCode.Add(InnerSpacing);
            hashCode.Add(Indent);
            hashCode.Add(ExtraRowHeight);
            hashCode.Add(ScrollBarSize);
            hashCode.Add(WindowBorderRadius);
            hashCode.Add(WindowBorderThickness);
            hashCode.Add(BorderRadius);
            hashCode.Add(BorderThickness);
            hashCode.Add(ReadOnlyColorMultiplier);
            hashCode.Add(Background);
            hashCode.Add(Foreground);
            hashCode.Add(Control);
            hashCode.Add(Accent);
            hashCode.Add(Variance);
            return hashCode.ToHashCode();
        }

        #endregion Equality
    }

    public static class ImThemeBuiltin
    {
        public static ImTheme LightTouch() => Light().EnlargedForTouch();

        public static ImTheme DarkTouch() => Dark().EnlargedForTouch();
        
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
                Variance = 0.18f,
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
        
        public static ImTheme EnlargedForTouch(this ImTheme theme)
        {
            const float textSize = 23/20f;
            const float spacing = 5f/3f;
            const float innerSpacing = 6.5f/5f;
            const float extraRowHeight = 11f/4f;
            
            theme.TextSize *= textSize;
            theme.Spacing *= spacing;
            theme.InnerSpacing *= innerSpacing;
            theme.ExtraRowHeight *= extraRowHeight;
            return theme;
        }
    }
}