using Imui.Core;
using Imui.IO.Events;
using Imui.Rendering;
using Imui.Style;
using UnityEngine;

namespace Imui.Controls
{
    public struct ImColorPickerState
    {
        public float Hue;
        public float Saturation;
        public float Value;

        public ImColorPickerState(float h, float s, float v)
        {
            Hue = h;
            Saturation = s;
            Value = v;
        }
    }

    public static class ImColorPicker
    {
        public static float GetHeight(ImGui gui, float width)
        {
            return width - (gui.GetRowHeight() + gui.Style.Layout.InnerSpacing) * 2;
        }

        public static ImRect AddRect(ImGui gui, ImSize size = default)
        {
            if (size.Mode == ImSizeMode.Fixed)
            {
                return gui.AddLayoutRect(size.Width, size.Height);
            }

            var width = gui.GetLayoutWidth();
            var height = GetHeight(gui, width);

            return gui.AddLayoutRect(width, height);
        }

        public static Color ColorPicker(this ImGui gui, Color color, ImSize size = default)
        {
            ColorPicker(gui, ref color, size);
            return color;
        }

        public static bool ColorPicker(this ImGui gui, ref Color color, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var id = gui.GetNextControlId();
            var rect = AddRect(gui, size);

            return ColorPicker(gui, id, ref color, rect);
        }

        public static bool ColorPicker(this ImGui gui, uint id, ref Color color, ImRect rect)
        {
            gui.PushId(id);

            ref var state = ref GetState(gui, id, color);
            var alpha = color.a;

            var changed = false;
            var svBarId = gui.GetNextControlId();
            var hueBarId = gui.GetNextControlId();
            var alphaBarId = gui.GetNextControlId();
            var alphaBarRect = rect.TakeRight(gui.GetRowHeight(), gui.Style.Layout.InnerSpacing, out rect);
            var hueBarRect = rect.TakeRight(gui.GetRowHeight(), gui.Style.Layout.InnerSpacing, out rect);

            changed |= HueBar(gui, hueBarId, ref state.Hue, hueBarRect);
            changed |= AlphaBar(gui, alphaBarId, color, ref alpha, alphaBarRect);
            changed |= SaturationValueSquare(gui, svBarId, state.Hue, ref state.Saturation, ref state.Value, rect);

            gui.PopId();

            if (!changed)
            {
                return false;
            }

            color = Color.HSVToRGB(state.Hue, state.Saturation, state.Value);
            color.a = alpha;

            return true;
        }

        public static bool SaturationValueSquare(ImGui gui, uint id, float h, ref float s, ref float v, ImRect rect)
        {
            var black = (Color32)Color.black;
            var white = (Color32)Color.white;
            var clear = (Color32)Color.clear;
            var color = (Color32)Color.HSVToRGB(h, 1, 1);

            var v0 = new ImVertex(rect.TopLeft, white, default, default);
            var v1 = new ImVertex(rect.TopRight, color, default, default);
            var v2 = new ImVertex(rect.BottomRight, color, default, default);
            var v3 = new ImVertex(rect.BottomLeft, white, default, default);
            gui.MeshDrawer.AddQuadTextured(v0, v1, v2, v3);

            v0.Color = clear;
            v1.Color = clear;
            v2.Color = black;
            v3.Color = black;
            gui.MeshDrawer.AddQuadTextured(v0, v1, v2, v3);

            gui.Canvas.RectOutline(rect, gui.Style.ColorPicker.BorderColor, gui.Style.ColorPicker.BorderThickness);

            var point = rect.GetPointAtNormalPosition(s, v);
            var radius = gui.Style.Layout.TextSize * gui.Style.ColorPicker.PreviewCircleScale;
            var circleColor = Color.HSVToRGB(h, s, v);
            var borderColor = GetBrightness(circleColor) >= 0.5f ? Color.black : Color.white;
            gui.Canvas.CircleWithOutline(point, radius, circleColor, borderColor, 1.0f);

            gui.RegisterControl(id, rect);

            var changed = false;
            var hovered = gui.IsControlHovered(id);
            var active = gui.IsControlActive(id);

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down when evt.LeftButton && hovered && !active:
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                    break;
                case ImMouseEventType.Drag or ImMouseEventType.BeginDrag when evt.LeftButton && active:
                    var pnorm = rect.GetNormalPositionAtPoint(gui.Input.MousePosition);
                    s = Mathf.Clamp01(pnorm.x);
                    v = Mathf.Clamp01(pnorm.y);
                    changed = true;
                    break;
            }

            return changed;
        }

        public static bool HueBar(ImGui gui, uint id, ref float h, ImRect rect)
        {
            const int STEPS = 8;

            var col = Color.HSVToRGB(0, 1, 1);
            var v0 = new ImVertex(rect.BottomRight, col, default, default);
            var v1 = new ImVertex(rect.BottomLeft, col, default, default);

            for (int i = 1; i <= STEPS; ++i)
            {
                var t = i / (float)STEPS;

                col = Color.HSVToRGB(t, 1, 1);

                var v2 = new ImVertex(rect.GetPointAtNormalPosition(0, t), col, default, default);
                var v3 = new ImVertex(rect.GetPointAtNormalPosition(1, t), col, default, default);

                gui.MeshDrawer.AddQuadTextured(v0, v1, v2, v3);

                v0 = v3;
                v1 = v2;
            }

            gui.Canvas.RectOutline(rect, gui.Style.ColorPicker.BorderColor, gui.Style.ColorPicker.BorderThickness);

            return SliderVertical(gui, id, rect.WithPadding(gui.Style.ColorPicker.BorderThickness), ref h);
        }

        public static bool AlphaBar(ImGui gui, uint id, Color color, ref float a, ImRect rect)
        {
            CheckerboardPatternVertical(gui, rect);

            var clear = color;

            clear.a = 0.0f;
            color.a = 1.0f;

            var v0 = new ImVertex(rect.TopLeft, color, default, default);
            var v1 = new ImVertex(rect.TopRight, color, default, default);
            var v2 = new ImVertex(rect.BottomRight, clear, default, default);
            var v3 = new ImVertex(rect.BottomLeft, clear, default, default);
            gui.MeshDrawer.AddQuadTextured(v0, v1, v2, v3);
            gui.Canvas.RectOutline(rect, gui.Style.ColorPicker.BorderColor, gui.Style.ColorPicker.BorderThickness);

            return SliderVertical(gui, id, rect.WithPadding(gui.Style.ColorPicker.BorderThickness), ref a);
        }

        public static void SliderPointer(ImGui gui, ref ImRect rect, float value)
        {
            const float SIZE = 6;
            const float SHADOW_SIZE = 1;

            rect.AddPadding(top: SIZE / 2, bottom: SIZE / 2);

            var shadowColor = Color.black;
            var arrowColor = Color.white;
            var arrowPoint = rect.GetPointAtNormalPosition(0.0f, value);
            var shadowRect = new ImRect(arrowPoint.x, arrowPoint.y - SIZE / 2.0f, rect.W, SIZE);
            var arrowRect = shadowRect.WithPadding(SHADOW_SIZE);

            gui.Canvas.Rect(shadowRect, shadowColor);
            gui.Canvas.Rect(arrowRect, arrowColor);
        }

        public static bool SliderVertical(ImGui gui, uint id, ImRect rect, ref float value)
        {
            SliderPointer(gui, ref rect, value);

            gui.RegisterControl(id, rect);

            var changed = false;
            var hovered = gui.IsControlHovered(id);
            var active = gui.IsControlActive(id);

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down when hovered && !active:
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                    break;
                case ImMouseEventType.Drag or ImMouseEventType.BeginDrag when active:
                    value = Mathf.Clamp01(rect.GetNormalPositionAtPoint(gui.Input.MousePosition).y);
                    changed = true;
                    break;
            }

            return changed;
        }

        public static void CheckerboardPatternVertical(ImGui gui, ImRect rect)
        {
            var count = Mathf.CeilToInt(rect.H / rect.W);
            var tmp = gui.Canvas.GetTexScaleOffset();

            var color = (Color32)Color.white;
            var height = rect.H;

            while (count > 0)
            {
                var h = Mathf.Min(height, rect.W);
                var r = new ImRect(rect.X, rect.Y + (rect.H - height), rect.W, h);

                SetPatternAspect(gui, h / rect.W);
                gui.Canvas.Rect(r, color);

                height -= h;
                count--;
            }

            gui.Canvas.SetTexScaleOffset(tmp);
            return;

            void SetPatternAspect(ImGui gui, float aspect)
            {
                var scaleOffset = ImCanvas.GetTexScaleOffsetFor(ImCanvasBuiltinTex.Checkerboard);
                scaleOffset.y *= aspect;

                gui.Canvas.SetTexScaleOffset(scaleOffset);
            }
        }

        public static float GetBrightness(Color color)
        {
            return 0.2125f * color.r + 0.7152f * color.g + 0.0722f * color.b;
        }

        public static ref ImColorPickerState GetState(ImGui gui, uint id, Color color)
        {
            Color.RGBToHSV(color, out var h, out var s, out var v);

            ref var state = ref gui.Storage.Get(id, new ImColorPickerState(h, s, v));

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (s != 0.0f && v != 0.0f && (state.Hue != 1.0f || h != 0.0f))
            {
                state.Hue = h;
            }

            if (v != 0.0f)
            {
                state.Saturation = s;
            }

            state.Value = v;

            return ref state;
        }
    }
}