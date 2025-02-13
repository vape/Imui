using System;
using Imui.Core;
using Imui.Style;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImColorEdit
    {
        public static ImRect AddRect(ImGui gui, ImSize size)
        {
            if (size.Mode is ImSizeMode.Auto or ImSizeMode.Fit)
            {
                var textWidth = gui.MeasureTextSize("255").x + gui.Style.Layout.InnerSpacing * 2;
                var minWidth = textWidth * 5 + gui.Style.Layout.InnerSpacing * 4;
                var width = Mathf.Max(gui.GetLayoutWidth(), minWidth);

                return gui.Layout.AddRect(width, gui.GetRowHeight());
            }

            return gui.AddSingleRowRect(size);
        }

        public static Color ColorEdit(this ImGui gui, Color color, ImSize size = default)
        {
            ColorEdit(gui, ref color, size);
            return color;
        }

        public static bool ColorEdit(this ImGui gui, ref Color color, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var id = gui.GetNextControlId();
            var rect = AddRect(gui, size);

            return ColorEdit(gui, id, ref color, rect);
        }

        public static bool ColorEdit(this ImGui gui, uint id, ref Color color, ImRect rect)
        {
            gui.PushId(id);

            var rId = gui.GetNextControlId();
            var gId = gui.GetNextControlId();
            var bId = gui.GetNextControlId();
            var aId = gui.GetNextControlId();
            var cId = gui.GetNextControlId();

            ref readonly var style = ref gui.Style.TextEdit.Normal.Box;

            Span<ImRect> rects = stackalloc ImRect[5];
            rect.SplitHorizontal(ref rects, rects.Length, gui.Style.Layout.InnerSpacing);

            var changed = false;

            using (gui.StyleScope(ref gui.Style.TextEdit))
            {
                gui.Style.TextEdit.Alignment.X = 0.5f;

                var col32 = (Color32)color;

                // TODO (artem-s): cut drawcalls generated because of text masking
                changed |= ImNumericEdit.NumericEdit(gui, rId, ref col32.r, rects[0], flags: ImNumericEditFlag.Slider);
                changed |= ImNumericEdit.NumericEdit(gui, gId, ref col32.g, rects[1], flags: ImNumericEditFlag.Slider);
                changed |= ImNumericEdit.NumericEdit(gui, bId, ref col32.b, rects[2], flags: ImNumericEditFlag.Slider);
                changed |= ImNumericEdit.NumericEdit(gui, aId, ref col32.a, rects[3], flags: ImNumericEditFlag.Slider);

                if (changed)
                {
                    color = col32;
                }
            }

            ColorIndicator(gui, rects[0], Color.red, in style);
            ColorIndicator(gui, rects[1], new Color32(0, 200, 0, 255), in style);
            ColorIndicator(gui, rects[2], new Color32(32, 64, 255, 255), in style);
            ColorIndicator(gui, rects[3], Color.white, in style);

            changed |= gui.ColorPickerButton(cId, ref color, rects[4], ImColorButtonFlag.AlphaOnePreview);

            gui.PopId();

            return changed;
        }

        public static void ColorIndicator(ImGui gui, ImRect rect, Color32 color, in ImStyleBox style)
        {
            var radius = new ImRectRadius(bottomRight: style.BorderRadius.BottomRight, bottomLeft: style.BorderRadius.BottomLeft);
            rect.AddPadding(style.BorderThickness);
            rect.H = Mathf.Max(2, style.BorderRadius.BottomLeft - style.BorderThickness);
            gui.Canvas.Rect(rect, color.WithAlpha(0.5f), radius);
        }
    }
}