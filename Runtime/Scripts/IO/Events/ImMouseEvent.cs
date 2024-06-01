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
        Drag
    }
    
    public readonly struct ImMouseEvent
    {
        public readonly ImMouseEventType Type;
        public readonly int Button;
        public readonly EventModifiers Modifiers;
        public readonly Vector2 Delta;

        public ImMouseEvent(ImMouseEventType type, int button, EventModifiers modifiers, Vector2 delta)
        {
            Type = type;
            Button = button;
            Modifiers = modifiers;
            Delta = delta;
        }

        public override string ToString()
        {
            return $"type:{Type} btn:{Button} mod:{Modifiers} dt:{Delta}";
        }
    }
}