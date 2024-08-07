using System;
using Imui.Core;
using Imui.Controls.Styling;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls.Windows
{
    public static class ImDebugWindow
    {
        private const int MOVING_AVERAGE_INTERVAL = 90;
        private const int FRAME_TIMES_BUFFER_SIZE = 120;
        
        private static readonly Color32 groupColor = new Color32(255, 0, 0, 32);
        private static readonly Color32 controlColor = new Color32(0, 255, 0, 64);
        
        private static bool debugOverlay = false;
        private static ImCircularBuffer<float> frameTimes = new ImCircularBuffer<float>(FRAME_TIMES_BUFFER_SIZE);
        private static float maxFrameTime;
        private static float avgFrameTime;
        
        public static void Draw(ImGui gui)
        {
            gui.BeginWindow("Imui Debug", width: 350, 415);
            
            var storageRatio = gui.Formatter.Join(gui.Formatter.Format(gui.Storage.OccupiedSize)," / ", gui.Formatter.Format(gui.Storage.Capacity));
            var fpsValue = gui.Formatter.Format(avgFrameTime <= 0 ? 0 : 1 / avgFrameTime, "0");
            var msValue = gui.Formatter.Format(avgFrameTime * 1000, "0.0");

            gui.Text(gui.Formatter.Join("Hovered: ", gui.Formatter.Format(gui.frameData.HoveredControl.Id)));
            gui.Text(gui.Formatter.Join("Active: ", gui.Formatter.Format(gui.GetActiveControl())));
            gui.Text(gui.Formatter.Join("Storage: ", storageRatio, " bytes"));
            gui.Text(gui.Formatter.Join("Arena: ", gui.Formatter.Format(gui.frameData.ArenaSize), " bytes"));
            gui.Text(gui.Formatter.Join("Vertices: ", gui.Formatter.Format(gui.frameData.VerticesCount)));
            gui.Text(gui.Formatter.Join("Indices: ", gui.Formatter.Format(gui.frameData.IndicesCount)));
            gui.Text(gui.Formatter.Join("FPS: ", fpsValue, " (", msValue, " ms)"));

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
                
                gui.Canvas.Rect(gui.frameData.HoveredControl.Rect, controlColor);

                for (int i = 0; i < gui.frameData.HoveredGroups.Count; ++i)
                {
                    gui.Canvas.Rect(gui.frameData.HoveredGroups.Array[i].Rect, groupColor);
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

            gui.BeginPanel(rect);
            gui.Canvas.Line(points, ImTheme.Active.Text.Color, false, 1);
            gui.EndPanel();
        }
    }
}