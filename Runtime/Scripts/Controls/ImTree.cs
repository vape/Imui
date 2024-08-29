using System;
using Imui.Controls.Styling;
using Imui.Core;

namespace Imui.Controls
{
    public static class ImTree
    {
        public static bool BeginTreeNode(this ImGui gui, ReadOnlySpan<char> label, ref bool selected, ref bool open, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            
            return BeginTreeNode(gui, label, ImControls.AddRowRect(gui, size), ref selected, ref open);
        }

        public static bool BeginTreeNode(this ImGui gui, ReadOnlySpan<char> label, ref bool selected, ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            
            return BeginTreeNode(gui, label, ImControls.AddRowRect(gui, size), ref selected);
        }

        
        public static bool BeginTreeNode(this ImGui gui, ReadOnlySpan<char> label, ImRect rect, ref bool selected)
        {
            gui.PushId(label);

            _ = rect.SplitLeft(rect.H, out var labelRect);
            var changed = false;
            
            if (gui.InvisibleButton(labelRect, out var labelButtonState, ImButtonFlag.ActOnPress))
            {
                selected = true;
                changed = true;
            }
            
            ref readonly var style = ref (selected
                ? ref ImButton.GetStateStyle(in ImTheme.Active.List.ItemSelected, labelButtonState)
                : ref ImButton.GetStateStyle(in ImTheme.Active.List.ItemNormal, labelButtonState));
            
            if (selected)
            {
                gui.Canvas.Rect(rect, style.BackColor);
            }

            var textSettings = new ImTextSettings(ImTheme.Active.Controls.TextSize, ImTheme.Active.Foldout.TextAlignment, ImTheme.Active.Button.TextWrap);
            gui.Text(label, textSettings, labelRect, color: style.FrontColor);
            
            gui.BeginIndent();
            
            return changed;
        }

        public static bool BeginTreeNode(this ImGui gui, ReadOnlySpan<char> label, ImRect rect, ref bool selected, ref bool open)
        {
            gui.PushId(label);

            var arrowRect = rect.SplitLeft(rect.H, out var labelRect);
            var changed = false;
            
            if (gui.InvisibleButton(arrowRect, ImButtonFlag.ActOnPress))
            {
                open = !open;
                changed = true;
            }

            if (gui.InvisibleButton(labelRect, out var labelButtonState, ImButtonFlag.ActOnPress))
            {
                selected = true;
                changed = true;
            }
            
            ref readonly var style = ref (selected
                ? ref ImButton.GetStateStyle(in ImTheme.Active.List.ItemSelected, labelButtonState)
                : ref ImButton.GetStateStyle(in ImTheme.Active.List.ItemNormal, labelButtonState));
            
            if (selected)
            {
                gui.Canvas.Rect(rect, style.BackColor);
            }

            arrowRect = arrowRect.ScaleFromCenter(ImTheme.Active.Foldout.ArrowOuterScale);
            
            if (open)
            {
                ImFoldout.DrawOpenArrow(gui.Canvas, arrowRect, style.FrontColor);
            }
            else
            {
                ImFoldout.DrawClosedArrow(gui.Canvas, arrowRect, style.FrontColor);
            }

            var textSettings = new ImTextSettings(ImTheme.Active.Controls.TextSize, ImTheme.Active.Foldout.TextAlignment, ImTheme.Active.Button.TextWrap);
            gui.Text(label, textSettings, labelRect, color: style.FrontColor);
            
            gui.BeginIndent();
            
            return changed;
        }

        public static void EndTreeNode(this ImGui gui)
        {
            gui.EndIndent();
            gui.PopId();
        }
    }
}