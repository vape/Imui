using System;
using Imui.Core;
using Imui.Style;
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
                    var boxSize = gui.Style.Layout.TextSize;
                    
                    if (label.IsEmpty)
                    {
                        return gui.Layout.AddRect(boxSize, gui.GetRowHeight());
                    }

                    var textSettings = GetTextSettings(gui);
                    var textSize = gui.MeasureTextSize(label, in textSettings);
                    var width = boxSize + gui.Style.Layout.InnerSpacing + textSize.x;
                    var height = Mathf.Max(boxSize, textSize.y);
                    
                    if (size.Type != ImSizeType.Fit && gui.Layout.Axis != ImAxis.Horizontal)
                    {
                        width = Mathf.Max(gui.GetLayoutWidth(), width);
                    }
                        
                    return gui.Layout.AddRect(width, height);
            }
        }
        
        public static bool Checkbox(this ImGui gui, bool value, ReadOnlySpan<char> label = default, ImSize size = default)
        {
            Checkbox(gui, ref value, label, size);
            return value;
        }
        
        public static bool Checkbox(this ImGui gui, ref bool value, ReadOnlySpan<char> label = default, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rect = GetRect(gui, size, label);
            return Checkbox(gui, ref value, label, rect);
        }

        public static bool Checkbox(this ImGui gui, bool value, ReadOnlySpan<char> label, ImRect rect)
        {
            Checkbox(gui, ref value, label, rect);
            return value;
        }
        
        public static bool Checkbox(this ImGui gui, ref bool value, ReadOnlySpan<char> label, ImRect rect)
        {
            var id = gui.GetNextControlId();
            var boxSize = gui.Style.Layout.TextSize;
            var boxRect = rect.SplitLeft(boxSize, out var textRect).WithAspect(1.0f);
            var changed = Checkbox(gui, id, ref value, boxRect);

            if (label.IsEmpty)
            {
                return changed;
            }

            var textSettings = GetTextSettings(gui);
            
            textRect.X += gui.Style.Layout.InnerSpacing;
            textRect.W -= gui.Style.Layout.InnerSpacing;
            gui.Canvas.Text(label, gui.Style.Text.Color, textRect, textSettings);
            
            if (gui.InvisibleButton(id, textRect, ImButtonFlag.ActOnPress))
            {
                value = !value;
                changed = true;
            }

            return changed;
        }
        
        public static bool Checkbox(this ImGui gui, uint id, ref bool value, ImRect rect)
        {
            ref readonly var style = ref (value ? ref gui.Style.Checkbox.Checked : ref gui.Style.Checkbox.Normal);
            
            using var _ = new ImStyleScope<ImStyleButton>(ref gui.Style.Button, in style);
            
            var clicked = gui.Button(id, rect, out var state);
            var frontColor = ImButton.GetStateFrontColor(gui, state);

            if (value)
            {
                var checkmarkRect = rect.ScaleFromCenter(gui.Style.Checkbox.CheckmarkScale);
                var checkmarkStrokeWidth = checkmarkRect.W * 0.2f;
                
                DrawCheckmark(gui.Canvas, checkmarkRect, frontColor, checkmarkStrokeWidth);
            }
            
            if (clicked)
            {
                value = !value;
            }

            return clicked;
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
        
        public static ImTextSettings GetTextSettings(ImGui gui)
        {
            return new ImTextSettings(gui.Style.Layout.TextSize, 0.0f, 0.5f, false);
        }
    }
}