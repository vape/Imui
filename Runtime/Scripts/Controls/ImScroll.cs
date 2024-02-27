using System;
using Imui.Core;
using Imui.Core.Input;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImScroll
    {
        public static void Scroll(ImGui gui, in ImRect view, Vector2 size, ref State state, in Style style)
        {
            Layout(ref state, in view, size, in style, out var adjust);

            var dx = 0f;
            var dy = 0f;

            var horId = gui.GetNextControlId();
            var verId = gui.GetNextControlId();

            size.x += adjust.x;
            size.y += adjust.y;
            
            if ((state.Layout & LayoutFlag.VerBarVisible) != 0)
            {
                var rect = GetVerticalBarRect(view, in style);
                var normalSize = view.H / size.y;
                var normalPosition = state.Offset.y / (size.y - view.H);
                var normalDelta = Bar(verId, gui, in style, rect, normalSize, normalPosition, 1);

                dy -= normalDelta * size.y;
            }

            if ((state.Layout & LayoutFlag.HorBarVisible) != 0)
            {
                var rect = GetHorizontalBarRect(view, in style, (state.Layout & LayoutFlag.VerBarVisible) != 0);
                var normalSize = view.W / size.x;
                var normalPosition = (state.Offset.x / (size.x - view.W));
                var normalDelta = Bar(horId, gui, in style, rect, normalSize, -normalPosition, 0);

                dx -= normalDelta * size.x;
            }

            state.Offset.x = Mathf.Clamp(state.Offset.x + dx, Mathf.Min(0, view.W - size.x), 0);
            state.Offset.y = Mathf.Clamp(state.Offset.y + dy, 0, Mathf.Max(size.y - view.H, 0));
        }
        
        public static float Bar(
            int id,
            ImGui gui,
            in Style style, 
            ImRect rect, 
            float normalSize, 
            float normalPosition, 
            int axis)
        {
            rect = rect.AddPadding(style.Margin);

            var delta = 0f;
            var absoluteSize = axis == 0 ? rect.W : rect.H;
            var size = Mathf.Clamp01(normalSize) * absoluteSize;
            var position = Mathf.Clamp01(normalPosition) * (absoluteSize - size);
            
            var handleRect = axis == 0 ? 
                new ImRect(rect.X + position, rect.Y, size, rect.H) : 
                new ImRect(rect.X, rect.Y + (rect.H - size) - position, rect.W, size);
            handleRect = handleRect.AddPadding(style.Padding);

            var hovered = gui.IsControlHovered(id);
            var pressed = gui.ActiveControl == id;
            
            var barStyle = pressed ? style.PressedState : hovered ? style.HoveredState : style.NormalState;
            gui.Canvas.Rect(rect, barStyle.BackColor, style.CornerRadius);
            gui.Canvas.Rect(handleRect, barStyle.FrontColor, style.CornerRadius - style.Padding);

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImInputEventMouseType.Up:
                    if (pressed)
                    {
                        gui.ActiveControl = default;
                    }

                    break;
                case ImInputEventMouseType.Down:
                    if (evt.Button == 0 && hovered)
                    {
                        gui.ActiveControl = id;
                        gui.Input.UseMouse();
                    }

                    break;
                
                case ImInputEventMouseType.Drag:
                    if (pressed)
                    {
                        delta = evt.Delta[axis] / absoluteSize;
                        gui.Input.UseMouse();
                    }

                    break;
            }
            
            gui.HandleControl(id, rect);

            return delta;
        }

        public static ImRect GetVisibleRect(ImRect view, in State state, in Style style)
        {
            if ((state.Layout & LayoutFlag.HorBarVisible) != 0)
            {
                view.Y += style.Width;
                view.H -= style.Width;
            }

            if ((state.Layout & LayoutFlag.VerBarVisible) != 0)
            {
                view.W -= style.Width;
            }

            return view;
        }
        
        public static ImRect GetHorizontalBarRect(ImRect view, in Style style, bool verBarVisible)
        {
            view.H = style.Width;

            if (verBarVisible)
            {
                view.W -= style.Width;
            }
            
            return view;
        }

        public static ImRect GetVerticalBarRect(ImRect view, in Style style)
        {
            view.X += view.W - style.Width;
            view.W = style.Width;

            return view;
        }
        
        private static void Layout(ref State state, in ImRect view, in Vector2 size, in Style style, out Vector2 adjust)
        {
            state.Layout = default;

            // doing calculations twice because showing one bar may require showing another
            for (int i = 0; i < 2; ++i)
            {
                state.Layout =
                    size.y > (view.H - ((state.Layout & LayoutFlag.HorBarVisible) != 0 ? style.Width : 0f))
                        ? (state.Layout | LayoutFlag.VerBarVisible)
                        : (state.Layout & ~LayoutFlag.VerBarVisible);

                state.Layout =
                    size.x > (view.W - ((state.Layout & LayoutFlag.VerBarVisible) != 0 ? style.Width : 0f))
                        ? (state.Layout | LayoutFlag.HorBarVisible)
                        : (state.Layout & ~LayoutFlag.HorBarVisible);
            }

            adjust = new Vector2(
                (state.Layout & LayoutFlag.VerBarVisible) != 0 ? style.Width : 0f, 
                (state.Layout & LayoutFlag.HorBarVisible) != 0 ? style.Width : 0f);
        }

        [Flags]
        public enum LayoutFlag
        {
            None          = 0,
            VerBarVisible = 1 << 0,
            HorBarVisible = 1 << 1
        }
        
        [Serializable]
        public struct State
        {
            public Vector2 Offset;
            public LayoutFlag Layout;
        }
        
        [Serializable]
        public struct StateStyle
        {
            public Color32 BackColor;
            public Color32 FrontColor;
        }

        [Serializable]
        public struct Style
        {
            public float Width;
            public float Margin;
            public float Padding;
            public float CornerRadius;
            public StateStyle NormalState;
            public StateStyle HoveredState;
            public StateStyle PressedState;
        }
    }
}