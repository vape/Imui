using UnityEngine;

namespace Imui.Utility
{
    public static class RectUtility
    {
        public static Rect Intersection(this Rect rect, Rect other)
        {
            var x1 = Mathf.Max(rect.x, other.x);
            var y1 = Mathf.Max(rect.y, other.y);
            var x2 = Mathf.Min(rect.x + rect.width, other.x + other.width);
            var y2 = Mathf.Min(rect.y + rect.height, other.y + other.height);

            return new Rect(x1, y1, x2 - x1, y2 - y1);
        }
    }
}