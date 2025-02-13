using System.Runtime.CompilerServices;

namespace Imui.Core
{
    public struct ImAABB
    {
        public float Left;
        public float Top;
        public float Right;
        public float Bottom;

        public ImAABB(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public ImAABB(ImRect rect)
        {
            Left = rect.X;
            Top = rect.Top;
            Right = rect.Right;
            Bottom = rect.Y;
        }
    }

    public static class ImAABBExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlaps(this ref ImAABB self, in ImAABB other)
        {
            return
                other.Right > self.Left &
                other.Left < self.Right &
                other.Top > self.Bottom &
                other.Bottom < self.Top;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlaps(this ref ImAABB self, in ImRect other)
        {
            var right = other.X + other.W;
            var top = other.Y + other.H;
            
            return
                right > self.Left &
                other.X < self.Right &
                top > self.Bottom &
                other.Y < self.Top;
        }
    }
}