using UnityEngine;

namespace Imui.Style
{
    public static class ImColorUtility
    {
        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }
        
        public static Color32 WithAlpha(this Color32 color, float alpha)
        {
            color.a = (byte)(255 * Mathf.Clamp01(alpha));
            return color;
        }
        
        public static void SetAlpha(this ref Color32 color, float alpha)
        {
            color.a = (byte)(255 * Mathf.Clamp01(alpha));
        }
    }
}