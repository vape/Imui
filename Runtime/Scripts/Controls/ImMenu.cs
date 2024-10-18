using System;
using Imui.Core;
using Imui.Style;
using UnityEngine;

namespace Imui.Controls
{
    public struct ImMenuState
    {
        public uint Selected;
        public uint Fixed;
        public uint Clicked;
        public bool Dismissed;
        public Vector2 Size;
    }
    
    public static unsafe class ImMenu
    {
        public static readonly Color32 FrontColor = Color.white;
        public static readonly Color32 BackColor = Color.black;
        public static readonly float ArrowScale = 0.6f;

        public static bool TryBeginMenu(this ImGui gui, ReadOnlySpan<char> name, ref bool open)
        {
            if (!open)
            {
                return false;
            }
            
            var id = gui.PushId(name);
            var state = gui.Storage.GetRef<ImMenuState>(id);
            var rect = gui.AddLayoutRect(state->Size);

            if (state->Dismissed)
            {
                open = false;
                state->Dismissed = false;
            }
            
            BeginMenu(gui, id, rect);

            return true;
        }

        public static void EndMenu(this ImGui gui)
        {
            if (!TryGetMenuState(gui, out var state))
            {
                return;
            }

            EndMenu(gui, state);

            gui.PopId();
        }
        
        public static void BeginMenu(ImGui gui, uint id, ImRect rect)
        {
            gui.PushId(id);
            gui.Layout.Push(ImAxis.Vertical, rect);
            gui.BeginPopup();

            gui.Storage.Get<ImMenuState>(id);
        }
        
        public static void EndMenu(ImGui gui, ImMenuState* state)
        {
            state->Size = gui.Layout.GetContentRect().Size;
            
            gui.Canvas.PushOrder(gui.Canvas.GetOrder() - 1);
            gui.Canvas.Rect(gui.Layout.GetContentRect(), BackColor);
            
            gui.EndPopupWithCloseButton(out var popupCloseButtonClicked);
            
            state->Dismissed = popupCloseButtonClicked | state->Clicked != default;
            
            if (state->Dismissed)
            {
                state->Selected = default;
                state->Fixed = default;
                state->Clicked = default;
            }

            gui.Layout.Pop();
            gui.PopId();
        }
        
        public static bool BeginSubMenu(this ImGui gui, ReadOnlySpan<char> label)
        {
            if (!TryGetMenuState(gui, out var containerState))
            {
                return false;
            }
            
            var id = gui.GetNextControlId();
            var state = gui.Storage.GetRef<ImMenuState>(id);
            var position = gui.GetLayoutPosition() + new Vector2(gui.GetLayoutWidth(), 0);
            
            MenuItem(gui, id, containerState, label, true, out var selected);

            if (!selected)
            {
                return false;
            }
            
            BeginMenu(gui, id, GetMenuRectAt(position, state->Size));

            return true;
        }

        public static void EndSubMenu(this ImGui gui)
        {
            if (!TryGetMenuState(gui, out var state))
            {
                return;
            }

            var itemClicked = state->Clicked;
            
            EndMenu(gui, state);

            if (itemClicked != default && TryGetMenuState(gui, out var parentsState, fail: false))
            {
                parentsState->Clicked = itemClicked;
            }
        }

        public static bool MenuItem(this ImGui gui, ReadOnlySpan<char> label)
        {
            if (!TryGetMenuState(gui, out var state))
            {
                return false;
            }
            
            var id = gui.GetNextControlId();

            return MenuItem(gui, id, state, label, false, out _);
        }
        
        public static bool MenuItem(ImGui gui, uint id, ImMenuState* state, ReadOnlySpan<char> label, bool expandable, out bool active)
        {
            var arrowSize = gui.Style.Layout.TextSize;
            var textSettings = new ImTextSettings(gui.Style.Layout.TextSize);
            var textSize = gui.MeasureTextSize(label, textSettings);
            var extraWidth = arrowSize;
            var contentWidth = Mathf.Max(gui.GetLayoutWidth(), textSize.x + extraWidth);
            var contentRect = gui.AddLayoutRect(new Vector2(contentWidth, textSize.y));
            var hovered = gui.IsControlHovered(id);

            gui.RegisterControl(id, contentRect);
            
            if (expandable && !hovered && state->Selected == id && state->Fixed != id && ShouldFixate(contentRect, gui.Input.MousePosition))
            {
                state->Fixed = id;
            }
            
            if (hovered)
            {
                if (state->Fixed != id)
                {
                    state->Fixed = default;
                }
                
                state->Selected = id;
            }
            else if (state->Selected == id)
            {
                state->Selected = default;
            }
            
            active = state->Selected == id || state->Fixed == id;
            
            if (active)
            {
                DrawButtonBox(gui, contentRect, in gui.Style.List.ItemSelected, ImButtonState.Hovered);
            }
            else
            {
                DrawButtonBox(gui, contentRect, in gui.Style.List.ItemNormal, ImButtonState.Normal);
            }

            var clicked = gui.InvisibleButton(id, contentRect, ImButtonFlag.ActOnPress);
            var labelRect = contentRect;
            
            if (expandable)
            {
                var arrowRect = contentRect.SplitRight(arrowSize, gui.Style.Layout.InnerSpacing, out labelRect).WithAspect(1.0f);
                ImFoldout.DrawArrowRight(gui.Canvas, arrowRect, FrontColor, ArrowScale);
            }
            
            gui.Text(label, in textSettings, FrontColor, labelRect);

            if (clicked)
            {
                state->Clicked = id;
            }

            return clicked;
        }

        public static ImRect GetMenuRectAt(Vector2 position, Vector2 size)
        {
            var rect = new ImRect(position.x, position.y - size.y, size.x, size.y);
            return rect;
        }
        
        public static void DrawButtonBox(ImGui gui, ImRect rect, in ImStyleButton style, ImButtonState state)
        {
            var boxStyle = ImButton.GetStateBoxStyle(in style, state);
            gui.Box(rect, in boxStyle);
        }

        private static bool TryGetMenuState(ImGui gui, out ImMenuState* state, bool fail = true)
        {
            if (gui.TryPeekId(out var menuId) && gui.Storage.TryGetRef(menuId, out state))
            {
                return true;
            }

            if (fail)
            {
                ImAssert.Error($"Menu state is missing. BeginMenu and corresponding EndMenu must be called before and after MenuItem accordingly");
            }
            
            state = default;
            return false;
        }
        
        private static bool ShouldFixate(ImRect rect, Vector2 mousePosition)
        {
            return mousePosition.x > rect.Right;
        }
    }
}