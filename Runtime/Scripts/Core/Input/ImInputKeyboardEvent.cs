using UnityEngine;

namespace Imui.Core.Input
{
    public enum ImInputEventKeyboardType
    {
        None,
        Down,
        Up
    }
    
    public readonly struct ImInputKeyboardEvent
    {
        public readonly ImInputEventKeyboardType Type;
        public readonly KeyCode Key;
        public readonly EventModifiers Modifiers;
        public readonly char Char;

        public ImInputKeyboardEvent(ImInputEventKeyboardType type, KeyCode key, EventModifiers modifiers, char c)
        {
            Type = type;
            Key = key;
            Modifiers = modifiers;
            Char = c;
        }
        
        public override string ToString()
        {
            return $"{Type}" +
                   (Key == KeyCode.None ? "" : $" {Key}") +
                   $" Modifiers: {Modifiers}" +
                   (Char == (char)0 ? "" : $" Char: {Char}");
        }
    }
}