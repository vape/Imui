using Imui.Core;
using Imui.Style;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Imui.Controls
{
    public static class ImRectExt
    {
        public static Vector2 Max(this Vector2 vec, float x, float y)
        {
            return new Vector2(Mathf.Max(vec.x, x), Mathf.Max(vec.y, y));
        }
        
        public static ImRect SplitTop(this ImRect rect, float height)
        {
            rect.Y += rect.H - height;
            rect.H = height;
            return rect;
        }

        
        public static ImRect SplitTop(this ImRect rect, float height, out ImRect bottom)
        {
            bottom = rect;
            bottom.H = rect.H - height;
            rect.Y += bottom.H;
            rect.H = height;
            return rect;
        }

        public static ImRect SplitLeft(this ImRect rect, float width, out ImRect right)
        {
            right = rect;
            right.X += width;
            right.W = rect.W - width;
            rect.W = width;
            return rect;
        }

        public static ImRect SplitRight(this ImRect rect, float width, out ImRect left)
        {
            left = rect;
            left.W -= width;
            rect.W = width;
            rect.X += left.W;
            return rect;
        }
        
        public static ImRect SplitLeft(this ImRect rect, float width, float space, out ImRect right)
        {
            right = rect;
            right.X += width + space;
            right.W = rect.W - width - space;
            rect.W = width;
            return rect;
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
        
        public static ImRect WithPadding(this ImRect rect, ImPadding padding)
        {
            rect.X += padding.Left;
            rect.Y += padding.Bottom;
            rect.W -= padding.Left + padding.Right;
            rect.H -= padding.Top + padding.Bottom;

            return rect;
        }
        
        public static void AddPaddingToSize(ref Vector2 size, ImPadding padding)
        {
            size.x += padding.Left + padding.Right;
            size.y += padding.Bottom + padding.Top;
        }
        
        public static void AddPadding(this ref ImRect rect, ImPadding padding)
        {
            rect.X += padding.Left;
            rect.Y += padding.Bottom;
            rect.W -= padding.Left + padding.Right;
            rect.H -= padding.Top + padding.Bottom;
        }
        
        public static ImRect ScaleFromCenter(this ImRect rect, float scale)
        {
            var w = rect.W;
            var h = rect.H;
            
            rect.W *= scale;
            rect.H *= scale;
            rect.X += (w - rect.W) * 0.5f;
            rect.Y += (h - rect.H) * 0.5f;

            return rect;
        }
        
        public static ImRect ScaleFromCenter(this ImRect rect, Vector2 scale)
        {
            var w = rect.W;
            var h = rect.H;
            
            rect.W *= scale.x;
            rect.H *= scale.y;
            rect.X += (w - rect.W) * 0.5f;
            rect.Y += (h - rect.H) * 0.5f;

            return rect;
        }

        public static ImRect Clamp(ImRect bounds, ImRect rect)
        {
            rect.W = Mathf.Min(rect.W, bounds.W);
            rect.H = Mathf.Min(rect.H, bounds.H);

            if (rect.X < bounds.X)
            {
                rect.X += bounds.X - rect.X;
            }

            if (rect.Right > bounds.Right)
            {
                rect.X -= rect.Right - bounds.Right;
            }

            if (rect.Y < bounds.Y)
            {
                rect.Y += bounds.Y - rect.Y;
            }

            if (rect.Top > bounds.Top)
            {
                rect.Y -= rect.Top - bounds.Top;
            }

            return rect;
        }
    }
}