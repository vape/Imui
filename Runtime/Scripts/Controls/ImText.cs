using System;
using Imui.Core;
using Imui.Styling;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImText
    {
        private const float MIN_WIDTH = 1;
        private const float MIN_HEIGHT = 1;

        public const int DEFAULT_TEXT_SIZE = 24;
        
        public static ImTextStyle Style = ImTextStyle.Default;
        public static ImTextSettings Settings = new ImTextSettings()
        {
            AlignX = 0,
            AlignY = 0,
            Size = DEFAULT_TEXT_SIZE
        };

        public static void Text(this ImGui gui, in ReadOnlySpan<char> text)
        {
            Text(gui, in text, Settings);
        }
        
        public static void Text(this ImGui gui, in ReadOnlySpan<char> text, in ImTextSettings settings)
        {
            var space = gui.Layout.GetFreeSpace().Max(MIN_WIDTH, MIN_HEIGHT);
            var rect = gui.Layout.GetRect(space);
            gui.Canvas.Text(in text, Style.Color, rect, in settings, out var textRect);
            gui.Layout.AddRect(textRect);
        }
        
        public static void Text(this ImGui gui, in ReadOnlySpan<char> text, ImRect rect, in ImTextSettings settings)
        {
            gui.Canvas.Text(in text, Style.Color, rect, in settings);
        }
    }
    
    [Serializable]
    public struct ImTextStyle
    {
        public static readonly ImTextStyle Default = new ImTextStyle()
        {
            Color = ImColors.Black
        };
        
        public Color32 Color;
    }
}