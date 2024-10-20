using System;
using Imui.Style;
using UnityEngine;

namespace Imui.Core
{
    [Serializable]
    public struct ImTextSettings
    {
        public float Size;
        public ImAlignment Align;
        public bool Wrap;

        public ImTextSettings(float size, float alignX = 0.0f, float alignY = 0.0f, bool wrap = false)
        {
            Size = size;
            Align.X = alignX;
            Align.Y = alignY;
            Wrap = wrap;
        }

        public ImTextSettings(float size, ImAlignment alignment, bool wrap = false)
        {
            Size = size;
            Align = alignment;
            Wrap = wrap;
        }
    }
}