using System;
using Imui.Core;
using Imui.Controls.Styling;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImText
    {
        public const float MIN_WIDTH = 1;
        public const float MIN_HEIGHT = 1;
        
        public static void Text(this ImGui gui, ReadOnlySpan<char> text, bool wrap = false)
        {
            Text(gui, text, GetTextSettings(wrap));
        }
        
        public static void Text(this ImGui gui, ReadOnlySpan<char> text, Color32 color, bool wrap = false)
        {
            Text(gui, text, GetTextSettings(wrap), color);
        }
        
        public static void Text(this ImGui gui, ReadOnlySpan<char> text, ImRect rect, bool wrap = false)
        {
            Text(gui, text, GetTextSettings(wrap), rect);
        }
        
        public static void Text(this ImGui gui, ReadOnlySpan<char> text, ImRect rect, Color32 color, bool wrap = false)
        {
            Text(gui, text, GetTextSettings(wrap), rect, color);
        }

        public static void TextAutoSize(this ImGui gui, ReadOnlySpan<char> text, ImRect rect, bool wrap = false)
        {
            TextAutoSize(gui, text, rect, ImTheme.Active.Text.Color, wrap);
        }
        
        public static void TextAutoSize(this ImGui gui, ReadOnlySpan<char> text, ImRect rect, Color32 color, bool wrap = false)
        {
            // (artem-s): at least try to skip costly auto-sizing
            if (gui.Canvas.Cull(rect))
            {
                return;
            }
            
            var settings = GetTextSettings(wrap);
            settings.Size = AutoSizeTextSlow(gui, text, settings, rect.Size);
            Text(gui, text, settings, rect, color);
        }
        
        public static void Text(this ImGui gui, ReadOnlySpan<char> text, in ImTextSettings settings)
        {
            Text(gui, text, in settings, ImTheme.Active.Text.Color);
        }
        
        public static void Text(this ImGui gui, ReadOnlySpan<char> text, in ImTextSettings settings, Color32 color)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            
            var space = gui.Layout.GetAvailableSize().Max(MIN_WIDTH, MIN_HEIGHT);
            var rect = gui.Layout.GetRect(space);
            gui.Canvas.Text(text, color, rect, in settings, out var textRect);
            gui.Layout.AddRect(textRect);
        }
        
        public static void Text(this ImGui gui, ReadOnlySpan<char> text, in ImTextSettings settings, ImRect rect)
        {
            gui.Canvas.Text(text, ImTheme.Active.Text.Color, rect, in settings);
        }
        
        public static void Text(this ImGui gui, ReadOnlySpan<char> text, in ImTextSettings settings, ImRect rect, Color32 color)
        {
            gui.Canvas.Text(text, color, rect, in settings);
        }

        public static ImTextSettings GetTextSettings(bool wrap)
        {
            return new ImTextSettings(ImTheme.Active.Controls.TextSize, ImTheme.Active.Text.Alignment, wrap);
        }

        // TODO (artem-s): Got to come up with better solution instead of just brute forcing the fuck of it every time
        public static float AutoSizeTextSlow(this ImGui gui, ReadOnlySpan<char> text, ImTextSettings settings, Vector2 bounds, float minSize = 1)
        {
            var textSize = gui.MeasureTextSize(text, in settings, bounds);
            while (settings.Size > minSize && (textSize.x > bounds.x || textSize.y > bounds.y))
            {
                settings.Size -= 1;
                textSize = gui.MeasureTextSize(text, in settings, bounds);
            }

            return settings.Size;
        }

        public static float GetFontSizeForContainerHeight(this ImGui gui, float containerHeight)
        {
            var scale = containerHeight / gui.TextDrawer.FontLineHeight;
            return scale * gui.TextDrawer.FontRenderSize;
        }
        
        public static Vector2 MeasureTextSize(this ImGui gui, ReadOnlySpan<char> text, in ImTextSettings textSettings, Vector2 bounds = default)
        {
            ref readonly var textLayout = ref gui.TextDrawer.BuildTempLayout(text, 
                bounds.x, 
                bounds.y,
                textSettings.Align.X,
                textSettings.Align.Y, 
                textSettings.Size,
                textSettings.Wrap);

            return new Vector2(textLayout.Width, textLayout.Height);
        }
    }
    
    [Serializable]
    public struct ImTextStyle
    {
        public Color32 Color;
        public ImTextAlignment Alignment;
    }
}