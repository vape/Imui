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
            
            Span<ImRect> rects = stackalloc ImRect[5];
            rect.SplitHorizontal(ref rects, rects.Length, gui.Style.Layout.InnerSpacing);

            ComponentLetter(gui, in rects[0], Color.red, 'R', out rects[0]);
            ComponentLetter(gui, in rects[1], Color.green,'G', out rects[1]);
            ComponentLetter(gui, in rects[2], Color.blue, 'B', out rects[2]);
            ComponentLetter(gui, in rects[3], null, 'A', out rects[3]);
            
            var changed = false;

            using (gui.StyleScope(ref gui.Style.TextEdit))
            {
                gui.Style.TextEdit.Alignment.X = 0.5f;

                var col32 = (Color32)color;

                // TODO (artem-s): cut drawcalls generated because of text masking
                changed |= ImNumericEdit.NumericEdit(gui, rId, ref col32.r, rects[0], flags: ImNumericEditFlag.Slider | ImNumericEditFlag.RightAdjacent);
                changed |= ImNumericEdit.NumericEdit(gui, gId, ref col32.g, rects[1], flags: ImNumericEditFlag.Slider | ImNumericEditFlag.RightAdjacent);
                changed |= ImNumericEdit.NumericEdit(gui, bId, ref col32.b, rects[2], flags: ImNumericEditFlag.Slider | ImNumericEditFlag.RightAdjacent);
                changed |= ImNumericEdit.NumericEdit(gui, aId, ref col32.a, rects[3], flags: ImNumericEditFlag.Slider | ImNumericEditFlag.RightAdjacent);

                if (changed)
                {
                    color = col32;
                }
            }

            changed |= gui.ColorPickerButton(cId, ref color, rects[4], ImColorButtonFlag.AlphaOnePreview);

            gui.PopId();

            return changed;
        }
        
        private static unsafe void ComponentLetter(ImGui gui, in ImRect rect, Color32? color, char component, out ImRect valueRect)
        {
            color = color == null ? gui.Style.Text.Color : Color32.Lerp(gui.Style.Text.Color, color.Value, 0.25f);
            
            var textStyle = new ImTextSettings(gui.Style.Layout.TextSize * 0.75f, 0.5f, 0.5f);
            var componentRect = rect.TakeLeft(gui.GetRowHeight() * 0.75f, -gui.Style.TextEdit.Normal.Box.BorderThickness, out valueRect);
            var componentText = new ReadOnlySpan<char>(&component, 1);
            var componentStyle = gui.Style.TextEdit.Normal.Box;
            componentStyle.MakeAdjacent(ImAdjacency.Left);
            
            gui.Box(componentRect, in componentStyle);
            gui.Text(componentText, textStyle, color.Value, componentRect);
        }
    }
}