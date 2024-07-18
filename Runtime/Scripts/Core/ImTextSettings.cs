using System;
using UnityEngine;

namespace Imui.Core
{
    [Serializable]
    public struct ImTextAlignment
    {
        [Range(0, 1)] public float X;
        [Range(0, 1)] public float Y;

        public ImTextAlignment(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
    
    [Serializable]
    public struct ImTextSettings
    {
        public float Size;
        public ImTextAlignment Align;
        public bool Wrap;

        public ImTextSettings(float size, float alignX = 0.0f, float alignY = 0.0f, bool wrap = true)
        {
            Size = size;
            Align.X = alignX;
            Align.Y = alignY;
            Wrap = wrap;
        }

        public ImTextSettings(float size, ImTextAlignment alignment, bool wrap = true)
        {
            Size = size;
            Align = alignment;
            Wrap = wrap;
        }
    }
}