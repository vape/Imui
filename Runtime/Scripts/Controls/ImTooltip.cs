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

            TooltipAtMouse(gui, text);
        }

        public static void TooltipAtMouse(this ImGui gui, ReadOnlySpan<char> text)
        {
            var size = CalculateSize(gui, text);
            var position = gui.Input.MousePosition;
            var rect = new ImRect(position.x, position.y, size.x, size.y);
            var safeRect = gui.Canvas.SafeScreenRect;

            if (rect.Right > safeRect.Right)
            {
                rect.X -= rect.W;
                rect.X -= gui.Style.Tooltip.OffsetPixels.x;
            }
            else
            {
                rect.X += gui.Style.Tooltip.OffsetPixels.x;
            }

            if (rect.Top > safeRect.Top)
            {
                rect.Y -= rect.H;
                rect.Y += gui.Style.Tooltip.OffsetPixels.y;
            }
            else
            {
                rect.Y -= gui.Style.Tooltip.OffsetPixels.y;
            }
            
            Tooltip(gui, text, rect);
        }

        public static void Tooltip(this ImGui gui, ReadOnlySpan<char> text, ImRect rect)
        {
            gui.BeginPopup();
            gui.Box(rect, gui.Style.Tooltip.Box);
            gui.Text(text, GetTextSettings(gui), rect);
            gui.EndPopup();
        }

        public static Vector2 CalculateSize(ImGui gui, ReadOnlySpan<char> text) => CalculateSize(gui, text, GetTextSettings(gui));
        public static Vector2 CalculateSize(ImGui gui, ReadOnlySpan<char> text, in ImTextSettings textSettings)
        {
            var textSize = gui.MeasureTextSize(text, textSettings);
            var width = textSize.x + gui.Style.Tooltip.Padding.Horizontal;
            var height = textSize.y + gui.Style.Tooltip.Padding.Vertical;
            
            return new Vector2(width, height);
        }

        public static ImTextSettings GetTextSettings(ImGui gui)
        {
            return new ImTextSettings(gui.Style.Layout.TextSize, 0.5f, 0.5f);
        }
    }
}