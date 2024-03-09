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
            using var _ = new ImStyleScope<ImButtonStyle>(ref ImButton.Style);
            ImButton.Style.Padding.Right += Style.ArrowWidth;

            var id = gui.GetNextControlId();
            var size = ImButton.MeasureSize(gui, in label);
            var rect = gui.Layout.AddRect(size);
            var clicked = gui.Button(id, rect, out var content, out var style);
            
            gui.Canvas.Text(in label, style.FrontColor, content, in ImButton.Style.Text);
            
            var arrowRect = content;
            arrowRect.X += arrowRect.W;
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
    }

    public struct ImSelectStyle
    {
        public static readonly ImSelectStyle Default = new()
        {
            ArrowWidth = 24,
            ArrowPadding = 6
        };
        
        public float ArrowWidth;
        public ImPadding ArrowPadding;
    }
}