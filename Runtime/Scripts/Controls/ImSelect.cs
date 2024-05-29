using System;
using Imui.Core;
using Imui.Styling;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImSelect
    {
        private const float ARROW_ASPECT_RATIO = 1.1547f; // ~ 2/sqrt(3)

        public static ImSelectStyle Style = ImSelectStyle.Default;
        
        public static bool Select(this ImGui gui, in ReadOnlySpan<char> label)
        {
            gui.AddControlSpacing();

            var textSettings = GetTextSettings(gui);
            var contentSize = gui.MeasureTextSize(label, in textSettings);
            contentSize.x += Style.ArrowWidth;
            var rect = gui.Layout.AddRect(Style.Button.GetButtonSize(contentSize));
            return Select(gui, label, in rect);
        }

        public static bool Select(this ImGui gui, in ReadOnlySpan<char> label, float width, float height)
        {
            gui.AddControlSpacing();

            var rect = gui.Layout.AddRect(width, height);
            return Select(gui, label, in rect);
        }
        
        public static bool Select(this ImGui gui, in ReadOnlySpan<char> label, Vector2 size)
        {
            gui.AddControlSpacing();

            var rect = gui.Layout.AddRect(size);
            return Select(gui, label, in rect);
        }

        public static bool Select(this ImGui gui, in ReadOnlySpan<char> label, in ImRect rect)
        {
            using var _ = new ImStyleScope<ImButtonStyle>(ref ImButton.Style, Style.Button);
            
            var id = gui.GetNextControlId();
            var clicked = gui.Button(id, rect, out var state);
            var style = Style.Button.GetStyle(state);
            var content = Style.Button.GetContentRect(rect);
            var textSettings = GetTextSettings(gui);
            
            gui.Canvas.Text(in label, style.FrontColor, content, in textSettings);
            
            var arrowRect = content;
            arrowRect.X += arrowRect.W - Style.ArrowWidth;
            arrowRect.W = Style.ArrowWidth;
            arrowRect = arrowRect.WithAspect(1f).WithAspect(ARROW_ASPECT_RATIO).WithPadding(Style.ArrowPadding);
            
            Span<Vector2> points = stackalloc Vector2[3]
            {
                new Vector2(arrowRect.X + arrowRect.W * 0.5f, arrowRect.Y),
                new Vector2(arrowRect.X + arrowRect.W, arrowRect.Y + arrowRect.H),
                new Vector2(arrowRect.X, arrowRect.Y + arrowRect.H),
            };
        
            gui.Canvas.ConvexFill(points, style.FrontColor);

            return clicked;
        }

        private static ImTextSettings GetTextSettings(ImGui gui)
        {
            return new ImTextSettings(gui.GetTextSize(), Style.Button.Alignment);
        }
    }

    public struct ImSelectStyle
    {
        public static readonly ImSelectStyle Default = new()
        {
            ArrowWidth = 24,
            ArrowPadding = 6,
            Button = CreateDefaultButtonStyle()
        };

        private static ImButtonStyle CreateDefaultButtonStyle()
        {
            var style = ImButtonStyle.Default;
            style.Alignment.X = 0;
            return style;
        }
        
        public float ArrowWidth;
        public ImPadding ArrowPadding;
        public ImButtonStyle Button;
    }
}