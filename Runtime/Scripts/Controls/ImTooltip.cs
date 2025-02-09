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

            Tooltip(gui, text, gui.Input.MousePosition + gui.Style.Tooltip.Offset);
        }

        public static void Tooltip(this ImGui gui, ReadOnlySpan<char> text, Vector2 position)
        {
            var textSettings = GetTextSettings(gui);
            var textSize = gui.MeasureTextSize(text, textSettings);
            var rect = new ImRect(position.x, position.y, textSize.x + gui.Style.Tooltip.Padding.Horizontal, textSize.y + gui.Style.Tooltip.Padding.Vertical);

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