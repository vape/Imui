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
            var rect = gui.Layout.AddRect(new Vector2(Style.DefaultSize, Style.DefaultSize));
            Toggle(gui, ref value, in rect, in rect);
        }
        
        public static void Toggle(this ImGui gui, ref bool value, in ReadOnlySpan<char> label)
        {
            ref readonly var textLayout = ref gui.TextDrawer.BuildTempLayout(in label, 0, 0, 
                Style.Button.Text.AlignX, Style.Button.Text.AlignY, Style.Button.Text.Size);

            var size = new Vector2(textLayout.Width + Style.DefaultSize + Style.Space,
                Mathf.Max(textLayout.Height, Style.DefaultSize));
            
            Toggle(gui, ref value, in label, size);
        }
        
        public static void Toggle(this ImGui gui, ref bool value, in ReadOnlySpan<char> label, Vector2 size)
        {
            Toggle(gui, ref value, in label, gui.Layout.AddRect(size));
        }
                
        public static void Toggle(this ImGui gui, ref bool value, in ReadOnlySpan<char> label, ImRect rect)
        {
            var toggleRect = rect.SplitLeft(Style.DefaultSize, out var textRect);
            Toggle(gui, ref value, toggleRect, in rect);

            textRect.X += Style.Space;
            textRect.W -= Style.Space;
            
            gui.Canvas.Text(in label, Style.TextColor, textRect, in Style.Button.Text);
        }
        
        public static void Toggle(this ImGui gui, ref bool value, in ImRect rect, in ImRect clickable)
        {
            var buttonRect = rect.WithAspect(1.0f);
            var id = gui.GetNextControlId();
            var clicked = gui.Button(id, in buttonRect, out var content, out var style, in clickable);

            if (value)
            {
                gui.Canvas.Rect(content.WithPadding(1), style.FrontColor, Style.Button.CornerRadius);
            }
            
            if (clicked)
            {
                value = !value;
            }
        }
    }

    public struct ImToggleStyle
    {
        public const float DEFAULT_SIZE = 24;

        public static ImToggleStyle Default = new ImToggleStyle()
        {
            DefaultSize = DEFAULT_SIZE,
            Button = ImButtonStyle.Default,
            TextColor = ImColors.Black,
            Space = 2
        };
        
        public float DefaultSize;
        public ImButtonStyle Button;
        public Color32 TextColor;
        public float Space;
    }
}