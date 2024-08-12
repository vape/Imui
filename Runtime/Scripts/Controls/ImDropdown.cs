using System;
using Imui.Core;
using Imui.Controls.Styling;
using UnityEngine;

namespace Imui.Controls
{
    [Flags]
    public enum ImDropdownFlag
    {
        None = 0,
        NoPreview = 1
    }
    
    public struct ImDropdownState
    {
        public bool Open;
    }
    
    public static class ImDropdown
    {
        public static bool Dropdown(this ImGui gui,
                                    ref int selected,
                                    ReadOnlySpan<string> items,
                                    ImSize size = default,
                                    ReadOnlySpan<char> defaultLabel = default,
                                    ImDropdownFlag flags = ImDropdownFlag.None)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            
            var id = gui.GetNextControlId();
            var label = selected < 0 || selected >= items.Length ? defaultLabel : items[selected];
            var prev = selected;
            ref var state = ref gui.Storage.Get<ImDropdownState>(id);
            
            Begin(gui, size);
            
            if ((flags & ImDropdownFlag.NoPreview) != 0)
            {
                NoPreview(gui, id, ref state.Open);
            }
            else
            {
                BeginPreview(gui);
                PreviewButton(gui, id, label, ref state.Open);
                EndPreview(gui, id, ref state.Open);
            }

            if (state.Open)
            {
                BeginList(gui, id, items.Length);
                for (int i = 0; i < items.Length; ++i)
                {
                    if (gui.ListItem(ref selected, i, items[i]))
                    {
                        state.Open = false;
                    }
                }
                EndList(gui, ref state.Open);
            }
            
            End(gui);

            return prev != selected;
        }
        
        public static void Begin(ImGui gui, ImSize size = default)
        {
            Begin(gui, ImControls.AddRowRect(gui, size));
        }
        
        public static void Begin(ImGui gui, ImRect rect)
        {
            gui.Layout.Push(ImAxis.Horizontal, rect);
        }

        public static void End(ImGui gui)
        {
            gui.Layout.Pop();
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

        public static void BeginList(ImGui gui, uint id, int itemsCount = 0)
        {
            var controlRect = gui.Layout.GetBoundsRect();
            var listRect = GetListRect(gui, controlRect, itemsCount);

            gui.PushId(id);
            gui.BeginPopup();
            gui.BeginList(listRect);
        }

        public static void EndList(ImGui gui, ref bool open)
        {
            gui.EndList();
            gui.EndPopup(out var closeClicked);
            gui.PopId();
            
            if (closeClicked)
            {
                open = false;
            }
        }

        public static void BeginPreview(ImGui gui)
        {
            var wholeRect = gui.Layout.GetBoundsRect();
            var arrowWidth = GetArrowWidth(wholeRect.W, wholeRect.H);
            wholeRect.SplitRight(arrowWidth, out var previewRect);
            
            gui.Layout.Push(ImAxis.Horizontal, previewRect);
        }

        public static void EndPreview(ImGui gui, uint id, ref bool open)
        {
            gui.Layout.Pop();

            var wholeRect = gui.Layout.GetBoundsRect();
            var buttonRect = wholeRect.SplitRight(GetArrowWidth(wholeRect.W, wholeRect.H), out _);

            if (ArrowButton(gui, id, true, buttonRect))
            {
                open = !open;
            }
        }

        public static void NoPreview(ImGui gui, uint id, ref bool open)
        {
            if (ArrowButton(gui, id, false, gui.Layout.GetBoundsRect()))
            {
                open = !open;
            }
        }

        public static void PreviewBackground(ImGui gui)
        {
            var rect = gui.Layout.GetBoundsRect();
            var style = ImTheme.Active.TextEdit.Normal.Box;
            style.BorderRadius.TopRight = 0;
            style.BorderRadius.BottomRight = 0;

            gui.Box(rect, in style);
        }

        public static void PreviewButton(ImGui gui, uint id, ReadOnlySpan<char> label, ref bool open)
        {
            using var _ = new ImStyleScope<ImButtonStyle>(ref ImTheme.Active.Button);
            ImTheme.Active.Button.Alignment = ImTheme.Active.Dropdown.Alignment;

            if (gui.Button(id, label, gui.Layout.GetBoundsRect(), flag: ImButtonFlag.NoRoundCornersRight))
            {
                open = !open;
            }
        }
        
        public static bool ArrowButton(ImGui gui, uint id, bool havePreview, ImRect rect)
        {
            var buttonFlags = havePreview ? ImButtonFlag.NoRoundCornersLeft : ImButtonFlag.None;
            var clicked = gui.Button(id, rect, out var state, buttonFlags);

            DrawArrow(gui, rect, ImButton.GetStateFontColor(state));

            return clicked;
        }

        public static void DrawArrow(ImGui gui, ImRect rect, Color32 color)
        {
            rect = rect.WithAspect(1.0f).ScaleFromCenter(ImTheme.Active.Dropdown.ArrowScale).WithAspect(1.1547f);
            
            Span<Vector2> points = stackalloc Vector2[3]
            {
                new Vector2(rect.X + rect.W * 0.5f, rect.Y),
                new Vector2(rect.X + rect.W, rect.Y + rect.H),
                new Vector2(rect.X, rect.Y + rect.H),
            };
        
            gui.Canvas.ConvexFill(points, color);
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