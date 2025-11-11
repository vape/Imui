using Imui.Core;
using Imui.Examples;
using UnityEditor;
using UnityEngine;

namespace Imui.Editor.Demo
{
    public class ImEditorDemoWindow : ImuiEditorWindow
    {
        [MenuItem("Window/Imui/Demo")]
        public static void ShowDemo()
        {
            var window = GetWindow<ImEditorDemoWindow>();
            window.titleContent = new GUIContent("Editor Demo");
            window.Show();
        }

        private bool open = true;

        protected override void OnAfterDraw()
        {
            base.OnAfterDraw();
            
            if (!open)
            {
                Close();
            }
        }

        protected override void OnDraw(ImGui gui)
        {
            ImDemoWindow.DrawContent(gui, ref open);
        }
    }
}