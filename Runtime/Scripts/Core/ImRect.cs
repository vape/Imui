using System;
using UnityEngine;

namespace Imui.Core
{
    [Serializable]
    public struct ImRect : IEquatable<ImRect>
    {
        public Vector2 TopLeft => new Vector2(X, Y + H);
        public Vector2 TopRight => new Vector2(X + W, Y + H);
        public Vector2 BottomLeft => Position;
        public Vector2 BottomRight => new Vector2(X + W, Y);
        public Vector2 Center => new Vector2(X + W / 2f, Y + H / 2f);

        public Vector2 Position => new Vector2(X, Y);
        public Vector2 Size => new Vector2(W, H);

        public float X;
        public float Y;
        public float W;
        public float H;

        public ImRect(float x, float y, float w, float h)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
        }

        public ImRect(Vector2 p, Vector2 s) : this(p.x, p.y, s.x, s.y)
        {
        }

        public ImRect(ImRect rect) : this(rect.X, rect.Y, rect.W, rect.H)
        {
        }

        public bool Contains(Vector2 point)
        {
            return point.x >= X && point.x <= (X + W) && point.y >= Y && point.y <= (Y + H);
        }

        public bool Overlaps(ImRect other)
        {
            var xMax = X + W;
            var yMax = Y + H;

            var otherXMax = other.X + other.W;
            var otherYMax = other.Y + other.H;

            return otherXMax > X &&
                   other.X < xMax &&
                   otherYMax > Y &&
                   other.Y < yMax;
        }

        public ImRect Intersection(ImRect other)
        {
            var x1 = Mathf.Max(X, other.X);
            var y1 = Mathf.Max(Y, other.Y);
            var x2 = Mathf.Min(X + W, other.X + other.W);
            var y2 = Mathf.Min(Y + H, other.Y + other.H);

            return new ImRect(x1, y1, x2 - x1, y2 - y1);
        }

        public override string ToString()
        {
            return $"X:{X} Y:{Y} W:{W} H:{H}";
        }

        public bool Equals(ImRect other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && W.Equals(other.W) && H.Equals(other.H);
        }

        public override bool Equals(object obj)
        {
            return obj is ImRect other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, W, H);
        }

        public static explicit operator Rect(in ImRect rect)
        {
            return new Rect(rect.X, rect.Y, rect.W, rect.H);
        }

        public static explicit operator Vector4(in ImRect rect)
        {
            return new Vector4(rect.X, rect.Y, rect.W, rect.H);
        }

        public static explicit operator ImRect(in Rect rect)
        {
            return new ImRect(rect.x, rect.y, rect.width, rect.height);
        }
        
        public static bool operator ==(ImRect r0, ImRect r1) => r0.Equals(r1);
        public static bool operator !=(ImRect r0, ImRect r1) => !r0.Equals(r1);
    }
}