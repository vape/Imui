using System;
using Imui.Core;
using Imui.IO.Events;
using UnityEngine;

namespace Imui.Controls
{
    [Flags]
    public enum ImScrollFlag
    {
        None            = 0,
        NoVerticalBar   = 1 << 0,
        NoHorizontalBar = 1 << 1
    }
    
    [Flags]
    public enum ImScrollLayoutFlag
    {
        None          = 0,
        VerBarVisible = 1 << 0,
        HorBarVisible = 1 << 1
    }
    
    public struct ImScrollState
    {
        public Vector2 Offset;
        public ImScrollLayoutFlag Layout;
    }
    
    public static class ImScroll
    {
        public static void BeginScrollable(this ImGui gui)
        {
            var id = gui.GetNextControlId();
            ref var state = ref gui.PushControlScope<ImScrollState>(id);
            
            ref readonly var frame = ref gui.Layout.GetFrame();
            var visibleRect = GetVisibleRect(gui, frame.Bounds, state);
            
            gui.Layout.Push(frame.Axis, visibleRect, ImLayoutFlag.None);
            gui.Layout.SetOffset(state.Offset);
        }
        
        public static void EndScrollable(this ImGui gui, ImScrollFlag flags = ImScrollFlag.None)
        {
            ref var state = ref gui.PopControlScope<ImScrollState>(out var id);
            
            gui.Layout.Pop(out var contentFrame);

            var bounds = gui.Layout.GetBoundsRect();
            
            Scroll(gui, id, ref state, bounds, contentFrame.Size, flags);
        }

        public static Vector2 GetScrollOffset(this ImGui gui)
        {
            return gui.PeekControlScope<ImScrollState>().Offset;
        }

        public static void SetScrollOffset(this ImGui gui, Vector2 offset)
        {
            ref var state = ref gui.PeekControlScope<ImScrollState>();
            state.Offset = offset;
        }
        
        public static void Scroll(ImGui gui, uint id, ref ImScrollState state, ImRect view, Vector2 size, ImScrollFlag flags)
        {
            Layout(gui, ref state, view, size, out var adjust, flags);

            var dx = 0f;
            var dy = 0f;

            var horId = gui.GetNextControlId();
            var verId = gui.GetNextControlId();

            size.x += adjust.x;
            size.y += adjust.y;
            
            if ((state.Layout & ImScrollLayoutFlag.VerBarVisible) != 0)
            {
                var rect = GetVerticalBarRect(gui, view);
                var normalSize = view.H / size.y;
                var normalPosition = state.Offset.y / (size.y - view.H);
                var normalDelta = Bar(verId, gui, rect, normalSize, normalPosition, 1);

                dy -= normalDelta * size.y;
            }

            if ((state.Layout & ImScrollLayoutFlag.HorBarVisible) != 0)
            {
                var rect = GetHorizontalBarRect(gui, view, (state.Layout & ImScrollLayoutFlag.VerBarVisible) != 0);
                var normalSize = view.W / size.x;
                var normalPosition = (state.Offset.x / (size.x - view.W));
                var normalDelta = Bar(horId, gui, rect, normalSize, -normalPosition, 0);

                dx -= normalDelta * size.x;
            }
            
            var groupRect = GetVisibleRect(gui, view, state);
            gui.RegisterGroup(id, groupRect);

            // Scroll bars should probably work even in read only mode
            // if (gui.IsReadOnly)
            // {
            //     return;
            // }

            var deferredUseMouseEvent = false;
            var groupHovered = gui.IsGroupHovered(id);
            var active = gui.IsControlActive(id);

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Scroll when groupHovered:
                    var scale = gui.Style.Layout.ScrollSpeedScale;
                    dx += evt.Delta.x * scale;
                    dy += evt.Delta.y * scale;
                    deferredUseMouseEvent = true;
                    break;
                
                case ImMouseEventType.BeginDrag when groupHovered && !active && !gui.ActiveControlIs(ImControlFlag.Draggable):
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                    break;
                
                case ImMouseEventType.Drag when active:
                    dx += evt.Delta.x;
                    dy += evt.Delta.y;
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Up when active:
                    gui.ResetActiveControl();
                    break;
            }
            
            var prevOffset = state.Offset;
            
            state.Offset.x = Mathf.Clamp(state.Offset.x + dx, Mathf.Min(0, view.W - size.x), 0);
            state.Offset.y = Mathf.Clamp(state.Offset.y + dy, 0, Mathf.Max(size.y - view.H, 0));

            // defer mouse event consumption so we can pass it to parent scroll rect in case offset hasn't changed
            if (prevOffset != state.Offset && deferredUseMouseEvent)
            {
                gui.Input.UseMouseEvent();
            }
        }
        
        public static float Bar(
            uint id,
            ImGui gui,
            ImRect rect, 
            float normalSize, 
            float normalPosition, 
            int axis)
        {
            ref readonly var style = ref gui.Style.Scroll;
                
            rect = rect.WithPadding(axis == 0 ? style.HMargin : style.VMargin);

            var delta = 0f;
            var absoluteSize = axis == 0 ? rect.W : rect.H;
            var size = Mathf.Clamp01(normalSize) * absoluteSize;
            var position = Mathf.Clamp01(normalPosition) * (absoluteSize - size);
            
            var handleRect = axis == 0 ? 
                new ImRect(rect.X + position, rect.Y, size, rect.H) : 
                new ImRect(rect.X, rect.Y + (rect.H - size) - position, rect.W, size);
            handleRect = handleRect.WithPadding(style.BorderThickness);

            var hovered = gui.IsControlHovered(id);
            var pressed = gui.IsControlActive(id);
            
            var barStyle = pressed ? style.PressedState : hovered ? style.HoveredState : style.NormalState;
            gui.Canvas.Rect(rect, barStyle.BackColor, style.BorderRadius);
            gui.Canvas.Rect(handleRect, barStyle.FrontColor, Mathf.Max(0, style.BorderRadius - style.BorderThickness));

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Up when pressed:
                    gui.ResetActiveControl();
                    break;
                
                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when hovered:
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Drag when pressed:
                    delta = evt.Delta[axis] / absoluteSize;
                    gui.Input.UseMouseEvent();
                    break;
            }
            
            gui.RegisterControl(id, rect);

            return delta;
        }

        public static ImRect GetVisibleRect(ImGui gui, ImRect view, ImScrollState state)
        {
            if ((state.Layout & ImScrollLayoutFlag.HorBarVisible) != 0)
            {
                var size = GetScrollBarSize(gui, 0);
                
                view.Y += size;
                view.H -= size;
            }

            if ((state.Layout & ImScrollLayoutFlag.VerBarVisible) != 0)
            {
                view.W -= GetScrollBarSize(gui, 1);
            }

            return view;
        }
        
        public static ImRect GetHorizontalBarRect(ImGui gui, ImRect view, bool verBarVisible)
        {
            view.H = GetScrollBarSize(gui, 0);

            if (verBarVisible)
            {
                view.W -= GetScrollBarSize(gui, 1);
            }
            
            return view;
        }

        public static ImRect GetVerticalBarRect(ImGui gui, ImRect view)
        {
            var size = GetScrollBarSize(gui, 1);
            
            view.X += view.W - size;
            view.W = size;

            return view;
        }
        
        private static void Layout(ImGui gui, ref ImScrollState state, ImRect view, Vector2 size, out Vector2 adjust, ImScrollFlag flags)
        {
            var styleSizeVer = GetScrollBarSize(gui, 1);
            var styleSizeHor = GetScrollBarSize(gui, 0);
            
            state.Layout = default;

            // doing calculations twice because showing one bar may require showing another
            for (int i = 0; i < 2; ++i)
            {
                state.Layout =
                    (flags & ImScrollFlag.NoVerticalBar) == 0 && size.y > (view.H - ((state.Layout & ImScrollLayoutFlag.HorBarVisible) != 0 ? styleSizeVer : 0f))
                        ? (state.Layout | ImScrollLayoutFlag.VerBarVisible)
                        : (state.Layout & ~ImScrollLayoutFlag.VerBarVisible);

                state.Layout =
                    (flags & ImScrollFlag.NoHorizontalBar) == 0 && size.x > (view.W - ((state.Layout & ImScrollLayoutFlag.VerBarVisible) != 0 ? styleSizeHor : 0f))
                        ? (state.Layout | ImScrollLayoutFlag.HorBarVisible)
                        : (state.Layout & ~ImScrollLayoutFlag.HorBarVisible);
            }

            adjust = new Vector2(
                (state.Layout & ImScrollLayoutFlag.VerBarVisible) != 0 ? styleSizeVer : 0f, 
                (state.Layout & ImScrollLayoutFlag.HorBarVisible) != 0 ? styleSizeHor : 0f);
        }

        private static float GetScrollBarSize(ImGui gui, int axis)
        {
            return (int)(gui.Style.Scroll.Size + (axis == 0 ? gui.Style.Scroll.HMargin.Vertical : gui.Style.Scroll.VMargin.Horizontal));
        }
    }
}