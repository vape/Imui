using Imui.Core;
using Imui.Styling;
using UnityEngine;

namespace Imui.Utility
{
    public static class RectUtility
    {
        public static Vector2 Max(this Vector2 vec, float x, float y)
        {
            return new Vector2(Mathf.Max(vec.x, x), Mathf.Max(vec.y, y));
        }
        
        public static Rect Intersection(this Rect rect, Rect other)
        {
            var x1 = Mathf.Max(rect.x, other.x);
            var y1 = Mathf.Max(rect.y, other.y);
            var x2 = Mathf.Min(rect.x + rect.width, other.x + other.width);
            var y2 = Mathf.Min(rect.y + rect.height, other.y + other.height);

            return new Rect(x1, y1, x2 - x1, y2 - y1);
        }
        
        public static ImRect SplitTop(this ImRect rect, float height, out ImRect next)
        {
            next = rect;
            next.H = rect.H - height;
            rect.Y += next.H;
            rect.H = height;
            return rect;
        }

        public static ImRect SplitLeft(this ImRect rect, float width, out ImRect next)
        {
            next = rect;
            next.X += width;
            next.W = rect.W - width;
            rect.W = width;
            return rect;
        }

        public static void AddPadding(this ref ImRect rect, float size)
        {
            rect.X += size;
            rect.Y += size;
            rect.W -= size * 2;
            rect.H -= size * 2;
        }
        
        public static ImRect WithAspect(this ImRect rect, float aspect)
        {
            var rectAspect = rect.W / rect.H;
            var w = rectAspect > aspect ? rect.W / (rectAspect / aspect) : rect.W;
            var h = rectAspect > aspect ? rect.H : rect.H * (rectAspect / aspect);
            var x = rect.X + (0.5f * (rect.W - w));
            var y = rect.Y + (0.5f * (rect.H - h));
            
            return new ImRect(x, y, w, h);
        }
        
        public static ImRect WithPadding(this ImRect rect, float size)
        {
            rect.X += size;
            rect.Y += size;
            rect.W -= size * 2;
            rect.H -= size * 2;

            return rect;
        }

        public static ImRect WithPadding(this ImRect rect, float left, float top, float right, float bottom)
        {
            rect.X += left;
            rect.Y += bottom;
            rect.W -= left + right;
            rect.H -= top + bottom;

            return rect;
        }

        public static ImRect WithPadding(this ImRect rect, ImPadding padding)
        {
            rect.X += padding.Left;
            rect.Y += padding.Bottom;
            rect.W -= padding.Left + padding.Right;
            rect.H -= padding.Top + padding.Bottom;

            return rect;
        }
    }
}