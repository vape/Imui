using UnityEngine;

namespace Imui.IO.Events
{
    public enum ImMouseEventType
    {
        None,
        Down,
        Up,
        Move,
        Scroll,
        BeginDrag,
        Drag,
        Hold
    }
    
    public readonly struct ImMouseEvent
    {
        public readonly ImMouseEventType Type;
        public readonly int Button;
        public readonly EventModifiers Modifiers;
        public readonly Vector2 Delta;
        public readonly int Count;
        public readonly bool LeftButton;

        public ImMouseEvent(ImMouseEventType type, int button, EventModifiers modifiers, Vector2 delta, int count = 1)
        {
            Type = type;
            Button = button;
            Modifiers = modifiers;
            Delta = delta;
            Count = count;
            LeftButton = button == 0;
        }

        public override string ToString()
        {
            return $"type:{Type} btn:{Button} mod:{Modifiers} dt:{Delta}";
        }
    }
}