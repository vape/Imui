using System;
using Imui.Core;
using Imui.Style;
using UnityEngine;

namespace Imui.Controls
{
    public struct ImMenuBarState
    {
        public uint Selected;
    }

    public static class ImMenuBar
    {
        public static float GetMenuBarHeight(ImGui gui) => gui.GetRowHeight();

        public static void BeginMenuBar(this ImGui gui)
        {
            var id = gui.GetNextControlId();

            if (gui.WindowManager.IsDrawingWindow())
            {
                var rect = gui.GetWindowMenuBarRect();
                
                BeginMenuBar(gui, id, rect, gui.Canvas.GetOrder() + ImWindow.WINDOW_MENU_ORDER_OFFSET - 1);
            }
            else
            {
                var height = GetMenuBarHeight(gui);
                var rect = gui.AddLayoutRect(gui.GetLayoutWidth(), height);

                if (Mathf.Approximately(gui.Canvas.ScreenRect.Top, rect.Top))
                {
                    gui.Canvas.SafeAreaPadding.Top += height;
                }
                
                BeginMenuBar(gui, id, rect, gui.Canvas.GetOrder());
            }
        }
        
        public static void BeginMenuBar(this ImGui gui, ImRect rect)
        {
            var id = gui.GetNextControlId();

            BeginMenuBar(gui, id, rect, gui.Canvas.GetOrder());
        }

        public static void BeginMenuBar(ImGui gui, uint id, ImRect rect, int order)
        {
            gui.PushId(id);
            gui.BeginScope<ImMenuBarState>(id);
            gui.Layout.Push(ImAxis.Horizontal, rect);
            gui.Canvas.PushOrder(order);

            gui.Box(rect, in gui.Style.MenuBar.Box);
        }

        public static void EndMenuBar(this ImGui gui)
        {
            gui.Canvas.PopOrder();
            gui.Layout.Pop();
            gui.EndScope<ImMenuBarState>();
            gui.PopId();
        }

        public static bool Button(ImGui gui, ReadOnlySpan<char> label)
        {
            var id = gui.GetNextControlId();

            return Button(gui, id, label, false, out _);
        }

        public static bool Button(ImGui gui, uint id, ReadOnlySpan<char> label, bool isSelected, out ImRect rect)
        {
            var textSettings = new ImTextSettings(gui.Style.Layout.TextSize, gui.Style.MenuBar.ItemNormal.Alignment);
            var textWidth = gui.MeasureTextSize(label, in textSettings).x;

            rect = gui.AddLayoutRect(textWidth + gui.Style.MenuBar.ItemExtraWidth, gui.GetLayoutHeight())
                      .WithPadding(gui.Style.MenuBar.Box.BorderThickness);

            ref var buttonStyle = ref (isSelected ? ref gui.Style.MenuBar.ItemActive : ref gui.Style.MenuBar.ItemNormal);
            using (gui.StyleScope(ref gui.Style.Button, in buttonStyle))
            {
                return gui.Button(id, label, rect);
            }
        }

        public static bool BeginItem(ImGui gui, ReadOnlySpan<char> label, ImMenuFlag flags = ImMenuFlag.None)
        {
            ref var barState = ref gui.GetCurrentScope<ImMenuBarState>();

            var id = gui.PushId(label);
            var clicked = Button(gui, id, label, barState.Selected == id, out var rect);
            var source = rect.BottomLeft;

            if (barState.Selected != 0 && gui.IsControlHovered(id))
            {
                barState.Selected = id;
            }

            if (clicked)
            {
                barState.Selected = barState.Selected == id ? 0 : id;
            }

            var open = barState.Selected == id;

            if (barState.Selected != default)
            {
                gui.BeginPopup();
                gui.RegisterControl(id, rect);
                gui.EndPopup();
            }

            if (!open)
            {
                gui.PopId();
                return false;
            }

            gui.BeginMenuPopup(label, ref open, source, flags: flags);

            if (!open && barState.Selected == id)
            {
                barState.Selected = default;
            }

            return true;
        }

        public static void EndItem(ImGui gui)
        {
            gui.EndMenuPopup();
            gui.PopId();
        }
    }
}