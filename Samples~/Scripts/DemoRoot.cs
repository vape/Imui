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
        [SerializeField] private ImCanvasBackend graphic;
        [SerializeField] private Font font;
        [SerializeField] private float fontSize;

        private ImGui gui;
        private bool demoOpen = true;

        private void Awake()
        {
            gui = new ImGui(graphic, graphic);
            gui.TextDrawer.LoadFont(font, fontSize);
        }

        private void Update()
        {
            gui.UiScale = canvas.scaleFactor;
            gui.BeginFrame();
            DrawRootMenu();
            ImDemoWindow.Draw(gui, ref demoOpen);
            gui.EndFrame();
        }

        private void DrawRootMenu()
        {
            gui.BeginPopup();
            
            gui.BeginMenuBar(gui.Canvas.ScreenRect.TakeTop(gui.GetRowHeight()));
            
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

        private void OnRenderObject()
        {
            gui.Render();
        }
    }
}