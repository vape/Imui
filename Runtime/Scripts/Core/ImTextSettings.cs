using System;
using UnityEngine;

namespace Imui.Core
{
    [Serializable]
    public struct ImTextAlignment
    {
        [Range(0, 1)] public float Hor;
        [Range(0, 1)] public float Ver;

        public ImTextAlignment(float horizontal, float vertical)
        {
            Hor = horizontal;
            Ver = vertical;
        }
    }
    
    [Serializable]
    public struct ImTextSettings
    {
        public float Size;
        public ImTextAlignment Align;

        public ImTextSettings(float size, float alignHor = 0.0f, float alignVer = 0.0f)
        {
            Size = size;
            Align.Hor = alignHor;
            Align.Ver = alignVer;
        }

        public ImTextSettings(float size, ImTextAlignment alignment)
        {
            Size = size;
            Align = alignment;
        }
    }
}