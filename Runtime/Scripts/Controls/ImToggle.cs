using System;
using Imui.Core;
using Imui.Styling;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImToggle
    {
        public static ImToggleStyle Style = ImToggleStyle.Default;
        
        public static void Toggle(this ImGui gui, ref bool value)
        {
            var rect = gui.Layout.AddRect(new Vector2(Style.CheckMarkSize, Style.CheckMarkSize));
            Toggle(gui, ref value, in rect, in rect);
        }
        
        public static void Toggle(this ImGui gui, ref bool value, in ReadOnlySpan<char> label)
        {
            ref readonly var textLayout = ref gui.TextDrawer.BuildTempLayout(in label, 0, 0, 
                Style.Button.Text.AlignX, Style.Button.Text.AlignY, Style.Button.Text.Size);

            var size = new Vector2(textLayout.Width + Style.CheckMarkSize + Style.Space,
                Mathf.Max(textLayout.Height, Style.CheckMarkSize));
            
            Toggle(gui, ref value, in label, size);
        }
        
        public static void Toggle(this ImGui gui, ref bool value, in ReadOnlySpan<char> label, Vector2 size)
        {
            Toggle(gui, ref value, in label, gui.Layout.AddRect(size));
        }
                
        public static void Toggle(this ImGui gui, ref bool value, in ReadOnlySpan<char> label, ImRect rect)
        {
            var toggleRect = rect.SplitLeft(Style.CheckMarkSize, out var textRect);
            Toggle(gui, ref value, toggleRect, in rect);

            textRect.X += Style.Space;
            textRect.W -= Style.Space;
            
            gui.Canvas.Text(in label, Style.TextColor, textRect, in Style.Button.Text);
        }
        
        public static void Toggle(this ImGui gui, ref bool value, in ImRect rect, in ImRect clickable)
        {
            using var _ = new ImStyleScope<ImButtonStyle>(ref ImButton.Style, Style.Button);
            
            var buttonRect = rect.WithAspect(1.0f);
            var id = gui.GetNextControlId();
            var clicked = gui.Button(id, in buttonRect, out var content, out var style, in clickable);

            if (value)
            {
                var checkMarkRect = buttonRect.WithPadding(Style.CheckMarkPadding);
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
    }

    public struct ImToggleStyle
    {
        public const float DEFAULT_CHECKMARK_SIZE = 24;

        public static readonly ImToggleStyle Default = new ImToggleStyle()
        {
            CheckMarkSize = DEFAULT_CHECKMARK_SIZE,
            Button = ImButtonStyle.Default,
            TextColor = ImColors.Black,
            Space = 2,
            CheckMarkPadding = 4
        };

        public float CheckMarkPadding;
        public float CheckMarkSize;
        public ImButtonStyle Button;
        public Color32 TextColor;
        public float Space;
    }
}