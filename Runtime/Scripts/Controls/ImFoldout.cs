using System;
using Imui.Controls.Styling;
using Imui.Core;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImFoldout
    {
        private const float ARROW_ASPECT_RATIO = 1.1547f; // ~ 2/sqrt(3)
        
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
            var arrowRect = ImButton.GetContentRect(rect);
            var arrowSize = (arrowRect.H - ImTheme.Active.Controls.ExtraRowHeight) * ImTheme.Active.Foldout.ArrowOuterScale;
            arrowRect.W = arrowSize;

            using var _ = new ImStyleScope<ImButtonStyle>(ref ImTheme.Active.Button);

            ImTheme.Active.Button.BorderWidth = ImTheme.Active.Foldout.BorderWidth;
            ImTheme.Active.Button.Alignment = ImTheme.Active.Foldout.TextAlignment;
            ImTheme.Active.Button.Padding.Left += arrowRect.W + ImTheme.Active.Controls.InnerSpacing;

            if (gui.Button(id, label, rect, out var state))
            {
                open = !open;
            }
            
            var frontColor = ImButton.GetStateFontColor(state);
            
            if (open)
            {
                DrawOpenArrow(gui.Canvas, arrowRect, frontColor);
            }
            else
            {
                DrawClosedArrow(gui.Canvas, arrowRect, frontColor);
            }

            return open;
        }
        
        public static void DrawClosedArrow(ImCanvas canvas, ImRect rect, Color32 color)
        {
            var arrowRect = rect.WithAspect(1.0f).ScaleFromCenter(ImTheme.Active.Foldout.ArrowInnerScale).WithAspect(1.0f / ARROW_ASPECT_RATIO);
            
            Span<Vector2> points = stackalloc Vector2[3]
            {
                new Vector2(arrowRect.X + arrowRect.W, arrowRect.Y + arrowRect.H * 0.5f),
                new Vector2(arrowRect.X, arrowRect.Y + arrowRect.H),
                new Vector2(arrowRect.X, arrowRect.Y)
            };
        
            canvas.ConvexFill(points, color);
        }

        public static void DrawOpenArrow(ImCanvas canvas, ImRect rect, Color32 color)
        {
            var arrowRect = rect.WithAspect(1.0f).ScaleFromCenter(ImTheme.Active.Foldout.ArrowInnerScale).WithAspect(ARROW_ASPECT_RATIO);
            
            Span<Vector2> points = stackalloc Vector2[3]
            {
                new Vector2(arrowRect.X + arrowRect.W * 0.5f, arrowRect.Y),
                new Vector2(arrowRect.X + arrowRect.W, arrowRect.Y + arrowRect.H),
                new Vector2(arrowRect.X, arrowRect.Y + arrowRect.H),
            };
        
            canvas.ConvexFill(points, color);
        }
    }

    [Serializable]
    public struct ImFoldoutStyle
    {
        public float ArrowInnerScale;
        public float ArrowOuterScale;
        public float BorderWidth;
        public ImTextAlignment TextAlignment;
    }
}