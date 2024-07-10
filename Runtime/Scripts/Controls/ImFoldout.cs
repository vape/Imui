using System;
using Imui.Controls.Styling;
using Imui.Core;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImFoldout
    {
        private const float ARROW_ASPECT_RATIO = 1.1547f; // ~ 2/sqrt(3)

        public static ImFoldoutStyle Style = ImFoldoutStyle.Default;
        
        public static void BeginFoldout(this ImGui gui, in ReadOnlySpan<char> label, out bool open, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            
            var id =  gui.PushId(label);
            var rect = ImControls.GetRowRect(gui, size);
            ref var state = ref gui.Storage.Get<bool>(id);
            Foldout(gui, id, ref state, in rect, in label);
            open = state;
        }

        public static void BeginFoldout(this ImGui gui, in ReadOnlySpan<char> label, ref bool open, in ImRect rect)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            
            var id = gui.PushId(label);
            Foldout(gui, id, ref open, in rect, in label);
        }
        
        public static void EndFoldout(this ImGui gui)
        {
            gui.PopId();
        }

        public static void Foldout(this ImGui gui, uint id, ref bool open, in ImRect rect, in ReadOnlySpan<char> label)
        {
            Foldout(gui, id, ref open, in rect, out var state);

            var textRect = Style.Button.GetContentRect(rect);
            var arrowOffset = textRect.H * Style.ArrowOuterScale + ImControls.Style.InnerSpacing;
            textRect.X += arrowOffset;
            textRect.W -= arrowOffset;
            
            using (new ImStyleScope<ImTextStyle>(ref ImText.Style))
            {
                ImText.Style.Color = ImButton.Style.GetStateStyle(state).FrontColor;
                
                gui.Text(in label, GetTextSettings(), textRect);
            }
        }
        
        public static void Foldout(this ImGui gui, uint id, ref bool open, in ImRect rect, out ImButtonState state)
        {
            using var __ = new ImStyleScope<ImButtonStyle>(ref ImButton.Style, Style.Button);

            var arrowRect = GetArrowRect(rect);
            var clicked = gui.Button(id, in rect, out state);
            var style = ImButton.Style.GetStateStyle(state);
            
            if (open)
            {
                DrawOpenArrow(gui.Canvas, arrowRect, style.FrontColor);
            }
            else
            {
                DrawClosedArrow(gui.Canvas, arrowRect, style.FrontColor);
            }
            
            if (clicked)
            {
                open = !open;
            }
        }

        public static void DrawClosedArrow(ImCanvas canvas, ImRect rect, Color32 color)
        {
            var arrowRect = rect.WithAspect(1.0f).ScaleFromCenter(Style.ArrowInnerScale).WithAspect(1.0f / ARROW_ASPECT_RATIO);
            
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
            var arrowRect = rect.WithAspect(1.0f).ScaleFromCenter(Style.ArrowInnerScale).WithAspect(ARROW_ASPECT_RATIO);
            
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
            return new ImTextSettings(ImControls.Style.TextSize, Style.Button.Alignment, Style.Button.TextWrap);
        }

        public static ImRect GetArrowRect(ImRect rect)
        {
            var arrowRect = Style.Button.GetContentRect(rect);
            arrowRect.W = arrowRect.H * Style.ArrowOuterScale;
            return arrowRect;
        }
    }

    public struct ImFoldoutStyle
    {
        public static readonly ImFoldoutStyle Default = CreateDefaultStyle();

        private static ImFoldoutStyle CreateDefaultStyle()
        {
            var style = new ImFoldoutStyle()
            {
                ArrowInnerScale = 0.7f,
                ArrowOuterScale = 0.7f,
                Button = ImButtonStyle.Default
            };

            style.Button.Alignment.X = 0.0f;
            style.Button.SetBorderWidth(0);
            return style;
        }

        public float ArrowInnerScale;
        public float ArrowOuterScale;
        public ImButtonStyle Button;
    }
}