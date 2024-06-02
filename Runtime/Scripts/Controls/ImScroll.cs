using System;
using Imui.Core;
using Imui.IO.Events;
using Imui.Styling;
using Imui.Utility;
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
    
    [Serializable]
    public struct ImScrollState
    {
        public Vector2 Offset;
        public ImScrollLayoutFlag Layout;
    }
    
    public static class ImScroll
    {
        public static ImScrollStyle Style = ImScrollStyle.Default;
        
        public static void BeginScrollable(this ImGui gui)
        {
            var id = gui.GetNextControlId();
            var state = gui.Storage.Get<ImScrollState>(id);
            
            ref readonly var frame = ref gui.Layout.GetFrame();
            var visibleRect = GetVisibleRect(frame.Bounds, in state);
            
            gui.Layout.Push(frame.Axis, visibleRect, ImLayoutFlag.None);
            gui.Layout.SetOffset(state.Offset);
            gui.BeginScope(id);
        }
        
        public static void EndScrollable(this ImGui gui, ImScrollFlag flags = ImScrollFlag.None)
        {
            gui.EndScope(out var id);
            gui.Layout.Pop(out var contentFrame);

            var bounds = gui.Layout.GetBoundsRect();
            
            Scroll(gui, id, in bounds, contentFrame.Size, flags);
        }

        public static Vector2 GetScrollOffset(this ImGui gui)
        {
            var id = gui.GetScope();
            
            return gui.Storage.Get<ImScrollState>(id).Offset;
        }

        public static void SetScrollOffset(this ImGui gui, Vector2 offset)
        {
            var id = gui.GetScope();
            ref var state = ref gui.Storage.Get<ImScrollState>(id);
            
            state.Offset = offset;
        }
        
        public static void Scroll(ImGui gui, uint id, in ImRect view, Vector2 size, ImScrollFlag flags)
        {
            ref var state = ref gui.Storage.Get<ImScrollState>(id);
            
            Layout(ref state, in view, size, out var adjust, flags);

            var dx = 0f;
            var dy = 0f;

            var horId = gui.GetNextControlId();
            var verId = gui.GetNextControlId();

            size.x += adjust.x;
            size.y += adjust.y;
            
            if ((state.Layout & ImScrollLayoutFlag.VerBarVisible) != 0)
            {
                var rect = GetVerticalBarRect(view);
                var normalSize = view.H / size.y;
                var normalPosition = state.Offset.y / (size.y - view.H);
                var normalDelta = Bar(verId, gui, rect, normalSize, normalPosition, 1);

                dy -= normalDelta * size.y;
            }

            if ((state.Layout & ImScrollLayoutFlag.HorBarVisible) != 0)
            {
                var rect = GetHorizontalBarRect(view, (state.Layout & ImScrollLayoutFlag.VerBarVisible) != 0);
                var normalSize = view.W / size.x;
                var normalPosition = (state.Offset.x / (size.x - view.W));
                var normalDelta = Bar(horId, gui, rect, normalSize, -normalPosition, 0);

                dx -= normalDelta * size.x;
            }

            var deferredUseMouseEvent = false;
            var groupHovered = gui.IsGroupHovered(id);
            var active = gui.IsControlActive(id);

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Scroll when groupHovered:
                    var scale = ImControls.Style.ScrollSpeedScale;
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

            var groupRect = GetVisibleRect(view, in state);
            var prevOffset = state.Offset;
            
            state.Offset.x = Mathf.Clamp(state.Offset.x + dx, Mathf.Min(0, view.W - size.x), 0);
            state.Offset.y = Mathf.Clamp(state.Offset.y + dy, 0, Mathf.Max(size.y - view.H, 0));

            // defer mouse event consumption so we can pass it to parent scroll rect in case offset hasn't changed
            if (prevOffset != state.Offset && deferredUseMouseEvent)
            {
                gui.Input.UseMouseEvent();
            }
            
            gui.RegisterGroup(id, groupRect);
        }
        
        public static float Bar(
            uint id,
            ImGui gui,
            ImRect rect, 
            float normalSize, 
            float normalPosition, 
            int axis)
        {
            rect = rect.WithPadding(Style.Margin);

            var delta = 0f;
            var absoluteSize = axis == 0 ? rect.W : rect.H;
            var size = Mathf.Clamp01(normalSize) * absoluteSize;
            var position = Mathf.Clamp01(normalPosition) * (absoluteSize - size);
            
            var handleRect = axis == 0 ? 
                new ImRect(rect.X + position, rect.Y, size, rect.H) : 
                new ImRect(rect.X, rect.Y + (rect.H - size) - position, rect.W, size);
            handleRect = handleRect.WithPadding(Style.Padding);

            var hovered = gui.IsControlHovered(id);
            var pressed = gui.IsControlActive(id);
            
            var barStyle = pressed ? Style.PressedState : hovered ? Style.HoveredState : Style.NormalState;
            gui.Canvas.Rect(rect, barStyle.BackColor, Style.BorderRadius);
            gui.Canvas.Rect(handleRect, barStyle.FrontColor, Style.BorderRadius - Style.Padding);

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

        public static ImRect GetVisibleRect(ImRect view, in ImScrollState state)
        {
            if ((state.Layout & ImScrollLayoutFlag.HorBarVisible) != 0)
            {
                view.Y += Style.Size;
                view.H -= Style.Size;
            }

            if ((state.Layout & ImScrollLayoutFlag.VerBarVisible) != 0)
            {
                view.W -= Style.Size;
            }

            return view;
        }
        
        public static ImRect GetHorizontalBarRect(ImRect view, bool verBarVisible)
        {
            view.H = Style.Size;

            if (verBarVisible)
            {
                view.W -= Style.Size;
            }
            
            return view;
        }

        public static ImRect GetVerticalBarRect(ImRect view)
        {
            view.X += view.W - Style.Size;
            view.W = Style.Size;

            return view;
        }
        
        private static void Layout(ref ImScrollState state, in ImRect view, in Vector2 size, out Vector2 adjust, ImScrollFlag flags)
        {
            state.Layout = default;

            // doing calculations twice because showing one bar may require showing another
            for (int i = 0; i < 2; ++i)
            {
                state.Layout =
                    (flags & ImScrollFlag.NoVerticalBar) == 0 && size.y > (view.H - ((state.Layout & ImScrollLayoutFlag.HorBarVisible) != 0 ? Style.Size : 0f))
                        ? (state.Layout | ImScrollLayoutFlag.VerBarVisible)
                        : (state.Layout & ~ImScrollLayoutFlag.VerBarVisible);

                state.Layout =
                    (flags & ImScrollFlag.NoHorizontalBar) == 0 && size.x > (view.W - ((state.Layout & ImScrollLayoutFlag.VerBarVisible) != 0 ? Style.Size : 0f))
                        ? (state.Layout | ImScrollLayoutFlag.HorBarVisible)
                        : (state.Layout & ~ImScrollLayoutFlag.HorBarVisible);
            }

            adjust = new Vector2(
                (state.Layout & ImScrollLayoutFlag.VerBarVisible) != 0 ? Style.Size : 0f, 
                (state.Layout & ImScrollLayoutFlag.HorBarVisible) != 0 ? Style.Size : 0f);
        }
    }
    
            
    [Serializable]
    public struct ImScrollBarStateStyle
    {
        public Color32 BackColor;
        public Color32 FrontColor;
    }

    [Serializable]
    public struct ImScrollStyle
    {
        public static readonly ImScrollStyle Default = new()
        {
            Size = 20,
            Margin = 1,
            Padding = 1,
            BorderRadius = 3,
            NormalState = new ImScrollBarStateStyle()
            {
                BackColor = ImColors.Black,
                FrontColor = ImColors.Gray7
            },
            HoveredState = new ImScrollBarStateStyle()
            {
                BackColor = ImColors.Black,
                FrontColor = ImColors.Gray8
            },
            PressedState = new ImScrollBarStateStyle()
            {
                BackColor  = ImColors.Black,
                FrontColor = ImColors.Gray6
            }
        };
        
        public float Size;
        public float Margin;
        public float Padding;
        public float BorderRadius;
        public ImScrollBarStateStyle NormalState;
        public ImScrollBarStateStyle HoveredState;
        public ImScrollBarStateStyle PressedState;
    }
}