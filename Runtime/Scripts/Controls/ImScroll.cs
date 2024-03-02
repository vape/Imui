using System;
using Imui.Core;
using Imui.Core.Input;
using Imui.Styling;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImScroll
    {
        public static ImScrollStyle Style = ImScrollStyle.Default;
        
        public static void Scroll(ImGui gui, in ImRect view, Vector2 size, ref State state)
        {
            Layout(ref state, in view, size, out var adjust);

            var dx = 0f;
            var dy = 0f;

            var horId = gui.GetControlId("h_bar");
            var verId = gui.GetControlId("v_bar");

            size.x += adjust.x;
            size.y += adjust.y;
            
            if ((state.Layout & LayoutFlag.VerBarVisible) != 0)
            {
                var rect = GetVerticalBarRect(view);
                var normalSize = view.H / size.y;
                var normalPosition = state.Offset.y / (size.y - view.H);
                var normalDelta = Bar(verId, gui, rect, normalSize, normalPosition, 1);

                dy -= normalDelta * size.y;
            }

            if ((state.Layout & LayoutFlag.HorBarVisible) != 0)
            {
                var rect = GetHorizontalBarRect(view, (state.Layout & LayoutFlag.VerBarVisible) != 0);
                var normalSize = view.W / size.x;
                var normalPosition = (state.Offset.x / (size.x - view.W));
                var normalDelta = Bar(horId, gui, rect, normalSize, -normalPosition, 0);

                dx -= normalDelta * size.x;
            }

            state.Offset.x = Mathf.Clamp(state.Offset.x + dx, Mathf.Min(0, view.W - size.x), 0);
            state.Offset.y = Mathf.Clamp(state.Offset.y + dy, 0, Mathf.Max(size.y - view.H, 0));
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
            var pressed = gui.ActiveControl == id;
            
            var barStyle = pressed ? Style.PressedState : hovered ? Style.HoveredState : Style.NormalState;
            gui.Canvas.Rect(rect, barStyle.BackColor, Style.CornerRadius);
            gui.Canvas.Rect(handleRect, barStyle.FrontColor, Style.CornerRadius - Style.Padding);

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

        public static ImRect GetVisibleRect(ImRect view, in State state)
        {
            if ((state.Layout & LayoutFlag.HorBarVisible) != 0)
            {
                view.Y += Style.Width;
                view.H -= Style.Width;
            }

            if ((state.Layout & LayoutFlag.VerBarVisible) != 0)
            {
                view.W -= Style.Width;
            }

            return view;
        }
        
        public static ImRect GetHorizontalBarRect(ImRect view, bool verBarVisible)
        {
            view.H = Style.Width;

            if (verBarVisible)
            {
                view.W -= Style.Width;
            }
            
            return view;
        }

        public static ImRect GetVerticalBarRect(ImRect view)
        {
            view.X += view.W - Style.Width;
            view.W = Style.Width;

            return view;
        }
        
        private static void Layout(ref State state, in ImRect view, in Vector2 size, out Vector2 adjust)
        {
            state.Layout = default;

            // doing calculations twice because showing one bar may require showing another
            for (int i = 0; i < 2; ++i)
            {
                state.Layout =
                    size.y > (view.H - ((state.Layout & LayoutFlag.HorBarVisible) != 0 ? Style.Width : 0f))
                        ? (state.Layout | LayoutFlag.VerBarVisible)
                        : (state.Layout & ~LayoutFlag.VerBarVisible);

                state.Layout =
                    size.x > (view.W - ((state.Layout & LayoutFlag.VerBarVisible) != 0 ? Style.Width : 0f))
                        ? (state.Layout | LayoutFlag.HorBarVisible)
                        : (state.Layout & ~LayoutFlag.HorBarVisible);
            }

            adjust = new Vector2(
                (state.Layout & LayoutFlag.VerBarVisible) != 0 ? Style.Width : 0f, 
                (state.Layout & LayoutFlag.HorBarVisible) != 0 ? Style.Width : 0f);
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
            Width = 20,
            Margin = 1,
            Padding = 1,
            CornerRadius = 3,
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
        
        public float Width;
        public float Margin;
        public float Padding;
        public float CornerRadius;
        public ImScrollBarStateStyle NormalState;
        public ImScrollBarStateStyle HoveredState;
        public ImScrollBarStateStyle PressedState;
    }
}