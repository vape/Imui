using System;

namespace Imui.Core
{
    public class ImFormatter
    {
        private ImArena arena;
        
        public ImFormatter(ImArena arena)
        {
            this.arena = arena;
        }

        public Span<char> Join(ReadOnlySpan<char> str, int value)
        {
            var valueSpan = Format(value);
            var span = arena.AllocArray<char>(str.Length + valueSpan.Length);
            var size = 0;
            str.CopyTo(span[size..]);
            size += str.Length;
            valueSpan.CopyTo(span[size..]);
            return span;
        }
        
        public Span<char> Join(ReadOnlySpan<char> str, float value)
        {
            var valueSpan = Format(value);
            var span = arena.AllocArray<char>(str.Length + valueSpan.Length);
            var size = 0;
            str.CopyTo(span[size..]);
            size += str.Length;
            str.CopyTo(span[size..]);
            return span;
        }

        public Span<char> Join(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1)
        {
            var span = arena.AllocArray<char>(str0.Length + str1.Length);
            var size = 0;
            str0.CopyTo(span[size..]); 
            size += str0.Length;
            str1.CopyTo(span[size..]);
            return span;
        }
        
        public Span<char> Join(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2)
        {
            var span = arena.AllocArray<char>(str0.Length + str1.Length + str2.Length);
            var size = 0;
            str0.CopyTo(span[size..]); 
            size += str0.Length;
            str1.CopyTo(span[size..]);
            size += str1.Length;
            str2.CopyTo(span[size..]);
            return span;
        }
        
        public Span<char> Join(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2, ReadOnlySpan<char> str3)
        {
            var span = arena.AllocArray<char>(str0.Length + str1.Length + str2.Length + str3.Length);
            var size = 0;
            str0.CopyTo(span[size..]); 
            size += str0.Length;
            str1.CopyTo(span[size..]);
            size += str1.Length;
            str2.CopyTo(span[size..]);
            size += str2.Length;
            str3.CopyTo(span[size..]);
            return span;
        }
        
        public Span<char> Join(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2, ReadOnlySpan<char> str3, ReadOnlySpan<char> str4)
        {
            var span = arena.AllocArray<char>(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length);
            var size = 0;
            str0.CopyTo(span[size..]); 
            size += str0.Length;
            str1.CopyTo(span[size..]);
            size += str1.Length;
            str2.CopyTo(span[size..]);
            size += str2.Length;
            str3.CopyTo(span[size..]);
            size += str3.Length;
            str4.CopyTo(span[size..]);
            return span;
        }
        
        
        public Span<char> JoinDuplicate(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, int repeat)
        {
            var span = arena.AllocArray<char>(str0.Length + str1.Length * (repeat < 0 ? 0 : repeat));
            var size = 0;
            str0.CopyTo(span[size..]); 
            size += str0.Length;
            for (int i = 0; i < repeat; ++i)
            {
                str1.CopyTo(span[size..]);
                size += str1.Length;
            }
            
            return span;
        }
        
        public Span<char> Format(float value, ReadOnlySpan<char> format = default)
        {
            const int MAX_LEN = 64;

            var span = arena.AllocArray<char>(format.IsEmpty || format.Length < MAX_LEN ? MAX_LEN : format.Length);
            value.TryFormat(span, out var written, format);
            return span[..written];
        }
        
        public Span<char> Format(int value, ReadOnlySpan<char> format = default)
        {
            const int MAX_LEN = 11;

            var span = arena.AllocArray<char>(format.IsEmpty || format.Length < MAX_LEN ? MAX_LEN : format.Length);
            value.TryFormat(span, out var written, format);
            return span[..written];
        }
        
        public Span<char> Format(uint value, ReadOnlySpan<char> format = default)
        {
            const int MAX_LEN = 10;

            var span = arena.AllocArray<char>(format.IsEmpty || format.Length < MAX_LEN ? MAX_LEN : format.Length);
            value.TryFormat(span, out var written, format);
            return span[..written];
        }
    }
}