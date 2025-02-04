using System;
using Imui.Utility;
using UnityEngine;

namespace Imui.Core
{
    [Flags]
    public enum ImLayoutFlag
    {
        None = 0,
        Root = 1 << 0,
        FixedBoundsWidth = 1 << 1,
        FixedBoundsHeight = 1 << 2,
        FixedBounds = FixedBoundsWidth | FixedBoundsHeight
    }

    public struct ImLayoutFrame
    {
        public ImAxis Axis;
        public Vector2 Size;
        public ImRect Bounds;
        public Vector2 Offset;
        public float Indent;
        public ImLayoutFlag Flags;

        public void Append(Vector2 size)
        {
            Append(size.x, size.y);
        }

        public void Append(float width, float height)
        {
            switch (Axis)
            {
                case ImAxis.Vertical:
                    Size.x = Mathf.Max(Size.x, width + Indent);
                    Size.y += height;
                    break;
                case ImAxis.Horizontal:
                    Size.x += width;
                    Size.y = Mathf.Max(Size.y, height);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void Append(float size)
        {
            switch (Axis)
            {
                case ImAxis.Vertical:
                    Size.y += size;
                    break;
                case ImAxis.Horizontal:
                    Size.x += size;
                    break;
            }
        }
    }

    public class ImLayout
    {
        private const int FRAME_STACK_CAPACITY = 32;

        public ImAxis Axis
        {
            get
            {
                ref readonly var frame = ref GetFrame();
                return frame.Axis;
            }
        }

        public int Depth => frames.Count;

        private ImDynamicArray<ImLayoutFrame> frames = new(FRAME_STACK_CAPACITY);

        public void Push(ImAxis axis, float width = 0, float height = 0)
        {
            Push(axis, new Vector2(width, height));
        }

        public void Push(ImAxis axis, Vector2 size)
        {
            var flags = ImLayoutFlag.FixedBounds;

            if (size.x == 0)
            {
                size.x = GetAvailableWidth();
                flags &= ~ImLayoutFlag.FixedBoundsWidth;
            }

            if (size.y == 0)
            {
                size.y = GetAvailableHeight();
                flags &= ~ImLayoutFlag.FixedBoundsHeight;
            }

            Push(axis, GetRect(size), flags);
        }

        public void Push(ImAxis axis, ImRect rect)
        {
            Push(axis, rect, ImLayoutFlag.Root | ImLayoutFlag.FixedBounds);
        }

        public void Push(ImAxis axis, ImRect rect, ImLayoutFlag flags)
        {
            var frame = new ImLayoutFrame
            {
                Axis = axis,
                Size = default,
                Bounds = rect,
                Flags = flags
            };

            frames.Push(in frame);
        }

        public void Pop() => Pop(out _);

        public void Pop(out ImLayoutFrame frame)
        {
            frame = frames.Pop();

            if ((frame.Flags & ImLayoutFlag.Root) == 0 && frames.Count > 0)
            {
                ref var active = ref frames.Peek();

                var size = frame.Size;
                if ((frame.Flags & ImLayoutFlag.FixedBoundsWidth) != 0)
                {
                    size.x = frame.Bounds.W;
                }

                if ((frame.Flags & ImLayoutFlag.FixedBoundsHeight) != 0)
                {
                    size.y = frame.Bounds.H;
                }

                active.Append(size);
            }
        }

        public void SetFlags(ImLayoutFlag flag)
        {
            ref var frame = ref frames.Peek();
            frame.Flags |= flag;
        }

        public void SetOffset(Vector2 offset)
        {
            ref var frame = ref frames.Peek();
            frame.Offset = offset;
        }

        public Vector2 GetNextPosition(float height = 0.0f)
        {
            if (frames.Count == 0)
            {
                return default;
            }

            ref readonly var frame = ref frames.Peek();

            return GetNextPosition(in frame, height);
        }

        public ref readonly ImLayoutFrame GetFrame()
        {
            return ref frames.Peek();
        }

        public ref readonly ImLayoutFrame GetParentFrame(int layers = 1)
        {
            return ref frames.Array[frames.Count - 1 - layers];
        }

        public float GetAvailableWidth()
        {
            ref readonly var frame = ref GetFrame();
            return frame.Axis == ImAxis.Horizontal ? frame.Bounds.W - frame.Size.x : frame.Bounds.W - frame.Indent;
        }

        public float GetAvailableHeight()
        {
            ref readonly var frame = ref GetFrame();
            return frame.Axis == ImAxis.Vertical ? frame.Bounds.H - frame.Size.y : frame.Bounds.H;
        }

        public Vector2 GetAvailableSize()
        {
            ref readonly var frame = ref GetFrame();
            var w = frame.Bounds.W;
            var h = frame.Bounds.H;

            switch (frame.Axis)
            {
                case ImAxis.Vertical:
                    h -= frame.Size.y;
                    w -= frame.Indent;
                    break;
                case ImAxis.Horizontal:
                    w -= frame.Size.x;
                    break;
                default:
                    throw new NotImplementedException();
            }

            return new Vector2(w, h);
        }

        public ImRect GetWholeRect()
        {
            var rect = GetBoundsRect();
            rect.Encapsulate(GetContentRect());

            return rect;
        }

        public ImRect GetBoundsRect()
        {
            ref readonly var frame = ref frames.Peek();
            return frame.Bounds;
        }

        public ImRect GetContentRect()
        {
            ref readonly var frame = ref frames.Peek();
            var x = frame.Bounds.X;
            var y = frame.Bounds.Y + frame.Bounds.H - frame.Size.y;
            var w = frame.Size.x;
            var h = frame.Size.y;
            return new ImRect(x, y, w, h);
        }

        public ImRect GetRect(Vector2 size)
        {
            return GetRect(size.x, size.y);
        }

        public ImRect GetRect(float width, float height)
        {
            ref readonly var frame = ref frames.Peek();
            var position = GetNextPosition(in frame, height);
            return new ImRect(position.x, position.y, width, height);
        }

        public ImRect AddRect(Vector2 size)
        {
            return AddRect(size.x, size.y);
        }

        public ImRect AddRect(float width, float height)
        {
            ref var frame = ref frames.Peek();
            var position = GetNextPosition(in frame, height);
            frame.Append(width, height);
            return new ImRect(position.x, position.y, width, height);
        }

        public ImRect AddRect(ImRect rect)
        {
            ref var frame = ref frames.Peek();
            rect.Encapsulate(GetNextPosition(in frame, 0));
            frame.Append(rect.Size);
            return rect;
        }

        public void AddSpace(float space)
        {
            ref var frame = ref frames.Peek();
            frame.Append(space);
        }

        public void AddIndent(float space)
        {
            ref var frame = ref frames.Peek();
            frame.Indent += space;
        }

        public static Vector2 GetNextPosition(in ImLayoutFrame frame, float height)
        {
            var hm = frame.Axis == ImAxis.Horizontal ? 1 : 0;
            var vm = frame.Axis == ImAxis.Vertical ? 1 : 0;

            var x = frame.Bounds.X + frame.Offset.x + (frame.Size.x * hm) + (frame.Indent * vm);
            var y = frame.Bounds.Y + frame.Offset.y + frame.Bounds.H + -(frame.Size.y * vm) - height;

            return new Vector2(x, y);
        }
    }
}