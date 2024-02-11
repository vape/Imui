using UnityEngine;

namespace Imui.Core.Input
{
    public enum ImInputEventMouseType
    {
        None,
        Down,
        Up,
        Move,
        Drag,
        Scroll
    }
    
    public readonly struct ImInputMouseEvent
    {
        public readonly ImInputEventMouseType Type;
        public readonly int Button;
        public readonly EventModifiers Modifiers;
        public readonly Vector2 Delta;

        public ImInputMouseEvent(ImInputEventMouseType type, int button, EventModifiers modifiers, Vector2 delta)
        {
            Type = type;
            Button = button;
            Modifiers = modifiers;
            Delta = delta;
        }

        public override string ToString()
        {
            return $"{Type}" +
                   $" Button: {Button}" +
                   (Modifiers == EventModifiers.None ? "" : $" Modifiers: {Modifiers}") +
                   (Delta == default ? "" : $" Delta: {Delta}");
        }
    }
}