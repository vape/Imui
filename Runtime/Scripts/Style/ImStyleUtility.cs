using UnityEngine;

namespace Imui.Style
{
    public static class ImStyleUtility
    {
        public static readonly Color32 Black = new Color32(0, 0, 0, 255);
        public static readonly Color32 White = new Color32(255, 255, 255, 255);

        public static Color32 WithAlpha(this Color32 color, byte alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}