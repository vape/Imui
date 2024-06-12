using System;
using Imui.Controls;
using Imui.Controls.Layout;
using Imui.Core;
using Imui.Styling;
using Imui.Utility;
using UnityEngine;

namespace Imui.Debugging
{
    public static class ImDebug
    {
        private static readonly Color32 groupColor = new Color32(255, 0, 0, 32);
        private static readonly Color32 controlColor = new Color32(0, 255, 0, 64);
        
        private static bool debugOverlay = false;
        private static char[] formatBuffer = new char[256];
        private static ImCircularBuffer<float> frameTimes = new ImCircularBuffer<float>(128);
        
        public static void Window(ImGui gui)
        {
            gui.BeginWindow("Imui Debug");

            var buffer = new Span<char>(formatBuffer);
            var length = 0;
            
            Append(buffer, "Hovered Control: ", ref length);
            Append(buffer, gui.frameData.HoveredControl.Id, ref length);
            Flush(gui, buffer, ref length);
            
            Append(buffer, "Hot Control: ", ref length);
            Append(buffer, gui.GetActiveControl(), ref length);
            Flush(gui, buffer, ref length);

            Append(buffer, "Storage: ", ref length);
            Append(buffer, gui.Storage.OccupiedSize, ref length);
            Append(buffer, "/", ref length);
            Append(buffer, gui.Storage.Capacity, ref length);
            Flush(gui, buffer, ref length);

            Append(buffer, "Frametime: ", ref length);
            Append(buffer, Time.deltaTime * 1000, ref length, "0.0");
            Append(buffer, "ms", ref length);
            Flush(gui, buffer, ref length);

            Append(buffer, "FPS:", ref length);
            Append(buffer, 1 / Time.deltaTime, ref length, "0");
            Flush(gui, buffer, ref length);

            frameTimes.PushFront(Time.deltaTime);
            DrawFrametimeGraph(gui);
            
            if (gui.ButtonFitted(debugOverlay ? "Disable Overlay" : "Enable Overlay"))
            {
                debugOverlay = !debugOverlay;
            }
            
            #if IMUI_DEBUG
            gui.Checkmark(ref gui.MeshRenderer.Wireframe, "Wireframe");
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

        private static void DrawFrametimeGraph(ImGui gui)
        {
            gui.AddSpacing();
            
            var width = gui.GetAvailableWidth();
            var height = 200.0f;
            var rect = gui.Layout.AddRect(width, height);

            var min = 0;
            var max = Application.targetFrameRate * 2.0f;

            for (int i = 0; i < frameTimes.Count; ++i)
            {
                var value = frameTimes.Array[i] * 2;
                if (value > max)
                {
                    max = value;
                }
            }

            Span<Vector2> points = stackalloc Vector2[frameTimes.Count];
            var nw = frameTimes.Count / (float)frameTimes.Capacity;
            for (int i = 0; i < frameTimes.Count; ++i)
            {
                var t = frameTimes.Get(i);
                var xn = i / (float)(frameTimes.Count - 1);
                var yn = Mathf.InverseLerp(min, max, t);

                points[i] = new Vector2(rect.X + xn * rect.W * nw, rect.Y + yn * rect.H);
            }
            
            gui.Canvas.RectWithOutline(rect, ImColors.Gray6, ImColors.Black, 1.0f);
            gui.Canvas.LineSimple(points, ImColors.Black, false, 1);
        }

        private static void Flush(ImGui gui, Span<char> buffer, ref int length)
        {
            gui.Text(buffer[..length]);
            length = 0;
        }

        private static void Append(Span<char> buffer, uint value, ref int totalLength)
        {
            value.TryFormat(buffer[totalLength..], out var written);
            totalLength += written;
        }
        
        private static void Append(Span<char> buffer, int value, ref int totalLength)
        {
            value.TryFormat(buffer[totalLength..], out var written);
            totalLength += written;
        }
        
        private static void Append(Span<char> buffer, float value, ref int totalLength, string format = "0.000")
        {
            value.TryFormat(buffer[totalLength..], out var written, format);
            totalLength += written;
        }
        
        private static void Append(Span<char> buffer, string str, ref int totalLength)
        {
            str.AsSpan().CopyTo(buffer[totalLength..]);
            totalLength += str.Length;
        }
    }
}