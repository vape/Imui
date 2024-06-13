using System;
using Imui.Core;
using Imui.Controls.Styling;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImCheckbox
    {
        public static ImCheckboxStyle Style = ImCheckboxStyle.Default;
        
        public static void Checkbox(this ImGui gui, ref bool value, ImSize size = default)
        {
            gui.AddControlSpacing();

            var rect = size.Type switch
            {
                ImSizeType.FixedSize => gui.Layout.AddRect(size.Width, size.Height),
                _ => gui.Layout.AddRect(GetCheckmarkBoxRectSize(gui))
            };
            
            Checkbox(gui, ref value, in rect);
        }
        
        public static void Checkbox(this ImGui gui, ref bool value, ReadOnlySpan<char> label, ImSize size = default)
        {
            gui.AddControlSpacing();

            ImRect rect;
            
            switch (size.Type)
            {
                case ImSizeType.FixedSize:
                    rect = gui.Layout.AddRect(size.Width, size.Height);
                    break;
                default:
                    var textSettings = GetTextSettings();
                    var textSize = gui.MeasureTextSize(in label, in textSettings);
                    var checkmarkBoxSize = GetCheckmarkBoxSize(gui);
                    rect = gui.Layout.AddRect(
                        checkmarkBoxSize + ImControls.Style.InnerSpacing + textSize.x, 
                        Mathf.Max(textSize.y, checkmarkBoxSize));
                    break;
            }
            
            Checkbox(gui, ref value, in label, in rect);
        }
        
        public static void Checkbox(this ImGui gui, ref bool value, in ReadOnlySpan<char> label, in ImRect rect)
        {
            var id = gui.GetNextControlId();
            var checkmarkBox = rect.SplitLeft(GetCheckmarkBoxSize(gui), out var textRect).WithAspect(1.0f);
            Checkbox(gui, id, ref value, in checkmarkBox);

            if (label.IsEmpty)
            {
                return;
            }
            
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
            using var _ = new ImStyleScope<ImButtonStyle>(ref ImButton.Style, Style.Checkbox);
            
            var clicked = gui.Button(id, in rect, out var state);
            var style = Style.Checkbox.GetStyle(state);

            if (value)
            {
                var checkmarkRect = Style.Checkbox.GetContentRect(rect).ScaleFromCenter(Style.CheckmarkScale);
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

        public static ImTextSettings GetTextSettings()
        {
            return new ImTextSettings(ImControls.Style.TextSize, Style.Checkbox.Alignment, Style.WrapText);
        }

        public static Vector2 GetCheckmarkBoxRectSize(ImGui gui)
        {
            var row = gui.GetRowHeight();
            return new Vector2(row, row);
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
                CheckmarkScale = 0.7f,
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
    }
}