using System;
using UnityEngine;

namespace Imui.Core.Input
{
    public enum ImInputEventKeyboardType
    {
        None,
        Down,
        Up
    }

    [Flags]
    public enum ImInputKeyboardCommand : uint
    {
        None      = 0,
        Selection = 1 << 0,
        JumpWord  = 1 << 1,
        SelectAll = 1 << 2
    }
    
    public readonly struct ImInputKeyboardEvent
    {
        public readonly ImInputEventKeyboardType Type;
        public readonly KeyCode Key;
        public readonly EventModifiers Modifiers;
        public readonly ImInputKeyboardCommand Command;
        public readonly char Char;

        public ImInputKeyboardEvent(ImInputEventKeyboardType type, KeyCode key, EventModifiers modifiers, ImInputKeyboardCommand command, char c)
        {
            Type = type;
            Key = key;
            Modifiers = modifiers;
            Command = command;
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