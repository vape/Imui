using System;
using UnityEngine;

namespace Imui.Core
{
    [Serializable]
    public struct ImRect : IEquatable<ImRect>
    {
        public float Top => Y + H;
        public float Bottom => Y;
        public float Right => X + W;
        public float Left => X;
        
        public Vector2 TopLeft => new Vector2(X, Y + H);
        public Vector2 TopRight => new Vector2(X + W, Y + H);
        public Vector2 BottomLeft => Position;
        public Vector2 BottomRight => new Vector2(X + W, Y);

        public Vector2 LeftCenter => new Vector2(X, Y + H / 2.0f);
        public Vector2 RightCenter => new Vector2(X + W, Y + H / 2.0f);
        public Vector2 TopCenter => new Vector2(X + W / 2.0f, Y + H);
        public Vector2 BottomCenter => new Vector2(X + W / 2.0f, Y);
        public Vector2 Center => new Vector2(X + W / 2f, Y + H / 2f);

        public float AspectRatio => W / H;

        public Vector2 Position
        {
            get
            {
                return new Vector2(X, Y);
            }
            set
            {
                X = value.x;
                Y = value.y;
            }
        }

        public Vector2 Size
        {
            get
            {
                return new Vector2(W, H);
            }
            set
            {
                W = value.x;
                H = value.y;
            }
        }

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

        public bool Contains(float x, float y)
        {
            return x >= X && x <= (X + W) && y >= Y && y <= (Y + H);
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

        public void Encapsulate(ImRect other)
        {
            var l = Mathf.Min(other.X, X);
            var b = Mathf.Min(other.Y, Y);
            var t = Mathf.Max(Y + H, other.Y + other.H);
            var r = Mathf.Max(X + W, other.X + other.W);

            X = l;
            Y = b;
            W = r - l;
            H = t - b;
        }

        public void Encapsulate(Vector2 point)
        {
            var l = Mathf.Min(point.x, X);
            var b = Mathf.Min(point.y, Y);
            var t = Mathf.Max(Y + H, point.y);
            var r = Mathf.Max(X + W, point.x);

            X = l;
            Y = b;
            W = r - l;
            H = t - b;
        }

        public Vector2 GetPointAtNormalPosition(float x, float y)
        {
            return new Vector2(Mathf.LerpUnclamped(X, X + W, x), Mathf.LerpUnclamped(Y, Y + H, y));
        }
        
        public Vector2 GetPointAtNormalPosition(Vector2 point)
        {
            return new Vector2(Mathf.LerpUnclamped(X, X + W, point.x), Mathf.LerpUnclamped(Y, Y + H, point.y));
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

        public static explicit operator Rect(ImRect rect)
        {
            return new Rect(rect.X, rect.Y, rect.W, rect.H);
        }

        public static explicit operator Vector4(ImRect rect)
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

    [Serializable]
    public struct ImRectRadius
    {
        public float TopLeft;
        public float TopRight;
        public float BottomRight;
        public float BottomLeft;

        public ImRectRadius(float topLeft = 0, float topRight = 0, float bottomRight = 0, float bottomLeft = 0)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
        }
        
        public ImRectRadius(float radius)
        {
            TopLeft = radius;
            TopRight = radius;
            BottomRight = radius;
            BottomLeft = radius;
        }

        public void Clamp(float size)
        {
            TopLeft = Mathf.Min(size, TopLeft);
            TopRight = Mathf.Min(size, TopRight);
            BottomRight = Mathf.Min(size, BottomRight);
            BottomLeft = Mathf.Min(size, BottomLeft);
        }

        public float RadiusForMask()
        {
            var max = TopLeft;
            max = TopRight > max ? TopRight : max;
            max = BottomRight > max ? BottomRight : max;
            max = BottomLeft > max ? BottomLeft : max;
            return max;
        }

        public static implicit operator ImRectRadius(float radius)
        {
            return new ImRectRadius(radius);
        }

        public static ImRectRadius operator -(ImRectRadius radius, float delta)
        {
            radius.BottomLeft -= delta;
            radius.BottomRight -= delta;
            radius.TopLeft -= delta;
            radius.TopRight -= delta;
            
            return radius;
        }
    }
}