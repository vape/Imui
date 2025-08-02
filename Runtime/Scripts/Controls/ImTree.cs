using System;
using Imui.Core;

namespace Imui.Controls
{
    [Flags]
    public enum ImTreeNodeFlags
    {
        None = 0,
        UnselectOnClick = 1 << 0,
        NonSelectable = 1 << 1,
        NonExpandable = 1 << 2
    }

    public struct ImTreeNodeState
    {
        public bool Expanded;
        public bool Selected;

        public ImTreeNodeState(bool expanded, bool selected)
        {
            Expanded = expanded;
            Selected = selected;
        }
    }

    public static class ImTree
    {
        public static bool BeginTreeNode(this ImGui gui, ReadOnlySpan<char> label, ImSize size = default, ImTreeNodeFlags flags = ImTreeNodeFlags.None)
        {
            var id = gui.GetNextControlId();
            ref var state = ref gui.Storage.Get<ImTreeNodeState>(id);

            flags |= ImTreeNodeFlags.NonSelectable;

            BeginTreeNode(gui, id, ref state, label, size, flags);

            return state.Expanded;
        }

        public static bool BeginTreeNode(this ImGui gui,
                                         ref bool selected,
                                         ReadOnlySpan<char> label,
                                         ImSize size = default,
                                         ImTreeNodeFlags flags = ImTreeNodeFlags.None)
        {
            var state = BeginTreeNode(gui, selected, label, size, flags);
            selected = state.Selected;

            return state.Expanded;
        }

        public static ImTreeNodeState BeginTreeNode(this ImGui gui,
                                                    bool selected,
                                                    ReadOnlySpan<char> label,
                                                    ImSize size = default,
                                                    ImTreeNodeFlags flags = ImTreeNodeFlags.None)
        {
            var id = gui.GetNextControlId();
            ref var expanded = ref gui.Storage.Get<bool>(id);
            var state = new ImTreeNodeState(expanded, selected);

            BeginTreeNode(gui, id, ref state, label, size, flags: flags);

            expanded = state.Expanded;

            return state;
        }

        public static bool BeginTreeNode(ImGui gui,
                                         uint id,
                                         ref ImTreeNodeState state,
                                         ReadOnlySpan<char> label,
                                         ImSize size = default,
                                         ImTreeNodeFlags flags = ImTreeNodeFlags.None)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rect = gui.AddSingleRowRect(size);
            gui.PushId(id);

            var idExpand = gui.GetNextControlId();
            var idSelect = gui.GetNextControlId();

            TreeNode(gui, idExpand, idSelect, ref state, label, rect, flags);

            if (!state.Expanded)
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

        public static void TreeNode(this ImGui gui,
                                    ReadOnlySpan<char> label,
                                    ImSize size = default,
                                    ImTreeNodeFlags flags = ImTreeNodeFlags.None)
        {
            TreeNode(gui, false, label, size, flags);
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

        public static bool TreeNode(this ImGui gui,
                                    ref bool selected,
                                    ReadOnlySpan<char> label,
                                    ImSize size = default,
                                    ImTreeNodeFlags flags = ImTreeNodeFlags.None)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            flags |= ImTreeNodeFlags.NonExpandable;

            var idExpand = gui.GetNextControlId();
            var idSelect = gui.GetNextControlId();
            var rect = gui.AddSingleRowRect(size);
            var state = new ImTreeNodeState(false, selected);
            var changed = TreeNode(gui, idExpand, idSelect, ref state, label, rect, flags);

            selected = state.Selected;

            return changed;
        }

        public static bool TreeNode(ImGui gui,
                                    uint idExpandButton,
                                    uint idSelectButton,
                                    ref ImTreeNodeState state,
                                    ReadOnlySpan<char> label,
                                    ImRect rect,
                                    ImTreeNodeFlags flags)
        {
            ref readonly var buttonStyle = ref (state.Selected ? ref gui.Style.Tree.ItemSelected : ref gui.Style.Tree.ItemNormal);

            var arrowSize = gui.Style.Layout.TextSize;
            var contentRect = ImButton.CalculateContentRect(gui, rect);
            var arrowRect = contentRect.TakeLeft(arrowSize, gui.Style.Layout.InnerSpacing, out var labelRect).WithAspect(1.0f);
            var changed = false;
            var buttonState = ImButtonState.Normal;
            var selectable = (flags & ImTreeNodeFlags.NonSelectable) == 0;
            var expandable = (flags & ImTreeNodeFlags.NonExpandable) == 0;
            var expandButtonRect = selectable ? arrowRect : rect;

            if (expandable && gui.InvisibleButton(idExpandButton, expandButtonRect, out buttonState, ImButtonFlag.ActOnPressMouse))
            {
                state.Expanded = !state.Expanded;
                changed = true;
            }

            if (selectable && gui.InvisibleButton(idSelectButton, labelRect, out buttonState, ImButtonFlag.ActOnPressMouse))
            {
                if ((flags & ImTreeNodeFlags.UnselectOnClick) != 0)
                {
                    state.Selected = !state.Selected;
                }
                else
                {
                    state.Selected = true;
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
                if (state.Expanded)
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