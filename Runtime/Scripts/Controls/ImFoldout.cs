using System;
using Imui.Core;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImFoldout
    {
        private const float ARROW_ASPECT_RATIO = 1.1547f; // ~ 2/sqrt(3)

        public static ImFoldoutStyle Style = ImFoldoutStyle.Default;
        
        public static void BeginFoldout(this ImGui gui, in ReadOnlySpan<char> label, out bool open)
        {
            gui.AddControlSpacing();
            
            var id =  gui.PushId(label);
            var width = gui.Layout.GetAvailableWidth();
            var height = Style.GetHeight(gui.GetRowHeight());
            var rect = gui.Layout.AddRect(width, height);
            ref var state = ref gui.Storage.Get<bool>(id);
            Foldout(gui, id, ref state, in rect, in label);
            open = state;
        }
        
        public static void BeginFoldout(this ImGui gui, ref bool open, in ReadOnlySpan<char> label)
        {
            gui.AddControlSpacing();
            
            var width = gui.Layout.GetAvailableWidth();
            var height = Style.GetHeight(gui.GetRowHeight());
            BeginFoldout(gui, ref open, in label, gui.Layout.AddRect(width, height));
        }
        
        public static void BeginFoldout(this ImGui gui, ref bool open, in ReadOnlySpan<char> label, Vector2 size)
        {
            BeginFoldout(gui, ref open, in label, gui.Layout.AddRect(size));
        }
        
        public static void BeginFoldout(this ImGui gui, ref bool open, in ReadOnlySpan<char> label, float width, float height)
        {
            BeginFoldout(gui, ref open, in label, gui.Layout.AddRect(width, height));
        }
        
        public static void BeginFoldout(this ImGui gui, ref bool open, in ReadOnlySpan<char> label, in ImRect rect)
        {
            gui.AddControlSpacing();
            
            var id =  gui.PushId(label);
            Foldout(gui, id, ref open, in rect, in label);
        }
        
        public static void BeginFoldout(this ImGui gui, ref bool open, in ImRect rect, out ImRect contentRect)
        {
            gui.AddControlSpacing();
            
            var id = gui.PushId();
            Foldout(gui, id, ref open, in rect, out contentRect);
        }
        
        public static void EndFoldout(this ImGui gui)
        {
            gui.PopId();
        }

        public static void Foldout(this ImGui gui, uint id, ref bool open, in ImRect rect, in ReadOnlySpan<char> label)
        {
            Foldout(gui, id, ref open, in rect, out var contentRect);
            gui.Text(in label, GetTextSettings(), contentRect);
        }
        
        public static void Foldout(this ImGui gui, uint id, ref bool open, in ImRect rect, out ImRect contentRect)
        {
            var clicked = gui.Button(id, in rect, out var state);
            var content = ImButton.Style.GetContentRect(rect);
            var left = content.SplitLeft(GetArrowSize(gui), ImControls.Style.InnerSpacing, out contentRect);
            var style = ImButton.Style.GetStyle(state);
            
            if (open)
            {
                DrawOpenArrow(gui.Canvas, in left, style.FrontColor);
            }
            else
            {
                DrawClosedArrow(gui.Canvas, in left, style.FrontColor);
            }
            
            if (clicked)
            {
                open = !open;
            }
        }

        public static void DrawClosedArrow(ImCanvas canvas, in ImRect rect, Color32 color)
        {
            var arrowRect = rect.WithAspect(1.0f).ScaleFromCenter(Style.ArrowScale).WithAspect(1.0f / ARROW_ASPECT_RATIO);
            
            Span<Vector2> points = stackalloc Vector2[3]
            {
                new Vector2(arrowRect.X + arrowRect.W, arrowRect.Y + arrowRect.H * 0.5f),
                new Vector2(arrowRect.X, arrowRect.Y + arrowRect.H),
                new Vector2(arrowRect.X, arrowRect.Y)
            };
        
            canvas.ConvexFill(points, color);
        }

        public static void DrawOpenArrow(ImCanvas canvas, in ImRect rect, Color32 color)
        {
            var arrowRect = rect.WithAspect(1.0f).ScaleFromCenter(Style.ArrowScale).WithAspect(ARROW_ASPECT_RATIO);
            
            Span<Vector2> points = stackalloc Vector2[3]
            {
                new Vector2(arrowRect.X + arrowRect.W * 0.5f, arrowRect.Y),
                new Vector2(arrowRect.X + arrowRect.W, arrowRect.Y + arrowRect.H),
                new Vector2(arrowRect.X, arrowRect.Y + arrowRect.H),
            };
        
            canvas.ConvexFill(points, color);
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

    public struct ImFoldoutStyle
    {
        public static readonly ImFoldoutStyle Default = CreateDefaultStyle();

        private static ImFoldoutStyle CreateDefaultStyle()
        {
            var style = new ImFoldoutStyle()
            {
                ArrowScale = 0.5f,
                Button = ImButtonStyle.Default
            };

            style.Button.Alignment.X = 0.0f;
            style.Button.SetBorderWidth(0);
            return style;
        }

        public float ArrowScale;
        public ImButtonStyle Button;

        public float GetHeight(float contentHeight)
        {
            return contentHeight + Button.Padding.Vertical;
        }
    }
}