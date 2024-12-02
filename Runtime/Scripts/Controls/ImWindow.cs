using System;
using Imui.Core;
using Imui.IO.Events;
using Imui.Rendering;
using Imui.Style;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImWindow
    {
        public const float DEFAULT_WIDTH = 512;
        public const float DEFAULT_HEIGHT = 512;

        public const float MIN_WIDTH = 64;
        public const float MIN_HEIGHT = 64;

        public const int WINDOW_ORDER_OFFSET = 128;
        public const int WINDOW_FRONT_ORDER_OFFSET = 64;
        public const int WINDOW_MENU_ORDER_OFFSET = WINDOW_FRONT_ORDER_OFFSET - 1;

        public static void BeginWindow(this ImGui gui, string title, ImSize size = default, ImWindowFlag flags = ImWindowFlag.None)
        {
            var open = true;
            BeginWindow(gui, title, ref open, size, flags | ImWindowFlag.NoCloseButton);
        }

        public static bool BeginWindow(this ImGui gui, string title, ref bool open, ImSize size = default, ImWindowFlag flags = ImWindowFlag.None)
        {
            var rect = GetInitialWindowRect(gui, size);

            return BeginWindow(gui, title, ref open, rect, flags);
        }

        public static void BeginWindow(this ImGui gui, string title, ImRect rect, ImWindowFlag flags = ImWindowFlag.None)
        {
            var open = true;
            BeginWindow(gui, title, ref open, rect, flags | ImWindowFlag.NoCloseButton);
        }

        public static bool BeginWindow(this ImGui gui, string title, ref bool open, ImRect rect, ImWindowFlag flags = ImWindowFlag.None)
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

            ref readonly var style = ref gui.Style.Window;
            ref var state = ref gui.WindowManager.BeginWindow(id, title, rect, flags);

            gui.Canvas.PushOrder(state.Order * WINDOW_ORDER_OFFSET + WINDOW_FRONT_ORDER_OFFSET);
            Foreground(gui, ref state, out var closeClicked);
            gui.Canvas.PopOrder();

            gui.Canvas.PushOrder(state.Order * WINDOW_ORDER_OFFSET);
            gui.Canvas.PushRectMask(state.Rect, style.Box.BorderRadius);
            gui.Canvas.PushClipRect(state.Rect);
            Background(gui, in state);

            var contentRect = GetContentRect(gui, in state);

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

        public static ImRect GetWindowMenuBarRect(this ImGui gui)
        {
            if (!gui.WindowManager.TryGetDrawingWindowId(out var windowId))
            {
                return default;
            }

            ref var state = ref gui.WindowManager.GetWindowState(windowId);
            return GetMenuBarRect(gui, in state);
        }

        public static ImRect GetWindowContentRect(this ImGui gui)
        {
            if (!gui.WindowManager.TryGetDrawingWindowId(out var windowId))
            {
                return default;
            }

            ref var state = ref gui.WindowManager.GetWindowState(windowId);
            return GetContentRect(gui, in state);
        }

        public static void Background(ImGui gui, in ImWindowState state)
        {
            gui.Canvas.Rect(state.Rect, gui.Style.Window.Box.BackColor, gui.Style.Window.Box.BorderRadius);
        }

        public static void Foreground(ImGui gui, ref ImWindowState state, out bool closeClicked)
        {
            closeClicked = false;

            if ((state.Flags & ImWindowFlag.NoResizing) == 0)
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
            var textSettings = new ImTextSettings(gui.Style.Layout.TextSize, style.TitleBar.Alignment, overflow: style.TitleBar.Overflow);
            var movable = (state.Flags & ImWindowFlag.NoMoving) == 0;
            var contentRect = rect.WithPadding(gui.Style.Layout.InnerSpacing);

            Span<Vector2> border = stackalloc Vector2[2] { rect.BottomLeft, rect.BottomRight };

            gui.Canvas.Rect(rect, style.TitleBar.BackColor, radius);
            gui.Canvas.Line(border, style.Box.BorderColor, false, style.Box.BorderThickness, 0.0f);

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when evt.LeftButton && hovered:
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

            if (!active)
            {
                state.NextRect.Position = KeepWindowInsideScreen(gui, state.NextRect.Position, state.NextRect.Size);
            }

            if ((state.Flags & ImWindowFlag.NoCloseButton) == 0)
            {
                var closeButtonRect = contentRect.TakeRight(gui.GetRowHeight() - gui.Style.Layout.InnerSpacing, out contentRect).WithAspect(1.0f);
                
                using (gui.StyleScope(ref gui.Style.Button, in gui.Style.Window.TitleBar.CloseButton))
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
            
            gui.Canvas.Text(text, style.TitleBar.FrontColor, contentRect, in textSettings);
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

            gui.Canvas.ConvexFill(buffer, hovered || active ? style.ResizeHandleActiveColor : style.ResizeHandleNormalColor);

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when evt.LeftButton && hovered:
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                    gui.Input.UseMouseEvent();
                    break;

                case ImMouseEventType.Drag when active:
                    var widthDelta = Mathf.Max(state.NextRect.W + evt.Delta.x, MIN_WIDTH) - state.NextRect.W;
                    var heightDelta = Mathf.Max(state.NextRect.H + -evt.Delta.y, MIN_HEIGHT) - state.NextRect.H;
                    
                    state.NextRect.W += widthDelta;
                    state.NextRect.H += heightDelta;
                    state.NextRect.Y -= heightDelta;
                    gui.Input.UseMouseEvent();
                    break;

                case ImMouseEventType.Up when active:
                    gui.ResetActiveControl();
                    break;
            }

            gui.RegisterControl(id, handleRect);
        }

        public static ImRect GetResizeHandleRect(ImGui gui, ImRect window, out float cornerRadius)
        {
            cornerRadius = Mathf.Max(gui.Style.Window.Box.BorderRadius.BottomRight, 0);

            var handleSize = gui.Style.Window.ResizeHandleSize;
            var handleRect = window;
            handleRect.X += handleRect.W - handleSize;
            handleRect.W = handleSize;
            handleRect.H = handleSize;

            return handleRect;
        }

        public static ImRect GetTitleBarRect(ImGui gui, ImRect window, out ImRectRadius cornerRadius)
        {
            ref readonly var style = ref gui.Style.Window;

            var height = GetTitleBarHeight(gui);
            var radiusTopLeft = style.Box.BorderRadius.TopLeft - style.Box.BorderThickness;
            var radiusTopRight = style.Box.BorderRadius.TopRight - style.Box.BorderThickness;
            cornerRadius = new ImRectRadius(radiusTopLeft, radiusTopRight);

            return window.WithPadding(style.Box.BorderThickness).TakeTop(height);
        }

        public static ImRect GetMenuBarRect(ImGui gui, in ImWindowState state)
        {
            var rect = state.Rect;

            if ((state.Flags & ImWindowFlag.NoTitleBar) == 0)
            {
                rect.TakeTop(GetTitleBarHeight(gui), out rect);
            }

            return rect.TakeTop(GetMenuBarHeight(gui));
        }

        public static ImRect GetContentRect(ImGui gui, in ImWindowState state)
        {
            var content = state.Rect;

            if ((state.Flags & ImWindowFlag.NoTitleBar) == 0)
            {
                content.TakeTop(GetTitleBarHeight(gui), out content);
            }

            if ((state.Flags & ImWindowFlag.HasMenuBar) != 0)
            {
                content.TakeTop(GetMenuBarHeight(gui), out content);
            }

            return content.WithPadding(gui.Style.Window.ContentPadding);
        }

        public static float GetTitleBarHeight(ImGui gui) => gui.Style.Layout.InnerSpacing * 2.0f + gui.GetRowHeight();
        public static float GetMenuBarHeight(ImGui gui) => ImMenuBar.GetMenuBarHeight(gui);

        private static ImRect GetInitialWindowRect(ImGui gui, ImSize size)
        {
            var (width, height) = size.Mode switch
            {
                ImSizeMode.Fixed => (size.Width, size.Height),
                _ => (DEFAULT_WIDTH, DEFAULT_HEIGHT)
            };

            var screenSize = gui.Canvas.ScreenSize;
            var position = new Vector2((screenSize.x - width) / 2f, (screenSize.y - height) / 2f);

            return new ImRect(position.x, position.y, width, height);
        }

        private static Vector2 KeepWindowInsideScreen(ImGui gui, Vector2 position, Vector2 size)
        {
            var titleBarHeight = GetTitleBarHeight(gui);
            var screenRect = gui.Canvas.ScreenRect;
            var left = screenRect.Left - size.x + titleBarHeight * 2; // close button
            var right = screenRect.Right - titleBarHeight;
            var top = screenRect.Top - size.y;
            var bottom = screenRect.Bottom - size.y + titleBarHeight;
            
            position.x = Mathf.Clamp(position.x, left, right);
            position.y = Mathf.Clamp(position.y, bottom, top);

            return position;
        }
    }
}