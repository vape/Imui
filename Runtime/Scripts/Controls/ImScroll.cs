using System;
using System.Runtime.CompilerServices;
using Imui.Core;
using Imui.IO.Events;
using UnityEngine;

namespace Imui.Controls
{
    [Flags]
    public enum ImScrollFlag
    {
        None = 0,
        HideVerBar = 1 << 0,
        HideHorBar = 1 << 1,
        PersistentHorBar = 1 << 2,
        PersistentVerBar = 1 << 3,
        DisableInertia = 1 << 4
    }

    [Flags]
    public enum ImScrollStateFlag
    {
        None = 0,
        VerBarVisible = 1 << 0,
        HorBarVisible = 1 << 1,
        VerScrollable = 1 << 2,
        HorScrollable = 1 << 3,
        ConventionalScroll = 1 << 4
    }
    
    public struct ImScrollState
    {
        public Vector2 Offset;
        public Vector2 Velocity;
        public ImScrollStateFlag State;
        public ImScrollFlag Flags;
    }

    public static class ImScroll
    {
        public const float DECELERATION_RATE = 0.15f;
        public const float VELOCITY_SHARPNESS = 15;
        
        public static void BeginScrollable(this ImGui gui)
        {
            var id = gui.GetNextControlId();
            ref var state = ref gui.BeginScope<ImScrollState>(id);

            ref readonly var frame = ref gui.Layout.GetFrame();
            var visibleRect = GetVisibleRect(gui, frame.Bounds, state);

            gui.Layout.Push(frame.Axis, visibleRect, ImLayoutFlag.None);
            gui.Layout.SetOffset(state.Offset);
        }

        public static void EndScrollable(this ImGui gui, ImScrollFlag flags = ImScrollFlag.None)
        {
            ref var state = ref gui.EndScope<ImScrollState>(out var id);
            state.Flags = flags;

            gui.Layout.Pop(out var contentFrame);

            var bounds = gui.Layout.GetBoundsRect();

            Scroll(gui, id, ref state, bounds, contentFrame.Size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector2 GetScrollOffset(this ImGui gui)
        {
            return gui.GetCurrentScopeUnsafe<ImScrollState>()->Offset;
        }

        public static void SetScrollOffset(this ImGui gui, Vector2 offset)
        {
            ref var state = ref gui.GetCurrentScope<ImScrollState>();
            state.Offset = offset;
        }

        public static void Scroll(ImGui gui, uint id, ref ImScrollState state, ImRect view, Vector2 size)
        {
            const ImScrollStateFlag ANY_AXES_SCROLLABLE = ImScrollStateFlag.HorScrollable | ImScrollStateFlag.VerScrollable;
            
            Layout(gui, ref state, view, size, out var adjust);

            var dx = 0f;
            var dy = 0f;

            var horId = gui.GetNextControlId();
            var verId = gui.GetNextControlId();

            size.x += adjust.x;
            size.y += adjust.y;

            if ((state.State & ImScrollStateFlag.VerBarVisible) != 0)
            {
                var rect = GetVerticalBarRect(gui, view);
                var normalSize = view.H / size.y;
                var normalPosition = state.Offset.y / (size.y - view.H);
                var normalDelta = Bar(verId, gui, rect, normalSize, normalPosition, 1);
                if (normalDelta != 0)
                {
                    state.State |= ImScrollStateFlag.ConventionalScroll;
                }

                dy -= normalDelta * size.y;
            }

            if ((state.State & ImScrollStateFlag.HorBarVisible) != 0)
            {
                var rect = GetHorizontalBarRect(gui, view, (state.State & ImScrollStateFlag.VerBarVisible) != 0);
                var normalSize = view.W / size.x;
                var normalPosition = (state.Offset.x / (size.x - view.W));
                var normalDelta = Bar(horId, gui, rect, normalSize, -normalPosition, 0);
                if (normalDelta != 0)
                {
                    state.State |= ImScrollStateFlag.ConventionalScroll;
                }

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
            var scrollable = (state.State & ANY_AXES_SCROLLABLE) != 0;
            
            if (!active && groupHovered && gui.Input.WasMouseDownThisFrame)
            {
                state.Velocity = default;
            }

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Scroll when groupHovered:
                    var factor = gui.Style.Layout.TextSize;
                    state.State |= ImScrollStateFlag.ConventionalScroll;
                    dx += evt.Delta.x * factor;
                    dy += evt.Delta.y * factor;
                    deferredUseMouseEvent = true;
                    break;
                
                case ImMouseEventType.BeginDrag when scrollable && groupHovered && !active && !gui.ActiveControlIs(ImControlFlag.Draggable):
                    state.Velocity = default;
                    state.State &= ~ImScrollStateFlag.ConventionalScroll;
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

            state.Offset.x += dx;
            state.Offset.y += dy;

            if (!active)
            {
                state.Offset += state.Velocity;
            }
            
            var targetOffset = new Vector2(
                Mathf.Clamp(state.Offset.x, Mathf.Min(0, view.W - size.x), 0.0f), 
                Mathf.Clamp(state.Offset.y, 0.0f, Mathf.Max(size.y - view.H, 0)));

            if ((state.Flags & ImScrollFlag.DisableInertia) == 0)
            {
                ProcessVelocity(active, ref state.Velocity, in prevOffset, in state.Offset);
            }
            else
            {
                state.Velocity = default;
            }
            
            state.Offset = targetOffset;

            // defer mouse event consumption, so we can pass it to parent scroll rect in case offset hasn't changed
            if (prevOffset != state.Offset && deferredUseMouseEvent)
            {
                gui.Input.UseMouseEvent();
            }
        }

        private static void ProcessVelocity(bool active, ref Vector2 velocity, in Vector2 prevOffset, in Vector2 currentOffset)
        {
            if (active)
            {
                velocity = Vector2.Lerp(velocity, currentOffset - prevOffset, Time.deltaTime * VELOCITY_SHARPNESS);
            }
            else
            {
                velocity *= Mathf.Pow(DECELERATION_RATE, Time.deltaTime);
            }
        }
        
        public static float Bar(uint id,
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
            var minSize = (axis == 0 ? rect.H : rect.W) - style.BorderThickness;
            var size = Mathf.Max(minSize, Mathf.Clamp01(normalSize) * absoluteSize);
            var position = Mathf.Clamp01(normalPosition) * (absoluteSize - size);

            var handleRect = axis == 0
                ? new ImRect(rect.X + position, rect.Y, size, rect.H)
                : new ImRect(rect.X, rect.Y + (rect.H - size) - position, rect.W, size);
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

                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when evt.LeftButton && hovered:
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
            if ((state.State & ImScrollStateFlag.HorBarVisible) != 0 || (state.Flags & ImScrollFlag.PersistentHorBar) != 0)
            {
                var size = GetScrollBarSize(gui, 0);

                view.Y += size;
                view.H -= size;
            }

            if ((state.State & ImScrollStateFlag.VerBarVisible) != 0 || (state.Flags & ImScrollFlag.PersistentVerBar) != 0)
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
        
        private static void Layout(ImGui gui, ref ImScrollState state, ImRect view, Vector2 size, out Vector2 adjust)
        {
            var styleSizeVer = GetScrollBarSize(gui, 1);
            var styleSizeHor = GetScrollBarSize(gui, 0);
            var flags = state.Flags;

            state.State = default;

            // doing calculations twice because showing one bar may require showing another
            for (int i = 0; i < 2; ++i)
            {
                var fitsInContainerVer = (size.y - (view.H - ((state.State & ImScrollStateFlag.HorBarVisible) != 0 ? styleSizeVer : 0f))) > 1.0f;
                var fitsInContainerHor = (size.x - (view.W - ((state.State & ImScrollStateFlag.VerBarVisible) != 0 ? styleSizeHor : 0f))) > 1.0f;
                
                state.State = fitsInContainerVer ? (state.State | ImScrollStateFlag.VerScrollable) : (state.State & ~ImScrollStateFlag.VerScrollable);
                state.State = fitsInContainerHor ? (state.State | ImScrollStateFlag.HorScrollable) : (state.State & ~ImScrollStateFlag.HorScrollable);
                
                state.State =
                    ((flags & ImScrollFlag.HideVerBar) == 0 && (state.State & ImScrollStateFlag.VerScrollable) != 0) ||
                    (flags & ImScrollFlag.PersistentVerBar) != 0
                        ? (state.State | ImScrollStateFlag.VerBarVisible)
                        : (state.State & ~ImScrollStateFlag.VerBarVisible);

                state.State =
                    ((flags & ImScrollFlag.HideHorBar) == 0 && (state.State & ImScrollStateFlag.HorScrollable) != 0) ||
                    (flags & ImScrollFlag.PersistentHorBar) != 0
                        ? (state.State | ImScrollStateFlag.HorBarVisible)
                        : (state.State & ~ImScrollStateFlag.HorBarVisible);
            }

            adjust = new Vector2(
                (state.State & ImScrollStateFlag.VerBarVisible) != 0 ? styleSizeVer : 0f,
                (state.State & ImScrollStateFlag.HorBarVisible) != 0 ? styleSizeHor : 0f);
        }

        private static float GetScrollBarSize(ImGui gui, int axis)
        {
            return (int)(gui.Style.Scroll.Size + (axis == 0 ? gui.Style.Scroll.HMargin.Vertical : gui.Style.Scroll.VMargin.Horizontal));
        }
    }
}