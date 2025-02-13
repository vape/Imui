using System;
using Imui.Core;
using Imui.IO.Events;
using Imui.Style;

namespace Imui.Controls
{
    [Flags]
    public enum ImTreeNodeState
    {
        None = 0,
        Expanded = 1 << 0,
        Selected = 1 << 1
    }

    [Flags]
    public enum ImTreeNodeFlags
    {
        None = 0,
        UnselectOnClick = 1 << 0
    }

    public static class ImTree
    {
        public static bool BeginTreeNode(this ImGui gui, ReadOnlySpan<char> label, ImSize size = default, ImTreeNodeFlags flags = ImTreeNodeFlags.None)
        {
            var selected = false;

            return BeginTreeNode(gui, ref selected, label, size, selectable: false, flags: flags);
        }

        public static (bool expanded, bool selected) BeginTreeNode(this ImGui gui,
                                                                   bool selected,
                                                                   ReadOnlySpan<char> label,
                                                                   ImSize size = default,
                                                                   ImTreeNodeFlags flags = ImTreeNodeFlags.None)
        {
            return (BeginTreeNode(gui, ref selected, label, size, flags: flags), selected);
        }

        public static bool BeginTreeNode(this ImGui gui,
                                         ref bool selected,
                                         ReadOnlySpan<char> label,
                                         ImSize size = default,
                                         bool expandable = true,
                                         bool selectable = true,
                                         ImTreeNodeFlags flags = ImTreeNodeFlags.None)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var id = gui.PushId(label);
            ref var expanded = ref gui.Storage.Get<bool>(id);
            var rect = gui.AddSingleRowRect(size);
            var state = ImTreeNodeState.None;

            state |= selected ? ImTreeNodeState.Selected : ImTreeNodeState.None;
            state |= expanded ? ImTreeNodeState.Expanded : ImTreeNodeState.None;

            TreeNode(gui, id, ref state, label, rect, expandable, selectable, flags);

            expanded = (state & ImTreeNodeState.Expanded) != 0;
            selected = (state & ImTreeNodeState.Selected) != 0;

            if (!expanded)
            {
                gui.PopId();
                return false;
            }

            gui.BeginIndent();
            return true;
        }

        public static void EndTreeNode(this ImGui gui)
        {
            gui.EndIndent();
            gui.PopId();
        }

        public static void TreeNode(this ImGui gui, ReadOnlySpan<char> label, ImSize size = default, ImTreeNodeFlags flags = ImTreeNodeFlags.None)
        {
            var selected = false;

            TreeNode(gui, ref selected, label, size, flags);
        }

        public static bool TreeNode(this ImGui gui,
                                    bool selected,
                                    ReadOnlySpan<char> label,
                                    ImSize size = default,
                                    ImTreeNodeFlags flags = ImTreeNodeFlags.None)
        {
            TreeNode(gui, ref selected, label, size, flags);

            return selected;
        }

        public static void TreeNode(this ImGui gui,
                                    ref bool selected,
                                    ReadOnlySpan<char> label,
                                    ImSize size = default,
                                    ImTreeNodeFlags flags = ImTreeNodeFlags.None)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var id = gui.GetNextControlId();
            var state = selected ? ImTreeNodeState.Selected : ImTreeNodeState.None;
            var rect = gui.AddSingleRowRect(size);

            TreeNode(gui, id, ref state, label, rect, false, true, flags);

            selected = (state & ImTreeNodeState.Selected) != 0;
        }

        public static bool TreeNode(ImGui gui,
                                    uint id,
                                    ref ImTreeNodeState state,
                                    ReadOnlySpan<char> label,
                                    ImRect rect,
                                    bool expandable,
                                    bool selectable,
                                    ImTreeNodeFlags flags)
        {
            ref readonly var buttonStyle = ref ((state & ImTreeNodeState.Selected) != 0 ? ref gui.Style.Tree.ItemSelected : ref gui.Style.Tree.ItemNormal);

            var arrowSize = gui.Style.Layout.TextSize;
            var contentRect = ImButton.CalculateContentRect(gui, rect);
            var arrowRect = contentRect.TakeLeft(arrowSize, gui.Style.Layout.InnerSpacing, out var labelRect).WithAspect(1.0f);
            var changed = false;
            var buttonState = ImButtonState.Normal;
            var expandButtonRect = selectable ? arrowRect : rect;
            
            if (expandable && gui.InvisibleButton(expandButtonRect, out buttonState, ImButtonFlag.ActOnPressMouse))
            {
                state ^= ImTreeNodeState.Expanded;
                changed = true;
            }

            if (selectable && gui.InvisibleButton(labelRect, out buttonState, ImButtonFlag.ActOnPressMouse))
            {
                if ((flags & ImTreeNodeFlags.UnselectOnClick) != 0)
                {
                    state ^= ImTreeNodeState.Selected;
                }
                else
                {
                    state |= ImTreeNodeState.Selected;
                }

                changed = true;
            }

            var boxStyle = ImButton.GetStateBoxStyle(in buttonStyle, buttonState);
            if (boxStyle.BackColor.a > 0)
            {
                gui.Box(rect, boxStyle);
            }

            if (expandable)
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

            return changed;
        }
    }
}