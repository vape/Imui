using System;
using Imui.Core;
using Imui.IO.Events;
using Imui.Controls.Styling;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImWindow
    {
        private const int WINDOW_ORDER_OFFSET = 128;
        private const int WINDOW_FRONT_ORDER_OFFSET = 64;
        
        public static void BeginWindow(this ImGui gui, 
            string title, 
            float width = ImWindowManager.DEFAULT_WIDTH, 
            float height = ImWindowManager.DEFAULT_HEIGHT,
            ImWindowFlag flags = ImWindowFlag.None)
        {
            var id = gui.PushId(title);
            
            if (gui.IsGroupHovered(id))
            {
                ref readonly var evt = ref gui.Input.MouseEvent;
                if (evt.Type is ImMouseEventType.Down)
                {
                    gui.WindowManager.RequestFocus(id);
                }
            }

            ref readonly var style = ref ImTheme.Active.Window;
            ref var state = ref gui.WindowManager.BeginWindow(id, title, width, height, flags);
            
            gui.Canvas.PushOrder(state.Order * WINDOW_ORDER_OFFSET);
            gui.Canvas.PushRectMask(state.Rect, style.Box.BorderRadius);
            gui.Canvas.PushClipRect(state.Rect);
            Back(gui, in state, out var contentRect);
            
            gui.RegisterControl(id, state.Rect);
            gui.RegisterGroup(id, state.Rect);
            
            gui.Layout.Push(ImAxis.Vertical, contentRect);
            
            // TODO (artem-s): do not apply padding to scorll bar, specifically in windows
            gui.BeginScrollable();
        }

        public static void EndWindow(this ImGui gui)
        {
            gui.EndScrollable();
            gui.Layout.Pop();
            
            var id = gui.WindowManager.EndWindow();
            ref var state = ref gui.WindowManager.GetWindowState(id);

            var frontOrder = gui.Canvas.GetOrder() + WINDOW_FRONT_ORDER_OFFSET;
            gui.Canvas.PushOrder(frontOrder);
            
            var clicked = false;
            var activeRect = state.Rect;
            
            if ((state.Flags & ImWindowFlag.DisableResize) == 0)
            {
                clicked |= ResizeHandle(gui, ref state.Rect);
            }
            
            if ((state.Flags & ImWindowFlag.DisableTitleBar) == 0)
            {
                clicked |= TitleBar(gui, state.Title, ref state, activeRect);
            }
            
            if (clicked)
            {
                gui.WindowManager.RequestFocus(id);
            }
            
            gui.Canvas.PopOrder();
            
            gui.Canvas.PopRectMask();
            gui.Canvas.PopClipRect();
            
            gui.Canvas.PushOrder(frontOrder);
            Outline(gui, activeRect);
            gui.Canvas.PopOrder();
            
            gui.Canvas.PopOrder();
            
            gui.PopId();
        }
        
        public static void Back(ImGui gui, in ImWindowState state, out ImRect content)
        {
            ref readonly var style = ref ImTheme.Active.Window;
            
            gui.Canvas.Rect(state.Rect, style.Box.BackColor, style.Box.BorderRadius);

            if ((state.Flags & ImWindowFlag.DisableTitleBar) != 0)
            {
                content = state.Rect;
                return;
            }
            
            var titleBarRect = GetTitleBarRect(gui, state.Rect, out _);
            state.Rect.SplitTop(titleBarRect.H, out content);
            content.AddPadding(style.ContentPadding);
        }

        public static void Outline(ImGui gui, ImRect rect)
        {
            ref readonly var style = ref ImTheme.Active.Window;
            gui.Canvas.RectOutline(rect, style.Box.BorderColor, style.Box.BorderWidth, style.Box.BorderRadius);
        }

        public static bool TitleBar(ImGui gui, ReadOnlySpan<char> text, ref ImWindowState state, ImRect windowRect)
        {
            ref readonly var style = ref ImTheme.Active.Window;
            
            var id = gui.GetNextControlId();
            var hovered = gui.IsControlHovered(id);
            var active = gui.IsControlActive(id);
            var rect = GetTitleBarRect(gui, windowRect, out var radius);
            var textSettings = new ImTextSettings(ImTheme.Active.Controls.TextSize, style.TitleBar.Alignment);
            var movable = (state.Flags & ImWindowFlag.DisableMoving) == 0;
            
            gui.Canvas.Rect(rect, style.TitleBar.BackColor, radius);
            gui.Canvas.Text(text, style.TitleBar.FrontColor, rect, in textSettings);

            var clicked = false;
            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when hovered:
                    clicked = true;
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Drag when active && movable:
                    state.Rect.Position += evt.Delta;
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Up when active:
                    gui.ResetActiveControl();
                    break;
            }
            
            gui.RegisterControl(id, rect);
            
            return clicked;
        }
        
        public static bool ResizeHandle(ImGui gui, ref ImRect rect)
        {
            const float PI = Mathf.PI;
            const float HALF_PI = PI / 2;

            var id = gui.GetNextControlId();
            var hovered = gui.IsControlHovered(id);
            var handleRect = GetResizeHandleRect(rect, out var radius);
            var active = gui.IsControlActive(id);
            ref readonly var style = ref ImTheme.Active.Window;
            
            var segments = ImShapes.SegmentCountForRadius(radius);
            var step = (1f / segments) * HALF_PI;

            Span<Vector2> buffer = stackalloc Vector2[segments + 1 + 2];

            buffer[0] = handleRect.BottomLeft;
            buffer[^1] = handleRect.TopRight;

            var cx = handleRect.BottomRight.x - radius;
            var cy = handleRect.Y + radius;
            
            for (int i = 0; i < segments + 1; ++i)
            {
                var a = PI + HALF_PI + step * i;
                buffer[i + 1].x = cx + Mathf.Cos(a) * radius;
                buffer[i + 1].y = cy + Mathf.Sin(a) * radius;
            }
            
            gui.Canvas.ConvexFill(buffer, style.ResizeHandleColor);

            var clicked = false;
            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when hovered:
                    clicked = true;
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Drag when active:
                    rect.W += evt.Delta.x;
                    rect.H -= evt.Delta.y;
                    rect.Y += evt.Delta.y;
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Up when active:
                    gui.ResetActiveControl();
                    break;
            }
            
            gui.RegisterControl(id, handleRect);
            return clicked;
        }

        public static ImRect GetResizeHandleRect(ImRect rect, out float cornerRadius)
        {
            ref readonly var style = ref ImTheme.Active.Window;
            
            cornerRadius = Mathf.Max(style.Box.BorderRadius.BottomRight, 0);
            
            var handleSize = Mathf.Max(style.ResizeHandleSize, cornerRadius);
            var handleRect = rect;
            handleRect.X += handleRect.W - handleSize;
            handleRect.W = handleSize;
            handleRect.H = handleSize;
            
            return handleRect;
        }
        
        public static ImRect GetTitleBarRect(ImGui gui, ImRect rect, out ImRectRadius cornerRadius)
        {
            ref readonly var style = ref ImTheme.Active.Window;
            
            var height = ImTheme.Active.Controls.InnerSpacing + gui.GetRowHeight();
            var radiusTopLeft = style.Box.BorderRadius.TopLeft - style.Box.BorderWidth;
            var radiusTopRight = style.Box.BorderRadius.TopRight - style.Box.BorderWidth;
            cornerRadius = new ImRectRadius(radiusTopLeft, radiusTopRight);
            
            return rect.WithPadding(style.Box.BorderWidth).SplitTop(height);
        }
    }
    
    [Serializable]
    public struct ImWindowTitleBarStyle
    {
        public Color32 BackColor;
        public Color32 FrontColor;
        public ImTextAlignment Alignment;
    }
        
    [Serializable]
    public struct ImWindowStyle
    {
        public ImBoxStyle Box;
        public Color32 ResizeHandleColor;
        public float ResizeHandleSize;
        public ImPadding ContentPadding;
        public ImWindowTitleBarStyle TitleBar;
    }
}
