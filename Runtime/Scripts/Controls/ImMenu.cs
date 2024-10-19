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

            BeginMenu(gui, state, rect);

            return true;
        }

        public static void EndMenu(this ImGui gui)
        {
            if (!TryGetActiveMenuState(gui, out var state))
            {
                return;
            }

            EndMenu(gui, state);

            gui.PopId();
        }

        public static void BeginMenu(ImGui gui, ImMenuState* state, ImRect rect)
        {
            gui.Layout.Push(ImAxis.Vertical, rect.WithPadding(gui.Style.Menu.Padding));
            gui.BeginPopup((int)(state->Depth * 2));

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
        }

        public static bool TryBeginSubMenu(this ImGui gui, ReadOnlySpan<char> label)
        {
            if (!TryGetActiveMenuState(gui, out var containerState))
            {
                return false;
            }

            // TODO (artem-s): will fail miserably when we'll try to make two sub menus with the same name under the same root menu
            // TODO (artem-s): it will also fail when we try to wrap it with PushId/PopId calls, because we won't find containerState, duh
            var id = gui.PushId(label);
            var state = gui.Storage.GetRef<ImMenuState>(id);
            var position = gui.GetLayoutPosition() + new Vector2(gui.GetLayoutWidth(), 0);

            MenuItem(gui, id, containerState, label, true, out var active);

            if (!active)
            {
                state->Size = default;
                state->Flags &= ~ImMenuStateFlag.LayoutBuilt;
                gui.PopId();
                return false;
            }

            state->Depth = containerState->Depth + 1;

            BeginMenu(gui, state, GetMenuRectAt(position, state->Size));

            return true;
        }

        public static void EndSubMenu(this ImGui gui)
        {
            if (!TryGetActiveMenuState(gui, out var state))
            {
                return;
            }

            var clicked = state->Clicked;

            EndMenu(gui, state);
            gui.PopId();

            if (clicked != default && TryGetActiveMenuState(gui, out var parentsState, fail: false))
            {
                parentsState->Clicked = clicked;
            }
        }

        public static bool MenuItem(this ImGui gui, ReadOnlySpan<char> label)
        {
            if (!TryGetActiveMenuState(gui, out var state))
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
            var textSettings = new ImTextSettings(gui.Style.Layout.TextSize, gui.Style.Menu.ItemNormal.Alignment);
            var textSize = gui.MeasureTextSize(label, textSettings);
            var extraWidth = arrowSize;
            var minWidth = Mathf.Max(gui.GetLayoutWidth(), gui.Style.Menu.MinWidth);
            var contentWidth = Mathf.Max(minWidth, gui.Style.Layout.InnerSpacing + textSize.x + extraWidth);
            var contentRect = gui.AddLayoutRect(new Vector2(contentWidth, gui.GetRowHeight()));
            var hovered = gui.IsControlHovered(id);

            gui.RegisterControl(id, contentRect);

            var shouldFix = expandable && ShouldFixSelection(gui, contentRect);
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

        // TODO (artem-s): should clamp at the screen borders and switch to opening left when there is no space on the right
        private static ImRect GetMenuRectAt(Vector2 position, Vector2 size)
        {
            var rect = new ImRect(position.x, position.y - size.y, size.x, size.y);
            return rect;
        }

        private static bool TryGetActiveMenuState(ImGui gui, out ImMenuState* state, bool fail = true)
        {
            if (gui.TryPeekId(out var menuId) && gui.Storage.TryGetRef(menuId, out state))
            {
                return true;
            }

            if (fail)
            {
                ImAssert.Error($"{nameof(ImMenuState)} is missing. {nameof(BeginMenu)} and corresponding {nameof(EndMenu)} must be called before and after " +
                               $"any {nameof(MenuItem)} accordingly");
            }

            state = default;
            return false;
        }

        // TODO (artem-s): assumes we only opening sub-menu to the right
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