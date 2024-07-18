using System;

namespace Imui.Controls.Styling
{
    [Serializable]
    public struct ImPadding
    {
        public float Vertical => Top + Bottom;
        public float Horizontal => Left + Right;
        
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;

        public ImPadding(float padding) : this(padding, padding, padding, padding)
        { }
        
        public ImPadding(float left, float right, float top, float bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public void Add(float value)
        {
            Left += value;
            Right += value;
            Top += value;
            Bottom += value;
        }

        public static implicit operator ImPadding(float padding) => new(padding);
        
        public static ImPadding operator +(ImPadding padding, float value) 
        {
            padding.Add(value);
            return padding;
        }
        
        public static ImPadding operator -(ImPadding padding, float value) 
        {
            padding.Add(-value);
            return padding;
        }
    }
}