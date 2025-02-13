using System;
using Imui.Core;
using Imui.Style;

namespace Imui.Controls
{
    public struct ImMenuBarState
    {
        public uint Selected;
    }

    public static class ImMenuBar
    {
        public static float GetMenuBarHeight(ImGui gui) => gui.GetRowHeight();

        public static void BeginWindowMenuBar(this ImGui gui)
        {
            ImAssert.IsTrue(gui.WindowManager.IsDrawingWindow(), "gui.WindowManager.IsDrawingWindow()");

            var id = gui.GetNextControlId();
            var rect = gui.GetWindowMenuBarRect();

            BeginMenuBar(gui, id, rect, gui.Canvas.GetOrder() + ImWindow.WINDOW_MENU_ORDER_OFFSET - 1);
        }

        public static void EndWindowMenuBar(this ImGui gui)
        {
            EndMenuBar(gui);
        }

        public static void BeginMenuBar(this ImGui gui, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rect = gui.AddSingleRowRect(size);

            BeginMenuBar(gui, rect);
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

        public static bool BeginMenuBarItem(this ImGui gui, ReadOnlySpan<char> label)
        {
            ref var barState = ref gui.GetCurrentScope<ImMenuBarState>();

            var id = gui.PushId(label);
            var textSettings = new ImTextSettings(gui.Style.Layout.TextSize, gui.Style.MenuBar.ItemNormal.Alignment);
            var textWidth = gui.MeasureTextSize(label, in textSettings).x;
            var rect = gui.AddLayoutRect(textWidth + gui.Style.MenuBar.ItemExtraWidth, gui.GetLayoutHeight());
            var source = rect.BottomLeft;
            rect.AddPadding(gui.Style.MenuBar.Box.BorderThickness);
            var clicked = false;

            ref var buttonStyle = ref (barState.Selected == id ? ref gui.Style.MenuBar.ItemActive : ref gui.Style.MenuBar.ItemNormal);
            using (gui.StyleScope(ref gui.Style.Button, in buttonStyle))
            {
                clicked = gui.Button(id, label, rect);
            }

            if (barState.Selected != default && gui.IsControlHovered(id))
            {
                barState.Selected = id;
            }

            if (clicked)
            {
                barState.Selected = barState.Selected == id ? default : id;
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

            gui.BeginMenu(label, ref open, source);

            if (!open && barState.Selected == id)
            {
                barState.Selected = default;
            }

            return true;
        }

        public static void EndMenuBarItem(this ImGui gui)
        {
            gui.EndMenu();
            gui.PopId();
        }
    }
}