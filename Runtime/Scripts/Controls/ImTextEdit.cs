using System;
using System.Globalization;
using Imui.Core;
using Imui.Rendering;
using Imui.Styling;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public abstract class ImTextEditFilter
    {
        public abstract bool IsValid(in ReadOnlySpan<char> buffer);
    }

    public sealed class ImTextEditIntegerFilter : ImTextEditFilter
    {
        public override bool IsValid(in ReadOnlySpan<char> buffer)
        {
            return long.TryParse(buffer, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
        }
    }
    
    public sealed class ImTextEditFloatFilter : ImTextEditFilter
    {
        public override bool IsValid(in ReadOnlySpan<char> buffer)
        {
            return double.TryParse(buffer, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
        }
    }
    
    public static class ImTextEdit
    {
        private const float CARET_BLINKING_TIME = 0.3f;

        public static ImTextEditStyle Style = ImTextEditStyle.Default;

        public static readonly ImTextEditIntegerFilter IntegerFilter = new();
        public static readonly ImTextEditFloatFilter FloatFilter = new();

        public static void TextEdit(this ImGui gui, ref string text, ImTextEditFilter filter = null)
        {
            var size = Style.TextSettings.Size;
            var width = Mathf.Max(size, gui.Layout.GetFreeSpace().x);
            TextEdit(gui, width, ref text, filter);
        }
        
        public static void TextEdit(this ImGui gui, float width, ref string text, ImTextEditFilter filter = null)
        {
            var height = gui.TextDrawer.GetLineHeight(Style.TextSettings.Size);
            var size = new Vector2(width, height + (Style.FrameWidth + Style.Padding) * 2 + 0.1f);
            TextEdit(gui, in size, ref text, filter, false);
        }
        
        public static void TextEdit(this ImGui gui, in Vector2 size, ref string text, ImTextEditFilter filter = null, bool multiline = true)
        {
            var rect = gui.Layout.AddRect(size);
            TextEdit(gui, in rect, ref text, filter, multiline);
        }
        
        public static void TextEdit(this ImGui gui, in ImRect rect, ref string text, ImTextEditFilter filter = null, bool multiline = true)
        {
            var id = gui.GetNextControlId();
            ref var state = ref gui.Storage.Get<ImTextEditState>(id);

            TextEdit(gui, id, in rect, ref text, ref state, filter, multiline);
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
        
        private static bool TextEdit(
            ImGui gui, 
            uint id, 
            in ImRect rect, 
            ref ImTextEditBuffer buffer, 
            ref ImTextEditState state,
            ImTextEditFilter filter,
            bool multiline)
        {
            var selected = gui.ActiveControl == id;
            var hovered = gui.GetHoveredControl() == id;
            var stateStyle = selected ? Style.Selected : Style.Normal;
            var textChanged = false;
            
            DrawBack(gui, in stateStyle, in rect, out var textRect);
            
            var layout = gui.TextDrawer.BuildTempLayout(
                buffer, 
                textRect.W, textRect.H, 
                Style.TextSettings.AlignX, Style.TextSettings.AlignY, Style.TextSettings.Size);
            
            gui.Canvas.PushRectMask(rect, Style.CornerRadius);
            gui.Layout.Push(textRect, ImAxis.Vertical, ImLayoutFlag.Root);
            gui.BeginScrollable();
            
            textRect = gui.Layout.AddRect(layout.Width, layout.Height);
            gui.Canvas.Text(buffer, stateStyle.FrontColor, textRect.TopLeft, in layout);

            state.Caret = Mathf.Clamp(state.Caret, 0, buffer.Length);
            
            ref readonly var mouseEvent = ref gui.Input.MouseEvent;
            switch (mouseEvent.Type)
            {
                case ImInputMouseEventType.Down when hovered:
                {
                    if (!selected)
                    {
                        gui.ActiveControl = id;
                    }
                
                    state.Selection = 0;
                    state.Caret = ViewToCaretPosition(gui.Input.MousePosition, gui.TextDrawer, in textRect, in layout, in buffer);
                    
                    gui.Input.UseMouseEvent();
                    break;
                }
                case ImInputMouseEventType.Drag when selected:
                {
                    var newCaretPosition = ViewToCaretPosition(gui.Input.MousePosition, gui.TextDrawer, in textRect, in layout, in buffer);
                    state.Selection -= newCaretPosition - state.Caret;
                    state.Caret = newCaretPosition;
                    
                    gui.Input.UseMouseEvent();
                    ScrollToCaret(gui, in state, in textRect, in layout, in buffer);
                    break;
                }
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
                            filter, 
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
                    case ImInputTextEventType.Cancel:
                        gui.ActiveControl = 0;
                        break;
                    case ImInputTextEventType.Submit:
                        gui.ActiveControl = 0;
                        buffer = new ImTextEditBuffer(textEvent.Text);
                        textChanged = true;
                        break;
                }
            }
            
            gui.HandleControl(id, rect);
                        
            gui.EndScrollable(multiline ? ImScrollFlag.None : ImScrollFlag.NoHorizontalBar | ImScrollFlag.NoVerticalBar);
            gui.Layout.Pop();
            gui.Canvas.PopRectMask();
            
            return textChanged;
        }
        
        private static bool HandleKeyboardEvent(ImGui gui, 
            in ImInputKeyboardEvent evt, 
            ref ImTextEditState state, 
            ref ImTextEditBuffer buffer, 
            in ImRect textRect, 
            in TextDrawer.Layout layout,
            ImTextEditFilter filter,
            bool multiline,
            out bool textChanged)
        {
            var stateChanged = false;
            
            textChanged = false;
            
            if (evt.Type != ImInputKeyboardEventType.Down)
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
                        case ImInputKeyboardCommandFlag.SelectAll:
                            state.Selection = -buffer.Length;
                            state.Caret = buffer.Length;
                            stateChanged = true;
                            break;
                        
                        case ImInputKeyboardCommandFlag.Copy:
                            gui.Input.Clipboard = new string(GetSelectedText(in state, in buffer));
                            stateChanged = true;
                            break;
                        
                        case ImInputKeyboardCommandFlag.Paste:
                            textChanged |= PasteFromClipboard(gui, ref state, ref buffer, filter);
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
                            textChanged |= TryInsert(ref state, ref buffer, evt.Char, filter);
                            break;
                        }
                    }

                    break;
                }
            }
            
            return stateChanged || textChanged;
        }

        private static bool PasteFromClipboard(ImGui gui, ref ImTextEditState state, ref ImTextEditBuffer buffer, ImTextEditFilter filter)
        {
            var clipboardText = gui.Input.Clipboard;
            if (clipboardText.Length == 0)
            {
                return false;
            }

            var textChanged = DeleteSelection(ref state, ref buffer);
            if (TryInsert(ref state, ref buffer, clipboardText, filter))
            {
                textChanged = true;
                state.Caret += clipboardText.Length;
            }

            return textChanged;
        }

        private static unsafe bool TryInsert(ref ImTextEditState state, ref ImTextEditBuffer buffer, char chr, ImTextEditFilter filter)
        {
            return TryInsert(ref state, ref buffer, new ReadOnlySpan<char>(&chr, 1), filter);
        }

        private static bool TryInsert(ref ImTextEditState state, ref ImTextEditBuffer buffer, ReadOnlySpan<char> text, ImTextEditFilter filter)
        {
            const int MAX_STACK_ALLOC_SIZE_IN_BYTES = 2048;
            
            if (filter == null)
            {
                buffer.Insert(state.Caret, in text);
                state.Caret += text.Length;
                return true;
            }

            var length = buffer.Length + text.Length;
            var tempBuffer = length > (MAX_STACK_ALLOC_SIZE_IN_BYTES / sizeof(char)) ? new char[length] : stackalloc char[length];

            ((ReadOnlySpan<char>)buffer).CopyTo(tempBuffer);
            if (state.Caret < buffer.Length)
            {
                tempBuffer[state.Caret..].CopyTo(tempBuffer[(state.Caret + text.Length)..]);
            }
            
            text.CopyTo((tempBuffer)[state.Caret..]);

            if (!filter.IsValid(tempBuffer))
            {
                return false;
            }
            
            buffer.Insert(state.Caret, text);
            state.Caret += text.Length;
            return true;
        }
        
        private static bool DeleteSelection(ref ImTextEditState state, ref ImTextEditBuffer buffer)
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

        private static bool DeleteBackward(ref ImTextEditState state, ref ImTextEditBuffer buffer)
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

        private static bool DeleteForward(ref ImTextEditState state, ref ImTextEditBuffer buffer)
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
        
        private static int FindEndOfWordOrSpacesSequence(int caret, int dir, ReadOnlySpan<char> buffer)
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

        private static bool MoveCaretVertical(
            ImGui gui, 
            in ImRect textRect,
            in TextDrawer.Layout layout,
            ref ImTextEditState state, 
            in ImTextEditBuffer buffer, 
            int dir, 
            ImInputKeyboardCommandFlag cmd)
        {
            if (cmd.HasFlag(ImInputKeyboardCommandFlag.NextWord))
            {
                return false;
            }

            var prevCaret = state.Caret;
            var prevSelection = state.Selection;
            var viewPosition = CaretToViewPosition(state.Caret, gui.TextDrawer, in textRect, in layout, in buffer);
            viewPosition.y += (-layout.LineHeight * 0.5f) + (dir * layout.LineHeight);
            state.Caret = ViewToCaretPosition(viewPosition, gui.TextDrawer, in textRect, in layout, in buffer);

            if (cmd.HasFlag(ImInputKeyboardCommandFlag.Selection))
            {
                state.Selection += prevCaret - state.Caret;
            }
            else
            {
                state.Selection = 0;
            }

            return state.Caret != prevCaret || state.Selection != prevSelection;
        }
        
        private static bool MoveCaretHorizontal(
            ref ImTextEditState state, 
            in ImTextEditBuffer buffer, 
            int dir, 
            ImInputKeyboardCommandFlag cmd)
        {
            var prevCaret = state.Caret;
            var prevSelection = state.Selection;
            
            if (state.Selection != 0 && !cmd.HasFlag(ImInputKeyboardCommandFlag.Selection))
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

            if (cmd.HasFlag(ImInputKeyboardCommandFlag.NextWord))
            {
                state.Caret = FindEndOfWordOrSpacesSequence(state.Caret, dir, buffer);
            }

            if (cmd.HasFlag(ImInputKeyboardCommandFlag.Selection))
            {
                state.Selection += prevCaret - state.Caret;
            }
            else
            {
                state.Selection = 0;
            }

            return state.Caret != prevCaret || state.Selection != prevSelection;
        }

        private static void ScrollToCaret(
            ImGui gui, 
            in ImTextEditState state, 
            in ImRect textRect, 
            in TextDrawer.Layout layout, 
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

        private static ReadOnlySpan<char> GetSelectedText(in ImTextEditState state, in ImTextEditBuffer buffer)
        {
            if (state.Selection == 0)
            {
                return ReadOnlySpan<char>.Empty;
            }
            
            var begin = state.Selection < 0 ? state.Caret + state.Selection : state.Caret;
            var end = state.Selection < 0 ? state.Caret : state.Caret + state.Selection;

            return ((ReadOnlySpan<char>)buffer).Slice(begin, end - begin);
        }

        private static int ViewToCaretPosition(Vector2 position, TextDrawer drawer, in ImRect rect, in TextDrawer.Layout layout, in ImTextEditBuffer buffer)
        {
            var origin = rect.TopLeft;
            var py = origin.y + layout.OffsetY;
            var line = 0;

            if (position.y > origin.y)
            {
                return 0;
            }

            // TODO (artem-s): can't we just do something like 'position / layout.Height * layout.LinesCount'
            while (line <= layout.LinesCount + 1 && (py < position.y || (py - layout.LineHeight) > position.y))
            {
                py -= layout.LineHeight;
                line++;
            }

            if (line >= layout.LinesCount)
            {
                return buffer.Length;
            }

            var caret = layout.Lines[line].Start;
            var px = origin.x + layout.Lines[line].OffsetX;
            var span = ((ReadOnlySpan<char>)buffer);
            
            var start = caret;
            var end = start + layout.Lines[line].Count;
            if (span[end - 1] == '\n')
            {
                end -= 1;
            }
            
            for (int i = start; i < end; ++i)
            {
                var width = drawer.GetCharacterWidth(span[i], layout.Size);
                if (px > position.x || (px + width) < position.x)
                {
                    px += width;
                    caret++;
                    continue;
                }

                if ((position.x - px) > (width / 2.0f))
                {
                    caret++;
                }

                break;
            }

            return caret;
        }
        
        private static Vector2 CaretToViewPosition(int caret, TextDrawer drawer, in ImRect rect, in TextDrawer.Layout layout, in ImTextEditBuffer buffer)
        {
            return LineOffsetToViewPosition(FindLineAtCaretPosition(caret, in layout, out var linePosition), linePosition, buffer, in rect, drawer, in layout);
        }

        private static Vector2 LineOffsetToViewPosition(int line, int offset, ReadOnlySpan<char> buffer, in ImRect rect, TextDrawer drawer, in TextDrawer.Layout layout)
        {
            var yOffset = line * -layout.LineHeight + layout.OffsetY;
            var xOffset = line >= layout.LinesCount ? layout.OffsetX : layout.Lines[line].OffsetX;
            var totalLen = buffer.Length;
            
            if (line < layout.LinesCount && offset <= layout.Lines[line].Count && offset <= totalLen)
            {
                ref readonly var lineLayout = ref layout.Lines[line];
                
                var count = Mathf.Min(totalLen, layout.Lines[line].Count);
                var start = lineLayout.Start;
                var end = start + count;
                var slice = buffer[start..end];
                
                for (int i = 0; i < offset && i < slice.Length; ++i)
                {
                    xOffset += drawer.GetCharacterWidth(slice[i], layout.Size);
                }
            }

            return rect.TopLeft + new Vector2(xOffset, yOffset);
        }

        private static int FindLineAtCaretPosition(int caret, in TextDrawer.Layout layout, out int linePosition)
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
        
        private static void DrawCaret(ImGui gui, 
            int position,
            in ImRect textRect, 
            in TextDrawer.Layout layout, 
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
                gui.Canvas.Rect(caretViewRect, style.FrontColor);
            }
        }

        private static void DrawSelection(ImGui gui, 
            int position,
            int size,
            in ImRect textRect, 
            in TextDrawer.Layout layout, 
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
        
        private static void DrawBack(ImGui gui, in ImTextEditStateStyle style, in ImRect rect, out ImRect content)
        {
            gui.Canvas.Rect(rect, style.BackColor, Style.CornerRadius);
            gui.Canvas.RectOutline(rect, style.FrameColor, Style.FrameWidth, Style.CornerRadius);
            
            content = rect.WithPadding(Style.FrameWidth + Style.Padding);
        }
    }

    public struct ImTextEditState
    {
        public int Caret;
        public int Selection;
    }

    public class ImTextEditStyle
    {
        public static readonly ImTextEditStyle Default = new ImTextEditStyle()
        {
            Normal = new ImTextEditStateStyle()
            {
                BackColor = ImColors.Gray7,
                FrontColor = ImColors.Black,
                FrameColor = ImColors.Gray1,
                SelectionColor = ImColors.Black.WithAlpha(32)
            },
            Selected = new ImTextEditStateStyle()
            {
                BackColor = ImColors.White,
                FrontColor = ImColors.Black,
                FrameColor = ImColors.Black,
                SelectionColor = ImColors.Black.WithAlpha(64)
            },
            CornerRadius = 3.0f,
            FrameWidth = 1.0f,
            CaretWidth = 2.0f,
            Padding = 1.0f,
            TextSettings = new ImTextSettings()
            {
                AlignX = 0,
                AlignY = 0,
                Size = ImText.DEFAULT_TEXT_SIZE
            }
        };
        
        public ImTextEditStateStyle Normal;
        public ImTextEditStateStyle Selected;
        public float FrameWidth;
        public float CornerRadius;
        public float CaretWidth;
        public float Padding;
        public ImTextSettings TextSettings;
    }

    public class ImTextEditStateStyle
    {
        public Color32 BackColor;
        public Color32 FrontColor;
        public Color32 FrameColor;
        public Color32 SelectionColor;
    }

    public ref struct ImTextEditBuffer
    {
        private static char[] StaticBuffer = new char[1024];

        public int Length;
        public string InitText;
        public char[] Buffer;

        public ImTextEditBuffer(string text)
        {
            InitText = text;
            Buffer = null;
            Length = text.Length;
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

        public void MakeMutable(int length)
        {
            if (InitText != null)
            {
                var nextLength = Mathf.NextPowerOfTwo(Mathf.Max(InitText.Length, length));
                if (nextLength > StaticBuffer.Length)
                {
                    Array.Resize(ref StaticBuffer, nextLength);
                }

                Buffer = StaticBuffer;
                InitText.CopyTo(0, Buffer, 0, InitText.Length);
                Length = InitText.Length;
                InitText = null;
            }
            else if (Buffer.Length < length)
            {
                Array.Resize(ref Buffer, Mathf.NextPowerOfTwo(length));
            }
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
}