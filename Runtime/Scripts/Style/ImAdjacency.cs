using System;

namespace Imui.Style
{
    [Flags]
    public enum ImAdjacency
    {
        None = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8,
        Middle = Left | Right
    }
}