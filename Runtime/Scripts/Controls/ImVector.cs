using System;
using Imui.Core;
using Imui.Style;
using UnityEngine;

namespace Imui.Controls
{
    public static unsafe class ImVector
    {
        private static ImRect AddSpacingAndGetRect(ImGui gui, ImSize size)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            return gui.AddSingleRowRect(size);
        }
        
        public static bool Vector(this ImGui gui, ref Vector2 value, ImSize size = default) => Vector(gui, ref value, AddSpacingAndGetRect(gui, size));
        public static bool Vector(this ImGui gui, ref Vector3 value, ImSize size = default) => Vector(gui, ref value, AddSpacingAndGetRect(gui, size));
        public static bool Vector(this ImGui gui, ref Vector4 value, ImSize size = default) => Vector(gui, ref value, AddSpacingAndGetRect(gui, size));
        public static bool Vector(this ImGui gui, ref Vector2Int value, ImSize size = default) => Vector(gui, ref value, AddSpacingAndGetRect(gui, size));
        public static bool Vector(this ImGui gui, ref Vector3Int value, ImSize size = default) => Vector(gui, ref value, AddSpacingAndGetRect(gui, size));
        
        public static bool Vector(this ImGui gui, ref Vector2 value, ImRect rect)
        {
            var columns = gui.Arena.AllocArray<ImRect>(2);
            rect.SplitHorizontal(ref columns, columns.Length, gui.Style.Layout.Spacing);

            var changed = false;

            changed |= Component(gui, gui.GetNextControlId(), ref value.x, columns[0], 'x');
            changed |= Component(gui, gui.GetNextControlId(), ref value.y, columns[1], 'y');

            return changed;
        }
        
        public static bool Vector(this ImGui gui, ref Vector3 value, ImRect rect)
        {
            var columns = gui.Arena.AllocArray<ImRect>(3);
            rect.SplitHorizontal(ref columns, columns.Length, gui.Style.Layout.Spacing);

            var changed = false;

            changed |= Component(gui, gui.GetNextControlId(), ref value.x, columns[0], 'x');
            changed |= Component(gui, gui.GetNextControlId(), ref value.y, columns[1], 'y');
            changed |= Component(gui, gui.GetNextControlId(), ref value.z, columns[2], 'z');

            return changed;
        }
        
        public static bool Vector(this ImGui gui, ref Vector4 value, ImRect rect)
        {
            var columns = gui.Arena.AllocArray<ImRect>(4);
            rect.SplitHorizontal(ref columns, columns.Length, gui.Style.Layout.Spacing);

            var changed = false;

            changed |= Component(gui, gui.GetNextControlId(), ref value.x, columns[0], 'x');
            changed |= Component(gui, gui.GetNextControlId(), ref value.y, columns[1], 'y');
            changed |= Component(gui, gui.GetNextControlId(), ref value.z, columns[2], 'z');
            changed |= Component(gui, gui.GetNextControlId(), ref value.w, columns[3], 'w');

            return changed;
        }
        
        public static bool Vector(this ImGui gui, ref Vector2Int value, ImRect rect)
        {
            var columns = gui.Arena.AllocArray<ImRect>(2);
            rect.SplitHorizontal(ref columns, columns.Length, gui.Style.Layout.Spacing);

            var changed = false;
            var x = value.x;
            var y = value.y;

            if (Component(gui, gui.GetNextControlId(), ref x, columns[0], 'x'))
            {
                value.x = x;
                changed = true;
            }

            if (Component(gui, gui.GetNextControlId(), ref y, columns[1], 'y'))
            {
                value.y = y;
                changed = true;
            }
            
            return changed;
        }
        
        public static bool Vector(this ImGui gui, ref Vector3Int value, ImRect rect)
        {
            var columns = gui.Arena.AllocArray<ImRect>(3);
            rect.SplitHorizontal(ref columns, columns.Length, gui.Style.Layout.Spacing);

            var changed = false;
            var x = value.x;
            var y = value.y;
            var z = value.z;

            if (Component(gui, gui.GetNextControlId(), ref x, columns[0], 'x'))
            {
                value.x = x;
                changed = true;
            }

            if (Component(gui, gui.GetNextControlId(), ref y, columns[1], 'y'))
            {
                value.y = y;
                changed = true;
            }

            if (Component(gui, gui.GetNextControlId(), ref z, columns[2], 'z'))
            {
                value.z = z;
                changed = true;
            }

            return changed;
        }

        public static bool Component(ImGui gui, uint id, ref float value, ImRect rect, char component)
        {
            ComponentLiteral(gui, in rect, component, out var valueRect);
            return ImNumericEdit.NumericEdit(gui, id, ref value, valueRect, flags: ImNumericEditFlag.RightAdjacent);
        }
        
        public static bool Component(ImGui gui, uint id, ref int value, ImRect rect, char component)
        {
            ComponentLiteral(gui, in rect, component, out var valueRect);
            return ImNumericEdit.NumericEdit(gui, id, ref value, valueRect, flags: ImNumericEditFlag.RightAdjacent);
        }

        public static void ComponentLiteral(ImGui gui, in ImRect rect, char component, out ImRect valueRect)
        {
            var textStyle = new ImTextSettings(gui.Style.Layout.TextSize * 0.75f, 0.5f, 0.5f);
            var componentRect = rect.TakeLeft(gui.GetRowHeight() * 0.75f, -gui.Style.TextEdit.Normal.Box.BorderThickness, out valueRect);
            var componentText = new ReadOnlySpan<char>(&component, 1);
            var componentStyle = gui.Style.TextEdit.Normal.Box;
            componentStyle.MakeAdjacent(ImAdjacency.Left);
            
            gui.Box(componentRect, in componentStyle);
            gui.Text(componentText, textStyle, componentRect);
        }
    }
}