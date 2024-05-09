using System;
using Imui.Core;
using Imui.Core.Input;
using Imui.Rendering;
using Imui.Styling;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    // TODO (artem-s): add text selection
    // TODO (artem-s): add copy/paste
    // TODO (artem-s): handle 'Delete' key
    // TODO (artem-s): handle shortcuts like cmd+a (and ctrl+a for windows)
    // TODO (artem-s): implement some filtering API for numeric only input fields
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
            var settings = new ImTextSettings
            {
                Size = 24,
                AlignX = 0,
                AlignY = 0
            };
            
            TextEdit(gui, in rect, ref text, in settings);
        }
        
        public static void TextEdit(this ImGui gui, in ImRect rect, ref string text, in ImTextSettings settings)
        {
            var id = gui.GetNextControlId();
            ref var state = ref gui.Storage.Get<ImTextEditState>(id);

            TextEdit(gui, id, in rect, ref text, in settings, ref state);
        }
        
        public static void TextEdit(this ImGui gui, uint id, in ImRect rect, ref string text, in ImTextSettings settings, ref ImTextEditState state)
        {
            var buffer = new ImTextEditBuffer(text);
            var changed = TextEdit(gui, id, in rect, ref buffer, in settings, ref state);
            if (changed)
            {
                text = buffer.GetString();
            }
        }
        
        private static bool TextEdit(this ImGui gui, uint id, in ImRect rect, ref ImTextEditBuffer text, in ImTextSettings settings, ref ImTextEditState state)
        {
            var selected = gui.ActiveControl == id;
            var hovered = gui.GetHoveredControl() == id;
            
            var stateStyle = selected ? Style.Selected : Style.Normal;
            DrawBack(gui, in stateStyle, in rect, out var contentRect);
            
            var layout = gui.TextDrawer.BuildTempLayout(
                text, 
                rect.W, rect.H, 
                settings.AlignX, settings.AlignY, settings.Size);
            
            gui.Canvas.Text(text, stateStyle.FrontColor, contentRect, in layout);
            
            ref readonly var mouseEvent = ref gui.Input.MouseEvent;
            if (mouseEvent.Type == ImInputEventMouseType.Down && hovered)
            {
                if (!selected)
                {
                    gui.ActiveControl = id;
                }
                
                state.Caret = ViewToCaretPosition(gui.Input.MousePosition, gui.TextDrawer, in contentRect, in layout, in text);
                gui.Input.UseMouse();
            }
            
            state.Caret = Mathf.Clamp(state.Caret, 0, text.Length);
            
            var changed = false;
            if (selected)
            {
                var caretViewPosition = CaretToViewPosition(state.Caret, gui.TextDrawer, in contentRect, in layout, in text);
                DrawCaret(gui, in layout, in stateStyle, caretViewPosition);
                
                changed = HandleKeyboard(gui, ref state, ref text, in gui.TextDrawer, in contentRect, in layout);
            }
            
            gui.HandleControl(id, rect);
            
            return changed;
        }

        private static bool HandleKeyboard(ImGui gui, ref ImTextEditState state, ref ImTextEditBuffer buffer, in TextDrawer drawer, in ImRect rect, in TextDrawer.Layout layout)
        {
            ref readonly var kb = ref gui.Input.KeyboardEvent;
            if (kb.Type != ImInputEventKeyboardType.Down)
            {
                return false;
            }

            switch (kb.Key)
            {
                case KeyCode.LeftArrow:
                    gui.Input.UseKeyboard();
                    state.Caret = Mathf.Max(state.Caret - 1, 0);
                    break;
                case KeyCode.RightArrow:
                    gui.Input.UseKeyboard();
                    state.Caret = Mathf.Min(state.Caret + 1, buffer.Length);
                    break;
                case KeyCode.UpArrow:
                {
                    gui.Input.UseKeyboard();
                    var viewPosition = CaretToViewPosition(state.Caret, drawer, in rect, in layout, in buffer);
                    viewPosition.y += layout.LineHeight - (layout.LineHeight * 0.5f);
                    state.Caret = ViewToCaretPosition(viewPosition, drawer, in rect, in layout, in buffer);
                    break;
                }
                case KeyCode.DownArrow:
                {
                    gui.Input.UseKeyboard();
                    var viewPosition = CaretToViewPosition(state.Caret, drawer, in rect, in layout, in buffer);
                    viewPosition.y -= layout.LineHeight + (layout.LineHeight * 0.5f);
                    state.Caret = ViewToCaretPosition(viewPosition, drawer, in rect, in layout, in buffer);
                    break;
                }
                case KeyCode.Backspace:
                    gui.Input.UseKeyboard();
                    if (state.Caret > 0)
                    {
                        buffer.Delete(--state.Caret, 1);
                        return true;
                    }
                    break;
                default:
                    var c = kb.Char;
                    gui.Input.UseKeyboard();
                    
                    if (c != 0)
                    {
                        buffer.Insert(state.Caret, c);
                        state.Caret = Mathf.Min(state.Caret + 1, buffer.Length);
                        return true;
                    }
                    break;
            }

            return false;
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
            var line = 0;
            while (layout.LinesCount - 1 > line && layout.Lines[line].Count <= caret)
            {
                caret -= layout.Lines[line].Count;
                line++;
            }

            var yOffset = line * -layout.LineHeight + layout.OffsetY;
            var xOffset = line >= layout.LinesCount ? layout.OffsetX : layout.Lines[line].OffsetX;

            if (line < layout.LinesCount && caret <= layout.Lines[line].Count)
            {
                ref readonly var lineLayout = ref layout.Lines[line];
                var slice = ((ReadOnlySpan<char>)buffer)[lineLayout.Start..(lineLayout.Start + lineLayout.Count)];
                
                for (int i = 0; i < caret; ++i)
                {
                    xOffset += drawer.GetCharacterWidth(slice[i], layout.Size);
                }
            }

            return rect.TopLeft + new Vector2(xOffset, yOffset);
        }

        private static void DrawCaret(ImGui gui, in TextDrawer.Layout layout, in ImTextEditStateStyle style, Vector2 position)
        {
            var rect = new ImRect(
                position.x, 
                position.y - layout.LineHeight, 
                Style.CaretWidth,
                layout.LineHeight);
            
            gui.Canvas.Rect(rect, style.FrontColor);
        }
        
        private static void DrawBack(ImGui gui, in ImTextEditStateStyle style, in ImRect rect, out ImRect content)
        {
            gui.Canvas.Rect(rect, style.BackColor, Style.CornerRadius);
            gui.Canvas.RectOutline(rect, style.FrameColor, Style.FrameWidth, Style.CornerRadius);
            
            content = rect.WithPadding(Style.FrameWidth);
        }
    }

    public struct ImTextEditState
    {
        public int Caret;
    }

    public class ImTextEditStyle
    {
        public static readonly ImTextEditStyle Default = new ImTextEditStyle()
        {
            Normal = new ImTextEditStateStyle()
            {
                BackColor = ImColors.Gray7,
                FrontColor = ImColors.Black,
                FrameColor = ImColors.Gray1
            },
            Selected = new ImTextEditStateStyle()
            {
                BackColor = ImColors.White,
                FrontColor = ImColors.Black,
                FrameColor = ImColors.Black
            },
            CornerRadius = 3.0f,
            FrameWidth = 1.0f,
            CaretWidth = 2.0f
        };
        
        public ImTextEditStateStyle Normal;
        public ImTextEditStateStyle Selected;
        public float FrameWidth;
        public float CornerRadius;
        public float CaretWidth;
    }

    public class ImTextEditStateStyle
    {
        public Color32 BackColor;
        public Color32 FrontColor;
        public Color32 FrameColor;
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

        public unsafe void Delete(int position, int count)
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