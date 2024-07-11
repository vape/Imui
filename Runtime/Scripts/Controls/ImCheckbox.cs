using System;
using Imui.Core;
using Imui.Controls.Styling;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImCheckbox
    {
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

                    var textSettings = GetTextSettings();
                    var textSize = gui.MeasureTextSize(label, in textSettings);
                    var width = size.Type == ImSizeType.Fit
                        ? boxSize + ImTheme.Active.Controls.InnerSpacing + textSize.x
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

            var textSettings = GetTextSettings();
            
            textRect.X += ImTheme.Active.Controls.InnerSpacing;
            textRect.W -= ImTheme.Active.Controls.InnerSpacing;
            gui.Canvas.Text(label, ImTheme.Active.Text.Color, textRect, textSettings);
            
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
            var clicked = gui.Button(id, rect, out var state);
            var style = ImButton.GetStateStyle(state);

            if (value)
            {
                var checkmarkRect = rect.ScaleFromCenter(ImTheme.Active.Checkbox.CheckmarkScale);
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
        
        public static ImTextSettings GetTextSettings()
        {
            return new ImTextSettings(ImTheme.Active.Controls.TextSize, ImTheme.Active.Checkbox.TextAlignment, ImTheme.Active.Checkbox.WrapText);
        }
    }

    public struct ImCheckboxStyle
    {
        public float CheckmarkScale;
        public ImTextAlignment TextAlignment;
        public bool WrapText;
    }
}