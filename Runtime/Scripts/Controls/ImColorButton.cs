using System;
using Imui.Core;
using Imui.Style;
using UnityEngine;

namespace Imui.Controls
{
    [Flags]
    public enum ImColorButtonFlag
    {
        None = 0,
        AlphaOnePreview = 1
    }
    
    public static class ImColorButton
    {
        public static bool ColorPickerButton(this ImGui gui, ref Color color, ImSize size = default, ImColorButtonFlag flags = ImColorButtonFlag.None)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            
            var id = gui.GetNextControlId();
            var rect = ImControls.AddRowRect(gui, size, gui.GetRowHeight());
            
            return ColorPickerButton(gui, id, ref color, rect, flags);
        }
        
        public static bool ColorPickerButton(this ImGui gui, uint id, ref Color color, ImRect rect, ImColorButtonFlag flags = ImColorButtonFlag.None)
        {
            ref var open = ref gui.Storage.Get(id, false);

            if (ColorButton(gui, id, color, rect, flags))
            {
                open = !open;
            }

            if (!open)
            {
                return false;
            }

            gui.PushId(id);
            gui.BeginPopup();
            
            var pickerId = gui.GetNextControlId();
            var pickerRect = FindColorPickerPopupRect(gui, rect);
            gui.Box(pickerRect, gui.Style.Tooltip.Box);
            var changed = gui.ColorPicker(pickerId, ref color, pickerRect.WithPadding(gui.Style.Tooltip.Padding));
            
            gui.EndPopupWithCloseButton(out var close);
            gui.PopId();

            if (close)
            {
                open = false;
            }

            return changed;
        }

        public static bool ColorButton(this ImGui gui, Color color, ImSize size = default, ImColorButtonFlag flags = ImColorButtonFlag.None)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            
            var id = gui.GetNextControlId();
            var rect = ImControls.AddRowRect(gui, size, gui.GetRowHeight());

            return ColorButton(gui, id, color, rect, flags);
        }

        public static bool ColorButton(this ImGui gui, uint id, Color color, ImRect rect, ImColorButtonFlag flags = ImColorButtonFlag.None)
        {
            var clicked = gui.InvisibleButton(id, rect, out var state);

            if (gui.Canvas.Cull(rect))
            {
                return clicked;
            }
            
            ref readonly var stateStyle = ref ImButton.GetStateStyle(gui, state);
            var boxStyle = new ImStyleBox
            {
                BackColor = color,
                FrontColor = default,
                BorderColor = stateStyle.BorderColor,
                BorderThickness = gui.Style.Button.BorderThickness,
            };

            DrawCheckerboardPattern(gui, rect);

            if ((flags & ImColorButtonFlag.AlphaOnePreview) != 0)
            {
                var leftBoxColor = color;
                leftBoxColor.a = 1.0f;
            
                gui.Canvas.Rect(rect.SplitLeft(rect.W / 2.0f), leftBoxColor);
            }
            
            gui.Box(rect, boxStyle);
            
            return clicked;
        }
        
        public static void DrawCheckerboardPattern(ImGui gui, ImRect rect)
        {
            void SetPatternAspect(float aspect)
            {
                var scaleOffset = ImCanvas.GetTexScaleOffsetFor(ImCanvasBuiltinTex.Checkerboard);
                scaleOffset.x *= aspect;
                
                gui.Canvas.SetTexScaleOffset(scaleOffset);
            }
            
            var count = Mathf.CeilToInt(rect.W / rect.H);
            var tmp = gui.Canvas.GetTexScaleOffset();

            var color = (Color32)Color.white;
            var width = rect.W;
            
            while (count > 0)
            {
                var w = Mathf.Min(width, rect.H);
                var r = new ImRect(rect.X + (rect.W - width), rect.Y, w, rect.H);
                
                SetPatternAspect(w / rect.H);
                gui.Canvas.Rect(r, color);

                width -= w;
                count--;
            }

            gui.Canvas.SetTexScaleOffset(tmp);
        }

        public static ImRect FindColorPickerPopupRect(ImGui gui, ImRect buttonRect)
        {
            var width = Mathf.Max(buttonRect.W, gui.GetRowHeight() * 10) + gui.Style.Tooltip.Padding.Horizontal;
            var height = ImColorPicker.GetHeight(gui, width) + gui.Style.Tooltip.Padding.Vertical;

            var x = Mathf.Max(gui.Canvas.ScreenRect.X, Mathf.Min(buttonRect.X, buttonRect.Right - width));
            var y = buttonRect.Y - height - gui.Style.Layout.Spacing;

            if (y < gui.Canvas.ScreenRect.Y)
            {
                y = buttonRect.Top + gui.Style.Layout.Spacing;
            }

            return new ImRect(x, y, width, height);
        }
    }
}