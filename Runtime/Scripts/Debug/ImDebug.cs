using System;
using Imui.Controls;
using Imui.Core;
using UnityEngine;

namespace Imui.Debug
{
    public static class ImDebug
    {
        private static readonly Color32 groupColor = new Color32(255, 0, 0, 32);
        private static readonly Color32 controlColor = new Color32(0, 255, 0, 64);
        
        private static bool debugOverlay = false;
        private static char[] formatBuffer = new char[256];
        
        public static void Window(ImGui gui)
        {
            gui.BeginWindow("Imui Debug");

            var buffer = new Span<char>(formatBuffer);
            var length = 0;
            
            Append(buffer, "Hovered Control: ", ref length);
            Append(buffer, gui.frameData.HoveredControl.Id, ref length);
            Flush(gui, buffer, ref length);
            
            Append(buffer, "Hot Control: ", ref length);
            Append(buffer, gui.ActiveControl, ref length);
            Flush(gui, buffer, ref length);

            Append(buffer, "Storage: ", ref length);
            Append(buffer, gui.Storage.OccupiedSize, ref length);
            Append(buffer, "/", ref length);
            Append(buffer, gui.Storage.Capacity, ref length);
            Flush(gui, buffer, ref length);

            if (gui.Button(debugOverlay ? "Disable Overlay" : "Enable Overlay"))
            {
                debugOverlay = !debugOverlay;
            }
            
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
        
        private static void Append(Span<char> buffer, float value, ref int totalLength)
        {
            value.TryFormat(buffer[totalLength..], out var written, "0.000");
            totalLength += written;
        }

        private static void Append(Span<char> buffer, string str, ref int totalLength)
        {
            str.AsSpan().CopyTo(buffer[totalLength..]);
            totalLength += str.Length;
        }
    }
}