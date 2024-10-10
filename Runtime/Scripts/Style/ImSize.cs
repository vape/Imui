using UnityEngine;

namespace Imui.Style
{
    public enum ImSizeMode
    {
        Auto  = 0,
        Fixed = 1,
        Fit   = 2
    }
    
    public struct ImSize
    {
        public float Width;
        public float Height;
        public ImSizeMode Mode;

        public ImSize(float width, float height, ImSizeMode mode)
        {
            Width = width;
            Height = height;
            Mode = mode;
        }

        public ImSize(float width, float height)
        {
            Width = width;
            Height = height;
            Mode = ImSizeMode.Fixed;
        }

        public ImSize(ImSizeMode mode)
        {
            Width = 0;
            Height = 0;
            Mode = mode;
        }

        public static implicit operator ImSize(Vector2 size)
        {
            return new ImSize(size.x, size.y);
        }

        public static implicit operator ImSize((float width, float height) size)
        {
            return new ImSize(size.width, size.height);
        }

        public static implicit operator ImSize(ImSizeMode mode)
        {
            return new ImSize(mode);
        }
    }
}