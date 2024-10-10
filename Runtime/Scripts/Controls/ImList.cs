using System;
using Imui.Core;
using Imui.Style;

namespace Imui.Controls
{
    // TODO (artem-s): add optional titlebar
    public static class ImList
    {
        public static ImRect GetRect(ImGui gui, ImSize size)
        {
            return ImControls.AddRowRect(gui, size);
        }

        public static void BeginList(this ImGui gui, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            
            BeginList(gui, GetRect(gui, size));
        }

        public static void BeginList(this ImGui gui, ImRect rect)
        {
            gui.Box(rect, gui.Style.List.Box);
            gui.RegisterRaycastTarget(rect);

            var layoutRect = rect.WithPadding(gui.Style.List.Padding);
            var maskRect = rect.WithPadding(gui.Style.List.Box.BorderWidth);
            
            gui.Layout.Push(ImAxis.Vertical, layoutRect);
            gui.Canvas.PushRectMask(maskRect, gui.Style.List.Box.BorderRadius);
            gui.Canvas.PushClipRect(maskRect); // need this to properly handle clicking outside drawing area
            gui.BeginScrollable();
        }

        public static void EndList(this ImGui gui)
        {
            gui.EndScrollable();
            gui.Canvas.PopClipRect();
            gui.Canvas.PopRectMask();
            gui.Layout.Pop();
        }

        public static bool ListItem(this ImGui gui, ref int selectedIndex, int index, ReadOnlySpan<char> label)
        {
            ref readonly var style = ref (index == selectedIndex ? ref gui.Style.List.ItemSelected : ref gui.Style.List.ItemNormal);

            using (new ImStyleScope<ImButtonStyle>(ref gui.Style.Button, in style))
            {
                if (gui.Button(label))
                {
                    selectedIndex = index;
                    return true;
                }
            }

            return false;
        }

        public static bool ListItem(this ImGui gui, bool isSelected, ReadOnlySpan<char> label)
        {
            ref readonly var style = ref (isSelected ? ref gui.Style.List.ItemSelected : ref gui.Style.List.ItemNormal);

            using (new ImStyleScope<ImButtonStyle>(ref gui.Style.Button, in style))
            {
                if (gui.Button(label))
                {
                    return true;
                }
            }

            return false;
        }

        public static float GetEnclosingHeight(ImGui gui, float contentHeight)
        {
            return contentHeight + gui.Style.List.Padding.Vertical;
        }
    }
}