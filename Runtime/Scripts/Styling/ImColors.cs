using UnityEngine;

namespace Imui.Styling
{
    public static class ImColors
    {
        public static readonly Color32 Clear = new Color32(0, 0, 0, 0);
        
        public static readonly Color32 Black = new Color32(0, 0, 0, 255);
        public static readonly Color32 White = new Color32(255, 255, 255, 255);

        public static readonly Color32 Gray0 = Black;
        public static readonly Color32 Gray1 = new Color32(32, 32, 32, 255);
        public static readonly Color32 Gray2 = new Color32(64, 64, 64, 255);
        public static readonly Color32 Gray3 = new Color32(96, 96, 96, 255);
        public static readonly Color32 Gray4 = new Color32(128, 128, 128, 255);
        public static readonly Color32 Gray5 = new Color32(160, 160, 160, 255);
        public static readonly Color32 Gray6 = new Color32(192, 192, 192, 255);
        public static readonly Color32 Gray7 = new Color32(224, 224, 224, 255);
        public static readonly Color32 Gray8 = White;

        public static readonly Color32 DarkBlue = new Color32(26, 66, 153, 255);
        public static readonly Color32 Blue = new Color32(40, 87, 189, 255);
        public static readonly Color32 LightBlue = new Color32(70, 123, 240, 255);

        public static Color32 WithAlpha(this Color32 color, byte alpha)
        {
            color.a = alpha;
            return color;
        }

        public static Color32 Multiply(this Color32 color, float value)
        {
            value = Mathf.Clamp01(value);

            color.r = (byte)(color.r * value);
            color.g = (byte)(color.g * value);
            color.b = (byte)(color.b * value);

            return color;
        }
    }
}