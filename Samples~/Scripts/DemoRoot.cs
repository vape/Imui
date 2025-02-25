using Imui.Controls;
using Imui.Core;
using Imui.Examples;
using Imui.IO.UGUI;
using UnityEngine;

namespace Imui.Demo
{
    public class DemoRoot : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private Canvas canvas;
        [SerializeField] private ImuiUnityGUIBackend backend;

        private ImGui gui;
        private bool demoOpen = true;

        private void Awake()
        {
            gui = new ImGui(backend, backend);
        }

        private void Update()
        {
            gui.BeginFrame();
            
            DrawRootMenu();
            ImDemoWindow.Draw(gui, ref demoOpen);
            
            gui.EndFrame();
        }
        
        private void OnRenderObject()
        {
            gui.Render();
        }

        private void DrawRootMenu()
        {
            gui.BeginPopup();

            var menuBarHeight = gui.GetRowHeight();
            var menuBarRect = gui.Canvas.ScreenRect.TakeTop(menuBarHeight);

            gui.Canvas.SafeAreaPadding.Top += menuBarHeight;
            
            gui.BeginMenuBar(menuBarRect);
            
            if (gui.BeginMenuBarItem("Demo"))
            {
                gui.MenuItem("Show Demo", ref demoOpen);
                gui.EndMenuBarItem();
            }
            
            if (gui.BeginMenuBarItem("View"))
            {
                gui.AddSpacingIfLayoutFrameNotEmpty();
                gui.BeginHorizontal();
                gui.Text("Clear Color");
                gui.AddSpacing(20);
                cam.backgroundColor = gui.ColorEdit(cam.backgroundColor);
                gui.EndHorizontal();
                gui.EndMenuBarItem();
            }
            
            gui.EndMenuBar();
            
            gui.EndPopup();
        }
    }
}