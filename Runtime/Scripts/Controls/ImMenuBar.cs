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
        public static void BeginMenuBar(this ImGui gui)
        {
            var id = gui.GetNextControlId();
            
            if (!gui.WindowManager.IsDrawingWindow() || !gui.IsLayoutEmpty())
            {
                BeginMenuBar(gui, id, gui.AddLayoutRect(gui.GetLayoutWidth(), gui.GetRowHeight()), gui.Canvas.GetOrder());
                return;
            }
            
            var rect = gui.GetCurrentWindowContentRect().SplitTop(gui.GetRowHeight(), out var newContentRect);
            newContentRect.AddPadding(gui.Style.Window.ContentPadding);
            gui.SetCurrentWindowContentRect(newContentRect);
            
            BeginMenuBar(gui, id, rect, gui.Canvas.GetOrder() + ImWindow.WINDOW_FRONT_ORDER_OFFSET);
        }
        
        public static void BeginMenuBar(this ImGui gui, ImRect rect)
        {
            var id = gui.GetNextControlId();

            BeginMenuBar(gui, id, rect, gui.Canvas.GetOrder());
        }
        
        public static void BeginMenuBar(ImGui gui, uint id, ImRect rect, int order)
        {
            gui.PushId(id);
            gui.Layout.Push(ImAxis.Horizontal, rect);
            gui.Canvas.PushOrder(order);
            
            gui.Box(rect, in gui.Style.MenuBar.Box);
        }

        public static void EndMenuBar(this ImGui gui)
        {
            gui.Canvas.PopOrder();
            gui.Layout.Pop();
            gui.PopId();
        }
        
        // TODO (artem-s): clicking on item again should close the menu
        public static bool TryBeginMenuBarItem(this ImGui gui, ReadOnlySpan<char> label)
        {
            ref var barState = ref gui.Storage.Get<ImMenuBarState>(gui.PeekId());
            
            var id = gui.PushId(label);
            var rect = ImButton.GetRect(gui, ImSizeMode.Fit, label);
            var clicked = false;
            
            ref var buttonStyle = ref (barState.Selected == id ? ref gui.Style.MenuBar.ItemActive : ref gui.Style.MenuBar.ItemNormal);
            using (new ImStyleScope<ImStyleButton>(ref gui.Style.Button, in buttonStyle))
            {
                clicked = gui.Button(id, label, rect);
            }
            
            if (clicked || (barState.Selected != default && gui.IsControlHovered(id)))
            {
                barState.Selected = id;
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

            var buttonRect = gui.LastControlRect;
            buttonRect.W = 0;
            buttonRect.H = 0;
            gui.Layout.Push(ImAxis.Vertical, buttonRect);

            gui.TryBeginMenu(label, ref open);

            if (!open && barState.Selected == id)
            {
                barState.Selected = default;
            }
            
            return true;
        }
        
        public static void EndMenuBarItem(this ImGui gui)
        {
            gui.EndMenu();
            gui.Layout.Pop();
            gui.PopId();
        }
    }
}