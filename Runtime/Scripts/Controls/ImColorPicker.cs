using Imui.Core;
using Imui.IO.Events;
using Imui.Rendering;
using Imui.Style;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImColorPicker
    {
        public static float GetHeight(ImGui gui, float width)
        {
            return width - (gui.GetRowHeight() + gui.Style.Layout.Spacing) * 2;
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
            Color.RGBToHSV(color, out var colorHue, out var s, out var v);
            var a = color.a;

            gui.PushId(id);

            ref var h = ref gui.Storage.Get(id, colorHue);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (s != 0.0f && v != 0.0f && (h != 1.0f || colorHue != 0.0f))
            {
                h = colorHue;
            }

            var changed = false;
            var svBarId = gui.GetNextControlId();
            var hueBarId = gui.GetNextControlId();
            var alphaBarId = gui.GetNextControlId();
            var alphaBarRect = rect.SplitRight(gui.GetRowHeight(), gui.Style.Layout.Spacing, out rect);
            var hueBarRect = rect.SplitRight(gui.GetRowHeight(), gui.Style.Layout.Spacing, out rect);

            changed |= DrawHueBar(gui, hueBarId, ref h, hueBarRect);
            changed |= DrawAlphaBar(gui, alphaBarId, color, ref a, alphaBarRect);
            changed |= DrawSaturationValueSquare(gui, svBarId, h, ref s, ref v, rect);

            gui.PopId();

            if (!changed)
            {
                return false;
            }

            color = Color.HSVToRGB(h, s, v);
            color.a = a;

            return true;
        }

        public static bool DrawSaturationValueSquare(ImGui gui, uint id, float h, ref float s, ref float v, ImRect rect)
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
            gui.Canvas.CircleWithOutline(point, radius, circleColor, borderColor, gui.Style.ColorPicker.BorderThickness);

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
                    var pnorm = rect.GetNormalPositionAtPoint(gui.Input.MousePosition);
                    s = Mathf.Clamp01(pnorm.x);
                    v = Mathf.Clamp01(pnorm.y);
                    changed = true;
                    break;
            }

            return changed;
        }

        public static bool DrawHueBar(ImGui gui, uint id, ref float h, ImRect rect)
        {
            const int STEPS = 8;

            var c = Color.HSVToRGB(0, 1, 1);
            var v0 = new ImVertex(rect.BottomRight, c, default, default);
            var v1 = new ImVertex(rect.BottomLeft, c, default, default);

            for (int i = 1; i <= STEPS; ++i)
            {
                var t = i / (float)STEPS;

                c = Color.HSVToRGB(t, 1, 1);

                var v2 = new ImVertex(rect.GetPointAtNormalPosition(0, t), c, default, default);
                var v3 = new ImVertex(rect.GetPointAtNormalPosition(1, t), c, default, default);

                gui.MeshDrawer.AddQuadTextured(v0, v1, v2, v3);

                v0 = v3;
                v1 = v2;
            }
            
            var inputRect = rect.WithPadding(gui.Style.ColorPicker.BorderThickness);
            DrawBarPointer(gui, ref inputRect, h);
            
            gui.Canvas.RectOutline(rect, gui.Style.ColorPicker.BorderColor, gui.Style.ColorPicker.BorderThickness);

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
                    h = Mathf.Clamp01(inputRect.GetNormalPositionAtPoint(gui.Input.MousePosition).y);
                    changed = true;
                    break;
            }

            return changed;
        }

        public static bool DrawAlphaBar(ImGui gui, uint id, Color color, ref float a, ImRect rect)
        {
            DrawCheckerboardPattern(gui, rect);

            color.a = 1.0f;
            
            var clear = Color.clear;

            var v0 = new ImVertex(rect.TopLeft, color, default, default);
            var v1 = new ImVertex(rect.TopRight, color, default, default);
            var v2 = new ImVertex(rect.BottomRight, clear, default, default);
            var v3 = new ImVertex(rect.BottomLeft, clear, default, default);
            gui.MeshDrawer.AddQuadTextured(v0, v1, v2, v3);

            var inputRect = rect.WithPadding(gui.Style.ColorPicker.BorderThickness);
            DrawBarPointer(gui, ref inputRect, a);

            gui.Canvas.RectOutline(rect, gui.Style.ColorPicker.BorderColor, gui.Style.ColorPicker.BorderThickness);

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
                    a = Mathf.Clamp01(inputRect.GetNormalPositionAtPoint(gui.Input.MousePosition).y);
                    changed = true;
                    break;
            }

            return changed;
        }

        public static void DrawCheckerboardPattern(ImGui gui, ImRect rect)
        {
            void SetPatternAspect(float aspect)
            {
                var scaleOffset = ImCanvas.GetTexScaleOffsetFor(ImCanvasBuiltinTex.Checkerboard);
                scaleOffset.y *= aspect;

                gui.Canvas.SetTexScaleOffset(scaleOffset);
            }

            var count = Mathf.CeilToInt(rect.H / rect.W);
            var tmp = gui.Canvas.GetTexScaleOffset();

            var color = (Color32)Color.white;
            var height = rect.H;

            while (count > 0)
            {
                var h = Mathf.Min(height, rect.W);
                var r = new ImRect(rect.X, rect.Y + (rect.H - height), rect.W, h);

                SetPatternAspect(h / rect.W);
                gui.Canvas.Rect(r, color);

                height -= h;
                count--;
            }

            gui.Canvas.SetTexScaleOffset(tmp);
        }

        public static void DrawBarPointer(ImGui gui, ref ImRect rect, float value)
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
        
        public static float GetBrightness(Color color)
        {
            return 0.2125f * color.r + 0.7152f * color.g + 0.0722f * color.b;
        }
    }
}