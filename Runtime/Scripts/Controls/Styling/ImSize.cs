using UnityEngine;

namespace Imui.Controls.Styling
{
    public enum ImSizeType
    {
        Default   = 0,
        FixedSize = 1,
        AutoFit   = 2
    }
    
    public struct ImSize
    {
        public float Width;
        public float Height;
        public ImSizeType Type;

        public ImSize(float width, float height, ImSizeType type)
        {
            Width = width;
            Height = height;
            Type = type;
        }

        public ImSize(float width, float height)
        {
            Width = width;
            Height = height;
            Type = ImSizeType.FixedSize;
        }

        public ImSize(ImSizeType type)
        {
            Width = 0;
            Height = 0;
            Type = type;
        }

        public static implicit operator ImSize(Vector2 size)
        {
            return new ImSize(size.x, size.y);
        }

        public static implicit operator ImSize((float width, float height) size)
        {
            return new ImSize(size.width, size.height);
        }

        public static implicit operator ImSize(ImSizeType type)
        {
            return new ImSize(type);
        }
    }
}