using System;
using Imui.Core;
using Imui.Style;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImFoldout
    {
        private const float VERTICAL_ARROW_ASPECT_RATIO = 1.1547f; // ~ 2/sqrt(3)
        private const float HORIZONTAL_ARROW_ASPECT_RATIO = 1 / VERTICAL_ARROW_ASPECT_RATIO;
        
        public static bool BeginFoldout(this ImGui gui, ReadOnlySpan<char> label, ImSize size = default, bool defaultOpen = false)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            
            var id = gui.PushId(label);
            var rect = gui.AddSingleRowRect(size);
            
            ref var open = ref gui.Storage.Get<bool>(id, defaultOpen);
            DrawFoldout(gui, id, ref open, label, rect);
            
            if (!open)
            {
                gui.PopId();
                return false;
            }

            return true;
        }
        
        public static void EndFoldout(this ImGui gui)
        {
            gui.PopId();
        }
        
        public static void DrawFoldout(ImGui gui, uint id, ref bool open, ReadOnlySpan<char> label, ImRect rect)
        {
            using var _ = gui.StyleScope(ref gui.Style.Button, gui.Style.Foldout.Button);

            var textSettings = ImButton.CreateTextSettings(gui);
            var arrowSize = gui.Style.Layout.TextSize;
            var contentRect = ImButton.CalculateContentRect(gui, rect);
            var arrowRect = contentRect.TakeLeft(arrowSize, gui.Style.Layout.InnerSpacing, out var labelRect).WithAspect(1.0f);

            if (gui.Button(id, rect, out var state))
            {
                open = !open;
            }
            
            var frontColor = ImButton.GetStateFrontColor(gui, state);
            
            if (open)
            {
                DrawArrowDown(gui.Canvas, arrowRect, frontColor, gui.Style.Foldout.ArrowScale);
            }
            else
            {
                DrawArrowRight(gui.Canvas, arrowRect, frontColor, gui.Style.Foldout.ArrowScale);
            }
            
            gui.Text(label, in textSettings, labelRect);
        }
        
        public static void DrawArrowRight(ImCanvas canvas, ImRect rect, Color32 color, float scale = 1.0f)
        {
            if (scale <= 0.0f)
            {
                return;
            }
            
            if (scale != 1.0f)
            {
                rect = rect.ScaleFromCenter(scale);
            }

            if (canvas.Cull(rect))
            {
                return;
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
            if (scale <= 0.0f)
            {
                return;
            }
            
            if (scale != 1.0f)
            {
                rect = rect.ScaleFromCenter(scale);
            }

            if (canvas.Cull(rect))
            {
                return;
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
}