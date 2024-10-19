using System;
using Imui.Core;
using Imui.Style;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls.Windows
{
    public class ImConsoleWindow : IDisposable
    {
        [Flags]
        public enum TypeMask
        {
            None = 0,
            Info = 1,
            Warning = 2,
            Error = 4,
            All = Info | Warning | Error
        }

        public struct Message
        {
            public DateTime Time;
            public LogType Type;
            public string Text;
            public string Stacktrace;
            public int OneLineLength;

            public string GetWholeMessage()
            {
                return $"{Text}\n{Stacktrace}";
            }
        }

        private TypeMask typeMask = TypeMask.All;
        private ImCircularBuffer<Message> messages;
        private bool disposed;
        private Message selectedMessage;

        public ImConsoleWindow()
        {
            Application.logMessageReceived += OnLogMessageReceived;
            messages = new ImCircularBuffer<Message>(512);
        }

        public void Draw(ImGui gui, ref bool open)
        {
            if (!gui.BeginWindow("Console", ref open, (768, 512)))
            {
                return;
            }

            DrawMenu(gui, ref open);

            var infoColor0 = new Color32(96, 96, 128, 32);
            var infoColor1 = new Color32(96, 96, 128, 48);
            var warnColor0 = new Color32(255, 160, 32, 32);
            var warnColor1 = new Color32(255, 160, 32, 48);
            var erroColor0 = new Color32(255, 0, 0, 32);
            var erroColor1 = new Color32(255, 0, 0, 48);
            
            gui.AddSpacing();
            gui.BeginHorizontal();
            gui.Radio(ref typeMask);
            gui.EndHorizontal();
            gui.Separator();

            gui.Layout.Push(ImAxis.Vertical, gui.GetLayoutSize());
            gui.Canvas.PushClipRect(gui.Layout.GetBoundsRect());
            gui.BeginScrollable();

            var lw = gui.GetLayoutWidth();
            var rh = gui.GetRowHeight();

            for (int i = 0; i < messages.Count; ++i)
            {
                ref readonly var msg = ref messages.Get(i);

                var isErro = msg.Type is LogType.Error or LogType.Assert or LogType.Exception;
                var isWarn = msg.Type is LogType.Warning;
                var isInfo = !isErro && !isWarn;

                var masked = ((typeMask & TypeMask.Error) == 0 && isErro) || ((typeMask & TypeMask.Warning) == 0 && isWarn) ||
                             ((typeMask & TypeMask.Info) == 0 && isInfo);

                if (masked)
                {
                    continue;
                }

                var settings = new ImTextSettings(gui.Style.Layout.TextSize, 0.0f, 0.5f, false);
                var rect = gui.AddLayoutRect(lw, rh);
                var textRect = rect.WithPadding(left: gui.Style.Layout.InnerSpacing);

                if (!gui.Canvas.Cull(rect))
                {
                    var text = ((ReadOnlySpan<char>)msg.Text)[..msg.OneLineLength];
                    var color = i % 2 == 0
                        ? (isErro ? erroColor0 : isWarn ? warnColor0 : infoColor0)
                        : (isErro ? erroColor1 : isWarn ? warnColor1 : infoColor1);

                    gui.Canvas.Rect(rect, color);
                    gui.Canvas.Text(text, gui.Style.Text.Color, textRect, in settings);

                    if (gui.InvisibleButton(rect))
                    {
                        selectedMessage = msg;
                    }
                }
            }

            gui.EndScrollable();
            gui.Canvas.PopClipRect();
            gui.Layout.Pop();

            DrawDetails(gui);

            gui.EndWindow();
        }

        private void DrawMenu(ImGui gui, ref bool open)
        {
            gui.BeginMenuBar();
            if (gui.TryBeginMenuBarItem("Console"))
            {
                if (gui.MenuItem("Close"))
                {
                    open = false;
                }

                gui.EndMenuBarItem();
            }
            
            if (gui.TryBeginMenuBarItem("View"))
            {
                if (gui.MenuItem("Clear Messages"))
                {
                    Clear();
                }
                
                gui.EndMenuBarItem();
            }

            if (gui.TryBeginMenuBarItem("Test"))
            {
                if (gui.MenuItem("Send Info"))
                {
                    Debug.Log("Info Message");
                }

                if (gui.MenuItem("Send Warning"))
                {
                    Debug.LogWarning("Warning Message");
                }

                if (gui.MenuItem("Send Error"))
                {
                    Debug.LogError("Error Message");
                }
                
                if (gui.MenuItem("Send Expection"))
                {
                    Debug.LogException(new Exception("Example exception"));
                }

                gui.EndMenuBarItem();
            }

            gui.EndMenuBar();
        }

        private void DrawDetails(ImGui gui)
        {
            if (selectedMessage.Text == null)
            {
                return;
            }

            var windowRect = gui.WindowManager.GetCurrentWindowRect();
            windowRect.SplitTop(windowRect.H * 0.4f, out var rect);

            rect.AddPadding(gui.Style.Window.ContentPadding);
            gui.Box(rect, in gui.Style.List.Box);
            rect.AddPadding(gui.Style.Window.ContentPadding);

            gui.Layout.Push(ImAxis.Vertical, rect);
            gui.BeginHorizontal();
            if (gui.Button("Copy"))
            {
                gui.Input.Clipboard = selectedMessage.GetWholeMessage();
            }

            if (gui.Button("Close"))
            {
                selectedMessage = default;
            }

            gui.EndHorizontal();

            var wholeMessageSpan = gui.Formatter.Join(selectedMessage.Text, "\n", selectedMessage.Stacktrace);
            gui.TextEditReadonly(wholeMessageSpan, gui.AddLayoutRect(gui.GetLayoutSize()), multiline: true);
            gui.Layout.Pop();
        }

        private void OnLogMessageReceived(string condition, string stacktrace, LogType type)
        {
            var oneLineLength = condition.IndexOf('\n');
            if (oneLineLength < 0)
            {
                oneLineLength = condition.Length;
            }

            messages.PushFront(new Message
            {
                Time = DateTime.UtcNow,
                Type = type,
                Text = condition,
                Stacktrace = stacktrace,
                OneLineLength = oneLineLength
            });
        }

        public void Clear()
        {
            messages.Clear();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            Application.logMessageReceived -= OnLogMessageReceived;
            messages = default;

            disposed = true;
        }
    }
}