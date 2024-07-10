using System;
using Imui.Core;
using Imui.Controls.Styling;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImCheckbox
    {
        public static ImCheckboxStyle Style = ImCheckboxStyle.Default;

        public static ImRect GetRect(ImGui gui, ImSize size, ReadOnlySpan<char> label = default)
        {
            switch (size.Type)
            {
                case ImSizeType.Fixed:
                    return gui.Layout.AddRect(size.Width, size.Height);
                default:
                    var boxSize = GetBoxSize(gui);
                    
                    if (label.IsEmpty)
                    {
                        return gui.Layout.AddRect(boxSize, boxSize);
                    }

                    var textSettings = Style.GetTextSettings();
                    var textSize = gui.MeasureTextSize(label, in textSettings);
                    var width = size.Type == ImSizeType.Fit
                        ? boxSize + ImControls.InnerSpacing + textSize.x
                        : gui.GetLayoutWidth();
                    var height = Mathf.Max(boxSize, textSize.y);
                        
                    return gui.Layout.AddRect(width, height);
            }
        }
        
        public static void Checkbox(this ImGui gui, ref bool value, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rect = GetRect(gui, size);
            Checkbox(gui, ref value, rect);
        }
        
        public static void Checkbox(this ImGui gui, ref bool value, ReadOnlySpan<char> label, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rect = GetRect(gui, size, label);
            Checkbox(gui, ref value, label, rect);
        }
        
        public static void Checkbox(this ImGui gui, ref bool value, ReadOnlySpan<char> label, ImRect rect)
        {
            var id = gui.GetNextControlId();
            var boxSize = GetBoxSize(gui);
            var boxRect = rect.SplitLeft(boxSize, out var textRect).WithAspect(1.0f);
            
            Checkbox(gui, id, ref value, boxRect);

            if (label.IsEmpty)
            {
                return;
            }

            var textSettings = Style.GetTextSettings();
            
            textRect.X += ImControls.Style.InnerSpacing;
            textRect.W -= ImControls.Style.InnerSpacing;
            gui.Canvas.Text(in label, Style.TextColor, textRect, textSettings);
            
            if (gui.InvisibleButton(id, textRect))
            {
                value = !value;
            }
        }

        public static void Checkbox(this ImGui gui, ref bool value, ImRect rect)
        {
            var id = gui.GetNextControlId();
            Checkbox(gui, id, ref value, rect);
        }
        
        public static void Checkbox(this ImGui gui, uint id, ref bool value, ImRect rect)
        {
            using var _ = new ImStyleScope<ImButtonStyle>(ref ImButton.Style, Style.Checkbox);
            
            var clicked = gui.Button(id, in rect, out var state);
            var style = Style.Checkbox.GetStateStyle(state);

            if (value)
            {
                var checkmarkRect = rect.ScaleFromCenter(Style.CheckmarkScale);
                var checkmarkStrokeWidth = checkmarkRect.W * 0.2f;
                
                DrawCheckmark(gui.Canvas, checkmarkRect, style.FrontColor, checkmarkStrokeWidth);
            }
            
            if (clicked)
            {
                value = !value;
            }
        }

        public static void DrawCheckmark(ImCanvas canvas, ImRect rect, Color32 color, float thickness)
        {
            ReadOnlySpan<Vector2> path = stackalloc Vector2[3]
            {
                rect.GetPointAtNormalPosition(0.00f, 0.60f), 
                rect.GetPointAtNormalPosition(0.35f, 0.15f), 
                rect.GetPointAtNormalPosition(1.00f, 0.80f)
            };

            canvas.LineMiter(path, color, false, thickness);
        }

        public static float GetBoxSize(ImGui gui)
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
                CheckmarkScale = 0.6f,
                Checkbox = ImButtonStyle.Default,
                TextColor = ImColors.Black,
                WrapText = false
            };

            style.Checkbox.Alignment.X = 0.0f;
            style.Checkbox.Alignment.Y = 0.5f;

            return style;
        }

        public float CheckmarkScale;
        public ImButtonStyle Checkbox;
        public bool WrapText;
        public Color32 TextColor;
        
        public ImTextSettings GetTextSettings()
        {
            return new ImTextSettings(ImControls.Style.TextSize, Checkbox.Alignment, WrapText);
        }
    }
}