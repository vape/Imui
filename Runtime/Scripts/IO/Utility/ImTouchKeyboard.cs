using System;
using Imui.IO.Events;
using UnityEngine;

namespace Imui.IO.Utility
{
    public class ImTouchKeyboard : IDisposable
    {
        private const int TOUCH_KEYBOARD_CLOSE_FRAMES_THRESHOLD = 3;

        public TouchScreenKeyboard TouchKeyboard;
        
        private int touchKeyboardRequestFrame;
        
        public void RequestTouchKeyboard(ReadOnlySpan<char> text)
        {
            #if UNITY_WEBGL
            // TODO (artem-s): fix touch keyboard handling for webgl
            return;
            #endif
            
            if (!TouchScreenKeyboard.isSupported)
            {
                return;
            }

            if (TouchKeyboard == null)
            {
                TouchKeyboard = TouchScreenKeyboard.Open(new string(text), TouchScreenKeyboardType.Default);
            }

            if (!TouchKeyboard.active)
            {
                TouchKeyboard.active = true;
            }
            
            touchKeyboardRequestFrame = Time.frameCount;
        }

        public void HandleTouchKeyboard(out ImTextEvent textEvent)
        {
            textEvent = default;
            
            if (TouchKeyboard != null)
            {
                var shouldHide = Mathf.Abs(Time.frameCount - touchKeyboardRequestFrame) > TOUCH_KEYBOARD_CLOSE_FRAMES_THRESHOLD;
                
                switch (TouchKeyboard.status)
                {
                    case TouchScreenKeyboard.Status.Canceled:
                        textEvent = new ImTextEvent(ImTextEventType.Cancel);
                        shouldHide = true;
                        break;
                    case TouchScreenKeyboard.Status.Done:
                        textEvent = new ImTextEvent(ImTextEventType.Submit, TouchKeyboard.text);
                        shouldHide = true;
                        break;
                }
                
                if (shouldHide)
                {
                    TouchKeyboard.active = false;
                    TouchKeyboard = null;
                }
            }
        }

        public void Dispose()
        {
            if (TouchKeyboard != null)
            {
                TouchKeyboard.active = false;
                TouchKeyboard = null;
            }
        }
    }
}