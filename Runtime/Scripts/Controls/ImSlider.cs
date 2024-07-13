using System;
using Imui.Core;
using Imui.IO.Events;
using Imui.Controls.Styling;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImSlider
    {
        public static ImRect GetRect(ImGui gui, ImSize size)
        {
            return size.Type switch
            {
                ImSizeType.Fixed => gui.Layout.AddRect(size.Width, size.Height),
                _ => gui.Layout.AddRect(gui.Layout.GetAvailableWidth(), gui.GetRowHeight())
            };
        }
        
        public static bool Slider(this ImGui gui, ref float value, float min, float max, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rect = GetRect(gui, size);
            return Slider(gui, ref value, min, max, rect); 
        }
        
        public static bool Slider(this ImGui gui, ref float value, float min, float max, ImRect rect)
        {
            const float EPSILON = 0.000001f;
            
            var normValue = Mathf.InverseLerp(min, max, value);

            gui.Box(rect, in ImTheme.Active.Slider.Box);

            var rectPadded = rect.WithPadding(ImTheme.Active.Slider.Padding);

            var handleW = rectPadded.H * ImTheme.Active.Slider.HandleAspectRatio;
            var handleH = rectPadded.H;
            
            var xmin = rectPadded.X + handleW / 2.0f;
            var xmax = rectPadded.X + rectPadded.W - handleW / 2.0f;
            
            var handleX = Mathf.Lerp(xmin, xmax, normValue) - (handleW / 2.0f);
            var handleY = rectPadded.Y + (rectPadded.H / 2.0f) - (handleH / 2.0f);
            var handleRect = new ImRect(handleX, handleY, handleW, handleH);

            var id = gui.GetNextControlId();
            var hovered = gui.IsControlHovered(id);
            var active = gui.IsControlActive(id);

            using (new ImStyleScope<ImButtonStyle>(ref ImTheme.Active.Button, ImTheme.Active.Slider.Handle))
            {
                gui.Button(id, handleRect, out _);
            }
            
            gui.RegisterControl(id, rect);

            if (gui.IsReadOnly)
            {
                return false;
            }
            
            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when hovered:
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Drag when active:
                    normValue = Mathf.InverseLerp(xmin, xmax, Mathf.Lerp(xmin, xmax, normValue) + evt.Delta.x);
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Up when active:
                    gui.ResetActiveControl();
                    break;
            }
            
            var newValue = Mathf.Lerp(min, max, normValue);
            if (Mathf.Abs(newValue - value) > EPSILON)
            {
                value = newValue;
                return true;
            }

            return false;
        }
    }

    [Serializable]
    public struct ImSliderStyle
    {
        public ImBoxStyle Box;
        public ImButtonStyle Handle;
        public ImPadding Padding;
        public float HandleAspectRatio;
    }
}