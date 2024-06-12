using System;
using Imui.Core;
using Imui.Controls.Styling;
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

            var textSettings = GetTextSettings();
            var contentSize = gui.MeasureTextSize(label, in textSettings);
            contentSize.x += ImControls.Style.InnerSpacing + GetArrowSize(gui);
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
            var id = gui.GetNextControlId();
            return Select(gui, id, in label, in rect);
        }

        public static bool Select(this ImGui gui, uint id, in ReadOnlySpan<char> label, in ImRect rect)
        {
            using var _ = new ImStyleScope<ImButtonStyle>(ref ImButton.Style, Style.Button);
            
            var arrowSize = GetArrowSize(gui);
            var clicked = gui.Button(id, rect, out var state);
            var style = Style.Button.GetStyle(state);
            var content = Style.Button.GetContentRect(rect);
            content = content.SplitLeft(content.W - arrowSize - ImControls.Style.InnerSpacing, out var arrowRect);
            var textSettings = GetTextSettings();
            
            gui.Canvas.Text(in label, style.FrontColor, content, in textSettings);

            arrowRect.X += ImControls.Style.InnerSpacing;
            arrowRect.W -= ImControls.Style.InnerSpacing;
            arrowRect = arrowRect.WithAspect(1f).ScaleFromCenter(Style.ArrowScale).WithAspect(ARROW_ASPECT_RATIO);

            Span<Vector2> points = stackalloc Vector2[3]
            {
                new Vector2(arrowRect.X + arrowRect.W * 0.5f, arrowRect.Y),
                new Vector2(arrowRect.X + arrowRect.W, arrowRect.Y + arrowRect.H),
                new Vector2(arrowRect.X, arrowRect.Y + arrowRect.H),
            };
        
            gui.Canvas.ConvexFill(points, style.FrontColor);

            return clicked;
        }

        public static ImTextSettings GetTextSettings()
        {
            return new ImTextSettings(ImControls.Style.TextSize, Style.Button.Alignment);
        }

        public static float GetArrowSize(ImGui gui)
        {
            return gui.GetRowHeight();
        }
    }

    public struct ImSelectStyle
    {
        public static readonly ImSelectStyle Default = CreateDefaultStyle();

        public static ImSelectStyle CreateDefaultStyle()
        {
            var style = new ImSelectStyle()
            {
                ArrowScale = 0.5f,
                Button = ImButtonStyle.Default
            };
            
            style.Button.Alignment.X = 0;
            return style;
        }

        public float ArrowScale;
        public ImButtonStyle Button;
    }
}