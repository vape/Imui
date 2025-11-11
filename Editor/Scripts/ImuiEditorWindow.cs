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
        protected virtual void OnDraw(ImGui gui) { }

        private void OnEnable()
        {
            imuiElement = new ImuiElement(this);
            rootVisualElement.Add(imuiElement);
            imuiElement.StretchToParentSize();
            imuiElement.PixelsPerPoint = EditorGUIUtility.pixelsPerPoint;
        }

        public void Draw(ImGui gui)
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
            imuiElement.DoFrame();
            OnAfterDraw();
        }

        private void OnDisable()
        {
            imuiElement?.Dispose();
            imuiElement = null;
            rootVisualElement.Clear();
        }
    }
}