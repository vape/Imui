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

    public struct ImDropdownState
    {
        public bool Open;
    }

    public static unsafe class ImDropdown
    {
        public static bool BeginDropdown(this ImGui gui,
                                         ReadOnlySpan<char> label,
                                         ImSize size = default,
                                         ImDropdownPreviewType preview = ImDropdownPreviewType.Default)
        {
            if (!BeginDropdownButton(gui, label, size, preview))
            {
                return false;
            }

            gui.BeginPopup();

            ref var state = ref gui.GetCurrentScope<ImDropdownState>();

            if (!gui.BeginMenuPopup(label, ref state.Open, gui.LastControlRect.BottomLeft, gui.LastControlRect.W))
            {
                gui.EndPopup();
                return false;
            }

            return true;
        }

        public static void EndDropdown(this ImGui gui)
        {
            gui.EndMenuPopup();
            gui.EndPopup();
            gui.EndDropdownButton();
        }

        public static bool BeginDropdownButton(this ImGui gui,
                                               ReadOnlySpan<char> label,
                                               ImSize size = default,
                                               ImDropdownPreviewType preview = ImDropdownPreviewType.Default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var id = gui.GetNextControlId();
            var rect = gui.AddSingleRowRect(size, minWidth: gui.GetRowHeight());

            ref var state = ref gui.BeginScope<ImDropdownState>(id);

            var clicked = DropdownButton(gui, id, label, rect, preview);
            if (clicked)
            {
                state.Open = !state.Open;
            }

            if (!state.Open)
            {
                gui.EndScope<ImDropdownState>();
                return false;
            }

            gui.PushId(id);

            return state.Open;
        }

        public static void EndDropdownButton(this ImGui gui)
        {
            gui.PopId();
            gui.EndScope<ImDropdownState>();
        }

        public static void CloseDropdown(this ImGui gui)
        {
            if (gui.TryGetCurrentScopeUnsafe(out ImDropdownState* state))
            {
                state->Open = false;
            }
        }

        public static bool DropdownButton(ImGui gui, uint id, ReadOnlySpan<char> label, ImRect rect, ImDropdownPreviewType preview)
        {
            if (preview == ImDropdownPreviewType.Arrow || (rect.H > 0.0f && rect.W / rect.H < 2.0f))
            {
                return ArrowButton(gui, id, rect);
            }

            var borderWidth = gui.Style.Dropdown.Button.BorderThickness;
            var arrowWidth = Mathf.Min(rect.W * 0.5f, rect.H);
            var buttonRect = rect.TakeRight(arrowWidth, -borderWidth, out var previewRect);
            var clicked = ArrowButton(gui, id, buttonRect, ImAdjacency.Right);

            switch (preview)
            {
                case ImDropdownPreviewType.Text:
                    gui.TextEditReadonly(label, previewRect, false, ImAdjacency.Left);
                    break;
                case ImDropdownPreviewType.Default:
                    using (gui.StyleScope(ref gui.Style.Button, in gui.Style.Dropdown.Button))
                    {
                        clicked |= gui.Button(id, label, previewRect, adjacency: ImAdjacency.Left);
                    }
                    break;
            }

            gui.SetLastControl(id, rect);

            return clicked;
        }

        public static bool Dropdown(this ImGui gui,
                                    ref int selected,
                                    ReadOnlySpan<string> items,
                                    ImSize size = default,
                                    ImDropdownPreviewType preview = ImDropdownPreviewType.Default,
                                    ReadOnlySpan<char> defaultLabel = default)
        {
            var changed = false;

            if (BeginDropdown(gui, selected < 0 || selected >= items.Length ? defaultLabel : items[selected], size, preview))
            {
                for (int i = 0; i < items.Length; ++i)
                {
                    if (gui.MenuItem(items[i], selected == i))
                    {
                        selected = i;
                        changed = true;

                        gui.CloseDropdown();
                    }
                }

                EndDropdown(gui);
            }

            return changed;
        }

        public static bool ArrowButton(ImGui gui, uint id, ImRect rect, ImAdjacency adjacency = ImAdjacency.None)
        {
            bool clicked;

            using (gui.StyleScope(ref gui.Style.Button, in gui.Style.Dropdown.Button))
            {
                clicked = gui.Button(id, rect, out var state, adjacency: adjacency);

                rect = rect.WithAspect(1.0f);
                rect = rect.ScaleFromCenter(gui.Style.Layout.TextSize / rect.W);

                ImFoldout.DrawArrowDown(gui.Canvas, rect, ImButton.GetStateFrontColor(gui, state), gui.Style.Dropdown.ArrowScale);
            }

            return clicked;
        }
    }
}