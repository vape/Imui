using System;
using Imui.Utility;
using UnityEngine;

namespace Imui.Core
{
    public enum ImLayoutType
    {
        Horizontal,
        Vertical
    }

    [Flags]
    public enum ImLayoutFlag
    {
        None = 0,
        Root = 1 << 0
    }
    
    public struct ImLayoutFrame
    {
        public ImLayoutType Type;
        public Vector2 Size;
        public ImRect Bounds;
        public ImLayoutFlag Flags;
        public Vector2 Anchor;

        public void AddSize(Vector2 size)
        {
            switch (Type)
            {
                case ImLayoutType.Vertical:
                    Size.x = Mathf.Max(Size.x, size.x);
                    Size.y += size.y;
                    break;
                case ImLayoutType.Horizontal:
                    Size.x += size.x;
                    Size.y = Mathf.Max(Size.y, size.y);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public static class ImLayoutAnchor
    {
        public static readonly Vector2 TopLeft = new Vector2(0, 1);
        public static readonly Vector2 TopRight = new Vector2(1, 1);
        public static readonly Vector2 BottomLeft = new Vector2(0, 0);
        public static readonly Vector2 BottomRight = new Vector2(1, 0);
    } 

    public class ImLayout
    {
        private const int FRAME_STACK_CAPACITY = 32;
        
        private DynamicArray<ImLayoutFrame> frames = new(FRAME_STACK_CAPACITY);
        
        public void Push(ImLayoutType type)
        {
            Push(Vector2.zero, type);
        }
        
        public void Push(Vector2 size, ImLayoutType type)
        {
            var anchor = Vector2.zero;
            if (frames.Count > 0)
            {
                ref readonly var parent = ref frames.Peek();
                anchor = parent.Anchor;
            }
            
            Push(size, type, anchor);
        }

        public void Push(Vector2 size, ImLayoutType type, Vector2 anchor)
        {
            Push(GetRect(size), type, anchor);
        }

        public void Push(ImRect rect, ImLayoutType type, Vector2 anchor)
        {
            var frame = new ImLayoutFrame
            {
                Type = type,
                Size = default,
                Bounds = rect,
                Flags = ImLayoutFlag.None,
                Anchor = anchor
            };

            frames.Push(in frame);
        }
        
        public void Pop()
        {
            var frame = frames.Pop();
            
            if ((frame.Flags & ImLayoutFlag.Root) == 0 && frames.Count > 0)
            {
                ref var active = ref frames.Peek();
                active.AddSize(frame.Size);
            }
        }

        public void MakeRoot()
        {
            ref var frame = ref frames.Peek();
            frame.Flags |= ImLayoutFlag.Root;
        }

        public ref readonly ImLayoutFrame GetFrame()
        {
            return ref frames.Peek();
        }

        public ImRect GetContentRect()
        {
            ref readonly var frame = ref frames.Peek();
            return new ImRect(GetPosition(frame.Type, frame.Bounds, frame.Anchor, Vector2.zero, frame.Size), frame.Size);
        }
        
        public ImRect GetRect(Vector2 size)
        {
            ref readonly var frame = ref frames.Peek();
            var position = GetPosition(frame.Type, frame.Bounds, frame.Anchor, frame.Size, size);
            return new ImRect(position, size);
        }

        public ImRect AddRect(Vector2 size)
        {
            ref var frame = ref frames.Peek();
            var position = GetPosition(frame.Type, frame.Bounds, frame.Anchor, frame.Size, size);
            frame.AddSize(size);
            return new ImRect(position, size);
        }
        
        private static Vector2 GetPosition(ImLayoutType type, ImRect bounds, Vector2 anchor, Vector2 size, Vector2 rect)
        {
            var ax = anchor.x * 2 - 1f;
            var ay = anchor.y * 2 - 1;
            
            var hm = type == ImLayoutType.Horizontal ? 1 : 0;
            var vm = type == ImLayoutType.Vertical ? 1 : 0;
            
            var x = bounds.X + (size.x * -ax * hm) + (bounds.W * anchor.x) + (rect.x * -anchor.x);
            var y = bounds.Y + (size.y * -ay * vm) + (bounds.H * anchor.y) + (rect.y * -anchor.y);
            
            return new Vector2(x, y);
        }
    }
}