using System;
using Imui.Core;
using Imui.Style;
using Imui.Utility;

namespace Imui.Controls
{
    public static class ImRadio
    {
        public static TEnum Radio<TEnum>(this ImGui gui, TEnum value, bool bitMasks = true) where TEnum : struct, Enum
        {
            Radio(gui, ref value, bitMasks);
            return value;
        }
        
        public static bool Radio<TEnum>(this ImGui gui, ref TEnum value, bool bitMasks = true) where TEnum : struct, Enum
        {
            var changed = false;
            
            if (!ImEnumUtility<TEnum>.IsFlags || !bitMasks)
            {
                var underlyingValue = ImEnumUtility<TEnum>.ToValue(value);

                for (int i = 0; i < ImEnumUtility<TEnum>.Names.Length; ++i)
                {
                    var name = ImEnumUtility<TEnum>.Names[i];
                    var val = ImEnumUtility<TEnum>.Values[i];
                    var selected = ImEnumUtility<TEnum>.ToValue(val) == underlyingValue;

                    if (Radio(gui, ref selected, name) && selected)
                    {
                        value = val;
                        changed = true;
                    }
                }
            }
            else
            {
                var mask = ImEnumUtility<TEnum>.ToValue(value);

                for (int i = 0; i < ImEnumUtility<TEnum>.Names.Length; ++i)
                {
                    var name = ImEnumUtility<TEnum>.Names[i];
                    var bit = ImEnumUtility<TEnum>.ToValue(ImEnumUtility<TEnum>.Values[i]);

                    changed |= RadioEnumMask(gui, ref mask, bit, name);
                }

                value = mask.ToEnumType();
            }

            return changed;
        }

        private static bool RadioEnumMask<TEnum>(this ImGui gui,
                                                 ref ImEnumValue<TEnum> mask,
                                                 ImEnumValue<TEnum> value,
                                                 ReadOnlySpan<char> label,
                                                 ImSize size = default) where TEnum : struct, Enum
        {
            var selected = value == 0 ? mask == 0 : (mask & value) == value;
            var changed = Radio(gui, ref selected, label, size);

            if (!changed)
            {
                return false;
            }

            if (value == 0)
            {
                mask = 0;
            }
            else if (selected)
            {
                mask |= value;
            }
            else
            {
                mask &= ~value;
            }

            return true;
        }

        public static bool Radio(this ImGui gui, bool value, ReadOnlySpan<char> label = default, ImSize size = default)
        {
            Radio(gui, ref value, label, size);
            return value;
        }
        
        public static bool Radio(this ImGui gui, ref bool value, ReadOnlySpan<char> label = default, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rect = ImCheckbox.AddRect(gui, size, label);
            return Radio(gui, ref value, label, rect);
        }

        public static bool Radio(this ImGui gui, bool value, ReadOnlySpan<char> label, ImRect rect)
        {
            Radio(gui, ref value, label, rect);
            return value;
        }

        public static bool Radio(this ImGui gui, ref bool value, ReadOnlySpan<char> label, ImRect rect)
        {
            var id = gui.GetNextControlId();
            var boxSize = gui.Style.Layout.TextSize;
            var boxRect = rect.TakeLeft(boxSize, out var textRect).WithAspect(1.0f);
            var changed = Radio(gui, id, ref value, boxRect);

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
        
        public static bool Radio(this ImGui gui, uint id, ref bool value, ImRect rect)
        {
            ref readonly var style = ref (value ? ref gui.Style.Radiobox.Checked : ref gui.Style.Radiobox.Normal);
            
            using var _ = gui.StyleScope(ref gui.Style.Button, in style);

            var clicked = gui.Button(id, rect, out var state);
            var frontColor = ImButton.GetStateFrontColor(gui, state);

            if (value)
            {
                var circleRect = rect.ScaleFromCenter(gui.Style.Radiobox.KnobScale);
                gui.Canvas.Ellipse(circleRect, frontColor);
            }

            if (clicked)
            {
                value = !value;
            }

            return clicked;
        }

        public static ImTextSettings GetTextSettings(ImGui gui)
        {
            return new ImTextSettings(gui.Style.Layout.TextSize, 0.0f, 0.5f, false);
        }
    }
}