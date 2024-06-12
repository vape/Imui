using System;
using Imui.Core;
using Imui.Controls.Styling;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImCheckbox
    {
        public static ImCheckboxStyle Style = ImCheckboxStyle.Default;
        
        public static void Checkbox(this ImGui gui, ref bool value)
        {
            gui.AddControlSpacing();

            var id = gui.GetNextControlId();
            var size = GetCheckmarkBoxSize(gui);
            var rect = gui.Layout.AddRect(size, size);
            Checkbox(gui, id, ref value, in rect);
        }
        
        public static void Checkbox(this ImGui gui, ref bool value, in ReadOnlySpan<char> label)
        {
            gui.AddControlSpacing();

            var textSettings = GetTextSettings();
            var textSize = gui.MeasureTextSize(in label, in textSettings);
            var checkmarkBoxSize = GetCheckmarkBoxSize(gui);
            var rect = gui.Layout.AddRect(
                checkmarkBoxSize + ImControls.Style.InnerSpacing + textSize.x, 
                Mathf.Max(textSize.y, checkmarkBoxSize));
            Checkbox(gui, ref value, in label, in rect);
        }

        public static void Checkbox(this ImGui gui, ref bool value, in ReadOnlySpan<char> label, float width, float height)
        {
            gui.AddControlSpacing();
            
            var rect = gui.Layout.AddRect(width, height);
            Checkbox(gui, ref value, in label, in rect);
        }
        
        public static void Checkbox(this ImGui gui, ref bool value, in ReadOnlySpan<char> label, Vector2 size)
        {
            gui.AddControlSpacing();
            
            var rect = gui.Layout.AddRect(size);
            Checkbox(gui, ref value, in label, in rect);
        }
                
        public static void Checkbox(this ImGui gui, ref bool value, in ReadOnlySpan<char> label, in ImRect rect)
        {
            var id = gui.GetNextControlId();
            var checkmarkBox = rect.SplitLeft(GetCheckmarkBoxSize(gui), out var textRect).WithAspect(1.0f);
            Checkbox(gui, id, ref value, in checkmarkBox);

            textRect.X += ImControls.Style.InnerSpacing;
            textRect.W -= ImControls.Style.InnerSpacing;
            gui.Canvas.Text(in label, Style.TextColor, textRect, GetTextSettings());
            
            if (gui.InvisibleButton(id, textRect))
            {
                value = !value;
            }
        }

        public static void Checkbox(this ImGui gui, ref bool value, in ImRect rect)
        {
            var id = gui.GetNextControlId();
            Checkbox(gui, id, ref value, in rect);
        }
        
        public static void Checkbox(this ImGui gui, uint id, ref bool value, in ImRect rect)
        {
            using var _ = new ImStyleScope<ImButtonStyle>(ref ImButton.Style, Style.Button);
            
            var clicked = gui.Button(id, in rect, out var state);
            var style = Style.Button.GetStyle(state);

            if (value)
            {
                var checkmarkRect = Style.Button.GetContentRect(rect);
                var checkmarkStrokeWidth = checkmarkRect.W * 0.2f;
                
                DrawCheckMark(gui.Canvas, checkmarkRect, style.FrontColor, checkmarkStrokeWidth);
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

            canvas.LineMiter(path, color, false, thickness);
        }

        public static ImTextSettings GetTextSettings()
        {
            return new ImTextSettings(ImControls.Style.TextSize, Style.Button.Alignment);
        }

        public static float GetCheckmarkBoxSize(ImGui gui)
        {
            return gui.GetRowHeight();
        }
    }

    public struct ImCheckboxStyle
    {
        public static readonly ImCheckboxStyle Default = CreateDefaultStyle();

        public static ImCheckboxStyle CreateDefaultStyle()
        {
            var style = new ImCheckboxStyle()
            {
                Button = ImButtonStyle.Default,
                TextColor = ImColors.Black
            };

            style.Button.Padding = 4.0f;
            return style;
        }

        public ImButtonStyle Button;
        public Color32 TextColor;
    }
}