using System;
using Imui.Core;
using Imui.Core.Input;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImWindow
    {
        public static void DrawBack(ImGui gui, in State state, in Style style, out ImRect content)
        {
            gui.Canvas.Rect(state.Rect, style.BackColor, style.CornerRadius);
            gui.Canvas.RectOutline(state.Rect, style.FrameColor, style.FrameWidth, style.CornerRadius);

            var titleBarRect = GetTitleBarRect(in state, in style, out _);
            state.Rect.AddPadding(style.FrameWidth).SplitTop(titleBarRect.H, out content);
        }

        public static void DrawFront(ImGui gui, in State state, in Style style)
        {
            DrawTitleBar(gui, in state, in style);
            DrawResizeHandle(gui, in state, in style);
        }

        public static void DrawTitleBar(ImGui gui, in State state, in Style style)
        {
            var rect = GetTitleBarRect(in state, in style, out var radius);
            var segments = gui.MeshDrawer.GetSegmentsCount(style.CornerRadius);
            
            gui.Canvas.Rect(
                rect, 
                style.TitleBar.BackColor, 
                gui.Canvas.DefaultTexScaleOffset,
                radius, segments);
            
            gui.Canvas.Text(state.Title, style.TitleBar.FrontColor, rect, in style.TitleBar.Text);
        }

        public static void DrawResizeHandle(ImGui gui, in State state, in Style style)
        {
            const float PI = Mathf.PI;
            const float HALF_PI = PI / 2;

            var rect = GetResizeHandleRect(in state, in style, out var radius);
            
            var segments = gui.MeshDrawer.GetSegmentsCount(radius);
            var step = (1f / segments) * HALF_PI;

            Span<Vector2> buffer = stackalloc Vector2[segments + 1 + 2];

            buffer[0] = rect.BottomLeft;
            buffer[^1] = rect.TopRight;

            var cx = rect.BottomRight.x - radius;
            var cy = rect.Y + radius;
            
            for (int i = 0; i < segments + 1; ++i)
            {
                var a = PI + HALF_PI + step * i;
                buffer[i + 1].x = cx + Mathf.Cos(a) * radius;
                buffer[i + 1].y = cy + Mathf.Sin(a) * radius;
            }
            
            gui.Canvas.ConvexFill(buffer, style.ResizeHandleColor);
        }

        public static void WindowBehaviour(ImGui gui, ref State state, in Style style)
        {
            TitleBarBehaviour(gui, ref state, in style);
            ResizeHandleBehaviour(gui, ref state, in style);
        }
        
        public static void TitleBarBehaviour(ImGui gui, ref State state, in Style style)
        {
            var id = gui.GetNextControlId();
            var hovered = gui.IsControlHovered(id);
            var rect = GetTitleBarRect(in state, in style, out _);

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImInputEventMouseType.Down:
                    if (hovered)
                    {
                        gui.ActiveControl = id;
                        gui.Input.UseMouse();
                    }

                    break;
                
                case ImInputEventMouseType.Drag:
                    if (gui.ActiveControl == id)
                    {
                        state.Rect.Position += evt.Delta;
                        gui.Input.UseMouse();
                    }

                    break;
                
                case ImInputEventMouseType.Up:
                    if (gui.ActiveControl == id)
                    {
                        gui.ActiveControl = 0;
                        gui.Input.UseMouse();
                    }

                    break;
            }
            
            gui.HandleControl(id, rect);
        }

        public static void ResizeHandleBehaviour(ImGui gui, ref State state, in Style style)
        {
            var id = gui.GetNextControlId();
            var hovered = gui.IsControlHovered(id);
            var rect = GetResizeHandleRect(in state, in style, out _);

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImInputEventMouseType.Down:
                    if (hovered)
                    {
                        gui.ActiveControl = id;
                        gui.Input.UseMouse();
                    }

                    break;
                
                case ImInputEventMouseType.Drag:
                    if (gui.ActiveControl == id)
                    {
                        state.Rect.W += evt.Delta.x;
                        state.Rect.H -= evt.Delta.y;
                        state.Rect.Y += evt.Delta.y;
                        gui.Input.UseMouse();
                    }

                    break;
                
                case ImInputEventMouseType.Up:
                    if (gui.ActiveControl == id)
                    {
                        gui.ActiveControl = 0;
                        gui.Input.UseMouse();
                    }

                    break;
            }
            
            gui.HandleControl(id, rect);
        }

        public static ImRect GetResizeHandleRect(in State state, in Style style, out float cornerRadius)
        {
            cornerRadius = Mathf.Max(style.CornerRadius - style.FrameWidth);
            var size = Mathf.Max(style.ResizeHandleSize, cornerRadius);
            var rect = state.Rect.AddPadding(style.FrameWidth);
            rect.X += rect.W - size;
            rect.W = size;
            rect.H = size;
            return rect;
        }
        
        public static ImRect GetTitleBarRect(in State state, in Style style, out Vector4 cornerRadius)
        {
            cornerRadius = new Vector4(style.CornerRadius - style.FrameWidth, style.CornerRadius - style.FrameWidth, 0, 0);
            return state.Rect.AddPadding(style.FrameWidth).SplitTop(Mathf.Max(cornerRadius[0], style.TitleBar.Height), out _);
        }

        public struct State
        {
            public string Title;
            public ImRect Rect;
        }

        [Serializable]
        public struct TitleBarStyle
        {
            public float Height;
            public Color32 BackColor;
            public Color32 FrontColor;
            public ImTextSettings Text;
        }
        
        [Serializable]
        public struct Style
        {
            public Color32 BackColor;
            public Color32 FrameColor;
            public Color32 ResizeHandleColor;
            public float FrameWidth;
            public float CornerRadius;
            public float ResizeHandleSize;
            public TitleBarStyle TitleBar;
        }
    }
}