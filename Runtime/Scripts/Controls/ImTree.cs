using System;
using Imui.Controls.Styling;
using Imui.Core;

namespace Imui.Controls
{
    [Flags]
    public enum ImTreeNodeState
    {
        None = 0,
        Expanded = 1,
        Selected = 2
    }

    [Flags]
    public enum ImTreeNodeFlags
    {
        None = 0,
        NonSelectable = 1,
        NonExpandable = 2
    }

    public static class ImTree
    {
        public static bool BeginTreeNode(this ImGui gui,
                                         ReadOnlySpan<char> label,
                                         ref ImTreeNodeState state,
                                         ImTreeNodeFlags flags = ImTreeNodeFlags.None,
                                         ImSize size = default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            return BeginTreeNode(gui, label, ImControls.AddRowRect(gui, size), ref state, flags);
        }

        public static bool BeginTreeNode(this ImGui gui, ReadOnlySpan<char> label, ImRect rect, ref ImTreeNodeState state, ImTreeNodeFlags flags)
        {
            gui.PushId(label);

            var arrowRect = rect.SplitLeft(rect.H * 0.7f, out var labelRect);
            var changed = false;
            var buttonState = ImButtonState.Normal;
            var nonExpandable = (flags & ImTreeNodeFlags.NonExpandable) != 0;
            var nonSelectable = (flags & ImTreeNodeFlags.NonSelectable) != 0;
            var expandButtonRect = nonSelectable ? rect : arrowRect;
            
            if (!nonExpandable && gui.InvisibleButton(expandButtonRect, out buttonState, ImButtonFlag.ActOnPress))
            {
                state ^= ImTreeNodeState.Expanded;
                changed = true;
            }

            if (!nonSelectable && gui.InvisibleButton(labelRect, out buttonState, ImButtonFlag.ActOnPress))
            {
                state |= ImTreeNodeState.Selected;
                changed = true;
            }
            
            ref readonly var style = ref ((state & ImTreeNodeState.Selected) != 0
                ? ref ImButton.GetStateStyle(in ImTheme.Active.List.ItemSelected, buttonState)
                : ref ImButton.GetStateStyle(in ImTheme.Active.List.ItemNormal, buttonState));

            if ((state & ImTreeNodeState.Selected) != 0)
            {
                gui.Canvas.Rect(rect, style.BackColor);
            }

            if ((flags & ImTreeNodeFlags.NonExpandable) == 0)
            {
                arrowRect = arrowRect.ScaleFromCenter(ImTheme.Active.Foldout.ArrowOuterScale);

                if ((state & ImTreeNodeState.Expanded) != 0)
                {
                    ImFoldout.DrawOpenArrow(gui.Canvas, arrowRect, style.FrontColor);
                }
                else
                {
                    ImFoldout.DrawClosedArrow(gui.Canvas, arrowRect, style.FrontColor);
                }
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