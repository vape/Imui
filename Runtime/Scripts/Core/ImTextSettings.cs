using System;
using Imui.Rendering;
using Imui.Style;

namespace Imui.Core
{
    [Serializable]
    public struct ImTextSettings
    {
        public float Size;
        public ImAlignment Align;
        public bool Wrap;
        public ImTextOverflow Overflow;
        
        public ImTextSettings(float size, float alignX = 0.0f, float alignY = 0.0f, bool wrap = false, ImTextOverflow overflow = ImTextOverflow.Overflow)
        {
            Size = size;
            Align.X = alignX;
            Align.Y = alignY;
            Wrap = wrap;
            Overflow = overflow;
        }

        public ImTextSettings(float size, ImAlignment alignment, bool wrap = false, ImTextOverflow overflow = ImTextOverflow.Overflow)
        {
            Size = size;
            Align = alignment;
            Wrap = wrap;
            Overflow = overflow;
        }
    }
}