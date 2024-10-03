using System;
using Imui.Controls.Styling;
using Imui.Core;
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

        public void Draw(ImGui gui)
        {
            var infoColor0 = new Color32(96, 96, 128, 32);
            var infoColor1 = new Color32(96, 96, 128, 48);
            var warnColor0 = new Color32(255, 160, 32, 32);
            var warnColor1 = new Color32(255, 160, 32, 48);
            var erroColor0 = new Color32(255, 0, 0, 32);
            var erroColor1 = new Color32(255, 0, 0, 48);
            
            gui.BeginWindow("Console", width: 768, 512);
            
            gui.AddSpacing();
            gui.BeginHorizontal();
            gui.Radio(ref typeMask);
            if (gui.Button("Clear", ImSizeType.Fit))
            {
                Clear();
            }
            gui.EndHorizontal();
            gui.BeginHorizontal();
            if (gui.Button("Info", ImSizeType.Fit, flags: ImButtonFlag.ReactToHeldDown)) { Debug.Log("Test Message"); }
            if (gui.Button("Warning", ImSizeType.Fit, flags: ImButtonFlag.ReactToHeldDown)) { Debug.LogWarning("Test Warning"); }
            if (gui.Button("Error", ImSizeType.Fit, flags: ImButtonFlag.ReactToHeldDown)) { Debug.LogError("Test Error"); }
            if (gui.Button("Exception", ImSizeType.Fit, flags: ImButtonFlag.ReactToHeldDown)) { Debug.LogException(new Exception("Test Exception")); }
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
                
                var masked = 
                    ((typeMask & TypeMask.Error)   == 0 && isErro) ||
                    ((typeMask & TypeMask.Warning) == 0 && isWarn) ||
                    ((typeMask & TypeMask.Info)    == 0 && isInfo);
                
                if (masked)
                {
                    continue;
                }
                
                var rect = gui.AddLayoutRect(lw, rh);
                if (!gui.Canvas.Cull(rect))
                {
                    var color = i % 2 == 0
                        ? (isErro ? erroColor0 : isWarn ? warnColor0 : infoColor0)
                        : (isErro ? erroColor1 : isWarn ? warnColor1 : infoColor1);

                    gui.Canvas.Rect(rect, color);
                    gui.Canvas.Text(msg.Text, ImTheme.Active.Text.Color, rect.TopLeft, ImTheme.Active.Controls.TextSize);

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

        private void DrawDetails(ImGui gui)
        {
            if (selectedMessage.Text == null)
            {
                return;
            }

            var windowRect = gui.WindowManager.GetCurrentWindowRect();
            windowRect.SplitTop(windowRect.H * 0.4f, out var rect);

            rect.AddPadding(ImTheme.Active.Window.ContentPadding);
            gui.Box(rect, in ImTheme.Active.List.Box);
            rect.AddPadding(ImTheme.Active.Window.ContentPadding);
            
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
            messages.PushFront(new Message
            {
                Time = DateTime.UtcNow,
                Type = type,
                Text = condition,
                Stacktrace = stacktrace
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