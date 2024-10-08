using System;
using Imui.Controls.Styling;
using Imui.Core;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImFoldout
    {
        private const float VERTICAL_ARROW_ASPECT_RATIO = 1.1547f; // ~ 2/sqrt(3)
        private const float HORIZONTAL_ARROW_ASPECT_RATIO = 1 / VERTICAL_ARROW_ASPECT_RATIO;
        
        public static void BeginFoldout(this ImGui gui, out bool open, ReadOnlySpan<char> label, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            
            var id =  gui.PushId(label);
            var rect = ImControls.AddRowRect(gui, size);
            ref var state = ref gui.Storage.Get<bool>(id);
            state = DrawFoldout(gui, id, state, label, rect);
            open = state;
        }
        
        public static void EndFoldout(this ImGui gui)
        {
            gui.PopId();
        }
        
        public static bool DrawFoldout(ImGui gui, uint id, bool open, ReadOnlySpan<char> label, ImRect rect)
        {
            using var _ = new ImStyleScope<ImButtonStyle>(ref ImTheme.Active.Button, ImTheme.Active.Foldout.Button);

            var textSettings = ImButton.CreateTextSettings();
            var arrowSize = ImTheme.Active.Controls.TextSize;
            var contentRect = ImButton.CalculateContentRect(rect);
            var arrowRect = contentRect.SplitLeft(arrowSize, ImTheme.Active.Controls.InnerSpacing, out var labelRect).WithAspect(1.0f);

            if (gui.Button(id, rect, out var state))
            {
                open = !open;
            }
            
            var frontColor = ImButton.GetStateFrontColor(state);
            
            if (open)
            {
                DrawArrowDown(gui.Canvas, arrowRect, frontColor, ImTheme.Active.Foldout.ArrowScale);
            }
            else
            {
                DrawArrowRight(gui.Canvas, arrowRect, frontColor, ImTheme.Active.Foldout.ArrowScale);
            }
            
            gui.Text(label, in textSettings, labelRect);

            return open;
        }
        
        public static void DrawArrowRight(ImCanvas canvas, ImRect rect, Color32 color, float scale = 1.0f)
        {
            if (scale != 1.0f)
            {
                rect = rect.ScaleFromCenter(scale);
            }
            
            rect = rect.WithAspect(HORIZONTAL_ARROW_ASPECT_RATIO);
            
            Span<Vector2> points = stackalloc Vector2[3]
            {
                new Vector2(rect.X + rect.W, rect.Y + rect.H * 0.5f),
                new Vector2(rect.X, rect.Y + rect.H),
                new Vector2(rect.X, rect.Y)
            };
        
            canvas.ConvexFill(points, color);
        }

        public static void DrawArrowDown(ImCanvas canvas, ImRect rect, Color32 color, float scale = 1.0f)
        {
            if (scale != 1.0f)
            {
                rect = rect.ScaleFromCenter(scale);
            }

            rect = rect.WithAspect(VERTICAL_ARROW_ASPECT_RATIO);
            
            Span<Vector2> points = stackalloc Vector2[3]
            {
                new Vector2(rect.X + rect.W * 0.5f, rect.Y),
                new Vector2(rect.X + rect.W, rect.Y + rect.H),
                new Vector2(rect.X, rect.Y + rect.H),
            };
        
            canvas.ConvexFill(points, color);
        }
    }

    [Serializable]
    public struct ImFoldoutStyle
    {
        public float ArrowScale;
        public ImButtonStyle Button;
    }
}