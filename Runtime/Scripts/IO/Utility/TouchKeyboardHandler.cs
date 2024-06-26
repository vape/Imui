using System;
using Imui.IO.Events;
using UnityEngine;

namespace Imui.IO.Utility
{
    public class TouchKeyboardHandler : IDisposable
    {
        private const int TOUCH_KEYBOARD_CLOSE_FRAMES_THRESHOLD = 3;

        public TouchScreenKeyboard TouchKeyboard;
        
        private int touchKeyboardRequestFrame;
        private bool touchKeyboardUnsupported;
        
        public void RequestTouchKeyboard(ReadOnlySpan<char> text)
        {
            if (!TouchScreenKeyboard.isSupported || touchKeyboardUnsupported)
            {
                return;
            }

            if (TouchKeyboard == null)
            {
                TouchKeyboard = TouchScreenKeyboard.Open(new string(text), TouchScreenKeyboardType.Default);
                
                if (TouchKeyboard.status == TouchScreenKeyboard.Status.Done)
                {
                    touchKeyboardUnsupported = true;
                    TouchKeyboard.active = false;
                    TouchKeyboard = null;
                    return;
                }
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