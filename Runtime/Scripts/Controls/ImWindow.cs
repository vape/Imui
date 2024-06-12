using System;
using Imui.Core;
using Imui.IO.Events;
using Imui.Rendering;
using Imui.Controls.Styling;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImWindow
    {
        private const int WINDOW_ORDER_OFFSET = 128;
        private const int WINDOW_FRONT_ORDER_OFFSET = 64;
        
        public static ImWindowStyle Style = ImWindowStyle.Default;
        
        public static void BeginWindow(this ImGui gui, 
            string title, 
            float width = ImWindowManager.DEFAULT_WIDTH, 
            float height = ImWindowManager.DEFAULT_HEIGHT,
            ImWindowFlag flags = ImWindowFlag.None)
        {
            var id = gui.PushId(title);
            
            ref var state = ref gui.WindowManager.BeginWindow(id, title, width, height, flags);
            
            gui.Canvas.PushOrder(state.Order * WINDOW_ORDER_OFFSET);
            gui.Canvas.PushRectMask(state.Rect, Style.Box.BorderRadius.GetMax());
            gui.Canvas.PushClipRect(state.Rect);
            Back(gui, in state, out var contentRect);
            
            gui.RegisterControl(id, state.Rect);
            
            gui.Layout.Push(ImAxis.Vertical, contentRect);
            gui.BeginScrollable();
        }

        public static void EndWindow(this ImGui gui)
        {
            gui.EndScrollable();
            gui.Layout.Pop();
            
            var id = gui.WindowManager.EndWindow();
            ref var state = ref gui.WindowManager.GetWindowState(id);
            
            gui.Canvas.PushOrder(gui.Canvas.GetOrder() + WINDOW_FRONT_ORDER_OFFSET);
            
            Front(gui, state.Rect);
            
            var clicked = false;
            var activeRect = state.Rect;
            
            if ((state.Flags & ImWindowFlag.DisableResize) == 0)
            {
                clicked |= ResizeHandle(gui, ref state.Rect);
            }
            
            if ((state.Flags & ImWindowFlag.DisableTitleBar) == 0)
            {
                clicked |= TitleBar(gui, state.Title, ref state, in activeRect);
            }
            
            if (clicked)
            {
                gui.WindowManager.RequestFocus(id);
            }
            
            gui.Canvas.PopOrder();
            
            gui.Canvas.PopRectMask();
            gui.Canvas.PopClipRect();
            gui.Canvas.PopOrder();
            
            gui.PopId();
        }
        
        public static void Back(ImGui gui, in ImWindowState state, out ImRect content)
        {
            gui.Canvas.Rect(state.Rect, Style.Box.BackColor, Style.Box.BorderRadius);

            if ((state.Flags & ImWindowFlag.DisableTitleBar) != 0)
            {
                content = state.Rect;
                return;
            }
            
            var titleBarRect = GetTitleBarRect(gui, in state.Rect, out _);
            state.Rect.SplitTop(titleBarRect.H, out content);
            content.AddPadding(Style.Padding);
        }

        public static void Front(ImGui gui, in ImRect rect)
        {
            gui.Canvas.RectOutline(rect, Style.Box.BorderColor, Style.Box.BorderWidth, Style.Box.BorderRadius);
        }

        public static bool TitleBar(ImGui gui, in ReadOnlySpan<char> text, ref ImWindowState state, in ImRect windowRect)
        {
            var id = gui.GetNextControlId();
            var hovered = gui.IsControlHovered(id);
            var active = gui.IsControlActive(id);
            var rect = GetTitleBarRect(gui, in windowRect, out var radius);
            var textSettings = GetTitleBarTextSettings();
            var movable = (state.Flags & ImWindowFlag.DisableMoving) == 0;
            
            gui.Canvas.Rect(rect, Style.TitleBar.BackColor, radius);
            gui.Canvas.Text(text, Style.TitleBar.FrontColor, rect, in textSettings);

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
            var handleRect = GetResizeHandleRect(in rect, out var radius);
            var active = gui.IsControlActive(id);
            
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
            
            gui.Canvas.ConvexFill(buffer, Style.ResizeHandleColor);

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

        public static ImRect GetResizeHandleRect(in ImRect rect, out float cornerRadius)
        {
            cornerRadius = Mathf.Max(Style.Box.BorderRadius.BottomRight, 0);
            
            var handleSize = Mathf.Max(Style.ResizeHandleSize, cornerRadius);
            var handleRect = rect;
            handleRect.X += handleRect.W - handleSize;
            handleRect.W = handleSize;
            handleRect.H = handleSize;
            
            return handleRect;
        }
        
        public static ImRect GetTitleBarRect(ImGui gui, in ImRect rect, out ImRectRadius cornerRadius)
        {
            var height = Style.TitleBar.GetHeight(gui.GetRowHeight());
            var radiusTopLeft = Style.Box.BorderRadius.TopLeft - Style.Box.BorderWidth;
            var radiusTopRight = Style.Box.BorderRadius.TopRight - Style.Box.BorderWidth;
            cornerRadius = new ImRectRadius(radiusTopLeft, radiusTopRight);
            
            return rect.WithPadding(Style.Box.BorderWidth).SplitTop(height);
        }

        public static ImTextSettings GetTitleBarTextSettings()
        {
            return new ImTextSettings(ImControls.Style.TextSize, Style.TitleBar.Alignment);
        }
    }
    
    [Serializable]
    public struct ImWindowTitleBarStyle
    {
        public Color32 BackColor;
        public Color32 FrontColor;
        public ImTextAlignment Alignment;
        public ImPadding Padding;

        public float GetHeight(float contentHeight)
        {
            return contentHeight + Padding.Vertical;
        }
    }
        
    [Serializable]
    public struct ImWindowStyle
    {
        public static readonly ImWindowStyle Default = new ImWindowStyle()
        {
            Box = new ImBoxStyle()
            {
                BackColor = ImColors.White,
                BorderColor = ImColors.Black,
                BorderWidth = 1.0f,
                BorderRadius = 4.0f
            },
            ResizeHandleColor = ImColors.Gray2.WithAlpha(196),
            ResizeHandleSize = 24,
            Padding = 4,
            TitleBar = new ImWindowTitleBarStyle()
            {
                BackColor = ImColors.Gray3,
                FrontColor = ImColors.White,
                Padding = 8,
                Alignment = new ImTextAlignment(0.5f, 0.5f)
            }
        };

        public ImBoxStyle Box;
        public Color32 ResizeHandleColor;
        public float ResizeHandleSize;
        public ImPadding Padding;
        public ImWindowTitleBarStyle TitleBar;
    }
}
