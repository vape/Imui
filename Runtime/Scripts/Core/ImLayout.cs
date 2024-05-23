using System;
using Imui.Utility;
using UnityEngine;

namespace Imui.Core
{
    [Flags]
    public enum ImLayoutFlag
    {
        None              = 0,
        Root              = 1 << 0,
        FixedBoundsWidth  = 1 << 1,
        FixedBoundsHeight = 1 << 2,
        FixedBounds       = FixedBoundsWidth | FixedBoundsHeight
    }
    
    public struct ImLayoutFrame
    {
        public ImAxis Axis;
        public Vector2 Size;
        public ImRect Bounds;
        public Vector2 Offset;
        public ImLayoutFlag Flags;

        public void AddSize(Vector2 size)
        {
            switch (Axis)
            {
                case ImAxis.Vertical:
                    Size.x = Mathf.Max(Size.x, size.x);
                    Size.y += size.y;
                    break;
                case ImAxis.Horizontal:
                    Size.x += size.x;
                    Size.y = Mathf.Max(Size.y, size.y);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    // TODO (artem-s): all APIs must accept axis as first argument
    // TODO (artem-s): check for all unnecessary SetFlags calls
    public class ImLayout
    {
        private const int FRAME_STACK_CAPACITY = 32;
        
        private DynamicArray<ImLayoutFrame> frames = new(FRAME_STACK_CAPACITY);
        
        public void Push(ImAxis axis)
        {
            Push(GetRect(GetAvailableSize()), axis, ImLayoutFlag.None);
        }

        public void Push(ImAxis axis, float width = 0, float height = 0)
        {
            Push(new Vector2(width, height), axis);
        }
        
        public void Push(Vector2 size, ImAxis axis)
        {
            var flags = ImLayoutFlag.FixedBounds;
            
            if (size.x == 0)
            {
                size.x = GetAvailableSize().x;
                flags &= ~ImLayoutFlag.FixedBoundsWidth;
            }

            if (size.y == 0)
            {
                size.y = GetAvailableSize().y;
                flags &= ~ImLayoutFlag.FixedBoundsHeight;
            }
            
            Push(GetRect(size), axis, flags);
        }
        
        public void Push(ImRect rect, ImAxis axis)
        {
            Push(rect, axis, ImLayoutFlag.Root | ImLayoutFlag.FixedBounds);
        }
        
        public void Push(ImRect rect, ImAxis axis, ImLayoutFlag flags)
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
                
                active.AddSize(size);
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
        
        public ref readonly ImLayoutFrame GetFrame()
        {
            return ref frames.Peek();
        }

        public float GetAvailableWidth()
        {
            ref readonly var frame = ref GetFrame();
            return frame.Axis == ImAxis.Horizontal ? frame.Bounds.W - frame.Size.x : frame.Bounds.W;
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

        public ImRect GetRect(float width, float height)
        {
            return GetRect(new Vector2(width, height));
        }
        
        public ImRect GetRect(Vector2 size)
        {
            ref readonly var frame = ref frames.Peek();
            var position = GetNextPosition(in frame, size);
            return new ImRect(position, size);
        }

        public ImRect AddRect(float width, float height)
        {
            return AddRect(new Vector2(width, height));
        }

        public ImRect AddRect(Vector2 size)
        {
            ref var frame = ref frames.Peek();
            var position = GetNextPosition(in frame, size);
            frame.AddSize(size);
            return new ImRect(position, size);
        }

        public ImRect AddRect(ImRect rect)
        {
            ref var frame = ref frames.Peek();
            rect.Encapsulate(GetNextPosition(in frame, Vector2.zero));
            frame.AddSize(rect.Size);
            return rect;
        }

        private static Vector2 GetNextPosition(in ImLayoutFrame frame, Vector2 size)
        {
            var hm = frame.Axis == ImAxis.Horizontal ? 1 : 0;
            var vm = frame.Axis == ImAxis.Vertical ? 1 : 0;
            
            var x = frame.Bounds.X + frame.Offset.x + (frame.Size.x * hm);
            var y = frame.Bounds.Y + frame.Offset.y + frame.Bounds.H + - (frame.Size.y * vm) - size.y;
            
            return new Vector2(x, y);
        }
    }
}