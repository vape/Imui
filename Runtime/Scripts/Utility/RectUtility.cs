using Imui.Core;
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
        
        public static ImRect AddPadding(this ImRect rect, float size)
        {
            rect.X += size;
            rect.Y += size;
            rect.W -= size * 2;
            rect.H -= size * 2;

            return rect;
        }

        public static ImRect AddPadding(this ImRect rect, float left, float top, float right, float bottom)
        {
            rect.X += left;
            rect.Y += bottom;
            rect.W -= left + right;
            rect.H -= top + bottom;

            return rect;
        }
    }
}