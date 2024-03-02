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
        private const short ORDER_OFFSET = 64;
        
        public static ImWindowStyle Style = ImWindowStyle.Default;
        
        public static void BeginWindow(this ImGui gui, string title)
        {
            var id = gui.PushId(title);
            
            ref var state = ref gui.WindowManager.RegisterWindow(id, title);
            
            gui.Canvas.PushOrder(state.Order * ORDER_OFFSET);
            gui.Canvas.PushRectMask(state.Rect, Style.CornerRadius);
            gui.Canvas.PushClipRect(state.Rect);
            Back(gui, in state.Rect, out var contentRect);
            
            gui.HandleControl(id, state.Rect);
            
            gui.BeginScope(id);
            gui.Layout.Push(contentRect, ImAxis.Vertical);
            gui.Layout.MakeRoot();
        }

        public static void EndWindow(this ImGui gui)
        {
            gui.Layout.Pop();
            gui.EndScope(out var id);
            
            ref var state = ref gui.WindowManager.GetWindowState(id);
            
            Front(gui, state.Rect);
            
            var clicked = false;
            clicked |= TitleBar(gui, state.Title, ref state.Rect);
            clicked |= ResizeHandle(gui, ref state.Rect);
            
            if (clicked)
            {
                gui.WindowManager.RequestFocus(id);
            }
            
            gui.HandleGroup(id, state.Rect);
            
            gui.Canvas.PopRectMask();
            gui.Canvas.PopClipRect();
            gui.Canvas.PopOrder();
            
            gui.PopId();
        }
        
        public static void Back(ImGui gui, in ImRect rect, out ImRect content)
        {
            gui.Canvas.Rect(rect, Style.BackColor, Style.CornerRadius);
            
            var titleBarRect = GetTitleBarRect(in rect, out _);
            rect.WithPadding(Style.FrameWidth).SplitTop(titleBarRect.H, out content);
            content.AddPadding(Style.Padding);
        }

        public static void Front(ImGui gui, in ImRect rect)
        {
            gui.Canvas.RectOutline(rect, Style.FrameColor, Style.FrameWidth, Style.CornerRadius);
        }
        
        public static bool TitleBar(ImGui gui, in ReadOnlySpan<char> text, ref ImRect rect)
        {
            var id = gui.GetControlId("title_bar");
            var hovered = gui.IsControlHovered(id);
            var titleBarRect = GetTitleBarRect(in rect, out var radius);

            var segments = gui.MeshDrawer.GetSegmentsCount(Style.CornerRadius);
            
            gui.Canvas.Rect(
                titleBarRect, 
                Style.TitleBar.BackColor, 
                gui.Canvas.DefaultTexScaleOffset,
                radius, segments);
            
            gui.Canvas.Text(text, Style.TitleBar.FrontColor, titleBarRect, in Style.TitleBar.Text);

            var clicked = false;
            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImInputEventMouseType.Down:
                    if (hovered)
                    {
                        clicked = true;
                        gui.ActiveControl = id;
                        gui.Input.UseMouse();
                    }

                    break;
                
                case ImInputEventMouseType.Drag:
                    if (gui.ActiveControl == id)
                    {
                        rect.Position += evt.Delta;
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
            
            gui.HandleControl(id, titleBarRect);
            
            return clicked;
        }
        
        public static bool ResizeHandle(ImGui gui, ref ImRect rect)
        {
            const float PI = Mathf.PI;
            const float HALF_PI = PI / 2;

            var id = gui.GetControlId("resize_handle");
            var hovered = gui.IsControlHovered(id);
            var handleRect = GetResizeHandleRect(in rect, out var radius);
            
            var segments = gui.MeshDrawer.GetSegmentsCount(radius);
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
                case ImInputEventMouseType.Down:
                    if (hovered)
                    {
                        clicked = true;
                        gui.ActiveControl = id;
                        gui.Input.UseMouse();
                    }

                    break;
                
                case ImInputEventMouseType.Drag:
                    if (gui.ActiveControl == id)
                    {
                        rect.W += evt.Delta.x;
                        rect.H -= evt.Delta.y;
                        rect.Y += evt.Delta.y;
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
            
            gui.HandleControl(id, handleRect);
            return clicked;
        }

        private static ImRect GetResizeHandleRect(in ImRect rect, out float cornerRadius)
        {
            cornerRadius = Mathf.Max(Style.CornerRadius - Style.FrameWidth, 0);
            
            var handleSize = Mathf.Max(Style.ResizeHandleSize, cornerRadius);
            var handleRect = rect;
            handleRect.AddPadding(Style.FrameWidth);
            handleRect.X += handleRect.W - handleSize;
            handleRect.W = handleSize;
            handleRect.H = handleSize;
            
            return handleRect;
        }
        
        private static ImRect GetTitleBarRect(in ImRect rect, out Vector4 cornerRadius)
        {
            cornerRadius = new Vector4(Style.CornerRadius - Style.FrameWidth, Style.CornerRadius - Style.FrameWidth, 0, 0);
            return rect.WithPadding(Style.FrameWidth).SplitTop(Mathf.Max(cornerRadius[0], Style.TitleBar.Height), out _);
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
            ResizeHandleSize = 20,
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
