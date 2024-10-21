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
        public uint Depth;

        public ImMenuStateFlag Flags;

        public Vector2 Size;
    }

    public static unsafe class ImMenu
    {
        public static bool BeginMenu(this ImGui gui, ReadOnlySpan<char> name, ref bool open)
        {
            if (!open)
            {
                return false;
            }

            var id = gui.PushId(name);
            
            var state = gui.PushControlScopePtr<ImMenuState>(id);
            var rect = gui.AddLayoutRect(state->Size);

            if ((state->Flags & ImMenuStateFlag.Dismissed) != 0)
            {
                open = false;
                state->Flags &= ~ImMenuStateFlag.Dismissed;
            }

            BeginMenu(gui, state, rect);

            return true;
        }

        public static void EndMenu(this ImGui gui)
        {
            EndMenu(gui, gui.PopControlScopePtr<ImMenuState>());

            gui.PopId();
        }

        public static void BeginMenu(ImGui gui, ImMenuState* state, ImRect rect)
        {
            gui.Layout.Push(ImAxis.Vertical, rect.WithPadding(gui.Style.Menu.Padding));

            if (IsRootMenu(state))
            {
                gui.BeginPopup();
            }
            else
            {
                gui.Canvas.PushOrder(gui.Canvas.GetOrder() + 2);
            }

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

            state->Size = contentRect.Size.Max(gui.Style.Menu.MinWidth, gui.Style.Menu.MinHeight);

            gui.Canvas.PushOrder(gui.Canvas.GetOrder() - 1);
            gui.Box(contentRect, gui.Style.Menu.Box);
            gui.Canvas.PopOrder();

            var closeButtonClicked = false;
            
            if (IsRootMenu(state))
            {
                gui.EndPopupWithCloseButton(out closeButtonClicked);
            }
            else
            {
                gui.Canvas.PopOrder();
            }

            state->Flags |= ImMenuStateFlag.LayoutBuilt;
            state->Flags |= closeButtonClicked | state->Clicked != default ? ImMenuStateFlag.Dismissed : ImMenuStateFlag.None;

            if ((state->Flags & ImMenuStateFlag.Dismissed) != 0)
            {
                state->Selected = default;
                state->Fixed = default;
                state->Clicked = default;
            }

            gui.Layout.Pop();
        }

        public static bool BeginSubMenu(this ImGui gui, ReadOnlySpan<char> label)
        {
            var parentState = gui.PeekControlScopePtr<ImMenuState>();
            
            var id = gui.PushId(label);
            var state = gui.PushControlScopePtr<ImMenuState>(id);
            var position = gui.GetLayoutPosition() + new Vector2(gui.GetLayoutWidth(), 0);

            MenuItem(gui, id, parentState, label, true, false, out var active, false);

            if (!active)
            {
                state->Size = default;
                state->Flags &= ~ImMenuStateFlag.LayoutBuilt;
                gui.PopControlScopePtr<ImMenuState>();
                gui.PopId();
                return false;
            }

            state->Depth = parentState->Depth + 1;

            BeginMenu(gui, state, GetMenuRectAt(position, state->Size));

            return true;
        }

        public static void EndSubMenu(this ImGui gui)
        {
            var state = gui.PopControlScopePtr<ImMenuState>();
            var clicked = state->Clicked;

            EndMenu(gui, state);
            gui.PopId();

            if (clicked != default && gui.TryPeekControlScopePtr<ImMenuState>(out var parentsState))
            {
                parentsState->Clicked = clicked;
            }
        }

        public static bool MenuItem(this ImGui gui, ReadOnlySpan<char> label, bool enabled)
        {
            MenuItem(gui, label, ref enabled);
            return enabled;
        }
        
        public static bool MenuItem(this ImGui gui, ReadOnlySpan<char> label, ref bool enabled)
        {
            var id = gui.GetNextControlId();
            var state = gui.PeekControlScopePtr<ImMenuState>();
            var clicked = MenuItem(gui, id, state, label, false, true, out _, enabled);

            if (clicked)
            {
                enabled = !enabled;
            }

            return clicked;
        }
        
        public static bool MenuItem(this ImGui gui, ReadOnlySpan<char> label)
        {
            var id = gui.GetNextControlId();
            var state = gui.PeekControlScopePtr<ImMenuState>();
            
            return MenuItem(gui, id, state, label, false, false, out _, false);
        }

        public static bool MenuItem(ImGui gui, uint id, ImMenuState* state, ReadOnlySpan<char> label, bool isExpandable, bool isToggleable, out bool active, bool toggleIsOn)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var extraWidth = gui.Style.Layout.TextSize;
            var textSettings = new ImTextSettings(gui.Style.Layout.TextSize, gui.Style.Menu.ItemNormal.Alignment);
            var textSize = gui.MeasureTextSize(label, textSettings);
            var minWidth = Mathf.Max(gui.GetLayoutWidth(), gui.Style.Menu.MinWidth);
            var contentWidth = Mathf.Max(minWidth, gui.Style.Layout.InnerSpacing + textSize.x + extraWidth);
            var contentRect = gui.AddLayoutRect(new Vector2(contentWidth, gui.GetRowHeight()));
            var hovered = gui.IsControlHovered(id);

            gui.RegisterControl(id, contentRect);

            var shouldFix = isExpandable && ShouldFixSelection(gui, contentRect);
            if (shouldFix && !hovered && state->Selected == id && state->Fixed != id)
            {
                state->Fixed = id;
            }
            else if (!shouldFix && state->Selected != default && state->Fixed == id)
            {
                state->Fixed = default;
            }

            if (hovered)
            {
                state->Selected = id;
            }
            else if (state->Selected == id)
            {
                state->Selected = default;
            }

            active = (state->Selected == id && (!isExpandable || state->Fixed == default)) || state->Fixed == id;

            ref var buttonStyle = ref (active ? ref gui.Style.Menu.ItemActive : ref gui.Style.Menu.ItemNormal);
            using var _ = new ImStyleScope<ImStyleButton>(ref gui.Style.Button, in buttonStyle);

            var clicked = gui.Button(id, contentRect, out var buttonState, ImButtonFlag.ActOnPress) && !isExpandable;
            var frontColor = ImButton.GetStateFrontColor(gui, buttonState);
            var labelRect = contentRect.WithPadding(left: gui.Style.Layout.InnerSpacing);
            var extraRect = labelRect.SplitRight(extraWidth, gui.Style.Layout.InnerSpacing, out labelRect).WithAspect(1.0f);
            
            if (isExpandable)
            {
                ImFoldout.DrawArrowRight(gui.Canvas, extraRect, frontColor, gui.Style.Menu.ArrowScale);
            }
            
            if (isToggleable && toggleIsOn)
            {
                ImCheckbox.DrawCheckmark(gui.Canvas, extraRect, frontColor, gui.Style.Menu.CheckmarkScale);
            }

            gui.Text(label, in textSettings, frontColor, labelRect);

            if (clicked)
            {
                state->Clicked = id;
            }

            return clicked;
        }

        // TODO (artem-s): should clamp at the screen borders and switch to opening left when there is no space on the right
        private static ImRect GetMenuRectAt(Vector2 position, Vector2 size)
        {
            var rect = new ImRect(position.x, position.y - size.y, size.x, size.y);
            return rect;
        }
        
        private static bool IsRootMenu(ImMenuState* state)
        {
            return state->Depth == 0;
        }

        // TODO (artem-s): assumes we only opening sub-menu to the right, which will be wrong in future (i hope)
        private static bool ShouldFixSelection(ImGui gui, ImRect buttonRect)
        {
            if (gui.Input.MousePosition.x > buttonRect.Right)
            {
                return true;
            }

            Span<Vector2> hoveringArea = stackalloc Vector2[]
            {
                new Vector2(buttonRect.Right, buttonRect.Bottom), new Vector2(buttonRect.Center.x, buttonRect.Bottom),
                new Vector2(buttonRect.Right, buttonRect.Bottom - buttonRect.H)
            };

            return ConvexIntersect(hoveringArea, gui.Input.MousePosition);
        }

        private static bool ConvexIntersect(Span<Vector2> shape, Vector2 point)
        {
            for (int i = 0; i < shape.Length; ++i)
            {
                var s = (shape[(i + 1) % shape.Length] - shape[i]);
                s = new Vector2(-s.y, s.x);
                var p = (point - shape[i]);

                if (Vector2.Dot(p, s) < 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}