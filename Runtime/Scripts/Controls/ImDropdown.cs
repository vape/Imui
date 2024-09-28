using System;
using Imui.Core;
using Imui.Controls.Styling;
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
                                         ReadOnlySpan<char> label,
                                         bool open,
                                         ImSize size = default,
                                         ImDropdownPreviewType preview = ImDropdownPreviewType.Default)
        {
            BeginDropdown(gui, label, ref open, size, preview);
            return open;
        }

        public static bool BeginDropdown(this ImGui gui,
                                         ReadOnlySpan<char> label,
                                         ref bool open,
                                         ImSize size = default,
                                         ImDropdownPreviewType preview = ImDropdownPreviewType.Default)
        {
            var id = gui.GetNextControlId();

            return BeginDropdown(gui, id, label, ref open, size, preview);
        }

        public static bool BeginDropdown(this ImGui gui,
                                         uint id,
                                         ReadOnlySpan<char> label,
                                         ref bool open,
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
                    PreviewButton(gui, id, label, ref open);
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

            BeginDropdown(gui, label, ref open, size, preview);
            if (open && DropdownList(gui, ref selected, items))
            {
                open = false;
            }
            EndDropdown(gui);

            return prev != selected;
        }

        public static ImRect GetListRect(ImGui gui, ImRect controlRect, int itemsCount = 0)
        {
            var width = Mathf.Max(controlRect.W, ImTheme.Active.Dropdown.MinListWidth);
            var itemsHeight = gui.GetRowHeight() * itemsCount + (ImTheme.Active.Controls.ControlsSpacing * Mathf.Max(0, itemsCount - 1));
            var height = Mathf.Min(ImTheme.Active.Dropdown.MaxListHeight, ImList.GetEnclosingHeight(itemsHeight));
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
        
        public static void PreviewButton(ImGui gui, uint id, ReadOnlySpan<char> label, ref bool open)
        {
            using var _ = new ImStyleScope<ImButtonStyle>(ref ImTheme.Active.Button);
            ImTheme.Active.Button.Alignment = ImTheme.Active.Dropdown.Alignment;

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
            var clicked = gui.Button(id, rect, out var state);

            rect = rect.WithAspect(1.0f).ScaleFromCenter(ImTheme.Active.Dropdown.ArrowScale).WithAspect(1.1547f);

            Span<Vector2> points = stackalloc Vector2[3]
            {
                new Vector2(rect.X + rect.W * 0.5f, rect.Y), new Vector2(rect.X + rect.W, rect.Y + rect.H), new Vector2(rect.X, rect.Y + rect.H),
            };

            gui.Canvas.ConvexFill(points, ImButton.GetStateFontColor(state));

            return clicked;
        }

        public static float GetArrowWidth(float controlWidth, float controlHeight)
        {
            return Mathf.Min(controlWidth * 0.5f, controlHeight);
        }
    }

    [Serializable]
    public struct ImDropdownStyle
    {
        public float ArrowScale;
        public ImTextAlignment Alignment;
        public float MaxListHeight;
        public float MinListWidth;
    }
}