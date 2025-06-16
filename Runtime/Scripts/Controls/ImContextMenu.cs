using Imui.Core;
using Imui.IO.Events;
using UnityEngine;

namespace Imui.Controls
{
    public struct ImContextMenuState
    {
        public bool Open;
        public Vector2 Source;
    }

    public static class ImContextMenu
    {
        public static bool BeginContextMenu(this ImGui gui, ImRect area)
        {
            var id = gui.GetNextControlId();
            ref var state = ref gui.Storage.Get<ImContextMenuState>(id);

            gui.RegisterGroup(id, area);

            ref readonly var evt = ref gui.Input.MouseEvent;
            if (!state.Open && gui.IsGroupHovered(id) && evt is { Type: ImMouseEventType.Down, Button: 1 })
            {
                state.Open = true;
                state.Source = gui.Input.MousePosition;

                gui.Input.UseMouseEvent();
            }

            return BeginContextMenu(gui, id, state.Source, ref state.Open);
        }

        public static bool BeginContextMenu(this ImGui gui, uint id, Vector2 source, ref bool open)
        {
            gui.PushId(id);

            if (!gui.BeginMenuPopup("context_menu", ref open, source))
            {
                gui.PopId();
                return false;
            }

            return true;
        }

        public static void EndContextMenu(this ImGui gui)
        {
            gui.EndMenuPopup();

            gui.PopId();
        }
    }
}