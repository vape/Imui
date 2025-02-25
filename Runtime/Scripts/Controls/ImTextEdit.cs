using System;
using System.Runtime.InteropServices;
using Imui.Core;
using Imui.IO.Events;
using Imui.IO.Touch;
using Imui.IO.Utility;
using Imui.Rendering;
using Imui.Style;
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

    // TODO (artem-s): do not handle drag events if control is not active
    public static class ImTextEdit
    {
        public const float CARET_BLINKING_TIME = 0.3f;
        public const float MIN_WIDTH = 1;
        public const float MIN_HEIGHT = 1;

        private const string TEMP_BUFFER_TAG = "temp_buffer";

        public static ImRect AddRect(ImGui gui, ImSize size, bool? multiline, out bool isActuallyMultiline /* wtf? */)
        {
            var minHeight = gui.GetRowHeight();

            if (multiline == null)
            {
                if (size.Mode == ImSizeMode.Fixed && size.Height > gui.GetRowHeight())
                {
                    multiline = true;
                }
                else
                {
                    multiline = false;
                }
            }

            if (multiline is true)
            {
                minHeight *= 3;
            }

            isActuallyMultiline = multiline.Value;

            return size.Mode switch
            {
                ImSizeMode.Fixed => gui.Layout.AddRect(size.Width, size.Height),
                _ => gui.Layout.AddRect(Mathf.Max(MIN_WIDTH, gui.GetLayoutWidth()), Mathf.Max(MIN_HEIGHT, minHeight))
            };
        }

        public static void TextEditReadonly(this ImGui gui, ReadOnlySpan<char> text, ImSize size = default, bool? multiline = null)
        {
            var rect = AddRect(gui, size, multiline, out var actuallyMultiline);
            TextEditReadonly(gui, text, rect, actuallyMultiline);
        }

        public static void TextEditReadonly(this ImGui gui, ReadOnlySpan<char> text, ImRect rect, bool multiline, ImAdjacency adjacency = ImAdjacency.None)
        {
            var buffer = new ImTextEditBuffer(text);

            // TODO (artem-s): in this case, readonly should not change styling
            gui.BeginReadOnly(true);
            TextEdit(gui, ref buffer, rect, multiline, adjacency: adjacency);
            gui.EndReadOnly();
        }

        public static string TextEdit(this ImGui gui, string text, ImSize size = default, bool? multiline = null, ImTextEditFilter filter = null)
        {
            TextEdit(gui, ref text, size, multiline, filter);
            return text;
        }

        public static bool TextEdit(this ImGui gui, ref string text, ImSize size = default, bool? multiline = null, ImTextEditFilter filter = null)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rect = AddRect(gui, size, multiline, out var actuallyMultiline);
            return TextEdit(gui, ref text, rect, actuallyMultiline, filter);
        }

        public static string TextEdit(this ImGui gui, string text, ImRect rect, bool multiline, ImTextEditFilter filter = null)
        {
            TextEdit(gui, ref text, rect, multiline, filter);
            return text;
        }

        public static bool TextEdit(this ImGui gui, ref string text, ImRect rect, ImTextEditFilter filter = default)
        {
            return TextEdit(gui, ref text, rect, true, filter);
        }

        public static bool TextEdit(this ImGui gui,
                                    ref string text,
                                    ImRect rect,
                                    bool multiline,
                                    ImTextEditFilter filter = default,
                                    ImAdjacency adjacency = ImAdjacency.None)
        {
            var id = gui.GetNextControlId();
            ref var state = ref gui.Storage.Get<ImTextEditState>(id);

            return TextEdit(gui, id, ref text, ref state, rect, multiline, filter, adjacency);
        }

        public static bool TextEdit(this ImGui gui,
                                    ref ImTextEditBuffer buffer,
                                    ImRect rect,
                                    bool multiline,
                                    ImTextEditFilter filter = default,
                                    ImAdjacency adjacency = ImAdjacency.None)
        {
            var id = gui.GetNextControlId();
            ref var state = ref gui.Storage.Get<ImTextEditState>(id);

            return TextEdit(gui, id, ref buffer, ref state, rect, multiline, filter, adjacency);
        }

        public static bool TextEdit(this ImGui gui,
                                    uint id,
                                    ref ImTextEditBuffer buffer,
                                    ImRect rect,
                                    bool multiline,
                                    ImTextEditFilter filter = default,
                                    ImAdjacency adjacency = default)
        {
            ref var state = ref gui.Storage.Get<ImTextEditState>(id);

            return TextEdit(gui, id, ref buffer, ref state, rect, multiline, filter, adjacency);
        }

        public static bool TextEdit(this ImGui gui,
                                    uint id,
                                    ref string text,
                                    ref ImTextEditState state,
                                    ImRect rect,
                                    bool multiline,
                                    ImTextEditFilter filter = default,
                                    ImAdjacency adjacency = default)
        {
            var buffer = new ImTextEditBuffer(text);
            var changed = TextEdit(gui, id, ref buffer, ref state, rect, multiline, filter, adjacency);
            if (changed)
            {
                text = buffer.GetString();
            }

            return changed;
        }

        public static unsafe bool TextEdit(ImGui gui,
                                           uint id,
                                           ref ImTextEditBuffer buffer,
                                           ref ImTextEditState state,
                                           ImRect rect,
                                           bool multiline,
                                           ImTextEditFilter filter,
                                           ImAdjacency adjacency)
        {
            ref readonly var style = ref gui.Style.TextEdit;

            var selected = gui.IsControlActive(id);
            var hovered = gui.IsControlHovered(id);
            var stateStyle = selected ? style.Selected : style.Normal;
            var textChanged = false;
            var editable = !gui.IsReadOnly;

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
                tempBuffer = GetTempFilterBuffer(gui, id);

                buffer.Clear(tempBuffer->Length);
                buffer.Insert(0, tempBuffer->AsSpan());
            }

            gui.Box(rect, stateStyle.Box.MakeAdjacent(adjacency));

            ImPadding textPadding = gui.Style.Layout.InnerSpacing;

            var textSize = gui.Style.Layout.TextSize;
            var textAlignment = gui.Style.TextEdit.Alignment;

            if (!multiline)
            {
                // single-line text is always drawn at vertical center
                var halfVertPadding = Mathf.Max(rect.H - gui.TextDrawer.GetLineHeightFromFontSize(textSize), 0.0f) / 2.0f;

                textPadding.Top = halfVertPadding;
                textPadding.Bottom = halfVertPadding;
            }

            var textRect = rect.WithPadding(textPadding);
            var layout = gui.TextDrawer.BuildTempLayout(
                buffer, textRect.W, textRect.H, textAlignment.X, textAlignment.Y, textSize,
                style.TextWrap, ImTextOverflow.Overflow);

            gui.Canvas.PushRectMask(rect, stateStyle.Box.BorderRadius);
            gui.Layout.Push(ImAxis.Vertical, textRect, ImLayoutFlag.Root);
            gui.BeginScrollable();

            textRect = gui.Layout.AddRect(layout.Width, layout.Height);
            gui.Canvas.Text(buffer, stateStyle.Box.FrontColor, textRect.TopLeft, in layout);

            state.Caret = Mathf.Clamp(state.Caret, 0, buffer.Length);

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down when evt.LeftButton && selected && hovered && evt.Count > 1:
                    state.Selection = 0;
                    state.Caret = ViewToCaretPosition(gui.Input.MousePosition, gui.TextDrawer, textRect, in layout, in buffer);

                    if (evt.Count == 2)
                    {
                        SelectWordOnTheSameLineAtCaret(ref state, in layout, buffer);
                    }
                    else
                    {
                        SelectLineAtCaret(ref state, in layout, buffer);
                    }
                    break;

                case ImMouseEventType.Click when evt.Device == ImMouseDevice.Touch && hovered:
                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when evt.Device == ImMouseDevice.Mouse && evt.LeftButton && hovered:
                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when evt.Device == ImMouseDevice.Touch && evt.LeftButton && hovered && selected:
                    if (!selected)
                    {
                        gui.SetActiveControl(id, ImControlFlag.Draggable);

                        if (filter != null)
                        {
                            GetTempFilterBuffer(gui, id)->Populate(buffer);
                        }
                    }

                    state.Selection = 0;
                    state.Caret = ViewToCaretPosition(gui.Input.MousePosition, gui.TextDrawer, textRect, in layout, in buffer);

                    gui.Input.UseMouseEvent();
                    break;

                case ImMouseEventType.Drag when selected:
                    var newCaretPosition = ViewToCaretPosition(gui.Input.MousePosition, gui.TextDrawer, textRect, in layout, in buffer);
                    state.Selection -= newCaretPosition - state.Caret;
                    state.Caret = newCaretPosition;

                    gui.Input.UseMouseEvent();
                    ScrollToCaret(gui, state, textRect, in layout, buffer);
                    break;

                case ImMouseEventType.Down when selected && !hovered:
                    gui.ResetActiveControl();
                    break;
            }

            if (selected)
            {
                DrawCaret(gui, state.Caret, textRect, in layout, in stateStyle, in buffer);
                DrawSelection(gui, state.Caret, state.Selection, textRect, in layout, in stateStyle, in buffer);

                for (int i = 0; i < gui.Input.KeyboardEventsCount; ++i)
                {
                    ref readonly var keyboardEvent = ref gui.Input.GetKeyboardEvent(i);

                    if (HandleKeyboardEvent(gui, in keyboardEvent, ref state, ref buffer, textRect, in layout, multiline, editable, out var isTextChanged))
                    {
                        textChanged |= isTextChanged;

                        gui.Input.UseKeyboardEvent(i);
                        ScrollToCaret(gui, state, textRect, in layout, buffer);
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
                        gui.Input.UseTextEvent();
                        break;
                    default:
                        if (editable)
                        {
                            var settings = new ImTouchKeyboardSettings()
                            {
                                Muiltiline = multiline,
                                Type = filter?.KeyboardType ?? ImTouchKeyboardType.Default,
                                CharactersLimit = filter == null ? 0 : ImTextTempFilterBuffer.BUFFER_LENGTH
                            };

                            gui.Input.RequestTouchKeyboard(id, buffer, settings);
                        }
                        break;
                }
            }

            gui.RegisterControl(id, rect);

            gui.EndScrollable(multiline ? ImScrollFlag.None : ImScrollFlag.HideHorBar | ImScrollFlag.HideVerBar);
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
                                               ImRect textRect,
                                               in ImTextLayout layout,
                                               bool multiline,
                                               bool editable,
                                               out bool textChanged)
        {
            var stateChanged = false;

            textChanged = false;

            if (evt.Type != ImKeyboardEventType.Down)
            {
                return false;
            }

            ImKeyboardCommandsHelper.TryGetCommand(evt, out var command);

            switch (evt.Key)
            {
                case KeyCode.LeftArrow:
                    stateChanged |= MoveCaretHorizontal(ref state, in buffer, -1, command);
                    break;

                case KeyCode.RightArrow:
                    stateChanged |= MoveCaretHorizontal(ref state, in buffer, +1, command);
                    break;

                case KeyCode.UpArrow:
                    stateChanged |= MoveCaretVertical(gui, textRect, in layout, ref state, in buffer, +1, command);
                    break;

                case KeyCode.DownArrow:
                    stateChanged |= MoveCaretVertical(gui, textRect, in layout, ref state, in buffer, -1, command);
                    break;

                case KeyCode.Delete when editable:
                    textChanged |= DeleteForward(ref state, ref buffer);
                    break;

                case KeyCode.Backspace when editable:
                    textChanged |= DeleteBackward(ref state, ref buffer);
                    break;

                default:
                {
                    switch (command)
                    {
                        case ImKeyboardCommandFlag.SelectAll:
                            state.Selection = -buffer.Length;
                            state.Caret = buffer.Length;
                            stateChanged = true;
                            break;
                        
                        case ImKeyboardCommandFlag.Cut:
                            gui.Input.Clipboard = new string(GetSelectedText(in state, in buffer));
                            if (editable)
                            {
                                textChanged |= DeleteSelection(ref state, ref buffer);
                            }
                            stateChanged = true;
                            break;

                        case ImKeyboardCommandFlag.Copy:
                            gui.Input.Clipboard = new string(GetSelectedText(in state, in buffer));
                            stateChanged = true;
                            break;

                        case ImKeyboardCommandFlag.Paste when editable:
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

                            if (!editable)
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

            buffer.Insert(state.Caret, text);
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

        public static bool MoveCaretVertical(ImGui gui,
                                             ImRect textRect,
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
            var viewPosition = CaretToViewPosition(state.Caret, gui.TextDrawer, textRect, in layout, in buffer);
            viewPosition.y += (-layout.LineHeight * 0.5f) + (dir * layout.LineHeight);
            state.Caret = ViewToCaretPosition(viewPosition, gui.TextDrawer, textRect, in layout, in buffer);

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

        public static bool MoveCaretHorizontal(ref ImTextEditState state, in ImTextEditBuffer buffer, int dir, ImKeyboardCommandFlag cmd)
        {
            var prevCaret = state.Caret;
            var prevSelection = state.Selection;

            if (state.Selection != 0 && !cmd.HasFlag(ImKeyboardCommandFlag.Selection))
            {
                state.Caret = dir < 0 ? Mathf.Min(state.Caret + state.Selection, state.Caret) : Mathf.Max(state.Caret + state.Selection, state.Caret);
            }
            else if (cmd.HasFlag(ImKeyboardCommandFlag.NextWord))
            {
                state.Caret = FindEndOfWordOrSpacesSequence(state.Caret, dir, buffer);
            }
            else
            {
                state.Caret = Mathf.Max(state.Caret + dir, 0);
            }

            state.Caret = Mathf.Clamp(state.Caret, 0, buffer.Length);

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

        public static void SelectWordOnTheSameLineAtCaret(ref ImTextEditState state, in ImTextLayout layout, ReadOnlySpan<char> buffer)
        {
            var line = FindLineAtCaretPosition(state.Caret, in layout, out _);
            var maxLeft = layout.Lines[line].Start;
            var maxRight = maxLeft + layout.Lines[line].Count;

            if (line < layout.LinesCount - 1)
            {
                maxRight -= 1;
            }

            var right = Mathf.Min(maxRight, FindEndOfWordOrSpacesSequence(state.Caret, 1, buffer));
            var left = Mathf.Max(maxLeft, FindEndOfWordOrSpacesSequence(state.Caret, -1, buffer));

            state.Caret = right;
            state.Selection = left - right;
        }

        public static void SelectLineAtCaret(ref ImTextEditState state, in ImTextLayout layout, ReadOnlySpan<char> buffer)
        {
            if (layout.LinesCount <= 0)
            {
                return;
            }

            var line = FindLineAtCaretPosition(state.Caret, in layout, out _);
            var left = layout.Lines[line].Start;
            var right = left + layout.Lines[line].Count;

            state.Caret = right;
            state.Selection = left - right;
        }

        // TODO: doesn't work when caret is horizontally outside of the scope
        public static void ScrollToCaret(ImGui gui, ImTextEditState state, ImRect textRect, in ImTextLayout layout, ImTextEditBuffer buffer)
        {
            var viewPosition = CaretToViewPosition(state.Caret, gui.TextDrawer, textRect, in layout, in buffer);

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

            var charWidth = state.Caret >= buffer.Length ? 0 : gui.TextDrawer.GetCharacterAdvance(buffer.At(state.Caret), layout.Size);
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

        public static int ViewToCaretPosition(Vector2 position, ImTextDrawer drawer, ImRect rect, in ImTextLayout layout, in ImTextEditBuffer buffer)
        {
            var origin = rect.TopLeft;
            int line;

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
                var characterWidth = drawer.GetCharacterAdvance(span[i], layout.Size);

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

        public static Vector2 CaretToViewPosition(int caret, ImTextDrawer drawer, ImRect rect, in ImTextLayout layout, in ImTextEditBuffer buffer)
        {
            return LineOffsetToViewPosition(FindLineAtCaretPosition(caret, in layout, out var linePosition), linePosition, buffer, rect, drawer, in layout);
        }

        public static Vector2 LineOffsetToViewPosition(int line,
                                                       int offset,
                                                       ReadOnlySpan<char> buffer,
                                                       ImRect rect,
                                                       ImTextDrawer drawer,
                                                       in ImTextLayout layout)
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
                    xOffset += drawer.GetCharacterAdvance(slice[i], layout.Size);
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
                                     ImRect textRect,
                                     in ImTextLayout layout,
                                     in ImStyleTextEditState style,
                                     in ImTextEditBuffer buffer)
        {
            var viewPosition = CaretToViewPosition(position, gui.TextDrawer, textRect, in layout, in buffer);
            var caretViewRect = new ImRect(viewPosition.x, viewPosition.y - layout.LineHeight, gui.Style.TextEdit.CaretWidth, layout.LineHeight);

            if ((long)(Time.unscaledTime / CARET_BLINKING_TIME) % 2 == 0)
            {
                gui.Canvas.Rect(caretViewRect, style.Box.FrontColor);
            }
        }

        public static void DrawSelection(ImGui gui,
                                         int position,
                                         int size,
                                         ImRect textRect,
                                         in ImTextLayout layout,
                                         in ImStyleTextEditState style,
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

                var p0 = LineOffsetToViewPosition(i, lineRelativeBegin, buffer, textRect, gui.TextDrawer, in layout);
                var p1 = LineOffsetToViewPosition(i, lineRelativeEnd, buffer, textRect, gui.TextDrawer, in layout);

                var lineSelectionRect = new ImRect(p0.x, p0.y - layout.LineHeight, p1.x - p0.x, layout.LineHeight);

                gui.Canvas.Rect(lineSelectionRect, style.SelectionColor);
            }
        }

        public static unsafe ImTextTempFilterBuffer* GetTempFilterBuffer(ImGui gui, uint id)
        {
            gui.PushId(id);
            var tempBufferId = gui.GetControlId(TEMP_BUFFER_TAG);
            var tempBuffer = gui.Storage.GetUnsafe<ImTextTempFilterBuffer>(tempBufferId);
            gui.PopId();

            return tempBuffer;
        }
    }

    // TODO (artem-s): use arena allocator instead of static array
    public ref struct ImTextEditBuffer
    {
        public const int DEFAULT_MUTABLE_BUFFER_CAPACITY = 1024;

        private static char[] staticBuffer = new char[DEFAULT_MUTABLE_BUFFER_CAPACITY];

        public int Length;
        public ReadOnlySpan<char> InitSpan;
        public bool InitWithSpan;
        public string InitText;
        public char[] Buffer;

        public ImTextEditBuffer(string text)
        {
            InitSpan = default;
            InitText = text ?? string.Empty;
            InitWithSpan = false;
            Buffer = null;
            Length = InitText.Length;
        }

        public ImTextEditBuffer(ReadOnlySpan<char> text)
        {
            InitSpan = text;
            InitText = null;
            InitWithSpan = true;
            Buffer = null;
            Length = text.Length;
        }

        public char At(int index)
        {
            return Buffer?[index] ?? (InitWithSpan ? InitSpan[index] : InitText[index]);
        }

        public string GetString()
        {
            if (Buffer != null)
            {
                return new string(Buffer, 0, Length);
            }

            return InitText ?? InitSpan.ToString();
        }

        public void MakeMutable(int capacity = DEFAULT_MUTABLE_BUFFER_CAPACITY)
        {
            if (InitWithSpan)
            {
                var nextLength = Mathf.NextPowerOfTwo(Mathf.Max(InitSpan.Length, capacity));
                if (nextLength > staticBuffer.Length)
                {
                    Array.Resize(ref staticBuffer, nextLength);
                }

                Buffer = staticBuffer;
                InitSpan.CopyTo(Buffer);
                Length = InitSpan.Length;
                InitWithSpan = false;
            }
            else if (InitText != null)
            {
                var nextLength = Mathf.NextPowerOfTwo(Mathf.Max(InitText.Length, capacity));
                if (nextLength > staticBuffer.Length)
                {
                    Array.Resize(ref staticBuffer, nextLength);
                }

                Buffer = staticBuffer;
                InitText.CopyTo(0, Buffer, 0, InitText.Length);
                Length = InitText.Length;
                InitText = null;
            }
            else
            {
                if (Buffer == null)
                {
                    Buffer = staticBuffer;
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
                Array.Copy(Buffer, position + count, Buffer, position, Length - (position + count));
                Array.Fill(Buffer, (char)0, Length - count, count);
                Length -= count;
            }
        }

        public unsafe void Insert(int position, char c)
        {
            Insert(position, new ReadOnlySpan<char>(&c, 1));
        }

        public void Insert(int position, ReadOnlySpan<char> text)
        {
            MakeMutable(Length + text.Length);

            position = Mathf.Clamp(position, 0, Length);
            if (position < Length)
            {
                Array.Copy(Buffer, position, Buffer, position + text.Length, Length - position);
            }

            text.CopyTo(((Span<char>)Buffer)[position..]);
            Length += text.Length;
        }

        public static implicit operator ReadOnlySpan<char>(ImTextEditBuffer buffer) =>
            buffer.InitWithSpan ? buffer.InitSpan : buffer.InitText ?? new ReadOnlySpan<char>(buffer.Buffer, 0, buffer.Length);
    }

    public abstract class ImTextEditFilter
    {
        public virtual ImTouchKeyboardType KeyboardType => ImTouchKeyboardType.Default;

        public abstract bool IsValid(ReadOnlySpan<char> buffer);
        public abstract string GetFallbackString();
    }
}