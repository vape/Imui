using System;
using Imui.Controls.Styling;
using Imui.Core;

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
            gui.Box(rect, ImTheme.Active.List.Box);
            gui.RegisterRaycastTarget(rect);

            var layoutRect = rect.WithPadding(ImTheme.Active.List.Padding);
            var maskRect = rect.WithPadding(ImTheme.Active.List.Box.BorderWidth);
            
            gui.Layout.Push(ImAxis.Vertical, layoutRect);
            gui.Canvas.PushRectMask(maskRect, ImTheme.Active.List.Box.BorderRadius);
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

        public static bool ListItem(this ImGui gui, int index, ReadOnlySpan<char> label, ref int selected)
        {
            ref readonly var style = ref (index == selected ? ref ImTheme.Active.List.ItemSelected : ref ImTheme.Active.List.ItemNormal);

            using (new ImStyleScope<ImButtonStyle>(ref ImTheme.Active.Button, in style))
            {
                if (gui.Button(label))
                {
                    selected = index;
                    return true;
                }
            }

            return false;
        }
        
        public static bool ListItem(this ImGui gui, ReadOnlySpan<char> label, ref bool isSelected)
        {
            ref readonly var style = ref (isSelected ? ref ImTheme.Active.List.ItemSelected : ref ImTheme.Active.List.ItemNormal);

            using (new ImStyleScope<ImButtonStyle>(ref ImTheme.Active.Button, in style))
            {
                if (gui.Button(label))
                {
                    isSelected = !isSelected;
                    return true;
                }
            }

            return false;
        }
        
        public static float GetEnclosingHeight(float contentHeight)
        {
            return contentHeight + ImTheme.Active.List.Padding.Vertical;
        }
    }

    public struct ImListStyle
    {
        public ImBoxStyle Box;
        public ImPadding Padding;
        public ImButtonStyle ItemNormal;
        public ImButtonStyle ItemSelected;
    }
}