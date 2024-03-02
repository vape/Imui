using System;
using Imui.Core;
using Imui.Core.Input;
using Imui.Styling;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImWindow
    {
        public static ImWindowStyle Style = ImWindowStyle.Default;
        
        public static void DrawBack(ImGui gui, in State state, out ImRect content)
        {
            gui.Canvas.Rect(state.Rect, Style.BackColor, Style.CornerRadius);
            gui.Canvas.RectOutline(state.Rect, Style.FrameColor, Style.FrameWidth, Style.CornerRadius);

            var titleBarRect = GetTitleBarRect(in state, out _);
            state.Rect.WithPadding(Style.FrameWidth).SplitTop(titleBarRect.H, out content);
            content.AddPadding(Style.Padding);
        }

        public static void DrawFront(ImGui gui, string title, in State state)
        {
            DrawTitleBar(gui, title, in state);
            DrawResizeHandle(gui, in state);
        }

        public static void DrawTitleBar(ImGui gui, string title, in State state)
        {
            var rect = GetTitleBarRect(in state, out var radius);
            var segments = gui.MeshDrawer.GetSegmentsCount(Style.CornerRadius);
            
            gui.Canvas.Rect(
                rect, 
                Style.TitleBar.BackColor, 
                gui.Canvas.DefaultTexScaleOffset,
                radius, segments);
            
            gui.Canvas.Text(title, Style.TitleBar.FrontColor, rect, in Style.TitleBar.Text);
        }

        public static void DrawResizeHandle(ImGui gui, in State state)
        {
            const float PI = Mathf.PI;
            const float HALF_PI = PI / 2;

            var rect = GetResizeHandleRect(in state, out var radius);
            
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
            
            gui.Canvas.ConvexFill(buffer, Style.ResizeHandleColor);
        }

        public static void WindowBehaviour(ImGui gui, ref State state)
        {
            TitleBarBehaviour(gui, ref state);
            ResizeHandleBehaviour(gui, ref state);
        }
        
        public static void TitleBarBehaviour(ImGui gui, ref State state)
        {
            var id = gui.GetControlId("title_bar");
            var hovered = gui.IsControlHovered(id);
            var rect = GetTitleBarRect(in state, out _);

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

        public static void ResizeHandleBehaviour(ImGui gui, ref State state)
        {
            var id = gui.GetControlId("size_handle");
            var hovered = gui.IsControlHovered(id);
            var rect = GetResizeHandleRect(in state, out _);

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

        public static ImRect GetResizeHandleRect(in State state, out float cornerRadius)
        {
            cornerRadius = Mathf.Max(Style.CornerRadius - Style.FrameWidth, 0);
            
            var size = Mathf.Max(Style.ResizeHandleSize, cornerRadius);
            var rect = state.Rect.WithPadding(Style.FrameWidth);
            rect.X += rect.W - size;
            rect.W = size;
            rect.H = size;
            return rect;
        }
        
        public static ImRect GetTitleBarRect(in State state, out Vector4 cornerRadius)
        {
            cornerRadius = new Vector4(Style.CornerRadius - Style.FrameWidth, Style.CornerRadius - Style.FrameWidth, 0, 0);
            return state.Rect.WithPadding(Style.FrameWidth).SplitTop(Mathf.Max(cornerRadius[0], Style.TitleBar.Height), out _);
        }

        [Serializable]
        public struct State
        {
            public ImRect Rect;
        }
    }
    
    [Serializable]
    public struct ImWindowTitleBarStyle
    {
        public float Height;
        public Color32 BackColor;
        public Color32 FrontColor;
        public ImTextSettings Text;
    }
        
    [Serializable]
    public struct ImWindowStyle
    {
        public static readonly ImWindowStyle Default = new ImWindowStyle()
        {
            BackColor = ImColors.White,
            FrameColor = ImColors.Black,
            ResizeHandleColor = ImColors.Gray2,
            FrameWidth = 1,
            CornerRadius = 5,
            ResizeHandleSize = 10,
            Padding = 1,
            TitleBar = new ImWindowTitleBarStyle()
            {
                BackColor = ImColors.Gray6,
                FrontColor = ImColors.Gray1,
                Height = 32,
                Text = new ImTextSettings()
                {
                    AlignX = 0.5f,
                    AlignY = 0.5f,
                    Size = 24
                }
            }
        };
        
        public Color32 BackColor;
        public Color32 FrameColor;
        public Color32 ResizeHandleColor;
        public float FrameWidth;
        public float CornerRadius;
        public float ResizeHandleSize;
        public float Padding;
        public ImWindowTitleBarStyle TitleBar;
    }
}