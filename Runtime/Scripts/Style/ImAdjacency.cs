using System;

namespace Imui.Style
{
    [Flags]
    public enum ImAdjacency
    {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = Left | Right
    }
}