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
            gui.BeginMenuBar();
            
            if (gui.BeginMenu("Demo"))
            {
                gui.Menu("Show Demo", ref demoOpen);
                gui.EndMenu();
            }
            
            if (gui.BeginMenu("View"))
            {
                gui.AddSpacingIfLayoutFrameNotEmpty();
                gui.BeginHorizontal();
                gui.Text("Clear Color");
                gui.AddSpacing(20);
                cam.backgroundColor = gui.ColorEdit(cam.backgroundColor);
                gui.EndHorizontal();
                gui.EndMenu();
            }
            
            gui.EndMenuBar();
        }
    }
}