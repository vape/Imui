using System;
using Imui.Core;
using Imui.Style;
using UnityEngine;

namespace Imui.Controls
{
    public enum ImDropdownPreviewType
    {
        Default = 0,
        Arrow = 1,
        Text = 2
    }
    
    public static class ImDropdown
    {
        public static bool BeginDropdown(this ImGui gui,
                                         bool open,
                                         ReadOnlySpan<char> label,
                                         ImSize size = default,
                                         ImDropdownPreviewType preview = ImDropdownPreviewType.Default)
        {
            BeginDropdown(gui, ref open, label, size, preview);
            return open;
        }

        public static bool BeginDropdown(this ImGui gui,
                                         ref bool open,
                                         ReadOnlySpan<char> label,
                                         ImSize size = default,
                                         ImDropdownPreviewType preview = ImDropdownPreviewType.Default)
        {
            var id = gui.GetNextControlId();

            return BeginDropdown(gui, id, ref open, label, size, preview);
        }

        public static bool BeginDropdown(this ImGui gui,
                                         uint id,
                                         ref bool open,
                                         ReadOnlySpan<char> label,
                                         ImSize size = default,
                                         ImDropdownPreviewType preview = ImDropdownPreviewType.Default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rect = ImControls.AddRowRect(gui, size);
            var prev = open;

            gui.PushId(id);
            gui.Layout.Push(ImAxis.Horizontal, rect);
            
            if (preview == ImDropdownPreviewType.Arrow)
            {
                NoPreview(gui, id, ref open);
            }
            else
            {
                BeginPreview(gui);

                if (preview == ImDropdownPreviewType.Text)
                {
                    PreviewText(gui, label);
                }
                else
                {
                    PreviewButton(gui, id, ref open, label);
                }

                EndPreview(gui, id, ref open);
            }

            return open != prev;
        }

        public static void EndDropdown(this ImGui gui)
        {
            gui.Layout.Pop();
            gui.PopId();
        }

        public static bool DropdownList(this ImGui gui, ref int selected, ReadOnlySpan<string> items)
        {
            var itemClicked = false;

            BeginList(gui, items.Length);

            for (int i = 0; i < items.Length; ++i)
            {
                if (gui.ListItem(ref selected, i, items[i]))
                {
                    itemClicked = true;
                }
            }

            EndList(gui, out var closeClicked);

            return itemClicked || closeClicked;
        }

        public static int Dropdown(this ImGui gui,
                                   int selected,
                                   ReadOnlySpan<string> items,
                                   ImSize size = default,
                                   ImDropdownPreviewType preview = ImDropdownPreviewType.Default,
                                   ReadOnlySpan<char> defaultLabel = default)
        {
            Dropdown(gui, ref selected, items, size, preview, defaultLabel);
            return selected;
        }

        public static bool Dropdown(this ImGui gui,
                                    ref int selected,
                                    ReadOnlySpan<string> items,
                                    ImSize size = default,
                                    ImDropdownPreviewType preview = ImDropdownPreviewType.Default,
                                    ReadOnlySpan<char> defaultLabel = default)
        {
            var id = gui.GetNextControlId();
            var label = selected < 0 || selected >= items.Length ? defaultLabel : items[selected];
            var prev = selected;

            ref var open = ref gui.Storage.Get<bool>(id);

            BeginDropdown(gui, ref open, label, size, preview);
            if (open && DropdownList(gui, ref selected, items))
            {
                open = false;
            }
            EndDropdown(gui);

            return prev != selected;
        }

        public static ImRect GetListRect(ImGui gui, ImRect controlRect, int itemsCount = 0)
        {
            var width = Mathf.Max(controlRect.W, gui.Style.Dropdown.MinListWidth);
            var itemsHeight = gui.GetRowHeight() * itemsCount + (gui.Style.Layout.ControlsSpacing * Mathf.Max(0, itemsCount - 1));
            var height = Mathf.Min(gui.Style.Dropdown.MaxListHeight, ImList.GetEnclosingHeight(gui, itemsHeight));
            var x = controlRect.X;
            var y = controlRect.Y - height;

            return new ImRect(x, y, width, height);
        }

        public static void BeginList(ImGui gui, int itemsCount = 0)
        {
            var controlRect = gui.Layout.GetBoundsRect();
            var listRect = GetListRect(gui, controlRect, itemsCount);

            gui.BeginPopup();
            gui.BeginList(listRect);
        }

        public static void EndList(ImGui gui, out bool closeClicked)
        {
            gui.EndList();
            gui.EndPopup(out closeClicked);
        }

        public static void BeginPreview(ImGui gui)
        {
            var wholeRect = gui.Layout.GetBoundsRect();
            var arrowWidth = GetArrowWidth(wholeRect.W, wholeRect.H);
            wholeRect.SplitRight(arrowWidth, out var previewRect);

            gui.Layout.Push(ImAxis.Horizontal, previewRect);
            gui.SetNextAdjacency(ImAdjacency.Left);
        }

        public static void EndPreview(ImGui gui, uint id, ref bool open)
        {
            gui.Layout.Pop();

            var wholeRect = gui.Layout.GetBoundsRect();
            var buttonRect = wholeRect.SplitRight(GetArrowWidth(wholeRect.W, wholeRect.H), out _);

            gui.SetNextAdjacency(ImAdjacency.Right);

            if (ArrowButton(gui, id, buttonRect))
            {
                open = !open;
            }
        }

        public static void NoPreview(ImGui gui, uint id, ref bool open)
        {
            if (ArrowButton(gui, id, gui.Layout.GetBoundsRect()))
            {
                open = !open;
            }
        }
        
        public static void PreviewButton(ImGui gui, uint id, ref bool open, ReadOnlySpan<char> label)
        {
            using var _ = new ImStyleScope<ImStyleButton>(ref gui.Style.Button, in gui.Style.Dropdown.Button);

            if (gui.Button(id, label, gui.Layout.GetBoundsRect()))
            {
                open = !open;
            }
        }

        public static void PreviewText(ImGui gui, ReadOnlySpan<char> label)
        {
            gui.TextEditReadonly(label, gui.Layout.GetBoundsRect(), false);
        }

        public static bool ArrowButton(ImGui gui, uint id, ImRect rect)
        {
            bool clicked;
            
            using (new ImStyleScope<ImStyleButton>(ref gui.Style.Button, in gui.Style.Dropdown.Button))
            {
                clicked = gui.Button(id, rect, out var state);
                
                ImFoldout.DrawArrowDown(gui.Canvas, rect, ImButton.GetStateFrontColor(gui, state), gui.Style.Dropdown.ArrowScale);
            }
            
            return clicked;
        }

        public static float GetArrowWidth(float controlWidth, float controlHeight)
        {
            return Mathf.Min(controlWidth * 0.5f, controlHeight);
        }
    }
}