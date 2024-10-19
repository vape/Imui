using System;
using Imui.Core;
using Imui.Style;
using UnityEngine;

namespace Imui.Controls
{
    [Flags]
    public enum ImMenuStateFlag
    {
        None = 0,
        Dismissed = 1,
        LayoutBuilt = 2
    }
    
    public struct ImMenuState
    {
        public uint Selected;
        public uint Fixed;
        public uint Clicked;
        
        public ImMenuStateFlag Flags;
        
        public Vector2 Size;
    }
    
    public static unsafe class ImMenu
    {
        public static bool TryBeginMenu(this ImGui gui, ReadOnlySpan<char> name, ref bool open)
        {
            if (!open)
            {
                return false;
            }
            
            var id = gui.PushId(name);
            var state = gui.Storage.GetRef<ImMenuState>(id);
            var rect = gui.AddLayoutRect(state->Size);

            if ((state->Flags & ImMenuStateFlag.Dismissed) != 0)
            {
                open = false;
                state->Flags &= ~ImMenuStateFlag.Dismissed;
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
            gui.Layout.Push(ImAxis.Vertical, rect.WithPadding(gui.Style.Menu.Padding));
            gui.BeginPopup();

            var state = gui.Storage.GetRef<ImMenuState>(id);
            if ((state->Flags & ImMenuStateFlag.LayoutBuilt) == 0)
            {
                gui.Canvas.PushClipEverything();
            }
        }
        
        public static void EndMenu(ImGui gui, ImMenuState* state)
        {
            if ((state->Flags & ImMenuStateFlag.LayoutBuilt) == 0)
            {
                gui.Canvas.PopClipRect();
            }

            var contentRect = gui.Layout.GetContentRect().WithPadding(-gui.Style.Menu.Padding);
            
            state->Size = contentRect.Size;
            
            gui.Canvas.PushOrder(gui.Canvas.GetOrder() - 1);
            gui.Box(contentRect, gui.Style.Menu.Box);
            
            gui.EndPopupWithCloseButton(out var popupCloseButtonClicked);

            state->Flags |= ImMenuStateFlag.LayoutBuilt;
            state->Flags |= popupCloseButtonClicked | state->Clicked != default ? ImMenuStateFlag.Dismissed : ImMenuStateFlag.None;
            
            if ((state->Flags & ImMenuStateFlag.Dismissed) != 0)
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
            
            MenuItem(gui, id, containerState, label, true, out var active);

            if (!active)
            {
                state->Size = default;
                state->Flags &= ~ImMenuStateFlag.LayoutBuilt;
                
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

            var clicked = state->Clicked;
            
            EndMenu(gui, state);

            if (clicked != default && TryGetMenuState(gui, out var parentsState, fail: false))
            {
                parentsState->Clicked = clicked;
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
            gui.AddSpacingIfLayoutFrameNotEmpty();
            
            var arrowSize = gui.Style.Layout.TextSize;
            var textSettings = new ImTextSettings(gui.Style.Layout.TextSize);
            var textSize = gui.MeasureTextSize(label, textSettings);
            var extraWidth = arrowSize;
            var contentWidth = Mathf.Max(gui.GetLayoutWidth(), gui.Style.Layout.InnerSpacing + textSize.x + extraWidth);
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

            ref var buttonStyle = ref (active ? ref gui.Style.Menu.ItemActive : ref gui.Style.Menu.ItemNormal);
            using var _ = new ImStyleScope<ImStyleButton>(ref gui.Style.Button, in buttonStyle);
            
            var clicked = gui.Button(id, contentRect, out var buttonState, ImButtonFlag.ActOnPress) && !expandable;
            var frontColor = ImButton.GetStateFrontColor(gui, buttonState);
            var labelRect = contentRect.WithPadding(left: gui.Style.Layout.InnerSpacing);
            
            if (expandable)
            {
                var arrowRect = labelRect.SplitRight(arrowSize, gui.Style.Layout.InnerSpacing, out labelRect).WithAspect(1.0f);
                ImFoldout.DrawArrowRight(gui.Canvas, arrowRect, frontColor, gui.Style.Menu.ArrowScale);
            }
            
            gui.Text(label, in textSettings, frontColor, labelRect);

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