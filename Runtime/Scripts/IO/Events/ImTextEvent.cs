namespace Imui.IO.Events
{
    public enum ImTextEventType
    {
        None,
        Cancel,
        Submit
    }

    public readonly struct ImTextEvent
    {
        public readonly ImTextEventType Type;
        public readonly string Text;

        public ImTextEvent(ImTextEventType type)
        {
            Type = type;
            Text = null;
        }

        public ImTextEvent(ImTextEventType type, string text)
        {
            Type = type;
            Text = text;
        }

        public override string ToString()
        {
            return $"type:{Type} text:{Text}";
        }
    }
}