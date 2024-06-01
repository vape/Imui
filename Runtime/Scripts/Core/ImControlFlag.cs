using System;

namespace Imui.Core
{
    [Flags]
    public enum ImControlFlag
    {
        None      = 0,
        Draggable = 1 << 0
    }
}