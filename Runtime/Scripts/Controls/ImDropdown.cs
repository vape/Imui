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
                                         ReadOnlySpan<char> label,
                                         ImSize size = default,
                                         ImDropdownPreviewType preview = ImDropdownPreviewType.Default)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            
            var id = gui.GetNextControlId();
            var rect = gui.AddSingleRowRect(size);
            ref var open = ref gui.Storage.Get<bool>(id);

            return BeginDropdown(gui, id, ref open, label, rect, preview);
        }
        
        public static bool BeginDropdown(this ImGui gui,
                                         uint id,
                                         ref bool open,
                                         ReadOnlySpan<char> label,
                                         ImRect rect,
                                         ImDropdownPreviewType preview)
        {
            var clicked = DropdownButton(gui, id, label, rect, preview);
            if (clicked)
            {
                open = !open;
            }

            if (!open)
            {
                return false;
            }
            
            gui.PushId(id);
            gui.BeginPopup();
            
            return true;
        }
        
        public static void EndDropdown(this ImGui gui)
        {
            gui.EndPopup();
            gui.PopId();
        }
        
        public static bool DropdownButton(ImGui gui, uint id, ReadOnlySpan<char> label, ImRect rect, ImDropdownPreviewType preview)
        {
            if (preview == ImDropdownPreviewType.Arrow)
            {
                return ArrowButton(gui, id, rect);
            }
            
            var borderWidth = gui.Style.Dropdown.Button.BorderThickness;
            var arrowWidth = GetArrowWidth(rect.W, rect.H);
            var buttonRect = rect.TakeRight(arrowWidth, -borderWidth, out var previewRect);
            var clicked = ArrowButton(gui, id, buttonRect, ImAdjacency.Right);

            switch (preview)
            {
                case ImDropdownPreviewType.Text:
                    gui.TextEditReadonly(label, previewRect, false, ImAdjacency.Left);
                    break;
                case ImDropdownPreviewType.Default:
                    using (new ImStyleScope<ImStyleButton>(ref gui.Style.Button, in gui.Style.Dropdown.Button))
                    {
                        clicked |= gui.Button(id, label, previewRect, adjacency: ImAdjacency.Left);
                    }
                    break;
            }

            return clicked;
        }
        
        public static bool Dropdown(this ImGui gui,
                                    ref int selected,
                                    ReadOnlySpan<string> items,
                                    ImSize size = default,
                                    ImDropdownPreviewType preview = ImDropdownPreviewType.Default,
                                    ReadOnlySpan<char> defaultLabel = default)
        {
            var id = gui.GetNextControlId();
            var rect = gui.AddSingleRowRect(size);
            ref var open = ref gui.Storage.Get<bool>(id);
            var label = selected < 0 || selected >= items.Length ? defaultLabel : items[selected];
            var prev = selected;
            
            if (BeginDropdown(gui, id, ref open, label, rect, preview))
            {
                if (gui.BeginMenu(label, ref open, rect.BottomLeft, minWidth: rect.W))
                {
                    for (int i = 0; i < items.Length; ++i)
                    {
                        if (gui.MenuItem(items[i], selected == i))
                        {
                            selected = i;
                            open = false;
                        }
                    }
                    gui.EndMenu();
                }

                EndDropdown(gui);
            }

            return prev != selected;
        }
        
        public static bool ArrowButton(ImGui gui, uint id, ImRect rect, ImAdjacency adjacency = ImAdjacency.None)
        {
            bool clicked;
            
            using (new ImStyleScope<ImStyleButton>(ref gui.Style.Button, in gui.Style.Dropdown.Button))
            {
                clicked = gui.Button(id, rect, out var state, adjacency: adjacency);
                
                rect = rect.WithAspect(1.0f);
                rect = rect.ScaleFromCenter(gui.Style.Layout.TextSize / rect.W);
                
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