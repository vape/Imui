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

        public ImTextSettings(float size, float alignX = 0.0f, float alignY = 0.0f)
        {
            Size = size;
            Align.X = alignX;
            Align.Y = alignY;
        }

        public ImTextSettings(float size, ImTextAlignment alignment)
        {
            Size = size;
            Align = alignment;
        }
    }
}