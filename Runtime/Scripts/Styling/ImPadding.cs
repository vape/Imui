using System;

namespace Imui.Styling
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

        public static implicit operator ImPadding(float padding) => new(padding);
    }
}