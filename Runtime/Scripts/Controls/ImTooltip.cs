using System;
using Imui.Core;
using UnityEngine;

namespace Imui.Controls
{
    [Flags]
    public enum ImTooltipShow
    {
        None = 0,
        OnHover = 1 << 0,
        OnActive = 1 << 1
    }

    public static class ImTooltip
    {
        public static void TooltipAtLastControl(this ImGui gui, ReadOnlySpan<char> text, ImTooltipShow show = ImTooltipShow.OnHover)
        {
            TooltipAtControl(gui, gui.LastControl, text, show);
        }

        public static void TooltipAtControl(this ImGui gui, uint control, ReadOnlySpan<char> text, ImTooltipShow show = ImTooltipShow.OnHover)
        {
            var shouldShow = false;

            shouldShow |= (show & ImTooltipShow.OnHover) != 0 && gui.IsControlHovered(control);
            shouldShow |= (show & ImTooltipShow.OnActive) != 0 && gui.IsControlActive(control);

            if (!shouldShow)
            {
                return;
            }

            Tooltip(gui, text, gui.Input.MousePosition + gui.Style.Tooltip.OffsetPixels / gui.Canvas.ScreenScale);
        }

        public static void Tooltip(this ImGui gui, ReadOnlySpan<char> text, Vector2 position)
        {
            var textSettings = GetTextSettings(gui);
            var textSize = gui.MeasureTextSize(text, textSettings);
            var width = textSize.x + gui.Style.Tooltip.Padding.Horizontal;
            var height = textSize.y + gui.Style.Tooltip.Padding.Vertical;
            var rect = new ImRect(position.x, gui.Style.Tooltip.AboveCursor ? position.y : position.y - height, width, height);
            
            // fit rect to screen
            var bounds = gui.Canvas.ScreenRect;
            var right = bounds.Right;
            var bottom = bounds.Bottom;
            var top = bounds.Top;
            var left = bounds.Left;
            
            if (rect.X < left)
                rect.X = left;
            else if(rect.Right > right)
                rect.X -= rect.Right - right;
            
            if (rect.Top > top)
                rect.Y -= rect.Top - top;
            else if (rect.Bottom < bottom)
                rect.Y -= rect.Bottom - bottom;

            Tooltip(gui, text, rect);
        }

        public static void Tooltip(this ImGui gui, ReadOnlySpan<char> text, ImRect rect)
        {
            gui.BeginPopup();
            gui.Box(rect, gui.Style.Tooltip.Box);
            gui.Text(text, GetTextSettings(gui), rect);
            gui.EndPopup();
        }

        public static ImTextSettings GetTextSettings(ImGui gui)
        {
            return new ImTextSettings(gui.Style.Layout.TextSize, 0.5f, 0.5f);
        }
    }
}