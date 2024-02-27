using System;
using Imui.Core;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImText
    {
        private const float MIN_WIDTH = 1;
        private const float MIN_HEIGHT = 1;
        
        public static void Text(this ImGui gui, in ReadOnlySpan<char> text, in Style style)
        {
            var space = gui.Layout.GetFreeSpace().Max(MIN_WIDTH, MIN_HEIGHT);
            var rect = gui.Layout.GetRect(space);
            gui.Canvas.Text(in text, style.Color, rect, in style.Settings, out var textRect);
            gui.Layout.AddRect(textRect);
        }
        
        public static void Text(this ImGui gui, in ReadOnlySpan<char> text, ImRect rect, in Style style)
        {
            gui.Canvas.Text(in text, style.Color, rect, in style.Settings);
        }
        
        [Serializable]
        public struct Style
        {
            public Color32 Color;
            public ImTextSettings Settings;
        }
    }
}