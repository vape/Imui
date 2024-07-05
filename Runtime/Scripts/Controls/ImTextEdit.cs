using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Imui.Core;
using Imui.IO.Events;
using Imui.Rendering;
using Imui.Controls.Styling;
using UnityEngine;

namespace Imui.Controls
{
    public struct ImTextEditState
    {
        public int Caret;
        public int Selection;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ImTextTempFilterBuffer
    {
        public const int BUFFER_LENGTH = 64;
        
        public fixed char Buffer[BUFFER_LENGTH];
        public byte Length;

        public void Populate(ReadOnlySpan<char> buffer)
        {
            fixed (char* buf = Buffer)
            {
                var span = new Span<char>(buf, BUFFER_LENGTH);
                var len = buffer.Length > BUFFER_LENGTH ? BUFFER_LENGTH : buffer.Length;
                
                buffer[..len].CopyTo(span);
                Length = (byte)len;
            }
        }

        public ReadOnlySpan<char> AsSpan()
        {
            fixed (char* buf = Buffer)
            {
                return new Span<char>(buf, Length);
            }
        }
    }
    
    // TODO (artem-s): text input with dropdown selection
    public static class ImTextEdit
    {
        public const float CARET_BLINKING_TIME = 0.3f;
        public const float MIN_WIDTH = 1;
        public const float MIN_HEIGHT = 1;

        private const string TEMP_BUFFER_TAG = "temp_buffer"; 

        public static ImTextEditStyle Style = ImTextEditStyle.Default;

        public static readonly ImTextEditIntegerFilter IntegerFilter = new();
        public static readonly ImTextEditFloatFilter FloatFilter = new();
        
        public static ImRect GetRect(ImGui gui, ImSize size)
        {
            return size.Type switch
            {
                ImSizeType.FixedSize => gui.Layout.AddRect(size.Width, size.Height),
                _ => gui.Layout.AddRect(
                    Mathf.Max(MIN_WIDTH, gui.GetAvailableWidth()), 
                    Mathf.Max(MIN_HEIGHT, Style.GetControlHeight(gui.GetRowHeight())))
            };
        }
        
        public static void TextEdit(this ImGui gui, ref int value, ImSize size = default, ReadOnlySpan<char> format = default)
        {
            TextEditNumeric(gui, ref value, IntegerFilter, format, size);
        }
        
        public static void TextEdit(this ImGui gui, ref float value, ImSize size = default, ReadOnlySpan<char> format = default)
        {
            TextEditNumeric(gui, ref value, FloatFilter, format, size);
        }

        private static void TextEditNumeric<T>(ImGui gui, ref T value, ImTextEditFilterNumeric<T> filter, ReadOnlySpan<char> format, ImSize size)
        {
            var buffer = new ImTextEditBuffer();
            buffer.MakeMutable();
            
            if (filter.TryFormat(buffer.Buffer, value, out var length, format))
            {
                buffer.Length = length;
            }
            else
            {
                buffer.Insert(0, filter.GetFallbackString());
            }
            
            var rect = GetRect(gui, size);
            var changed = TextEdit(gui, ref buffer, in rect, FloatFilter, multiline: false);
            if (changed && filter.TryParse(buffer, out var newValue))
            {
                value = newValue;
            }
        }
        
        public static void TextEdit(this ImGui gui, ref string text, ImSize size = default, ImTextEditFilter filter = null, bool multiline = true)
        {
            gui.AddControlSpacing();

            var rect = GetRect(gui, size);
            TextEdit(gui, ref text, in rect, filter, multiline);
        }
        
        public static void TextEdit(this ImGui gui, ref string text, in ImRect rect, ImTextEditFilter filter = null, bool multiline = true)
        {
            var id = gui.GetNextControlId();
            ref var state = ref gui.Storage.Get<ImTextEditState>(id);

            TextEdit(gui, id, in rect, ref text, ref state, filter, multiline);
        }
        
        public static bool TextEdit(this ImGui gui, ref ImTextEditBuffer buffer, in ImRect rect, ImTextEditFilter filter = null, bool multiline = true)
        {
            var id = gui.GetNextControlId();
            ref var state = ref gui.Storage.Get<ImTextEditState>(id);

            return TextEdit(gui, id, in rect, ref buffer, ref state, filter, multiline);
        }
        
        public static void TextEdit(this ImGui gui, uint id, in ImRect rect, ref string text, ref ImTextEditState state, ImTextEditFilter filter = null, bool multiline = true)
        {
            var buffer = new ImTextEditBuffer(text);
            var changed = TextEdit(gui, id, in rect, ref buffer, ref state, filter, multiline);
            if (changed)
            {
                text = buffer.GetString();
            }
        }
        
        public static unsafe bool TextEdit(
            ImGui gui, 
            uint id, 
            in ImRect rect, 
            ref ImTextEditBuffer buffer, 
            ref ImTextEditState state,
            ImTextEditFilter filter,
            bool multiline)
        {
            var selected = gui.IsControlActive(id);
            var hovered = gui.IsControlHovered(id);
            var stateStyle = selected ? Style.Selected : Style.Normal;
            var textChanged = false;

            ImTextTempFilterBuffer* tempBuffer = null;

            if (filter != null && !filter.IsValid(buffer))
            {
                var fallbackString = filter.GetFallbackString();
                buffer.Clear(fallbackString.Length);
                buffer.Insert(0, fallbackString);
                textChanged = true;
            }
            
            if (selected && filter != null)
            {
                gui.PushId(id);
                
                var tempBufferId = gui.GetControlId(TEMP_BUFFER_TAG);
                if (!gui.Storage.TryGetRef(tempBufferId, out tempBuffer))
                {
                    tempBuffer = gui.Storage.GetRef<ImTextTempFilterBuffer>(tempBufferId);
                    tempBuffer->Populate(buffer);
                }
                // else relying on collecting garbage on every frame to clean up filter state
                // TODO: if storage gc mechanism is somehow changed - return back here
                
                gui.PopId();
                
                buffer.Clear(tempBuffer->Length);
                buffer.Insert(0, tempBuffer->AsSpan());
            }
            
            gui.Box(in rect, in stateStyle.Box);
            var textRect = Style.GetContentRect(rect);

            var textSize = ImControls.Style.TextSize;
            var layout = gui.TextDrawer.BuildTempLayout(
                buffer, 
                textRect.W, textRect.H, 
                Style.Alignment.X, Style.Alignment.Y, textSize, Style.TextWrap);
            
            gui.Canvas.PushRectMask(rect, stateStyle.Box.BorderRadius);
            gui.Layout.Push(ImAxis.Vertical, textRect, ImLayoutFlag.Root);
            gui.BeginScrollable();
            
            textRect = gui.Layout.AddRect(layout.Width, layout.Height);
            gui.Canvas.Text(buffer, stateStyle.Box.FrontColor, textRect.TopLeft, in layout);

            state.Caret = Mathf.Clamp(state.Caret, 0, buffer.Length);
            
            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when hovered:
                    if (!selected)
                    {
                        gui.SetActiveControl(id, ImControlFlag.Draggable);
                    }
                
                    state.Selection = 0;
                    state.Caret = ViewToCaretPosition(gui.Input.MousePosition, gui.TextDrawer, in textRect, in layout, in buffer);
                    
                    gui.Input.UseMouseEvent();
                    break;
                
                case ImMouseEventType.Drag when selected:
                    var newCaretPosition = ViewToCaretPosition(gui.Input.MousePosition, gui.TextDrawer, in textRect, in layout, in buffer);
                    state.Selection -= newCaretPosition - state.Caret;
                    state.Caret = newCaretPosition;
                    
                    gui.Input.UseMouseEvent();
                    ScrollToCaret(gui, in state, in textRect, in layout, in buffer);
                    break;
                
                case ImMouseEventType.Down when selected && !hovered:
                    gui.ResetActiveControl();
                    break;
            }
            
            if (selected)
            {
                gui.Input.RequestTouchKeyboard(buffer);
                
                DrawCaret(gui, state.Caret, in textRect, in layout, in stateStyle, in buffer);
                DrawSelection(gui, state.Caret, state.Selection, in textRect, in layout, in stateStyle, in buffer);
                
                for (int i = 0; i < gui.Input.KeyboardEventsCount; ++i)
                {
                    ref readonly var keyboardEvent = ref gui.Input.GetKeyboardEvent(i);

                    if (HandleKeyboardEvent(
                            gui, 
                            in keyboardEvent, 
                            ref state, 
                            ref buffer, 
                            in textRect, 
                            in layout, 
                            multiline,
                            out var isTextChanged))
                    {
                        textChanged |= isTextChanged;
                        
                        gui.Input.UseKeyboardEvent(i);
                        ScrollToCaret(gui, in state, in textRect, in layout, in buffer);
                    }
                }
                
                ref readonly var textEvent = ref gui.Input.TextEvent;
                switch (textEvent.Type)
                {
                    case ImTextEventType.Cancel:
                        gui.ResetActiveControl();
                        break;
                    case ImTextEventType.Submit:
                        gui.ResetActiveControl();
                        textChanged = buffer.Length != 0 || textEvent.Text.Length != 0;
                        buffer.Clear(textEvent.Text.Length);
                        Insert(ref state, ref buffer, textEvent.Text);
                        break;
                }
            }
            
            gui.RegisterControl(id, rect);
                        
            gui.EndScrollable(multiline ? ImScrollFlag.None : ImScrollFlag.NoHorizontalBar | ImScrollFlag.NoVerticalBar);
            gui.Layout.Pop();
            gui.Canvas.PopRectMask();

            if (filter != null && !filter.IsValid(buffer))
            {
                textChanged = false;
            }
            
            if (tempBuffer != null)
            {
                tempBuffer->Populate(buffer);
            }
            
            return textChanged;
        }
        
        public static bool HandleKeyboardEvent(ImGui gui, 
            in ImKeyboardEvent evt, 
            ref ImTextEditState state, 
            ref ImTextEditBuffer buffer, 
            in ImRect textRect, 
            in ImTextLayout layout,
            bool multiline,
            out bool textChanged)
        {
            var stateChanged = false;
            
            textChanged = false;
            
            if (evt.Type != ImKeyboardEventType.Down)
            {
                return false;
            }

            switch (evt.Key)
            {
                case KeyCode.LeftArrow:
                    stateChanged |= MoveCaretHorizontal(ref state, in buffer, -1, evt.Command);
                    break;
                
                case KeyCode.RightArrow:
                    stateChanged |= MoveCaretHorizontal(ref state, in buffer, +1, evt.Command);
                    break;
                
                case KeyCode.UpArrow:
                    stateChanged |= MoveCaretVertical(gui, in textRect, in layout, ref state, in buffer, +1, evt.Command);
                    break;
                
                case KeyCode.DownArrow:
                    stateChanged |= MoveCaretVertical(gui, in textRect, in layout, ref state, in buffer, -1, evt.Command);
                    break;
                
                case KeyCode.Delete:
                    textChanged |= DeleteForward(ref state, ref buffer);
                    break;
                
                case KeyCode.Backspace:
                    textChanged |= DeleteBackward(ref state, ref buffer);
                    break;

                default:
                {
                    switch (evt.Command)
                    {
                        case ImKeyboardCommandFlag.SelectAll:
                            state.Selection = -buffer.Length;
                            state.Caret = buffer.Length;
                            stateChanged = true;
                            break;
                        
                        case ImKeyboardCommandFlag.Copy:
                            gui.Input.Clipboard = new string(GetSelectedText(in state, in buffer));
                            stateChanged = true;
                            break;
                        
                        case ImKeyboardCommandFlag.Paste:
                            textChanged |= PasteFromClipboard(gui, ref state, ref buffer);
                            break;

                        default:
                        {
                            if (evt.Char == 0)
                            {
                                break;
                            }

                            // do not allow to add new lines while in single line mode
                            if (!multiline && evt.Char == '\n')
                            {
                                break;
                            }

                            textChanged |= DeleteSelection(ref state, ref buffer);
                            textChanged |= Insert(ref state, ref buffer, evt.Char);
                            break;
                        }
                    }

                    break;
                }
            }
            
            return stateChanged || textChanged;
        }

        public static bool PasteFromClipboard(ImGui gui, ref ImTextEditState state, ref ImTextEditBuffer buffer)
        {
            var clipboardText = gui.Input.Clipboard;
            if (clipboardText.Length == 0)
            {
                return false;
            }

            DeleteSelection(ref state, ref buffer);
            Insert(ref state, ref buffer, clipboardText);

            return true;
        }

        public static unsafe bool Insert(ref ImTextEditState state, ref ImTextEditBuffer buffer, char chr)
        {
            return Insert(ref state, ref buffer, new ReadOnlySpan<char>(&chr, 1));
        }

        public static bool Insert(ref ImTextEditState state, ref ImTextEditBuffer buffer, ReadOnlySpan<char> text)
        {
            if (text.Length == 0)
            {
                return false;
            }
            
            buffer.Insert(state.Caret, in text);
            state.Caret += text.Length;
            return true;
        }
        
        public static bool DeleteSelection(ref ImTextEditState state, ref ImTextEditBuffer buffer)
        {
            if (state.Selection == 0)
            {
                return false;
            }
            
            if (state.Selection < 0)
            {
                state.Caret += state.Selection;
            }
                        
            buffer.Delete(state.Caret, Mathf.Abs(state.Selection));
            state.Selection = 0;
            return true;
        }

        public static bool DeleteBackward(ref ImTextEditState state, ref ImTextEditBuffer buffer)
        {
            if (DeleteSelection(ref state, ref buffer))
            {
                return true;
            }
            
            if (state.Caret > 0)
            {
                buffer.Delete(--state.Caret, 1);
                return true;
            }

            return false;
        }

        public static bool DeleteForward(ref ImTextEditState state, ref ImTextEditBuffer buffer)
        {
            if (DeleteSelection(ref state, ref buffer))
            {
                return true;
            }
            
            if (state.Caret < buffer.Length)
            {
                buffer.Delete(state.Caret, 1);
                return true;
            }

            return false;
        }
        
        public static int FindEndOfWordOrSpacesSequence(int caret, int dir, ReadOnlySpan<char> buffer)
        {
            caret = Mathf.Clamp(caret + dir, 0, buffer.Length);

            var hasVisitedAnySymbol = false;
            var spacesCount = 0;
            
            while (caret > 0 && caret < buffer.Length)
            {
                var chr = buffer[caret];
                
                var isWhiteSpace = char.IsWhiteSpace(chr);
                if (isWhiteSpace)
                {
                    spacesCount++;
                }

                if (char.IsLetterOrDigit(chr) || char.IsSymbol(chr))
                {
                    hasVisitedAnySymbol = true;
                }
                else if (hasVisitedAnySymbol)
                {
                    if (dir < 0)
                    {
                        caret++;
                    }

                    break;
                }
                else if (!isWhiteSpace && spacesCount > 1)
                {
                    if (dir < 0)
                    {
                        caret++;
                    }

                    break;
                }
                
                caret += dir;
            }

            return caret;
        }

        public static bool MoveCaretVertical(
            ImGui gui, 
            in ImRect textRect,
            in ImTextLayout layout,
            ref ImTextEditState state, 
            in ImTextEditBuffer buffer, 
            int dir, 
            ImKeyboardCommandFlag cmd)
        {
            if (cmd.HasFlag(ImKeyboardCommandFlag.NextWord))
            {
                return false;
            }

            var prevCaret = state.Caret;
            var prevSelection = state.Selection;
            var viewPosition = CaretToViewPosition(state.Caret, gui.TextDrawer, in textRect, in layout, in buffer);
            viewPosition.y += (-layout.LineHeight * 0.5f) + (dir * layout.LineHeight);
            state.Caret = ViewToCaretPosition(viewPosition, gui.TextDrawer, in textRect, in layout, in buffer);

            if (cmd.HasFlag(ImKeyboardCommandFlag.Selection))
            {
                state.Selection += prevCaret - state.Caret;
            }
            else
            {
                state.Selection = 0;
            }

            return state.Caret != prevCaret || state.Selection != prevSelection;
        }
        
        public static bool MoveCaretHorizontal(
            ref ImTextEditState state, 
            in ImTextEditBuffer buffer, 
            int dir, 
            ImKeyboardCommandFlag cmd)
        {
            var prevCaret = state.Caret;
            var prevSelection = state.Selection;
            
            if (state.Selection != 0 && !cmd.HasFlag(ImKeyboardCommandFlag.Selection))
            {
                state.Caret = dir < 0
                    ? Mathf.Min(state.Caret + state.Selection, state.Caret)
                    : Mathf.Max(state.Caret + state.Selection, state.Caret);
            }
            else
            {
                state.Caret = Mathf.Max(state.Caret + dir, 0);
            }

            state.Caret = Mathf.Clamp(state.Caret, 0, buffer.Length);

            if (cmd.HasFlag(ImKeyboardCommandFlag.NextWord))
            {
                state.Caret = FindEndOfWordOrSpacesSequence(state.Caret, dir, buffer);
            }

            if (cmd.HasFlag(ImKeyboardCommandFlag.Selection))
            {
                state.Selection += prevCaret - state.Caret;
            }
            else
            {
                state.Selection = 0;
            }

            return state.Caret != prevCaret || state.Selection != prevSelection;
        }

        // TODO: doesn't work when caret is horizontally outside of the scope
        public static void ScrollToCaret(
            ImGui gui, 
            in ImTextEditState state, 
            in ImRect textRect, 
            in ImTextLayout layout, 
            in ImTextEditBuffer buffer)
        {
            var viewPosition = CaretToViewPosition(state.Caret, gui.TextDrawer, in textRect, in layout, in buffer);
            
            ref readonly var frame = ref gui.Layout.GetFrame();
            var scrollOffset = gui.GetScrollOffset();
            var caretTop = viewPosition;
            var caretBottom = viewPosition - new Vector2(0, layout.LineHeight);
            var caretOffset = new Vector2();
            
            if (frame.Bounds.Top < caretTop.y)
            {
                caretOffset.y += frame.Bounds.Top - caretTop.y;
            }
            else if (frame.Bounds.Bottom > caretBottom.y)
            {
                caretOffset.y += frame.Bounds.Bottom - caretBottom.y;
            }
            
            var charWidth = state.Caret >= buffer.Length
                ? 0
                : gui.TextDrawer.GetCharacterWidth(buffer.At(state.Caret), layout.Size);
            var caretLeft = caretTop.x;
            var caretRight = caretTop.x + charWidth;
            
            if (frame.Bounds.Left > caretLeft)
            {
                caretOffset.x += frame.Bounds.Left - caretLeft;
            }
            else if (frame.Bounds.Right < caretRight)
            {
                caretOffset.x += frame.Bounds.Right - caretRight;
            }

            if (caretOffset != default)
            {
                gui.SetScrollOffset(scrollOffset + caretOffset);
            }
        }

        public static ReadOnlySpan<char> GetSelectedText(in ImTextEditState state, in ImTextEditBuffer buffer)
        {
            if (state.Selection == 0)
            {
                return ReadOnlySpan<char>.Empty;
            }
            
            var begin = state.Selection < 0 ? state.Caret + state.Selection : state.Caret;
            var end = state.Selection < 0 ? state.Caret : state.Caret + state.Selection;

            return ((ReadOnlySpan<char>)buffer).Slice(begin, end - begin);
        }

        public static int ViewToCaretPosition(Vector2 position, ImTextDrawer drawer, in ImRect rect, in ImTextLayout layout, in ImTextEditBuffer buffer)
        {
            var origin = rect.TopLeft;
            var line = 0;

            if (position.y > origin.y)
            {
                line = 0;
            }
            else if (position.y < rect.Y)
            {
                line = layout.LinesCount - 1;
            }
            else
            {
                line = (int)(((rect.Y + rect.H - position.y) / layout.Height) * layout.LinesCount);
            }

            if (line < 0)
            {
                return 0;
            }
            
            var caret = layout.Lines[line].Start;
            var px = origin.x + layout.Lines[line].OffsetX;
            if (position.x < px)
            {
                return caret;
            }
            
            var span = ((ReadOnlySpan<char>)buffer);
            if (span.Length < 1)
            {
                return 0;
            }
            
            var start = caret;
            var end = start + layout.Lines[line].Count;
            if (span[end - 1] == '\n')
            {
                end -= 1;
            }
            
            for (int i = start; i < end; ++i)
            {
                var characterWidth = drawer.GetCharacterWidth(span[i], layout.Size);
                
                if (px > position.x || (px + characterWidth) < position.x)
                {
                    px += characterWidth;
                    caret++;
                    continue;
                }

                if ((position.x - px) > (characterWidth / 2.0f))
                {
                    caret++;
                }

                break;
            }

            return caret;
        }
        
        public static Vector2 CaretToViewPosition(int caret, ImTextDrawer drawer, in ImRect rect, in ImTextLayout layout, in ImTextEditBuffer buffer)
        {
            return LineOffsetToViewPosition(FindLineAtCaretPosition(caret, in layout, out var linePosition), linePosition, buffer, in rect, drawer, in layout);
        }

        public static Vector2 LineOffsetToViewPosition(int line, int offset, ReadOnlySpan<char> buffer, in ImRect rect, ImTextDrawer drawer, in ImTextLayout layout)
        {
            var yOffset = line * -layout.LineHeight + layout.OffsetY;
            var xOffset = line >= layout.LinesCount ? layout.OffsetX : layout.Lines[line].OffsetX;

            if (line < layout.LinesCount && offset <= layout.Lines[line].Count)
            {
                ref readonly var lineLayout = ref layout.Lines[line];
                
                var start = lineLayout.Start;
                var end = start + offset;
                var slice = buffer[start..end];
                
                for (int i = 0; i < slice.Length; ++i)
                {
                    xOffset += drawer.GetCharacterWidth(slice[i], layout.Size);
                }
            }

            return rect.TopLeft + new Vector2(xOffset, yOffset);
        }

        public static int FindLineAtCaretPosition(int caret, in ImTextLayout layout, out int linePosition)
        {
            var line = 0;
            while (layout.LinesCount - 1 > line && layout.Lines[line].Count <= caret)
            {
                caret -= layout.Lines[line].Count;
                line++;
            }

            linePosition = caret;
            return line;
        }
        
        public static void DrawCaret(ImGui gui, 
            int position,
            in ImRect textRect, 
            in ImTextLayout layout, 
            in ImTextEditStateStyle style, 
            in ImTextEditBuffer buffer)
        {
            var viewPosition = CaretToViewPosition(position, gui.TextDrawer, in textRect, in layout, in buffer);
            var caretViewRect = new ImRect(
                viewPosition.x, 
                viewPosition.y - layout.LineHeight, 
                Style.CaretWidth,
                layout.LineHeight);

            if ((long)(Time.unscaledTime / CARET_BLINKING_TIME) % 2 == 0)
            {
                gui.Canvas.Rect(caretViewRect, style.Box.FrontColor);
            }
        }

        public static void DrawSelection(ImGui gui, 
            int position,
            int size,
            in ImRect textRect, 
            in ImTextLayout layout, 
            in ImTextEditStateStyle style,
            in ImTextEditBuffer buffer)
        {
            if (size == 0)
            {
                return;
            }

            var begin = size < 0 ? position + size : position;
            var end = size < 0 ? position : position + size;
            
            var beginLine = FindLineAtCaretPosition(begin, in layout, out _);
            var endLine = FindLineAtCaretPosition(end, in layout, out _);
            
            for (int i = beginLine; i <= endLine; ++i)
            {
                ref readonly var line = ref layout.Lines[i];
                
                var lineRelativeBegin = Mathf.Max(0, begin - line.Start);
                var lineRelativeEnd = Mathf.Min(line.Count, end - line.Start);

                var p0 = LineOffsetToViewPosition(i, lineRelativeBegin, buffer, in textRect, gui.TextDrawer, in layout);
                var p1 = LineOffsetToViewPosition(i, lineRelativeEnd, buffer, in textRect, gui.TextDrawer, in layout);
                
                var lineSelectionRect = new ImRect(
                    p0.x, 
                    p0.y - layout.LineHeight, 
                    p1.x - p0.x,
                    layout.LineHeight);
                
                gui.Canvas.Rect(lineSelectionRect, style.SelectionColor);
            }
        }
    }
    
    public ref struct ImTextEditBuffer
    {
        public const int DEFAULT_MUTABLE_BUFFER_CAPACITY = 1024;
        
        private static char[] StaticBuffer = new char[DEFAULT_MUTABLE_BUFFER_CAPACITY];

        public int Length;
        public string InitText;
        public char[] Buffer;

        public ImTextEditBuffer(string text)
        {
            InitText = text ?? string.Empty;
            Buffer = null;
            Length = InitText.Length;
        }

        public char At(int index)
        {
            return Buffer?[index] ?? InitText[index];
        }
        
        public string GetString()
        {
            if (Buffer != null)
            {
                return new string(Buffer, 0, Length);
            }

            return InitText;
        }

        public void MakeMutable(int capacity = DEFAULT_MUTABLE_BUFFER_CAPACITY)
        {
            if (InitText != null)
            {
                var nextLength = Mathf.NextPowerOfTwo(Mathf.Max(InitText.Length, capacity));
                if (nextLength > StaticBuffer.Length)
                {
                    Array.Resize(ref StaticBuffer, nextLength);
                }

                Buffer = StaticBuffer;
                InitText.CopyTo(0, Buffer, 0, InitText.Length);
                Length = InitText.Length;
                InitText = null;
            }
            else
            {
                if (Buffer == null)
                {
                    Buffer = StaticBuffer;
                    Length = 0;
                }
                
                if (Buffer.Length < capacity)
                {
                    Array.Resize(ref Buffer, Mathf.NextPowerOfTwo(capacity));
                }
            }
        }

        public void Clear(int length)
        {
            MakeMutable(length);
            Length = 0;
        }
        
        public void Clear()
        {
            MakeMutable(Length);
            Length = 0;
        }

        public void Delete(int position, int count)
        {
            MakeMutable(Length);
            
            if (position < Length)
            {
                Array.Copy(
                    Buffer, 
                    position + count, 
                    Buffer, 
                    position, 
                    Length - (position + count));
                Array.Fill(Buffer, (char)0, Length - count, count);
                Length -= count;
            }
        }

        public unsafe void Insert(int position, char c)
        {
            Insert(position,  new ReadOnlySpan<char>(&c, 1));
        }

        public void Insert(int position, in ReadOnlySpan<char> text)
        {
            MakeMutable(Length + text.Length);
         
            position = Mathf.Clamp(position, 0, Length);
            if (position < Length)
            {
                Array.Copy(
                    Buffer, 
                    position, 
                    Buffer, 
                    position + text.Length, 
                    Length - position);
            }
            
            text.CopyTo(((Span<char>)Buffer)[position..]);
            Length += text.Length;
        }
        
        public static implicit operator ReadOnlySpan<char>(ImTextEditBuffer buffer) =>
            buffer.InitText ?? new ReadOnlySpan<char>(buffer.Buffer, 0, buffer.Length);
    }
        
    public abstract class ImTextEditFilter
    {
        public abstract bool IsValid(in ReadOnlySpan<char> buffer);
        public abstract string GetFallbackString();
    }

    public abstract class ImTextEditFilterNumeric<T> : ImTextEditFilter
    {
        public abstract bool TryParse(in ReadOnlySpan<char> buffer, out T value);
        public abstract bool TryFormat(in Span<char> buffer, T value, out int length, ReadOnlySpan<char> format);
    }

    public sealed class ImTextEditIntegerFilter : ImTextEditFilterNumeric<int>
    {
        public override bool IsValid(in ReadOnlySpan<char> buffer) => TryParse(in buffer, out _);
        public override string GetFallbackString() => "0";
        public override bool TryParse(in ReadOnlySpan<char> buffer, out int value) => int.TryParse(buffer, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        public override bool TryFormat(in Span<char> buffer, int value, out int length, ReadOnlySpan<char> format) => value.TryFormat(buffer, out length, format);
    }
    
    public sealed class ImTextEditFloatFilter : ImTextEditFilterNumeric<float>
    {
        public override bool IsValid(in ReadOnlySpan<char> buffer) => TryParse(in buffer, out _);
        public override string GetFallbackString() => "0.0";
        public override bool TryParse(in ReadOnlySpan<char> buffer, out float value) => float.TryParse(buffer, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        public override bool TryFormat(in Span<char> buffer, float value, out int length, ReadOnlySpan<char> format) => value.TryFormat(buffer, out length, format);
    }

    public class ImTextEditStyle
    {
        public static readonly ImTextEditStyle Default = new ImTextEditStyle()
        {
            Normal = new ImTextEditStateStyle()
            {
                Box = new ImBoxStyle()
                {
                    BackColor = ImColors.Gray7,
                    FrontColor = ImColors.Black,
                    BorderColor = ImColors.Gray1,
                    BorderRadius = 3.0f,
                    BorderWidth = 1.0f
                },
                SelectionColor = ImColors.Black.WithAlpha(32)
            },
            Selected = new ImTextEditStateStyle()
            {
                Box = new ImBoxStyle()
                {
                    BackColor = ImColors.White,
                    FrontColor = ImColors.Black,
                    BorderColor = ImColors.Black,
                    BorderRadius = 3.0f,
                    BorderWidth = 1.0f
                },
                SelectionColor = ImColors.Black.WithAlpha(64)
            },
            CaretWidth = 2.0f,
            Padding = 2.0f,
            Alignment = new ImTextAlignment(0.0f, 0.0f),
            TextWrap = false
        };
        
        public ImTextEditStateStyle Normal;
        public ImTextEditStateStyle Selected;
        public float CaretWidth;
        public ImPadding Padding;
        public ImTextAlignment Alignment;
        public bool TextWrap;

        public ImRect GetContentRect(ImRect rect)
        {
            return rect.WithPadding(Padding);
        }

        public float GetControlHeight(float contentHeight)
        {
            return contentHeight + Padding.Vertical;
        }

        public ImTextSettings GetTextSettings()
        {
            return new ImTextSettings(ImControls.Style.TextSize, Alignment);
        }
    }

    public class ImTextEditStateStyle
    {
        public ImBoxStyle Box;
        public Color32 SelectionColor;
    }
}