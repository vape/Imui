using Imui.Core;
using Imui.IO.Events;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImPopup
    {
        public const int POPUP_BASE_ORDER = 1024 * 1024; // above all windows, well, if you're not doing like 8191+ of them
        public const int POPUP_ORDER_STEP = 1024;
        public const int POPUP_CLOSE_BUTTON_ORDER_OFFSET = -16;

        public static void BeginPopup(this ImGui gui)
        {
            var overBaseOrder = Mathf.Max(0, gui.Canvas.GetOrder() - POPUP_BASE_ORDER);
            var order = POPUP_BASE_ORDER + ((overBaseOrder / POPUP_ORDER_STEP) * POPUP_ORDER_STEP + POPUP_ORDER_STEP);
            
            gui.Canvas.PushNoClipRect();
            gui.Canvas.PushNoRectMask();
            gui.Canvas.PushOrder(order);
        }

        public static void EndPopup(this ImGui gui)
        {
            gui.Canvas.PopOrder();
            gui.Canvas.PopClipRect();
            gui.Canvas.PopRectMask();
        }
        
        public static void EndPopupWithCloseButton(this ImGui gui, out bool close)
        {
            var order = gui.Canvas.GetOrder() + POPUP_CLOSE_BUTTON_ORDER_OFFSET;
            
            gui.Canvas.PopOrder();
            gui.Canvas.PopClipRect();
            gui.Canvas.PopRectMask();

            close = CloseButton(gui, order);
        }

        public static bool CloseButton(ImGui gui, int order)
        {
            gui.Canvas.PushNoClipRect();
            gui.Canvas.PushOrder(order);
            gui.RegisterRaycastTarget(gui.Canvas.ScreenRect);

            var id = gui.GetNextControlId();
            
            ref readonly var mouseEvent = ref gui.Input.MouseEvent;
            if (mouseEvent.Type == ImMouseEventType.Scroll || mouseEvent.Type == ImMouseEventType.Drag)
            {
                gui.Input.UseMouseEvent();
            }
            
            var clicked = gui.InvisibleButton(id, gui.Canvas.ScreenRect, ImButtonFlag.ActOnPress | ImButtonFlag.ReactToAnyButton);
            
            gui.Canvas.PopOrder();
            gui.Canvas.PopClipRect();

            return clicked;
        }
    }
}