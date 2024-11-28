using System;
using Imui.Controls;
using Imui.Core;
using Imui.Rendering;
using Imui.Style;

namespace Imui.Examples
{
    public static class ImThemeEditor
    {
        public struct LabeledScope : IDisposable
        {
            private ImGui gui;
            
            public LabeledScope(ImGui gui, ReadOnlySpan<char> label)
            {
                this.gui = gui;
                
                gui.AddSpacingIfLayoutFrameNotEmpty();
                gui.BeginHorizontal();
                var rect = gui.AddLayoutRect(gui.GetLayoutWidth() * 0.4f, gui.GetRowHeight());
                gui.Text(label, rect, overflow: ImTextOverflow.Ellipsis);
                gui.BeginVertical();
            }

            public void Dispose()
            {
                gui.EndVertical();
                gui.EndHorizontal();
            }
        }

        public static bool DrawEditor(ImGui gui, ref ImTheme theme)
        {
            var changed = false;

            gui.Separator("Colors");
            
            using (new LabeledScope(gui, nameof(theme.Foreground))) changed |= gui.ColorEdit(ref theme.Foreground);
            using (new LabeledScope(gui, nameof(theme.Background))) changed |= gui.ColorEdit(ref theme.Background);
            using (new LabeledScope(gui, nameof(theme.Accent))) changed |= gui.ColorEdit(ref theme.Accent);
            using (new LabeledScope(gui, nameof(theme.Control))) changed |= gui.ColorEdit(ref theme.Control);
            using (new LabeledScope(gui, nameof(theme.Variance))) changed |= gui.Slider(ref theme.Variance, 0.0f, 1.0f);

            gui.Separator("Values");

            using (new LabeledScope(gui, nameof(theme.TextSize))) changed |= gui.Slider(ref theme.TextSize, 4.0f, 128.0f);
            using (new LabeledScope(gui, nameof(theme.Spacing))) changed |= gui.Slider(ref theme.Spacing, 0.0f, 32.0f);
            using (new LabeledScope(gui, nameof(theme.InnerSpacing))) changed |= gui.Slider(ref theme.InnerSpacing, 0.0f, 32.0f);
            using (new LabeledScope(gui, nameof(theme.Indent))) changed |= gui.Slider(ref theme.Indent, 0.0f, 128.0f);
            using (new LabeledScope(gui, nameof(theme.ExtraRowHeight))) changed |= gui.Slider(ref theme.ExtraRowHeight, 0.0f, 128.0f);
            using (new LabeledScope(gui, nameof(theme.ScrollBarSize))) changed |= gui.Slider(ref theme.ScrollBarSize, 2.0f, 128.0f);
            using (new LabeledScope(gui, nameof(theme.WindowBorderRadius))) changed |= gui.Slider(ref theme.WindowBorderRadius, 0.0f, 32.0f);
            using (new LabeledScope(gui, nameof(theme.WindowBorderThickness))) changed |= gui.Slider(ref theme.WindowBorderThickness, 0.0f, 8.0f);
            using (new LabeledScope(gui, nameof(theme.BorderRadius))) changed |= gui.Slider(ref theme.BorderRadius, 0.0f, 16.0f);
            using (new LabeledScope(gui, nameof(theme.BorderThickness))) changed |= gui.Slider(ref theme.BorderThickness, 0.0f, 8.0f);
            using (new LabeledScope(gui, nameof(theme.ReadOnlyColorMultiplier))) changed |= gui.Slider(ref theme.ReadOnlyColorMultiplier, 0.0f, 8.0f);
            
            return changed;
        }
    }
}