using System;
using Imui.Core;
using Imui.Styling;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImCheckmark
    {
        public static ImCheckmarkStyle Style = ImCheckmarkStyle.Default;
        
        public static void Checkmark(this ImGui gui, ref bool value)
        {
            gui.AddControlSpacing();

            var size = GetCheckmarkBoxSize(gui);
            var rect = gui.Layout.AddRect(size, size);
            Checkmark(gui, ref value, in rect, in rect);
        }
        
        public static void Checkmark(this ImGui gui, ref bool value, in ReadOnlySpan<char> label)
        {
            gui.AddControlSpacing();

            var textSettings = GetTextSettings(gui);
            var textSize = gui.MeasureTextSize(in label, in textSettings);

            var checkmarkBoxSize = GetCheckmarkBoxSize(gui);
            var rect = gui.Layout.AddRect(
                textSize.x + checkmarkBoxSize + Style.Space, 
                Mathf.Max(textSize.y, checkmarkBoxSize));
            Checkmark(gui, ref value, in label, in rect);
        }

        public static void Checkmark(this ImGui gui, ref bool value, in ReadOnlySpan<char> label, float width, float height)
        {
            gui.AddControlSpacing();
            
            var rect = gui.Layout.AddRect(width, height);
            Checkmark(gui, ref value, in label, in rect);
        }
        
        public static void Checkmark(this ImGui gui, ref bool value, in ReadOnlySpan<char> label, Vector2 size)
        {
            gui.AddControlSpacing();
            
            var rect = gui.Layout.AddRect(size);
            Checkmark(gui, ref value, in label, in rect);
        }
                
        public static void Checkmark(this ImGui gui, ref bool value, in ReadOnlySpan<char> label, in ImRect rect)
        {
            var checkmarkBox = rect.SplitLeft(GetCheckmarkBoxSize(gui), out var textRect);
            Checkmark(gui, ref value, checkmarkBox, in rect);

            textRect.X += Style.Space;

            gui.Canvas.Text(in label, Style.TextColor, textRect, GetTextSettings(gui));
        }
        
        public static void Checkmark(this ImGui gui, ref bool value, in ImRect rect, in ImRect clickable)
        {
            using var _ = new ImStyleScope<ImButtonStyle>(ref ImButton.Style, Style.Button);
            
            var buttonRect = rect.WithAspect(1.0f);
            var id = gui.GetNextControlId();
            var clicked = gui.Button(id, in buttonRect, out var content, out var style, in clickable);

            if (value)
            {
                var checkMarkRect = buttonRect.WithPadding(Style.CheckmarkPadding);
                DrawCheckMark(gui.Canvas, checkMarkRect, style.FrontColor, checkMarkRect.W * 0.2f);
            }
            
            if (clicked)
            {
                value = !value;
            }
        }

        public static void DrawCheckMark(ImCanvas canvas, ImRect rect, Color32 color, float thickness)
        {
            ReadOnlySpan<Vector2> path = stackalloc Vector2[3]
            {
                rect.GetPointAtNormalPosition(0.00f, 0.60f), 
                rect.GetPointAtNormalPosition(0.35f, 0.15f), 
                rect.GetPointAtNormalPosition(1.00f, 0.80f)
            };

            canvas.Line(path, color, false, thickness);
        }

        private static ImTextSettings GetTextSettings(ImGui gui)
        {
            return new ImTextSettings(gui.GetTextSize(), Style.Button.Alignment);
        }

        private static float GetCheckmarkBoxSize(ImGui gui)
        {
            return gui.GetRowHeight();
        }
    }

    public struct ImCheckmarkStyle
    {
        public static readonly ImCheckmarkStyle Default = new ImCheckmarkStyle()
        {
            Button = ImButtonStyle.Default,
            TextColor = ImColors.Black,
            Space = 2,
            CheckmarkPadding = 4
        };

        public float CheckmarkPadding;
        public ImButtonStyle Button;
        public Color32 TextColor;
        public float Space;
    }
}