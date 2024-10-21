using System;
using Imui.Core;
using Imui.Style;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls.Windows
{
    public static class ImDebugWindow
    {
        private const int MOVING_AVERAGE_INTERVAL = 90;
        private const int FRAME_TIMES_BUFFER_SIZE = 120;
        
        private static readonly Color32 GroupColor = new Color32(255, 0, 0, 32);
        private static readonly Color32 ControlColor = new Color32(0, 255, 0, 64);
        
        private static bool debugOverlay;
        private static ImCircularBuffer<float> frameTimes = new ImCircularBuffer<float>(FRAME_TIMES_BUFFER_SIZE);
        private static float maxFrameTime;
        private static float avgFrameTime;
        
        public static void Draw(ImGui gui, ref bool open)
        {
            if (!gui.BeginWindow("Imui Debug", ref open, (350, 450)))
            {
                return;
            }
            
            var storageRatio = gui.Formatter.Concat(gui.Formatter.Format(gui.Storage.OccupiedSize)," / ", gui.Formatter.Format(gui.Storage.Capacity));
            var fpsValue = gui.Formatter.Format(avgFrameTime <= 0 ? 0 : 1 / avgFrameTime, "0");
            var msValue = gui.Formatter.Format(avgFrameTime * 1000, "0.0");

            gui.Text(gui.Formatter.Concat("Hovered: ", gui.Formatter.Format(gui.frameData.HoveredControl.Id)));
            gui.Text(gui.Formatter.Concat("Hovered Order: ", gui.Formatter.Format(gui.frameData.HoveredControl.Order)));
            gui.Text(gui.Formatter.Concat("Active: ", gui.Formatter.Format(gui.GetActiveControl())));
            gui.Text(gui.Formatter.Concat("Storage: ", storageRatio, " bytes"));
            gui.Text(gui.Formatter.Concat("Arena: ", gui.Formatter.Format(gui.frameData.ArenaSize), " bytes"));
            gui.Text(gui.Formatter.Concat("Vertices: ", gui.Formatter.Format(gui.frameData.VerticesCount)));
            gui.Text(gui.Formatter.Concat("Indices: ", gui.Formatter.Format(gui.frameData.IndicesCount)));
            gui.Text(gui.Formatter.Concat("FPS: ", fpsValue, " (", msValue, " ms)"));

            AppendFrameTime();
            DrawFrametimeGraph(gui);
            
            gui.Checkbox(ref debugOverlay, "Highlight Controls/Groups");
            
            #if IMUI_DEBUG
            gui.Checkbox(ref gui.MeshRenderer.Wireframe, "Wireframe");
            #endif
            
            gui.EndWindow();
            
            if (debugOverlay)
            {
                gui.Canvas.PushOrder(int.MaxValue);
                
                gui.Canvas.Rect(gui.frameData.HoveredControl.Rect, ControlColor);

                for (int i = 0; i < gui.frameData.HoveredGroups.Count; ++i)
                {
                    gui.Canvas.Rect(gui.frameData.HoveredGroups.Array[i].Rect, GroupColor);
                }
                
                gui.Canvas.PopOrder();
            }
        }

        private static void AppendFrameTime()
        {
            frameTimes.PushFront(Time.deltaTime);
            
            maxFrameTime = 0.0f;
            avgFrameTime = 0.0f;

            var avgCount = 0;
            
            for (int i = frameTimes.Count - 1; i >= 0; --i)
            {
                var value = frameTimes.Get(i);
                if (value > maxFrameTime)
                {
                    maxFrameTime = value;
                }

                if (avgCount < MOVING_AVERAGE_INTERVAL)
                {
                    avgFrameTime += value;
                    avgCount++;
                }
            }

            avgFrameTime /= avgCount;
        }

        private static void DrawFrametimeGraph(ImGui gui)
        {
            gui.AddSpacing();
            
            var width = gui.GetLayoutWidth();
            var height = 100.0f;
            var rect = gui.Layout.AddRect(width, height);
            var min = 0.0f;
            var max = Mathf.Max(maxFrameTime, Mathf.RoundToInt(avgFrameTime / 0.004f) * 0.004f * 2);
            
            Span<Vector2> points = stackalloc Vector2[frameTimes.Count];
            var nw = frameTimes.Count / (float)frameTimes.Capacity;
            for (int i = 0; i < frameTimes.Count; ++i)
            {
                var t = frameTimes.Get(i);
                var xn = i / (float)(frameTimes.Count - 1);
                var yn = Mathf.InverseLerp(min, max, t);

                points[i] = new Vector2(rect.X + xn * rect.W * nw, rect.Y + yn * rect.H);
            }

            gui.BeginList(rect);
            gui.Canvas.Line(points, gui.Style.Text.Color, false, 1);
            gui.EndList();
        }
    }
}