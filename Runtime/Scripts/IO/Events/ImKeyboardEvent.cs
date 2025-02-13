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

    public readonly struct ImKeyboardEvent
    {
        public readonly ImKeyboardEventType Type;
        public readonly KeyCode Key;
        public readonly EventModifiers Modifiers;
        public readonly char Char;

        public ImKeyboardEvent(ImKeyboardEventType type, KeyCode key, EventModifiers modifiers, char c)
        {
            Type = type;
            Key = key;
            Modifiers = modifiers;
            Char = c;
        }

        public override string ToString()
        {
            return $"type:{Type} key:{Key} mod:{Modifiers} char:{Char}";
        }
    }
}