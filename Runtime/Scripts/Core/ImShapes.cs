using System;
using Imui.Utility;
using UnityEngine;
using UnityEngine.Profiling;

namespace Imui.Core
{
    public static class ImShapes
    {
        private const int MIN_SEGMENTS = 2;
        private const int MAX_SEGMENTS = 16;
        
        private const float PI = Mathf.PI;
        private const float HALF_PI = PI / 2;
        
        private static ImResizeableBuffer<Vector2> TempBuffer = new (4096);
        
        public static int SegmentCountForRadius(float radius, float maxError = 2)
        {
            if (radius <= maxError)
            {
                return MIN_SEGMENTS;
            }

            var segments = Mathf.Clamp(Mathf.CeilToInt(Mathf.PI / Mathf.Acos(1 - Mathf.Min(maxError, radius) / radius)), MIN_SEGMENTS, MAX_SEGMENTS);
            return ((segments + 1) / 2) * 2;
        }

        public static Span<Vector2> Ellipse(ImRect rect)
        {
            var segments = SegmentCountForRadius(rect.W / 2f) * 4;
            var span = TempBuffer.AsSpan(segments);

            Ellipse(rect, span, segments);

            return span;
        }

        public static void Ellipse(ImRect rect, Span<Vector2> buffer, int segments)
        {
            var step = (1f / segments) * PI * 2;
            var rx = rect.W / 2.0f;
            var ry = rect.H / 2.0f;
            var cx = rect.X + rx;
            var cy = rect.Y + ry;
            
            for (int i = 0; i < segments; ++i)
            {
                var a = step * (i + 1);
                buffer[i].x = cx + Mathf.Cos(a) * rx;
                buffer[i].y = cy + Mathf.Sin(a) * ry;
            }
        }

        public static Span<Vector2> Rect(ImRect rect, ImRectRadius radius)
        {
            radius.Clamp(Mathf.Min(rect.W, rect.H) / 2.0f);

            var segments = SegmentCountForRadius(radius.RadiusForMask());
            var span = TempBuffer.AsSpan((segments + 1) * 4);
            
            Rect(rect, radius, span, segments);
            
            return span;
        }
        
        public static int Rect(ImRect rect, ImRectRadius radius, Span<Vector2> buffer, int segments)
        {
            Profiler.BeginSample("ImShapes.RectOutline");
            
            var p = 0;
            var step = (1f / segments) * HALF_PI;
            
            var cx = rect.X + rect.W - radius.BottomRight;
            var cy = rect.Y + radius.BottomRight;
            buffer[p].x = cx + Mathf.Cos(PI + HALF_PI) * radius.BottomRight;
            buffer[p].y = cy + Mathf.Sin(PI + HALF_PI) * radius.BottomRight;
            p++;
            
            for (int i = 0; i < segments; ++i)
            {
                var a = PI + HALF_PI + step * (i + 1);
                buffer[p].x = cx + Mathf.Cos(a) * radius.BottomRight;
                buffer[p].y = cy + Mathf.Sin(a) * radius.BottomRight;
                p++;
            }
            
            cx = rect.X + rect.W - radius.TopRight;
            cy = rect.Y + rect.H - radius.TopRight;
            buffer[p].x = cx + Mathf.Cos(0) * radius.TopRight;
            buffer[p].y = cy + Mathf.Sin(0) * radius.TopRight;
            p++;
            
            for (int i = 0; i < segments; ++i)
            {
                var a = 0 + step * (i + 1);
                buffer[p].x = cx + Mathf.Cos(a) * radius.TopRight;
                buffer[p].y = cy + Mathf.Sin(a) * radius.TopRight;
                p++;
            }
            
            cx = rect.X + radius.TopLeft;
            cy = rect.Y + rect.H - radius.TopLeft;
            buffer[p].x = cx + Mathf.Cos(HALF_PI) * radius.TopLeft;
            buffer[p].y = cy + Mathf.Sin(HALF_PI) * radius.TopLeft;
            p++;
            
            for (int i = 0; i < segments; ++i)
            {
                var a = HALF_PI + step * (i + 1);
                buffer[p].x = cx + Mathf.Cos(a) * radius.TopLeft;
                buffer[p].y = cy + Mathf.Sin(a) * radius.TopLeft;
                p++;
            }
                        
            cx = rect.X + radius.BottomLeft;
            cy = rect.Y + radius.BottomLeft;
            buffer[p].x = cx + Mathf.Cos(PI) * radius.BottomLeft;
            buffer[p].y = cy + Mathf.Sin(PI) * radius.BottomLeft;
            p++;
            
            for (int i = 0; i < segments; ++i)
            {
                var a = PI + step * (i + 1);
                buffer[p].x = cx + Mathf.Cos(a) * radius.BottomLeft;
                buffer[p].y = cy + Mathf.Sin(a) * radius.BottomLeft;
                p++;
            }
            
            Profiler.EndSample();

            return p;
        }
    }
}