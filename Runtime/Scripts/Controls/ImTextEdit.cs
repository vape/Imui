using System;
using Imui.Core;
using Imui.Rendering;
using Imui.Styling;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    // TODO (artem-s): implement some filtering API for numeric only input fields
    // TODO (artem-s): refactoring
    public static class ImTextEdit
    {
        public static ImTextEditStyle Style = ImTextEditStyle.Default;

        public static void TextEdit(this ImGui gui, in Vector2 size, ref string text)
        {
            var rect = gui.Layout.AddRect(size);
            TextEdit(gui, in rect, ref text);
        }
        
        public static void TextEdit(this ImGui gui, in ImRect rect, ref string text)
        {
            var id = gui.GetNextControlId();
            ref var state = ref gui.Storage.Get<ImTextEditState>(id);

            TextEdit(gui, id, in rect, ref text, ref state);
        }
        
        public static void TextEdit(this ImGui gui, uint id, in ImRect rect, ref string text, ref ImTextEditState state)
        {
            var buffer = new ImTextEditBuffer(text);
            var changed = TextEdit(gui, id, in rect, ref buffer, ref state);
            if (changed)
            {
                text = buffer.GetString();
            }
        }
        
        private static bool TextEdit(this ImGui gui, uint id, in ImRect rect, ref ImTextEditBuffer text, ref ImTextEditState state)
        {
            var selected = gui.ActiveControl == id;
            var hovered = gui.GetHoveredControl() == id;
            
            var stateStyle = selected ? Style.Selected : Style.Normal;
            DrawBack(gui, in stateStyle, in rect, out var contentRect);
            
            var layout = gui.TextDrawer.BuildTempLayout(
                text, 
                contentRect.W, contentRect.H, 
                Style.TextSettings.AlignX, Style.TextSettings.AlignY, Style.TextSettings.Size);
            
            gui.Canvas.PushRectMask(rect, Style.CornerRadius);
            
            gui.Layout.Push(contentRect, ImAxis.Vertical);
            gui.Layout.SetFlags(ImLayoutFlag.Root);
            gui.BeginScrollable();
            
            contentRect = gui.Layout.AddRect(layout.Width, layout.Height);
            gui.Canvas.Text(text, stateStyle.FrontColor, contentRect.TopLeft, in layout);
            gui.HandleControl(id, rect);
            
            selected = gui.ActiveControl == id;
            
            ref readonly var mouseEvent = ref gui.Input.MouseEvent;
            if (mouseEvent.Type == ImInputMouseEventType.Down && hovered)
            {
                if (!selected)
                {
                    gui.ActiveControl = id;
                }
                
                state.Caret = ViewToCaretPosition(gui.Input.MousePosition, gui.TextDrawer, in contentRect, in layout, in text);
                state.Selection = 0;

                gui.Input.UseMouseEvent();
            }
            else if (mouseEvent.Type == ImInputMouseEventType.Drag && selected)
            {
                var underMousePosition = ViewToCaretPosition(gui.Input.MousePosition, gui.TextDrawer, in contentRect, in layout, in text);
                var delta = underMousePosition - state.Caret;
                state.Selection -= delta;
                state.Caret = underMousePosition;
                
                gui.Input.UseMouseEvent();
            }
            
            state.Caret = Mathf.Clamp(state.Caret, 0, text.Length);

            var changed = false;
            if (selected)
            {
                var caretViewPosition = CaretToViewPosition(state.Caret, gui.TextDrawer, in contentRect, in layout, in text);
                DrawCaret(gui, in layout, in stateStyle, caretViewPosition);
                DrawSelection(gui, in state, in gui.TextDrawer, in contentRect, in text, in layout, in stateStyle);
                
                for (int i = 0; i < gui.Input.KeyboardEventsCount; ++i)
                {
                    ref readonly var evt = ref gui.Input.GetKeyboardEvent(i);

                    if (HandleKeyboard(gui, in evt, ref state, ref text, in gui.TextDrawer, in contentRect, in layout, out changed))
                    {
                        gui.Input.UseKeyboardEvent(i);
                    }
                }
                
                ref readonly var textEvt = ref gui.Input.TextEvent;
                switch (textEvt.Type)
                {
                    case ImInputTextEventType.Cancel:
                        gui.ActiveControl = 0;
                        break;
                    case ImInputTextEventType.Submit:
                        gui.ActiveControl = 0;
                        text = new ImTextEditBuffer(textEvt.Text);
                        changed = true;
                        break;
                }

                ref readonly var layoutFrame = ref gui.Layout.GetFrame();
                
                var scrollOffset = gui.GetScrollOffset();
                var caretViewRect = GetCaretRect(caretViewPosition, layout.LineHeight);
                if (layoutFrame.Bounds.Top < caretViewRect.Top)
                {
                    gui.SetScrollOffset(scrollOffset + new Vector2(0, layoutFrame.Bounds.Top - caretViewRect.Top));
                }
                else if (layoutFrame.Bounds.Bottom > caretViewRect.Bottom)
                {
                    gui.SetScrollOffset(scrollOffset + new Vector2(0, layoutFrame.Bounds.Bottom - caretViewRect.Bottom));
                }
            }
                        
            gui.EndScrollable();
            gui.Layout.Pop();

            gui.Canvas.PopRectMask();

            selected = gui.ActiveControl == id;
            if (selected)
            {
                gui.Input.RequestTouchKeyboard(text);
            }
            
            return changed;
        }

        private static bool HandleKeyboard(ImGui gui, 
            in ImInputKeyboardEvent evt, 
            ref ImTextEditState state, 
            ref ImTextEditBuffer buffer, 
            in TextDrawer drawer, 
            in ImRect rect, 
            in TextDrawer.Layout layout,
            out bool changed)
        {
            changed = false;
            
            if (evt.Type != ImInputKeyboardEventType.Down)
            {
                return false;
            }

            var previousCaret = state.Caret;

            switch (evt.Key)
            {
                case KeyCode.LeftArrow:
                    if (state.Selection != 0 && !evt.Command.HasFlag(ImInputKeyboardCommandFlag.Selection))
                    {
                        state.Caret = Mathf.Max(Mathf.Min(state.Caret + state.Selection, state.Caret), 0);
                    }
                    else
                    {
                        state.Caret = Mathf.Max(state.Caret - 1, 0);
                    }

                    if (evt.Command.HasFlag(ImInputKeyboardCommandFlag.NextWord))
                    {
                        state.Caret = FindEndOfWordOrSpacesSequence(state.Caret, -1, buffer);
                    }

                    if (evt.Command.HasFlag(ImInputKeyboardCommandFlag.Selection))
                    {
                        state.Selection += previousCaret - state.Caret;
                    }
                    else
                    {
                        state.Selection = 0;
                    }
                    break;
                
                case KeyCode.RightArrow:
                    if (state.Selection != 0 && !evt.Command.HasFlag(ImInputKeyboardCommandFlag.Selection))
                    {
                        state.Caret = Mathf.Min(Mathf.Max(state.Caret + state.Selection, state.Caret), buffer.Length);
                    }
                    else
                    {
                        state.Caret = Mathf.Min(state.Caret + 1, buffer.Length);
                    }
                    
                    if (evt.Command.HasFlag(ImInputKeyboardCommandFlag.NextWord))
                    {
                        state.Caret = FindEndOfWordOrSpacesSequence(state.Caret, 1, buffer);
                    }
                    
                    if (evt.Command.HasFlag(ImInputKeyboardCommandFlag.Selection))
                    {
                        state.Selection += previousCaret - state.Caret;
                    }
                    else
                    {
                        state.Selection = 0;
                    }

                    break;
                
                case KeyCode.UpArrow:
                {
                    var viewPosition = CaretToViewPosition(state.Caret, drawer, in rect, in layout, in buffer);
                    viewPosition.y += layout.LineHeight - (layout.LineHeight * 0.5f);
                    state.Caret = ViewToCaretPosition(viewPosition, drawer, in rect, in layout, in buffer);
                    break;
                }
                case KeyCode.DownArrow:
                {
                    var viewPosition = CaretToViewPosition(state.Caret, drawer, in rect, in layout, in buffer);
                    viewPosition.y -= layout.LineHeight + (layout.LineHeight * 0.5f);
                    state.Caret = ViewToCaretPosition(viewPosition, drawer, in rect, in layout, in buffer);
                    break;
                }
                case KeyCode.Delete:
                    if (state.Caret < buffer.Length)
                    {
                        if (state.Selection != 0)
                        {
                            if (state.Selection < 0)
                            {
                                state.Caret += state.Selection;
                            }
                            
                            buffer.Delete(state.Caret, Mathf.Abs(state.Selection));
                            state.Selection = 0;
                        }
                        else
                        {
                            buffer.Delete(state.Caret, 1);
                        }

                        changed = true;
                    }
                    break;
                case KeyCode.Backspace:
                    if (state.Selection != 0)
                    {
                        if (state.Selection < 0)
                        {
                            state.Caret += state.Selection;
                        }
                        
                        buffer.Delete(state.Caret, Mathf.Abs(state.Selection));
                        state.Selection = 0;
                        changed = true;
                    }
                    else if (state.Caret > 0)
                    {
                        buffer.Delete(--state.Caret, 1);
                        changed = true;
                    }
                    break;
                default:
                    var c = evt.Char;
                    if (evt.Command == ImInputKeyboardCommandFlag.SelectAll)
                    {
                        state.Selection = -buffer.Length;
                        state.Caret = buffer.Length;
                    }
                    else if (evt.Command == ImInputKeyboardCommandFlag.Copy)
                    {
                        gui.Input.Clipboard = new string(GetSelectedText(in state, in buffer));
                    }
                    else if (evt.Command == ImInputKeyboardCommandFlag.Paste)
                    {
                        if (state.Selection != 0)
                        {
                            if (state.Selection < 0)
                            {
                                state.Caret += state.Selection;
                            }
                        
                            buffer.Delete(state.Caret, Mathf.Abs(state.Selection));
                            state.Selection = 0;
                        }

                        var clipboardText = gui.Input.Clipboard;
                        buffer.Insert(state.Caret, clipboardText);
                        state.Caret += clipboardText.Length;
                        changed = true;
                    }
                    else if (c != 0)
                    {
                        if (state.Selection != 0)
                        {
                            if (state.Selection < 0)
                            {
                                state.Caret += state.Selection;
                            }
                        
                            buffer.Delete(state.Caret, Mathf.Abs(state.Selection));
                            state.Selection = 0;
                        }
                        
                        buffer.Insert(state.Caret, c);
                        state.Caret = Mathf.Min(state.Caret + 1, buffer.Length);
                        changed = true;
                    }
                    break;
            }

            return true;
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
            return LineToViewPosition(FindLineAtCaretPosition(caret, in layout, out var linePosition), linePosition, buffer, in rect, in drawer, in layout);
        }

        private static Vector2 LineToViewPosition(int line, int pos, ReadOnlySpan<char> buffer, in ImRect rect, in TextDrawer drawer, in TextDrawer.Layout layout)
        {
            var yOffset = line * -layout.LineHeight + layout.OffsetY;
            var xOffset = line >= layout.LinesCount ? layout.OffsetX : layout.Lines[line].OffsetX;

            if (line < layout.LinesCount && pos <= layout.Lines[line].Count)
            {
                ref readonly var lineLayout = ref layout.Lines[line];
                var slice = buffer[lineLayout.Start..(lineLayout.Start + lineLayout.Count)];
                
                for (int i = 0; i < pos; ++i)
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

        private static int FindEndOfWordOrSpacesSequence(int caret, int dir, ReadOnlySpan<char> buffer)
        {
            dir = Math.Sign(dir);
            caret = Mathf.Clamp(caret + dir, 0, buffer.Length);

            var visitedLetter = false;
            var spaces = 0;
            
            while (caret > 0 && caret < buffer.Length)
            {
                var c = buffer[caret];
                var isWhiteSpace = char.IsWhiteSpace(c);
                if (isWhiteSpace)
                {
                    spaces++;
                }

                var isLetter = char.IsLetterOrDigit(c) || char.IsSymbol(c);
                if (isLetter)
                {
                    visitedLetter = true;
                }
                else if (visitedLetter)
                {
                    if (dir < 0)
                    {
                        caret++;
                    }

                    break;
                }
                else if (!isWhiteSpace && spaces > 1)
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

        private static ImRect GetCaretRect(Vector2 position, float lineHeight)
        {
            return new ImRect(
                position.x, 
                position.y - lineHeight, 
                Style.CaretWidth,
                lineHeight);
        }
        
        private static void DrawCaret(ImGui gui, in TextDrawer.Layout layout, in ImTextEditStateStyle style, Vector2 position)
        {
            gui.Canvas.Rect(GetCaretRect(position, layout.LineHeight), style.FrontColor);
        }

        private static void DrawSelection(ImGui gui, 
            in ImTextEditState state, 
            in TextDrawer drawer, 
            in ImRect rect, 
            in ImTextEditBuffer buffer,
            in TextDrawer.Layout layout, 
            in ImTextEditStateStyle style)
        {
            if (state.Selection == 0)
            {
                return;
            }

            var begin = state.Selection < 0 ? state.Caret + state.Selection : state.Caret;
            var end = state.Selection < 0 ? state.Caret : state.Caret + state.Selection;
            
            var beginLine = FindLineAtCaretPosition(begin, in layout, out _);
            var endLine = FindLineAtCaretPosition(end, in layout, out _);
            
            for (int i = beginLine; i <= endLine; ++i)
            {
                ref readonly var line = ref layout.Lines[i];
                
                var lineRelativeBegin = Mathf.Max(0, begin - line.Start);
                var lineRelativeEnd = Mathf.Min(line.Count, end - line.Start);

                var p0 = LineToViewPosition(i, lineRelativeBegin, buffer, in rect, in drawer, in layout);
                var p1 = LineToViewPosition(i, lineRelativeEnd, buffer, in rect, in drawer, in layout);
                
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