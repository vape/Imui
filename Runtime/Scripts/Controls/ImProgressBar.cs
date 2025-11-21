using System;
using System.Runtime.CompilerServices;
using Imui.Core;
using Imui.Rendering;
using Imui.Style;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImProgressBar
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ProgressBarHeader(this ImGui gui,
                                             ReadOnlySpan<char> label,
                                             float value,
                                             ReadOnlySpan<char> valueFormat = default)
        {
            ProgressBarHeader(gui, label, gui.Formatter.Format(value, valueFormat));
        }

        public static void ProgressBarHeader(this ImGui gui,
                                             ReadOnlySpan<char> label,
                                             ReadOnlySpan<char> valueLabel = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            gui.BeginHorizontal();

            var rowHeight = gui.GetRowHeight();
            var height = rowHeight * gui.Style.ProgressBar.HeaderScale;
            var emptyVerticalSpace = (gui.Style.ProgressBar.BarScale.y * rowHeight) / 2.0f;
            var rect = gui.AddLayoutRect(gui.GetLayoutWidth(), height - emptyVerticalSpace);
            var fontSize = gui.TextDrawer.GetFontSizeFromLineHeight(height);

            rect.H += emptyVerticalSpace;
            rect.Y -= emptyVerticalSpace;

            var textSettings = new ImTextSettings(fontSize, 0.0f, 1.0f, overflow: ImTextOverflow.Ellipsis);
            gui.Text(label, textSettings, rect);

            textSettings.Align.X = 1.0f;
            gui.Text(valueLabel, textSettings, rect);

            gui.EndHorizontal();
        }

        public static void ProgressBar(this ImGui gui, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var id = gui.GetNextControlId();
            var rect = gui.AddSingleRowRect(size,
                                            gui.Style.ProgressBar.Box.BorderRadius.MinRectSideSize,
                                            gui.Style.ProgressBar.Box.BorderRadius.MinRectSideSize);

            ProgressBar(gui, id, rect);
        }

        public static void ProgressBar(this ImGui gui, float percent, ImSize size = default)
        {
            ProgressBar(gui, Mathf.Clamp01(percent), 0.0f, 1.0f, size);
        }

        public static void ProgressBar(this ImGui gui, float value, float min, float max, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var id = gui.GetNextControlId();
            var rect = gui.AddSingleRowRect(size,
                                            gui.Style.ProgressBar.Box.BorderRadius.MinRectSideSize,
                                            gui.Style.ProgressBar.Box.BorderRadius.MinRectSideSize);

            ProgressBar(gui, id, value, min, max, rect);
        }

        public static void ProgressBar(this ImGui gui, uint id, float value, float min, float max, ImRect rect)
        {
            gui.RegisterControl(id, rect);

            rect = rect.ScaleFromCenter(gui.Style.ProgressBar.BarScale);
            gui.Box(rect, in gui.Style.ProgressBar.Box);

            var normalized = Mathf.InverseLerp(min, max, value);
            if (normalized > 0)
            {
                rect.AddPadding(gui.Style.ProgressBar.FillPadding);
                rect = rect.TakeLeft(rect.W * normalized);
                rect.W = Mathf.Max(rect.W, gui.Style.ProgressBar.Fill.BorderRadius.MinRectSideSize);

                gui.Box(rect, in gui.Style.ProgressBar.Fill);
            }
        }

        public static void ProgressBar(this ImGui gui, uint id, ImRect rect)
        {
            gui.RegisterControl(id, rect);

            rect = rect.ScaleFromCenter(gui.Style.ProgressBar.BarScale);
            gui.Box(rect, in gui.Style.ProgressBar.Box);
            rect.AddPadding(gui.Style.ProgressBar.FillPadding);

            var minSize = gui.Style.ProgressBar.Fill.BorderRadius.MinRectSideSize;
            var width = Mathf.Max(rect.W * gui.Style.ProgressBar.IndeterminateWidth, minSize);
            var time = (float)gui.Input.Time * gui.Style.ProgressBar.IndeterminateSpeed;
            var progress = time - Mathf.Floor(time);

            var i0 = rect;
            i0.X += (progress * i0.W);
            i0.W = width;

            if (i0.Right > rect.Right)
            {
                var i1 = rect;
                i1.W = i0.Right - rect.Right;
                i0.W -= i1.W;

                BoxAutoScaled(gui, i1, in gui.Style.ProgressBar.Fill);
            }

            BoxAutoScaled(gui, i0, in gui.Style.ProgressBar.Fill);
        }

        private static void BoxAutoScaled(ImGui gui, ImRect rect, in ImStyleBox style)
        {
            if (rect.W <= 0 || rect.H <= 0)
            {
                return;
            }

            var maxRadius = Math.Min(rect.H, rect.W);
            var minRectSize = style.BorderRadius.MinRectSideSize;
            if (maxRadius >= minRectSize)
            {
                gui.Box(rect, in style);
            }
            else
            {
                gui.Box(rect.ScaleFromCenter(new Vector2(1.0f, maxRadius / minRectSize)), in style);
            }
        }
    }
}