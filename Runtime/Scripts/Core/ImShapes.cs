using System;
using Imui.Utility;
using UnityEngine;

namespace Imui.Core
{
    public static class ImShapes
    {
        private const int SIN_TABLE_PRECISION = 512;
        private const int COS_TABLE_PRECISION = 512;
        
        private const int MIN_SEGMENTS = 2;
        private const int MAX_SEGMENTS = 16;
        private const float SEGMENT_MAX_ERROR = 2;
        
        private const int SEGMENT_TABLE_SIZE = 200;
        private const float SEGMENT_TABLE_RES = 0.1f;
        private const int SEGMENT_TABLE_SIZE_MAX = (int)(SEGMENT_TABLE_SIZE * SEGMENT_TABLE_RES);

        private const float PI = Mathf.PI;
        private const float HALF_PI = PI / 2;
        private const float TWO_PI = PI * 2;
        
        private static float[] sinTable;
        private static float[] cosTable;
        private static int[] segmentTable;

        public static void BuildTables()
        {
            if (sinTable == null)
            {
                sinTable = new float[SIN_TABLE_PRECISION];
                
                for (int i = 0; i < SIN_TABLE_PRECISION; ++i)
                {
                    var value = Mathf.Sin((TWO_PI * i) / SIN_TABLE_PRECISION);
                    sinTable[i] = value;
                }
            }
            
            if (cosTable == null)
            {
                cosTable = new float[COS_TABLE_PRECISION];
                
                for (int i = 0; i < COS_TABLE_PRECISION; ++i)
                {
                    var value = Mathf.Cos((TWO_PI * i) / COS_TABLE_PRECISION);
                    cosTable[i] = value;
                }
            }

            if (segmentTable == null)
            {
                segmentTable = new int[SEGMENT_TABLE_SIZE];
                for (int i = 0; i < SEGMENT_TABLE_SIZE; ++i)
                {
                    var radius = i * SEGMENT_TABLE_RES;
                    var segments = Mathf.Clamp(Mathf.CeilToInt(Mathf.PI / Mathf.Acos(1 - Mathf.Min(SEGMENT_MAX_ERROR, radius) / radius)), MIN_SEGMENTS, MAX_SEGMENTS);
                    
                    segmentTable[i] = ((segments + 1) / 2) * 2;
                }
            }
        }

        public static int SegmentCountForRadius(float radius)
        {
            if (radius <= SEGMENT_MAX_ERROR)
            {
                return MIN_SEGMENTS;
            }

            if (radius < SEGMENT_TABLE_SIZE_MAX)
            {
                return segmentTable[(int)(radius / SEGMENT_TABLE_RES)];
            }
            
            var segments = Mathf.Clamp(Mathf.CeilToInt(Mathf.PI / Mathf.Acos(1 - Mathf.Min(SEGMENT_MAX_ERROR, radius) / radius)), MIN_SEGMENTS, MAX_SEGMENTS);
            return ((segments + 1) / 2) * 2;
        }

        public static Span<Vector2> Ellipse(ImArena arena, ImRect rect)
        {
            var segments = SegmentCountForRadius(rect.W / 2f) * 4;
            var span = arena.AllocArray<Vector2>(segments);

            Ellipse(rect, span, segments);

            return span;
        }

        public static void Ellipse(ImRect rect, Span<Vector2> buffer, int segments)
        {
            ImProfiler.BeginSample("ImShapes.Ellipse");
            
            var step = (1f / segments) * PI * 2;
            var rx = rect.W / 2.0f;
            var ry = rect.H / 2.0f;
            var cx = rect.X + rx;
            var cy = rect.Y + ry;

            for (int i = 0; i < segments; ++i)
            {
                var a = step * (i + 1);
                buffer[i].x = cx + cosTable[(int)(COS_TABLE_PRECISION * (a % TWO_PI) / TWO_PI)] * rx;
                buffer[i].y = cy + sinTable[(int)(SIN_TABLE_PRECISION * (a % TWO_PI) / TWO_PI)] * ry;
            }
            
            ImProfiler.EndSample();
        }

        public static Span<Vector2> Rect(ImArena arena, ImRect rect, ImRectRadius radius)
        {
            radius.Clamp(Mathf.Min(rect.W, rect.H) / 2.0f);

            var segTopL = radius.TopLeft < 1 ? 0 : SegmentCountForRadius(radius.TopLeft);
            var segTopR = radius.TopRight < 1 ? 0 : SegmentCountForRadius(radius.TopRight);
            var segBotR = radius.BottomRight < 1 ? 0 : SegmentCountForRadius(radius.BottomRight);
            var segBotL = radius.BottomLeft < 1 ? 0 : SegmentCountForRadius(radius.BottomLeft);
            var span = arena.AllocArray<Vector2>(4 + (segTopL + segTopR + segBotR + segBotL));

            Rect(in rect, in radius, ref span, segTopL, segTopR, segBotR, segBotL);

            return span;
        }
        
        public static int Rect(in ImRect rect,
                               in ImRectRadius radius,
                               ref Span<Vector2> buffer,
                               int segmentsTopLeft,
                               int segmentsTopRight,
                               int segmentsBottomRight,
                               int segmentsBottomLeft)
        {
            ImProfiler.BeginSample("ImShapes.Rect");

            var p = 0;

            var step = (1f / segmentsBottomRight) * HALF_PI;
            var cx = rect.X + rect.W - radius.BottomRight;
            var cy = rect.Y + radius.BottomRight;
            
            ref var bp = ref buffer[p];
            bp.x = cx + cosTable[(int)(COS_TABLE_PRECISION * 0.75f)] * radius.BottomRight;
            bp.y = cy + sinTable[(int)(SIN_TABLE_PRECISION * 0.75f)] * radius.BottomRight;
            p++;

            for (int i = 0; i < segmentsBottomRight; ++i)
            {
                var a = PI + HALF_PI + step * (i + 1);
                bp = ref buffer[p];
                bp.x = cx + cosTable[(int)(COS_TABLE_PRECISION * (a % TWO_PI) / TWO_PI)] * radius.BottomRight;
                bp.y = cy + sinTable[(int)(SIN_TABLE_PRECISION * (a % TWO_PI) / TWO_PI)] * radius.BottomRight;
                p++;
            }

            step = (1f / segmentsTopRight) * HALF_PI;
            cx = rect.X + rect.W - radius.TopRight;
            cy = rect.Y + rect.H - radius.TopRight;
            bp = ref buffer[p];
            bp.x = cx + cosTable[0] * radius.TopRight;
            bp.y = cy + sinTable[0] * radius.TopRight;
            p++;

            for (int i = 0; i < segmentsTopRight; ++i)
            {
                var a = 0 + step * (i + 1);
                bp = ref buffer[p];
                bp.x = cx + cosTable[(int)(COS_TABLE_PRECISION * (a % TWO_PI) / TWO_PI)] * radius.TopRight;
                bp.y = cy + sinTable[(int)(SIN_TABLE_PRECISION * (a % TWO_PI) / TWO_PI)] * radius.TopRight;
                p++;
            }

            step = (1f / segmentsTopLeft) * HALF_PI;
            cx = rect.X + radius.TopLeft;
            cy = rect.Y + rect.H - radius.TopLeft;
            bp = ref buffer[p];
            bp.x = cx + cosTable[(int)(COS_TABLE_PRECISION * 0.25f)] * radius.TopLeft;
            bp.y = cy + sinTable[(int)(SIN_TABLE_PRECISION * 0.25f)] * radius.TopLeft;
            p++;

            for (int i = 0; i < segmentsTopLeft; ++i)
            {
                var a = HALF_PI + step * (i + 1);
                bp = ref buffer[p];
                bp.x = cx + cosTable[(int)(COS_TABLE_PRECISION * (a % TWO_PI) / TWO_PI)] * radius.TopLeft;
                bp.y = cy + sinTable[(int)(SIN_TABLE_PRECISION * (a % TWO_PI) / TWO_PI)] * radius.TopLeft;
                p++;
            }

            step = (1f / segmentsBottomLeft) * HALF_PI;
            cx = rect.X + radius.BottomLeft;
            cy = rect.Y + radius.BottomLeft;
            bp = ref buffer[p];
            bp.x = cx + cosTable[(int)(COS_TABLE_PRECISION * 0.5f)] * radius.BottomLeft;
            bp.y = cy + sinTable[(int)(SIN_TABLE_PRECISION * 0.5f)] * radius.BottomLeft;
            p++;

            for (int i = 0; i < segmentsBottomLeft; ++i)
            {
                var a = PI + step * (i + 1);
                bp = ref buffer[p];
                bp.x = cx + cosTable[(int)(COS_TABLE_PRECISION * (a % TWO_PI) / TWO_PI)] * radius.BottomLeft;
                bp.y = cy + sinTable[(int)(SIN_TABLE_PRECISION * (a % TWO_PI) / TWO_PI)] * radius.BottomLeft;
                p++;
            }

            ImProfiler.EndSample();

            return p;
        }
    }
}