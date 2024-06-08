using Imui.IO.Events;
using Imui.Utility;
using UnityEngine;

namespace Imui.IO.Utility
{
    public static class KeyboardEventsUtility
    {
        public static bool TryParse(Event evt, ref CircularBuffer<ImKeyboardEvent> eventsQueue)
        {
            if (evt == null)
            {
                return false;
            }

            switch (evt.type)
            {
                case EventType.KeyDown:
                    eventsQueue.PushFront(new ImKeyboardEvent(ImKeyboardEventType.Down, evt.keyCode, evt.modifiers, ParseKeyboardCommand(evt), evt.character));
                    return true;
                case EventType.KeyUp:
                    eventsQueue.PushFront(new ImKeyboardEvent(ImKeyboardEventType.Up, evt.keyCode, evt.modifiers, ParseKeyboardCommand(evt), evt.character));
                    return true;
            }

            return false;
        }
        
        public static ImKeyboardCommandFlag ParseKeyboardCommand(Event evt)
        {
            var result = ImKeyboardCommandFlag.None;
            var arrow = (int)evt.keyCode >= 273 && (int)evt.keyCode <= 276;
            var jump = false;
            var control = false;

            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
            {
                jump = evt.modifiers.HasFlag(EventModifiers.Alt); // option key
                control = evt.modifiers.HasFlag(EventModifiers.Command);
            }
            else
            {
                jump = evt.modifiers.HasFlag(EventModifiers.Control);
                control = evt.modifiers.HasFlag(EventModifiers.Control);
            }
            
            if (arrow && evt.modifiers.HasFlag(EventModifiers.Shift))
            {
                result |= ImKeyboardCommandFlag.Selection;
            }

            if (arrow && jump)
            {
                result |= ImKeyboardCommandFlag.NextWord;
            }

            if (control && evt.keyCode == KeyCode.A)
            {
                result |= ImKeyboardCommandFlag.SelectAll;
            }

            if (control && evt.keyCode == KeyCode.C)
            {
                result |= ImKeyboardCommandFlag.Copy;
            }

            if (control && evt.keyCode == KeyCode.V)
            {
                result |= ImKeyboardCommandFlag.Paste;
            }
            
            return result;
        }
    }
}