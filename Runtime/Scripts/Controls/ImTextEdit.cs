using System;
using Imui.Core;
using Imui.IO.Events;
using Imui.IO.Touch;
using Imui.IO.Utility;
using Imui.Rendering;
using Imui.Style;
using UnityEngine;

namespace Imui.Controls
{
    [Flags]
    public enum ImTextEditFlag
    {
        None = 0,
        NoDecorations = 1 << 0,
        NoMasking = 1 << 1
    }

    [Flags]
    public enum ImTextEditStateFlag
    {
        None = 0,
        NeedScrollToCaret = 1 << 0
    }

    public struct ImTextEditState
    {
        public int Caret;
        public int Selection;
        public double BlinkTime;
        public ImTextEditStateFlag Flag;
    }

    public ref struct ImTextEditBuffer
    {
        public int MaxLength => maxLength;
        public int Length => mutableLength >= 0 ? mutableLength : source.Length;

        public char this[Index index] => ((ReadOnlySpan<char>)this)[index];

        private readonly ImArena arena;
        private readonly ReadOnlySpan<char> source;
        private readonly string sourceString;
        private readonly int maxLength;

        private Span<char> mutable;
        private int mutableLength;

        public ImTextEditBuffer(string source, ImArena arena, int maxLength)
        {
            this.arena = arena;
            this.source = source;
            this.sourceString = source;

            this.mutableLength = -1;
            this.mutable = default;
            this.maxLength = maxLength;
        }

        public ImTextEditBuffer(ReadOnlySpan<char> source, ImArena arena, int maxLength)
        {
            this.arena = arena;
            this.source = source;
            this.sourceString = null;

            this.mutableLength = -1;
            this.mutable = default;
            this.maxLength = maxLength;
        }

        public ImTextEditBuffer(Span<char> arr, int mutableLength, ImArena arena, int maxLength)
        {
            this.arena = arena;
            this.source = null;
            this.sourceString = null;

            this.mutableLength = mutableLength;
            this.mutable = arr;
            this.maxLength = maxLength;
        }

        private void MakeMutable(int capacity)
        {
            if (mutableLength < 0)
            {
                var adjustedSourceString = source[..Math.Min(source.Length, capacity)];

                mutable = arena.AllocArray<char>(capacity);
                adjustedSourceString.CopyTo(mutable);
                mutableLength = adjustedSourceString.Length;
            }
            else
            {
                mutable = arena.ReallocArray(ref mutable, capacity);

                if (mutableLength > capacity)
                {
                    mutableLength = capacity;
                }
            }
        }

        public void Clear(int len)
        {
            MakeMutable(len);
            mutableLength = 0;
        }

        public void Remove(int pos, int count)
        {
            if (pos < 0 || pos >= Length)
            {
                throw new ArgumentOutOfRangeException(nameof(pos));
            }

            if (count < 1)
            {
                return;
            }

            MakeMutable(Length);

            count = pos + count > mutableLength ? mutableLength - pos : count;
            mutable[(pos + count)..].CopyTo(mutable[pos..]);
            mutableLength -= count;
        }

        public unsafe int Insert(int pos, char chr)
        {
            return Insert(pos, new ReadOnlySpan<char>(&chr, 1));
        }

        public int Insert(int pos, ReadOnlySpan<char> span)
        {
            if (pos < 0 || pos > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(pos));
            }

            var count = maxLength == 0 ? span.Length : Math.Min(span.Length, maxLength - Length);
            if (count == 0)
            {
                return 0;
            }

            MakeMutable(Length + count);

            if (pos != mutableLength)
            {
                mutable[pos..mutableLength].CopyTo(mutable[(pos + count)..]);
            }

            span[..count].CopyTo(mutable[pos..]);
            mutableLength += count;

            return count;
        }

        public override string ToString()
        {
            return mutableLength >= 0 ? new string(mutable[..mutableLength]) : (sourceString ?? new string(source));
        }

        public static implicit operator ReadOnlySpan<char>(ImTextEditBuffer buffer)
        {
            return buffer.mutableLength >= 0 ? buffer.mutable[..buffer.mutableLength] : buffer.source;
        }
    }

    public static class ImTextEdit
    {
        public const float CARET_BLINKING_TIME = 0.25f;
        public const float NO_BLINK_COOLDOWN = CARET_BLINKING_TIME;

        public static ImRect AddRect(ImGui gui, ImSize size, bool? multiline, out bool isActuallyMultiline /* wtf? */)
        {
            var minWidth = gui.GetRowHeight() * 1.5f; // approximately 1 char width + scroll bar
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
                _ => gui.Layout.AddRect(Mathf.Max(minWidth, gui.GetLayoutWidth()), minHeight)
            };
        }

        public static void TextEditNonEditable(this ImGui gui, ReadOnlySpan<char> text, ImSize size = default, bool? multiline = null)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            var rect = AddRect(gui, size, multiline, out var actuallyMultiline);
            TextEditNonEditable(gui, text, rect, actuallyMultiline);
        }

        public static void TextEditNonEditable(this ImGui gui, ReadOnlySpan<char> text, ImRect rect, bool multiline)
        {
            var buffer = new ImTextEditBuffer(text, gui.Arena, 0);

            gui.BeginReadOnlyWithoutStyleChanges(true);
            TextEdit(gui, ref buffer, rect, multiline);
            gui.EndReadOnlyWithoutStyleChanges();
        }

        public static void TextSelectable(this ImGui gui, ReadOnlySpan<char> text)
        {
            var settings = new ImTextSettings(gui.Style.Layout.TextSize,
                                              gui.Style.TextEdit.Alignment,
                                              gui.Style.TextEdit.SelectableTextWrap,
                                              ImTextOverflow.Overflow);
            
            TextSelectable(gui, text, in settings);
        }
        
        public static void TextSelectable(this ImGui gui, ReadOnlySpan<char> text, in ImTextSettings settings)
        {
            ref readonly var textLayout = ref gui.TextDrawer.BuildTempLayout(text,
                                                                             settings.Wrap ? gui.GetLayoutWidth() : 0,
                                                                             0,
                                                                             settings.Align.X,
                                                                             settings.Align.Y,
                                                                             settings.Size,
                                                                             settings.Wrap,
                                                                             settings.Overflow);

            var rect = gui.AddLayoutRect(textLayout.Width, textLayout.Height);

            TextSelectable(gui, text, in settings, rect);
        }

        public static void TextSelectable(this ImGui gui, ReadOnlySpan<char> text, in ImTextSettings settings, ImRect rect)
        {
            var id = gui.GetNextControlId();
            var buffer = new ImTextEditBuffer(text, gui.Arena, text.Length);
            ref var state = ref gui.Storage.Get<ImTextEditState>(id);

            gui.BeginReadOnlyWithoutStyleChanges(true);
            TextEdit(gui,
                     id,
                     ref buffer,
                     ref state,
                     rect,
                     true,
                     ImTouchKeyboardType.Default,
                     ReadOnlySpan<char>.Empty,
                     in settings,
                     ImTextEditFlag.NoDecorations | ImTextEditFlag.NoMasking);
            gui.EndReadOnlyWithoutStyleChanges();
        }

        public static string TextEdit(this ImGui gui,
                                      string text,
                                      ImSize size = default,
                                      bool? multiline = null,
                                      int charactersLimit = 0,
                                      ImTouchKeyboardType keyboardType = ImTouchKeyboardType.Default,
                                      ReadOnlySpan<char> hint = default,
                                      ImTextEditFlag flags = ImTextEditFlag.None)
        {
            TextEdit(gui, ref text, size, multiline, charactersLimit, keyboardType, hint, flags);
            return text;
        }

        public static bool TextEdit(this ImGui gui,
                                    ref string text,
                                    ImSize size = default,
                                    bool? multiline = null,
                                    int charactersLimit = 0,
                                    ImTouchKeyboardType keyboardType = ImTouchKeyboardType.Default,
                                    ReadOnlySpan<char> hint = default,
                                    ImTextEditFlag flags = ImTextEditFlag.None)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rect = AddRect(gui, size, multiline, out var actuallyMultiline);
            return TextEdit(gui, ref text, rect, actuallyMultiline, charactersLimit, keyboardType, hint, flags);
        }

        public static bool TextEdit(this ImGui gui,
                                    ref ImTextEditBuffer text,
                                    ImSize size = default,
                                    bool? multiline = null,
                                    ImTouchKeyboardType keyboardType = ImTouchKeyboardType.Default,
                                    ReadOnlySpan<char> hint = default,
                                    ImTextEditFlag flags = ImTextEditFlag.None)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var rect = AddRect(gui, size, multiline, out var actuallyMultiline);
            return TextEdit(gui, ref text, rect, actuallyMultiline, keyboardType, hint, flags);
        }

        public static string TextEdit(this ImGui gui,
                                      string text,
                                      ImRect rect,
                                      bool multiline,
                                      int charactersLimit = 0,
                                      ImTouchKeyboardType keyboardType = ImTouchKeyboardType.Default,
                                      ReadOnlySpan<char> hint = default,
                                      ImTextEditFlag flags = ImTextEditFlag.None)
        {
            TextEdit(gui, ref text, rect, multiline, charactersLimit, keyboardType, hint, flags);
            return text;
        }

        public static bool TextEdit(this ImGui gui,
                                    ref string text,
                                    ImRect rect,
                                    bool multiline,
                                    int charactersLimit = 0,
                                    ImTouchKeyboardType keyboardType = ImTouchKeyboardType.Default,
                                    ReadOnlySpan<char> hint = default,
                                    ImTextEditFlag flags = ImTextEditFlag.None)
        {
            var id = gui.GetNextControlId();
            ref var state = ref gui.Storage.Get<ImTextEditState>(id);

            return TextEdit(gui, id, ref text, ref state, rect, multiline, charactersLimit, keyboardType, hint, flags);
        }

        public static bool TextEdit(this ImGui gui,
                                    ref ImTextEditBuffer buffer,
                                    ImRect rect,
                                    bool multiline,
                                    ImTouchKeyboardType keyboardType = ImTouchKeyboardType.Default,
                                    ReadOnlySpan<char> hint = default,
                                    ImTextEditFlag flags = ImTextEditFlag.None)
        {
            var id = gui.GetNextControlId();
            var settings = GetDefaultTextSettings(gui);
            ref var state = ref gui.Storage.Get<ImTextEditState>(id);

            return TextEdit(gui, id, ref buffer, ref state, rect, multiline, keyboardType, hint, in settings, flags);
        }

        public static bool TextEdit(this ImGui gui,
                                    uint id,
                                    ref ImTextEditBuffer buffer,
                                    ImRect rect,
                                    bool multiline,
                                    ImTouchKeyboardType keyboardType = ImTouchKeyboardType.Default,
                                    ReadOnlySpan<char> hint = default,
                                    ImTextEditFlag flags = ImTextEditFlag.None)
        {
            var settings = GetDefaultTextSettings(gui);
            ref var state = ref gui.Storage.Get<ImTextEditState>(id);

            return TextEdit(gui, id, ref buffer, ref state, rect, multiline, keyboardType, hint, in settings, flags);
        }

        public static bool TextEdit(this ImGui gui,
                                    uint id,
                                    ref string text,
                                    ref ImTextEditState state,
                                    ImRect rect,
                                    bool multiline,
                                    int charactersLimit = 0,
                                    ImTouchKeyboardType keyboardType = ImTouchKeyboardType.Default,
                                    ReadOnlySpan<char> hint = default,
                                    ImTextEditFlag flags = ImTextEditFlag.None)
        {
            var buffer = new ImTextEditBuffer(text, gui.Arena, charactersLimit);
            var settings = GetDefaultTextSettings(gui);
            var changed = TextEdit(gui, id, ref buffer, ref state, rect, multiline, keyboardType, hint, in settings, flags);
            if (changed)
            {
                text = buffer.ToString();
            }

            return changed;
        }

        public static ImTextSettings GetDefaultTextSettings(ImGui gui)
        {
            return new ImTextSettings(gui.Style.Layout.TextSize, gui.Style.TextEdit.Alignment, gui.Style.TextEdit.TextWrap, ImTextOverflow.Overflow);
        }

        public static bool TextEdit(ImGui gui,
                                    uint id,
                                    ref ImTextEditBuffer buffer,
                                    ref ImTextEditState state,
                                    ImRect rect,
                                    bool multiline,
                                    ImTouchKeyboardType keyboardType,
                                    ReadOnlySpan<char> hint,
                                    in ImTextSettings textSettings,
                                    ImTextEditFlag flags)
        {
            ref readonly var style = ref gui.Style.TextEdit;

            var selected = gui.IsControlActive(id);
            var hovered = gui.IsControlHovered(id);
            var textChanged = false;
            var editable = !gui.IsReadOnly;

            ref readonly var stateStyle = ref (selected ? ref style.Selected : ref style.Normal);

            if ((flags & ImTextEditFlag.NoDecorations) == 0)
            {
                gui.Box(rect, in stateStyle.Box);
            }

            var textPadding = (ImPadding)gui.Style.Layout.InnerSpacing;

            if (!multiline)
            {
                // single-line text is always drawn at vertical center
                var halfVertPadding = Mathf.Max(rect.H - gui.TextDrawer.GetLineHeightFromFontSize(textSettings.Size), 0.0f) / 2.0f;

                textPadding.Top = halfVertPadding;
                textPadding.Bottom = halfVertPadding;
            }

            var textBounds = rect;

            if ((flags & ImTextEditFlag.NoDecorations) == 0)
            {
                textBounds = textBounds.WithPadding(textPadding + gui.Style.TextEdit.Padding);
            }

            if ((flags & ImTextEditFlag.NoMasking) == 0)
            {
                gui.Canvas.PushRectMask(rect, stateStyle.Box.BorderRadius);
            }

            gui.Layout.Push(ImAxis.Vertical, textBounds, ImLayoutFlag.Root);
            gui.BeginScrollable();

            var layoutWidth = textSettings.Wrap ? gui.GetLayoutWidth() : textBounds.W;
            var layout = gui.TextDrawer.BuildTempLayout(
                buffer,
                layoutWidth,
                textBounds.H,
                textSettings.Align.X,
                textSettings.Align.Y,
                textSettings.Size,
                textSettings.Wrap,
                textSettings.Overflow);

            if (buffer.Length == 0 && !hint.IsEmpty)
            {
                var hintTextSettings = textSettings;
                hintTextSettings.Overflow = ImTextOverflow.Ellipsis;
                gui.Canvas.Text(hint, style.HintFrontColor, textBounds, in hintTextSettings);
            }

            var textRect = gui.Layout.AddRect(layout.Width, layout.Height);

            if ((state.Flag & ImTextEditStateFlag.NeedScrollToCaret) != 0)
            {
                state.Flag &= ~ImTextEditStateFlag.NeedScrollToCaret;
                textRect.Position += ScrollToCaret(gui, state, textRect, in layout, buffer);

                if (textRect.W > textBounds.W && textRect.Right < textBounds.Right)
                {
                    textRect.X += textBounds.Right - textRect.Right;
                }
            }

            state.Caret = Mathf.Clamp(state.Caret, 0, buffer.Length);

            if (selected)
            {
                DrawSelection(gui, state.Caret, state.Selection, textRect, in layout, in stateStyle, in buffer);
            }

            gui.Canvas.Text(buffer, stateStyle.Box.FrontColor, textRect.TopLeft, in layout);

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down when evt.LeftButton && selected && hovered && evt.Count > 1:
                    state.Selection = 0;
                    state.Caret = ViewToCaretPosition(gui.Input.MousePosition, gui.TextDrawer, textRect, in layout, in buffer);
                    state.BlinkTime = gui.Input.Time;

                    if (evt.Count == 2)
                    {
                        SelectWordOnTheSameLineAtCaret(ref state, in layout, buffer);
                    }
                    else
                    {
                        SelectLineAtCaret(ref state, in layout, buffer);
                    }
                    break;

                case ImMouseEventType.Click when evt.Device == ImMouseDevice.Touch && hovered && !selected:
                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when evt.Device == ImMouseDevice.Mouse && evt.LeftButton && hovered:
                case ImMouseEventType.Down or ImMouseEventType.BeginDrag when evt.Device == ImMouseDevice.Touch && evt.LeftButton && hovered && selected:
                    if (!selected)
                    {
                        gui.SetActiveControl(id, ImControlFlag.Draggable);
                    }

                    state.Selection = 0;
                    // TODO (artem-s): set caret to position where drag begins instead
                    state.Caret = ViewToCaretPosition(gui.Input.MousePosition, gui.TextDrawer, textRect, in layout, in buffer);
                    state.BlinkTime = gui.Input.Time;

                    gui.Input.UseMouseEvent();
                    break;

                case ImMouseEventType.Drag when selected:
                    var newCaretPosition = ViewToCaretPosition(gui.Input.MousePosition, gui.TextDrawer, textRect, in layout, in buffer);
                    state.Selection -= newCaretPosition - state.Caret;
                    state.Caret = newCaretPosition;
                    state.BlinkTime = gui.Input.Time;

                    gui.Input.UseMouseEvent();
                    ScrollToCaret(gui, state, textRect, in layout, buffer);
                    break;

                case ImMouseEventType.Down when selected && !hovered:
                    gui.ResetActiveControl();
                    break;
            }

            if (selected)
            {
                gui.SetPreferredRefreshRate(30);

                DrawCaret(gui, state.Caret, state.BlinkTime, textRect, in layout, in stateStyle, in buffer);

                for (int i = 0; i < gui.Input.KeyboardEventsCount; ++i)
                {
                    ref readonly var keyboardEvent = ref gui.Input.GetKeyboardEvent(i);

                    if (HandleKeyboardEvent(gui, in keyboardEvent, ref state, ref buffer, textRect, in layout, multiline, editable, out var isTextChanged))
                    {
                        textChanged |= isTextChanged;
                        state.BlinkTime = gui.Input.Time;
                        gui.Input.UseKeyboardEvent(i);

                        if (isTextChanged)
                        {
                            // (artem-s): we need to build the new text layout to handle scrolling to caret properly, so we do it on the next frame
                            state.Flag |= ImTextEditStateFlag.NeedScrollToCaret;
                        }
                        else
                        {
                            ScrollToCaret(gui, state, textRect, in layout, buffer);
                        }
                    }
                }

                ref readonly var textEvent = ref gui.Input.TextEvent;
                switch (textEvent.Type)
                {
                    case ImTextEventType.Cancel:
                        gui.ResetActiveControl();
                        break;
                    case ImTextEventType.Set:
                        textChanged = buffer.Length != 0 || textEvent.Text.Length != 0;
                        Set(ref state, ref buffer, textEvent.Text);
                        if (textEvent.Selection != null)
                        {
                            state.Caret = textEvent.Selection.Value.start;
                            state.Selection = textEvent.Selection.Value.length;
                        }
                        gui.Input.UseTextEvent();
                        break;
                    case ImTextEventType.Submit:
                        gui.ResetActiveControl();
                        textChanged = buffer.Length != 0 || textEvent.Text.Length != 0;
                        Set(ref state, ref buffer, textEvent.Text);
                        gui.Input.UseTextEvent();
                        break;
                    default:
                        if (editable)
                        {
                            // (artem-s): touch keyboard doesn't like negative length
                            var begin = state.Selection < 0 ? state.Caret + state.Selection : state.Caret;
                            var end = state.Selection < 0 ? state.Caret : state.Caret + state.Selection;
                            var settings = new ImTouchKeyboardSettings()
                            {
                                Muiltiline = multiline,
                                Type = keyboardType,
                                CharactersLimit = buffer.MaxLength,
                                Selection = new RangeInt(begin, end - begin)
                            };

                            gui.Input.RequestTouchKeyboard(id, buffer, settings);
                        }
                        break;
                }
            }

            gui.RegisterControl(id, rect);

            gui.EndScrollable(multiline ? ImScrollFlag.None : ImScrollFlag.HideAll);
            gui.Layout.Pop();

            if ((flags & ImTextEditFlag.NoMasking) == 0)
            {
                gui.Canvas.PopRectMask();
            }

            return textChanged;
        }

        public static void SelectAll(ImGui gui, uint id, int length)
        {
            ref var state = ref gui.Storage.Get<ImTextEditState>(id);
            state.Caret = length;
            state.Selection = -length;
            state.BlinkTime = gui.Input.Time;
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
                    stateChanged |= MoveCaretHorizontal(ref state, in buffer, in layout, -1, command);
                    break;

                case KeyCode.RightArrow:
                    stateChanged |= MoveCaretHorizontal(ref state, in buffer, in layout, +1, command);
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
            return Insert(ref state, ref buffer, clipboardText);
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

            var added = buffer.Insert(state.Caret, text);
            state.Caret += added;
            return added > 0;
        }

        public static void Set(ref ImTextEditState state, ref ImTextEditBuffer buffer, ReadOnlySpan<char> text)
        {
            buffer.Clear(text.Length);
            state.Caret = text.Length;
            state.Selection = 0;
            buffer.Insert(0, text);
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

            buffer.Remove(state.Caret, Mathf.Abs(state.Selection));
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
                buffer.Remove(--state.Caret, 1);
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
                buffer.Remove(state.Caret, 1);
                return true;
            }

            return false;
        }

        public static int FindEndOfLine(int caret, int dir, ReadOnlySpan<char> buffer, in ImTextLayout layout)
        {
            var lineIndex = FindLineAtCaretPosition(caret, in layout, out _);
            var line = layout.Lines[lineIndex];

            if (dir > 0)
            {
                var index = line.Start + line.Count;
                if (index > 0 && (index != buffer.Length || buffer[index - 1] == '\n'))
                {
                    index -= 1;
                }

                return index;
            }

            if (dir < 0)
            {
                return line.Start;
            }

            return caret;
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
            if ((cmd & (ImKeyboardCommandFlag.JumpWord | ImKeyboardCommandFlag.JumpEnd)) != 0)
            {
                return false;
            }

            var prevCaret = state.Caret;
            var prevSelection = state.Selection;
            var viewPosition = CaretToViewPosition(state.Caret, gui.TextDrawer, textRect, in layout, in buffer);
            viewPosition.y += (-layout.LineHeight * 0.5f) + (dir * layout.LineHeight);
            state.Caret = ViewToCaretPosition(viewPosition, gui.TextDrawer, textRect, in layout, in buffer);

            if (cmd.HasFlag(ImKeyboardCommandFlag.Select))
            {
                state.Selection += prevCaret - state.Caret;
            }
            else
            {
                state.Selection = 0;
            }

            return state.Caret != prevCaret || state.Selection != prevSelection;
        }

        public static bool MoveCaretHorizontal(ref ImTextEditState state,
                                               in ImTextEditBuffer buffer,
                                               in ImTextLayout layout,
                                               int dir,
                                               ImKeyboardCommandFlag cmd)
        {
            var prevCaret = state.Caret;
            var prevSelection = state.Selection;

            if (state.Selection != 0 && !cmd.HasFlag(ImKeyboardCommandFlag.Select))
            {
                state.Caret = dir < 0 ? Mathf.Min(state.Caret + state.Selection, state.Caret) : Mathf.Max(state.Caret + state.Selection, state.Caret);
            }
            else if (cmd.HasFlag(ImKeyboardCommandFlag.JumpEnd))
            {
                state.Caret = FindEndOfLine(state.Caret, dir, buffer, in layout);
            }
            else if (cmd.HasFlag(ImKeyboardCommandFlag.JumpWord))
            {
                state.Caret = FindEndOfWordOrSpacesSequence(state.Caret, dir, buffer);
            }
            else
            {
                state.Caret = Mathf.Max(state.Caret + dir, 0);
            }

            state.Caret = Mathf.Clamp(state.Caret, 0, buffer.Length);

            if (cmd.HasFlag(ImKeyboardCommandFlag.Select))
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

        public static Vector2 ScrollToCaret(ImGui gui, ImTextEditState state, ImRect textRect, in ImTextLayout layout, ImTextEditBuffer buffer)
        {
            var viewPosition = CaretToViewPosition(state.Caret, gui.TextDrawer, textRect, in layout, in buffer);

            ref readonly var frame = ref gui.Layout.GetFrame();
            var scrollOffset = gui.GetScrollOffset();
            var caretScreenPosition = viewPosition;
            var caretWidth = state.Caret >= buffer.Length ? 0 : gui.TextDrawer.GetCharacterAdvance(buffer[state.Caret], layout.Size);
            var caretTop = caretScreenPosition.y;
            var caretBottom = caretScreenPosition.y - layout.LineHeight;
            var caretLeft = caretScreenPosition.x;
            var caretRight = caretScreenPosition.x + caretWidth;
            var extraOffset = new Vector2();

            if (caretTop > frame.Bounds.Top)
            {
                extraOffset.y += frame.Bounds.Top - caretTop;
            }
            else if (caretBottom < frame.Bounds.Bottom)
            {
                extraOffset.y += frame.Bounds.Bottom - caretBottom;
            }

            if (frame.Bounds.Left > caretLeft)
            {
                extraOffset.x += frame.Bounds.Left - caretLeft;
            }
            else if (frame.Bounds.Right < caretRight)
            {
                extraOffset.x += frame.Bounds.Right - caretRight;
            }

            if (extraOffset != default)
            {
                scrollOffset += extraOffset;
                gui.SetScrollOffset(scrollOffset);
            }

            return extraOffset;
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
                                     double blinkTime,
                                     ImRect textRect,
                                     in ImTextLayout layout,
                                     in ImStyleTextEditState style,
                                     in ImTextEditBuffer buffer)
        {
            var viewPosition = CaretToViewPosition(position, gui.TextDrawer, textRect, in layout, in buffer);
            var caretViewRect = new ImRect(viewPosition.x, viewPosition.y - layout.LineHeight, gui.Style.TextEdit.CaretWidth, layout.LineHeight);

            var draw = ((long)(Math.Max(0, gui.Input.Time - blinkTime - NO_BLINK_COOLDOWN) / CARET_BLINKING_TIME)) % 2 == 0;
            if (draw)
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
    }
}