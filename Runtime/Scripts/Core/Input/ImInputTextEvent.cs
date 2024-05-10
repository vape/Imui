namespace Imui.Core.Input
{
    public enum ImInputTextEventType
    {
        None = 0,
        Cancel = 1,
        Submit = 2
    }
    
    public readonly struct ImInputTextEvent
    {
        public readonly ImInputTextEventType Type;
        public readonly string Text;

        public ImInputTextEvent(ImInputTextEventType type)
        {
            Type = type;
            Text = null;
        }
        
        public ImInputTextEvent(ImInputTextEventType type, string text)
        {
            Type = type;
            Text = text;
        }
    }
}