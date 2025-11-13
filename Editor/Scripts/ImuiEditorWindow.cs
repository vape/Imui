#if IMUI_UITOOLKIT_BACKEND

using Imui.Controls;
using Imui.Core;
using Imui.IO.UIToolkit;
using UnityEditor;
using UnityEngine.UIElements;

namespace Imui.Editor
{
    public class ImuiEditorWindow: EditorWindow, IImuiElementDelegate
    {
        private ImuiElement imuiElement;

        protected virtual void OnBeforeDraw() { }
        protected virtual void OnAfterDraw() { }
        
        protected virtual void OnInit(ImGui gui) { }
        protected virtual void OnDraw(ImGui gui) { }

        private void OnEnable()
        {
            AddElement();
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        }
        
        private void OnDisable()
        {
            RemoveElement();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
        }

        private void AddElement()
        {
            imuiElement = new ImuiElement(this);
            rootVisualElement.Add(imuiElement);
            imuiElement.StretchToParentSize();
        }
        
        private void RemoveElement()
        {
            if (imuiElement == null)
            {
                return;
            }
            
            rootVisualElement.Remove(imuiElement);
            imuiElement?.Dispose();
            imuiElement = null;
        }

        private void ReloadImui()
        {
            RemoveElement();
            AddElement();
        }
        
        private void OnPlayModeStateChange(PlayModeStateChange change)
        {
            if (change is PlayModeStateChange.EnteredEditMode or PlayModeStateChange.EnteredPlayMode)
            {
                ReloadImui();
            }
        }

        void IImuiElementDelegate.Init(ImGui gui)
        {
            OnInit(gui);
        }

        void IImuiElementDelegate.Draw(ImGui gui)
        {
            gui.Canvas.Rect(gui.Canvas.ScreenRect, gui.Style.Window.Box.BackColor);
            gui.Layout.Push(ImAxis.Vertical, gui.Canvas.SafeScreenRect.WithPadding(gui.Style.Window.ContentPadding));
            gui.BeginScrollable();

            OnDraw(gui);

            gui.EndScrollable();
            gui.Layout.Pop();
        }

        private void Update()
        {
            OnBeforeDraw();
            imuiElement.DoFrame(EditorApplication.timeSinceStartup);
            OnAfterDraw();
        }
    }
}

#endif