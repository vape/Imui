using System;

namespace Imui.Core
{
    [Flags]
    public enum ImControlAdjacency
    {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = Left | Right
    }
    
    public struct ImControlSettings
    {
        public ImControlAdjacency Adjacency;
    }
}