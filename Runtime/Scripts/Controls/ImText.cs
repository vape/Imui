using System;
using Imui.Core;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImText
    {
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