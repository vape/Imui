using System;
using Imui.Core;
using UnityEngine;

namespace Imui.Controls.Styling
{
    [Flags]
    public enum ImSizeFlag
    {
        Default   = 0,
        FixedSize = 1 << 0,
        AutoFit   = 1 << 1
    }
    
    public struct ImSize
    {
        public float Width;
        public float Height;
        public ImSizeFlag Flag;

        public ImSize(float width, float height, ImSizeFlag flag)
        {
            Width = width;
            Height = height;
            Flag = flag;
        }

        public ImSize(float width, float height)
        {
            Width = width;
            Height = height;
            Flag = ImSizeFlag.FixedSize;
        }

        public ImSize(ImSizeFlag flag)
        {
            Width = 0;
            Height = 0;
            Flag = flag;
        }

        public static implicit operator ImSize(Vector2 size)
        {
            return new ImSize(size.x, size.y);
        }

        public static implicit operator ImSize((float width, float height) size)
        {
            return new ImSize(size.width, size.height);
        }

        public static implicit operator ImSize(ImSizeFlag flag)
        {
            return new ImSize(flag);
        }
    }
}