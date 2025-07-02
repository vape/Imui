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
        Dismissed = 1 << 0,
        LayoutBuilt = 1 << 1,
        LayoutRoot = 1 << 2
    }

    [Flags]
    public enum ImMenuFlag
    {
        None = 0,
        DoNotDismissOnClick = 1
    }

    public struct ImMenuPosition
    {
        public readonly bool IsSet;
        public readonly float X;
        public readonly float Y;

        public ImMenuPosition(Vector2 position)
        {
            IsSet = true;
            X = position.x;
            Y = position.y;
        }

        public ImMenuPosition(float x, float y)
        {
            IsSet = true;
            X = x;
            Y = y;
        }

        public static implicit operator ImMenuPosition(Vector2 position)
        {
            return new ImMenuPosition(position);
        }
    }

    public struct ImMenuState
    {
        public uint Selected;
        public uint Fixed;
        public uint Clicked;
        public uint Depth;

        public ImMenuFlag BehaviourFlags;
        public ImMenuStateFlag StateFlags;

        public Vector2 Size;

        public float MinWidth;
    }

    public static unsafe class ImMenu
    {
        public static bool BeginMenuPopup(this ImGui gui,
                                          ReadOnlySpan<char> name,
                                          ref bool open,
                                          ImMenuPosition position = default,
                                          float minWidth = 0,
                                          ImMenuFlag flags = ImMenuFlag.None)
        {
            if (!open)
            {
                return false;
            }

            var id = gui.PushId(name);
            var state = gui.BeginScopeUnsafe<ImMenuState>(id);
            
            state->BehaviourFlags = flags;

            ImRect rect;

            if (position.IsSet)
            {
                rect = new ImRect(position.X, position.Y - state->Size.y, state->Size.x, state->Size.y);
                rect = ImRectUtility.Clamp(gui.Canvas.ScreenRect, rect);

                gui.Layout.Push(ImAxis.Vertical, rect);
                state->StateFlags |= ImMenuStateFlag.LayoutRoot;
            }
            else
            {
                rect = gui.AddLayoutRect(state->Size);
                state->StateFlags &= ~ImMenuStateFlag.LayoutRoot;
            }

            state->MinWidth = minWidth;

            if ((state->StateFlags & ImMenuStateFlag.Dismissed) != 0)
            {
                open = false;
                state->StateFlags &= ~ImMenuStateFlag.Dismissed;
            }

            BeginMenuPopup(gui, state, rect);

            return true;
        }

        public static void EndMenuPopup(this ImGui gui)
        {
            var state = gui.EndScopeUnsafe<ImMenuState>();

            EndMenuPopup(gui, state);

            if ((state->StateFlags & ImMenuStateFlag.LayoutRoot) != 0)
            {
                gui.Layout.Pop();
            }

            gui.PopId();
        }

        public static void BeginMenuPopup(ImGui gui, ImMenuState* state, ImRect rect)
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

            if ((state->StateFlags & ImMenuStateFlag.LayoutBuilt) == 0)
            {
                gui.Canvas.PushClipEverything();
            }
        }

        public static void EndMenuPopup(ImGui gui, ImMenuState* state)
        {
            var contentRect = gui.Layout.GetContentRect().WithPadding(-gui.Style.Menu.Padding);

            state->Size = contentRect.Size.Max(Mathf.Max(gui.Style.Menu.MinWidth, state->MinWidth), gui.Style.Menu.MinHeight);

            gui.Canvas.PushOrder(gui.Canvas.GetOrder() - 1);
            gui.Box(contentRect, gui.Style.Menu.Box);
            gui.Canvas.PopOrder();

            if ((state->StateFlags & ImMenuStateFlag.LayoutBuilt) == 0)
            {
                gui.Canvas.PopClipRect();
            }

            var closeButtonClicked = false;

            if (IsRootMenu(state))
            {
                gui.EndPopupWithCloseButton(out closeButtonClicked);
            }
            else
            {
                gui.Canvas.PopOrder();
            }

            var dismissOnClick = (state->BehaviourFlags & ImMenuFlag.DoNotDismissOnClick) == 0;

            state->StateFlags |= ImMenuStateFlag.LayoutBuilt;
            state->StateFlags |= closeButtonClicked | (state->Clicked != 0 && dismissOnClick) ? ImMenuStateFlag.Dismissed : ImMenuStateFlag.None;

            if ((state->StateFlags & ImMenuStateFlag.Dismissed) != 0)
            {
                state->Selected = default;
                state->Fixed = default;
                state->Clicked = default;
            }

            gui.Layout.Pop();
        }

        public static bool BeginMenuItem(this ImGui gui, ReadOnlySpan<char> label)
        {
            var parentState = gui.GetCurrentScopeUnsafe<ImMenuState>();

            var id = gui.PushId(label);
            var state = gui.BeginScopeUnsafe<ImMenuState>(id);
            var position = gui.GetLayoutPosition() + new Vector2(gui.GetLayoutWidth(), 0);

            state->BehaviourFlags = parentState->BehaviourFlags;
            
            MenuItem(gui, id, parentState, label, true, false, out var active, false);

            if (!active)
            {
                state->Size = default;
                state->StateFlags &= ~ImMenuStateFlag.LayoutBuilt;
                state->Fixed = default;
                state->Selected = default;

                gui.EndScopeUnsafe<ImMenuState>();
                gui.PopId();
                return false;
            }

            state->Depth = parentState->Depth + 1;

            BeginMenuPopup(gui, state, GetMenuRectAt(position, state->Size));

            return true;
        }

        public static void EndMenuItem(this ImGui gui)
        {
            var state = gui.EndScopeUnsafe<ImMenuState>();
            var clicked = state->Clicked;

            EndMenuPopup(gui, state);
            gui.PopId();

            if (clicked != default && gui.TryGetCurrentScopeUnsafe<ImMenuState>(out var parentsState))
            {
                parentsState->Clicked = clicked;
            }
        }

        public static bool MenuItem(this ImGui gui, ReadOnlySpan<char> label, bool enabled)
        {
            return MenuItem(gui, label, ref enabled);
        }

        public static bool MenuItem(this ImGui gui, ReadOnlySpan<char> label, ref bool enabled)
        {
            var id = gui.GetNextControlId();
            var state = gui.GetCurrentScopeUnsafe<ImMenuState>();
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
            var state = gui.GetCurrentScopeUnsafe<ImMenuState>();

            return MenuItem(gui, id, state, label, false, false, out _, false);
        }

        public static bool MenuItem(ImGui gui,
                                    uint id,
                                    ImMenuState* state,
                                    ReadOnlySpan<char> label,
                                    bool isExpandable,
                                    bool isToggleable,
                                    out bool active,
                                    bool toggleIsOn)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rowHeight = gui.GetRowHeight();
            var extraWidth = rowHeight;
            var textSettings = new ImTextSettings(gui.Style.Layout.TextSize, gui.Style.Menu.ItemNormal.Alignment);
            var textSize = gui.MeasureTextSize(label, textSettings);
            var minWidth = Mathf.Max(gui.GetLayoutWidth(), gui.Style.Menu.MinWidth);
            var contentWidth = Mathf.Max(minWidth, gui.Style.Layout.InnerSpacing + textSize.x + extraWidth);
            var contentRect = gui.AddLayoutRect(new Vector2(contentWidth, rowHeight));
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
            using var _ = gui.StyleScope(ref gui.Style.Button, in buttonStyle);

            var clicked = gui.Button(id, contentRect, out var buttonState, ImButtonFlag.ActOnPressMouse) && !isExpandable;
            var frontColor = ImButton.GetStateFrontColor(gui, buttonState);
            var labelRect = contentRect.WithPadding(left: gui.Style.Layout.InnerSpacing);
            var extraRect = labelRect.TakeRight(extraWidth, gui.Style.Layout.InnerSpacing, out labelRect)
                                     .ScaleFromCenter(new Vector2(gui.Style.Layout.TextSize / extraWidth, 1.0f))
                                     .WithAspect(1.0f);

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