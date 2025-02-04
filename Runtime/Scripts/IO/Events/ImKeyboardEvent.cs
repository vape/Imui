using System;
using UnityEngine;

namespace Imui.IO.Events
{
    public enum ImKeyboardEventType
    {
        None,
        Down,
        Up
    }

    // TODO (artem-s): add cut command
    [Flags]
    public enum ImKeyboardCommandFlag: uint
    {
        None = 0,
        Selection = 1 << 0,
        NextWord = 1 << 1,
        SelectAll = 1 << 2,
        Copy = 1 << 3,
        Paste = 1 << 4
    }

    public readonly struct ImKeyboardEvent
    {
        public readonly ImKeyboardEventType Type;
        public readonly KeyCode Key;
        public readonly EventModifiers Modifiers;
        public readonly ImKeyboardCommandFlag Command;
        public readonly char Char;

        public ImKeyboardEvent(ImKeyboardEventType type, KeyCode key, EventModifiers modifiers, ImKeyboardCommandFlag command, char c)
        {
            Type = type;
            Key = key;
            Modifiers = modifiers;
            Command = command;
            Char = c;
        }

        public override string ToString()
        {
            return $"type:{Type} key:{Key} mod:{Modifiers} cmd:{Command} char:{Char}";
        }
    }
}