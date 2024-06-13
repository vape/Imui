using Imui.Core;
using Imui.Controls.Styling;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImRectExtensions
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
        
        public static ImRect SplitLeft(this ImRect rect, float width, float space, out ImRect next)
        {
            next = rect;
            next.X += width + space;
            next.W = rect.W - width - space;
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
        
        public static void AddPadding(this ref ImRect rect, ImPadding padding)
        {
            rect.X += padding.Left;
            rect.Y += padding.Bottom;
            rect.W -= padding.Left + padding.Right;
            rect.H -= padding.Top + padding.Bottom;
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

        public static void ApplyPadding(this ref ImRect rect, float padding)
        {
            rect.X += padding;
            rect.Y += padding;
            rect.W -= padding * 2;
            rect.H -= padding * 2;
        }
        
        public static void ApplyPadding(this ref ImRect rect, ImPadding padding)
        {
            rect.X += padding.Left;
            rect.Y += padding.Bottom;
            rect.W -= padding.Left + padding.Right;
            rect.H -= padding.Top + padding.Bottom;
        }

        public static ImRect WithMargin(this ImRect rect, ImPadding margin)
        {
            rect.X -= margin.Left;
            rect.Y -= margin.Bottom;
            rect.W += margin.Left + margin.Right;
            rect.H += margin.Top + margin.Bottom;

            return rect;
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
    }
}