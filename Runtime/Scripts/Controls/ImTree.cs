using System;
using Imui.Core;
using Imui.Style;

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

    // TODO (artem-s): needs better api, like menus
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
            
            ref readonly var buttonStyle = ref ((state & ImTreeNodeState.Selected) != 0 ? ref gui.Style.Tree.ItemSelected : ref gui.Style.Tree.ItemNormal);

            var arrowSize = gui.Style.Layout.TextSize;
            var contentRect = ImButton.CalculateContentRect(gui, rect);
            var arrowRect = contentRect.SplitLeft(arrowSize, gui.Style.Layout.InnerSpacing, out var labelRect).WithAspect(1.0f);
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
            
            var boxStyle = ImButton.GetStateBoxStyle(in buttonStyle, buttonState);

            if (boxStyle.BackColor.a > 0)
            {
                gui.Box(rect, boxStyle);
            }

            if ((flags & ImTreeNodeFlags.NonExpandable) == 0)
            {
                if ((state & ImTreeNodeState.Expanded) != 0)
                {
                    ImFoldout.DrawArrowDown(gui.Canvas, arrowRect, boxStyle.FrontColor, gui.Style.Tree.ArrowScale);
                }
                else
                {
                    ImFoldout.DrawArrowRight(gui.Canvas, arrowRect, boxStyle.FrontColor, gui.Style.Tree.ArrowScale);
                }
            }
            
            var textSettings = new ImTextSettings(gui.Style.Layout.TextSize, buttonStyle.Alignment);
            gui.Text(label, textSettings, boxStyle.FrontColor, labelRect);

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