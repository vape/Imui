using System;
using Imui.Core;
using Imui.Styling;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImText
    {
        public const float MIN_WIDTH = 1;
        public const float MIN_HEIGHT = 1;
        
        public static ImTextStyle Style = ImTextStyle.Default;

        public static void Text(this ImGui gui, in ReadOnlySpan<char> text)
        {
            Text(gui, in text, GetTextSettings());
        }
        
        public static void Text(this ImGui gui, in ReadOnlySpan<char> text, in ImRect rect)
        {
            Text(gui, in text, GetTextSettings(), rect);
        }

        public static void TextFittedSlow(this ImGui gui, in ReadOnlySpan<char> text, in ImRect rect)
        {
            // (artem-s): at least try to skip costly auto-sizing
            if (gui.Canvas.Cull(rect))
            {
                return;
            }
            
            var settings = GetTextSettings();
            settings.Size = AutoSizeTextSlow(gui, in text, settings, rect.Size);
            Text(gui, in text, settings, rect);
        }
        
        public static void Text(this ImGui gui, in ReadOnlySpan<char> text, in ImTextSettings settings)
        {
            gui.AddControlSpacing();
            
            var space = gui.Layout.GetAvailableSize().Max(MIN_WIDTH, MIN_HEIGHT);
            var rect = gui.Layout.GetRect(space);
            gui.Canvas.Text(in text, Style.Color, rect, in settings, out var textRect);
            gui.Layout.AddRect(textRect);
        }
        
        public static void Text(this ImGui gui, in ReadOnlySpan<char> text, in ImTextSettings settings, ImRect rect)
        {
            gui.Canvas.Text(in text, Style.Color, rect, in settings);
        }

        public static ImTextSettings GetTextSettings()
        {
            return new ImTextSettings(ImControls.Style.TextSize, Style.Alignment);
        }

        // TODO (artem-s): Got to come up with better solution instead of just brute forcing the fuck of it every time
        public static float AutoSizeTextSlow(this ImGui gui, in ReadOnlySpan<char> text, ImTextSettings settings, Vector2 bounds, float minSize = 1)
        {
            var textSize = gui.MeasureTextSize(in text, in settings, bounds);
            while (settings.Size > minSize && (textSize.x > bounds.x || textSize.y > bounds.y))
            {
                settings.Size -= 1;
                textSize = gui.MeasureTextSize(in text, in settings, bounds);
            }

            return settings.Size;
        }

        public static float GetFontSizeForContainerHeight(this ImGui gui, float containerHeight)
        {
            var scale = containerHeight / gui.TextDrawer.FontLineHeight;
            return scale * gui.TextDrawer.FontRenderSize;
        }
        
        public static Vector2 MeasureTextSize(this ImGui gui, in ReadOnlySpan<char> text, in ImTextSettings textSettings, Vector2 bounds = default)
        {
            ref readonly var textLayout = ref gui.TextDrawer.BuildTempLayout(in text, 
                bounds.x, 
                bounds.y,
                textSettings.Align.X,
                textSettings.Align.Y, 
                textSettings.Size);

            return new Vector2(textLayout.Width, textLayout.Height);
        }
    }
    
    [Serializable]
    public struct ImTextStyle
    {
        public static readonly ImTextStyle Default = new ImTextStyle()
        {
            Color = ImColors.Black,
            Alignment = new ImTextAlignment(0.0f, 0.0f)
        };
        
        public Color32 Color;
        public ImTextAlignment Alignment;
    }
}