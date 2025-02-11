using System;
using Imui.IO.Events;
using UnityEngine;

namespace Imui.IO.Utility
{
    [Flags]
    public enum ImKeyboardCommandFlag: uint
    {
        None = 0,
        Selection = 1 << 0,
        NextWord = 1 << 1,
        SelectAll = 1 << 2,
        Copy = 1 << 3,
        Paste = 1 << 4,
        Cut = 1 << 5
    }
    
    public static class ImKeyboardCommandsHelper
    {
        public static bool TryGetCommand(ImKeyboardEvent evt, out ImKeyboardCommandFlag command)
        {
            command = ImKeyboardCommandFlag.None;
            
            var arrow = (int)evt.Key >= 273 && (int)evt.Key <= 276;
            bool jump;
            bool control;

            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
            {
                jump = evt.Modifiers.HasFlag(EventModifiers.Alt); // option key
                control = evt.Modifiers.HasFlag(EventModifiers.Command);
            }
            else
            {
                jump = evt.Modifiers.HasFlag(EventModifiers.Control);
                control = evt.Modifiers.HasFlag(EventModifiers.Control);
            }

            if (arrow && evt.Modifiers.HasFlag(EventModifiers.Shift))
            {
                command |= ImKeyboardCommandFlag.Selection;
            }

            if (arrow && jump)
            {
                command |= ImKeyboardCommandFlag.NextWord;
            }

            if (control && evt.Key == KeyCode.A)
            {
                command |= ImKeyboardCommandFlag.SelectAll;
            }

            if (control && evt.Key == KeyCode.C)
            {
                command |= ImKeyboardCommandFlag.Copy;
            }

            if (control && evt.Key == KeyCode.V)
            {
                command |= ImKeyboardCommandFlag.Paste;
            }

            if (control && evt.Key == KeyCode.X)
            {
                command |= ImKeyboardCommandFlag.Cut;
            }

            return command != ImKeyboardCommandFlag.None;
        }
    }
}