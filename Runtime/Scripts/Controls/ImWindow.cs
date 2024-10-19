using System;
using Imui.Core;
using Imui.IO.Events;
using Imui.Style;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImWindow
    {
        public const float DEFAULT_WIDTH = 512;
        public const float DEFAULT_HEIGHT = 512;
        
        public const int WINDOW_ORDER_OFFSET = 128;
        public const int WINDOW_FRONT_ORDER_OFFSET = 64;
        
        public static void BeginWindow(this ImGui gui,
                                       string title,
                                       ImSize size = default,
                                       ImWindowFlag flags = ImWindowFlag.None)
        {
            var open = true;
            BeginWindow(gui, title, ref open, size, flags | ImWindowFlag.NoCloseButton);
        }

        public static bool BeginWindow(this ImGui gui,
                                       string title,
                                       ref bool open,
                                       ImSize size = default,
                                       ImWindowFlag flags = ImWindowFlag.None)
        {
            if (!open)
            {
                return false;
            }
            
            var id = gui.PushId(title);
            
            if (gui.IsGroupHovered(id))
            {
                ref readonly var evt = ref gui.Input.MouseEvent;
                if (evt.Type is ImMouseEventType.Down)
                {
                    gui.WindowManager.RequestFocus(id);
                }
            }

            var (width, height) = size.Mode switch
            {
                ImSizeMode.Fixed => (size.Width, size.Height),
                _ => (DEFAULT_WIDTH, DEFAULT_HEIGHT)
            };
            
            ref readonly var style = ref gui.Style.Window;
            ref var state = ref gui.WindowManager.BeginWindow(id, title, width, height, flags);
            
            gui.Canvas.PushOrder(state.Order * WINDOW_ORDER_OFFSET + WINDOW_FRONT_ORDER_OFFSET);
            Foreground(gui, ref state, out var closeClicked);
            gui.Canvas.PopOrder();
            
            gui.Canvas.PushOrder(state.Order * WINDOW_ORDER_OFFSET);
            gui.Canvas.PushRectMask(state.Rect, style.Box.BorderRadius);
            gui.Canvas.PushClipRect(state.Rect);
            Background(gui, in state, out var contentRect);

            if (closeClicked)
            {
                open = false;
            }
            
            gui.RegisterControl(id, state.Rect);
            gui.RegisterGroup(id, state.Rect);
            
            gui.Layout.Push(ImAxis.Vertical, contentRect);
            gui.BeginScrollable();

            return true;
        }

        public static void EndWindow(this ImGui gui)
        {
            gui.EndScrollable();
            gui.Layout.Pop();
            
            gui.WindowManager.EndWindow();

            gui.Canvas.PopClipRect();
            gui.Canvas.PopRectMask();
            gui.Canvas.PopOrder();
            
            gui.PopId();
        }
        
        public static void Background(ImGui gui, in ImWindowState state, out ImRect content)
        {
            ref readonly var style = ref gui.Style.Window;
            
            gui.Canvas.Rect(state.Rect, style.Box.BackColor, style.Box.BorderRadius);

            if ((state.Flags & ImWindowFlag.NoTitleBar) != 0)
            {
                content = state.Rect;
                return;
            }
            
            var titleBarRect = GetTitleBarRect(gui, state.Rect, out _);
            state.Rect.SplitTop(titleBarRect.H, out content);
            content.AddPadding(style.ContentPadding);
        }

        public static void Foreground(ImGui gui, ref ImWindowState state, out bool closeClicked)
        {
            closeClicked = false;
            
            if ((state.Flags & ImWindowFlag.NoResize) == 0)
            {
                ResizeHandle(gui, ref state);
            }
            
            if ((state.Flags & ImWindowFlag.NoTitleBar) == 0)
            {
                TitleBar(gui, state.Title, ref state, out closeClicked);
            }
            
            Outline(gui, state.Rect);
        }

        public static void Outline(ImGui gui, ImRect rect)
        {
            ref readonly var style = ref gui.Style.Window;
            gui.Canvas.RectOutline(rect, style.Box.BorderColor, style.Box.BorderThickness, style.Box.BorderRadius);
        }

        public static void TitleBar(ImGui gui, ReadOnlySpan<char> text, ref ImWindowState state, out bool closeClicked)
        {
            ref readonly var style = ref gui.Style.Window;

            closeClicked = false;
            
            var id = gui.GetNextControlId();
            var hovered = gui.IsControlHovered(id);
            var active = gui.IsControlActive(id);
            var rect = GetTitleBarRect(gui, state.Rect, out var radius);
            var textSettings = new ImTextSettings(gui.Style.Layout.TextSize, style.TitleBar.Alignment);
            var movable = (state.Flags & ImWindowFlag.NoDrag) == 0;

            Span<Vector2> border = stackalloc Vector2[2]
            {
                rect.BottomLeft,
                rect.BottomRight
            };
            
            gui.Canvas.Rect(rect, style.TitleBar.BackColor, radius);
            gui.Canvas.Line(border, style.Box.BorderColor, false, style.Box.BorderThickness, 0.0f);
            gui.Canvas.Text(text, style.TitleBar.FrontColor, rect, in textSettings);

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when hovered:
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Drag when active && movable:
                    state.NextRect.Position += evt.Delta;
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Up when active:
                    gui.ResetActiveControl();
                    break;
            }
            
            gui.RegisterControl(id, rect);

            if ((state.Flags & ImWindowFlag.NoCloseButton) == 0)
            {
                var closeButtonRect = rect.SplitRight(gui.GetRowHeight() - gui.Style.Layout.InnerSpacing).WithAspect(1.0f);
                closeButtonRect.X -= closeButtonRect.Y - rect.Y;

                using (new ImStyleScope<ImStyleButton>(ref gui.Style.Button, in gui.Style.Window.TitleBar.CloseButton))
                {
                    closeClicked = gui.Button(closeButtonRect, out var buttonState);

                    var color = ImButton.GetStateFrontColor(gui, buttonState);
                    var width = closeButtonRect.W * 0.08f;
                    
                    Span<Vector2> path = stackalloc Vector2[2]
                    {
                        Vector2.Lerp(closeButtonRect.Center, closeButtonRect.TopRight, 0.35f),
                        Vector2.Lerp(closeButtonRect.Center, closeButtonRect.BottomLeft, 0.35f)
                    };
                    
                    gui.Canvas.Line(path, color, false, width);

                    path[0] = Vector2.Lerp(closeButtonRect.Center, closeButtonRect.TopLeft, 0.35f);
                    path[1] = Vector2.Lerp(closeButtonRect.Center, closeButtonRect.BottomRight, 0.35f);
                    
                    gui.Canvas.Line(path, color, false, width);
                }
            }
        }
        
        public static void ResizeHandle(ImGui gui, ref ImWindowState state)
        {
            const float PI = Mathf.PI;
            const float HALF_PI = PI / 2;

            var id = gui.GetNextControlId();
            var hovered = gui.IsControlHovered(id);
            var handleRect = GetResizeHandleRect(gui, state.Rect, out var radius);
            var active = gui.IsControlActive(id);
            ref readonly var style = ref gui.Style.Window;
            
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

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when hovered:
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Drag when active:
                    state.NextRect.W += evt.Delta.x;
                    state.NextRect.H -= evt.Delta.y;
                    state.NextRect.Y += evt.Delta.y;
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Up when active:
                    gui.ResetActiveControl();
                    break;
            }
            
            gui.RegisterControl(id, handleRect);
        }

        public static ImRect GetResizeHandleRect(ImGui gui, ImRect rect, out float cornerRadius)
        {
            cornerRadius = Mathf.Max(gui.Style.Window.Box.BorderRadius.BottomRight, 0);
            
            var handleSize = gui.Style.Window.ResizeHandleSize;
            var handleRect = rect;
            handleRect.X += handleRect.W - handleSize;
            handleRect.W = handleSize;
            handleRect.H = handleSize;
            
            return handleRect;
        }
        
        public static ImRect GetTitleBarRect(ImGui gui, ImRect rect, out ImRectRadius cornerRadius)
        {
            ref readonly var style = ref gui.Style.Window;
            
            var height = gui.Style.Layout.InnerSpacing + gui.GetRowHeight();
            var radiusTopLeft = style.Box.BorderRadius.TopLeft - style.Box.BorderThickness;
            var radiusTopRight = style.Box.BorderRadius.TopRight - style.Box.BorderThickness;
            cornerRadius = new ImRectRadius(radiusTopLeft, radiusTopRight);
            
            return rect.WithPadding(style.Box.BorderThickness).SplitTop(height);
        }

        public static ImRect GetCurrentWindowContentRect(this ImGui gui)
        {
            if (!gui.WindowManager.IsDrawingWindow())
            {
                return default;
            }

            var windowRect = gui.WindowManager.GetCurrentWindowRect();
            windowRect.SplitTop(GetTitleBarRect(gui, windowRect, out _).H, out var contentRect);

            return contentRect;
        }

        public static void SetCurrentWindowContentRect(this ImGui gui, ImRect rect)
        {
            if (!gui.WindowManager.IsDrawingWindow())
            {
                return;
            }
            
            gui.EndScrollable();
            gui.Layout.Pop();
            
            gui.Layout.Push(ImAxis.Vertical, rect);
            gui.BeginScrollable();
        }
    }
}
