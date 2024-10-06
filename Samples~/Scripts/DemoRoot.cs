using System;
using Imui.Controls.Windows;
using Imui.Core;
using Imui.IO.UGUI;
using UnityEngine;

namespace Imui.Demo
{
    public class DemoRoot : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private ImCanvasBackend graphic;
        [SerializeField] private Font font;
        [SerializeField] private float fontSize;

        private ImGui gui;

        private void Awake()
        {
            gui = new ImGui(graphic, graphic);
            gui.TextDrawer.LoadFont(font, fontSize);
        }

        private void Update()
        {
            gui.UiScale = canvas.scaleFactor;
            gui.BeginFrame();
            ImDemoWindow.Draw(gui);
            gui.EndFrame();
        }

        private void OnRenderObject()
        {
            gui.Render();
        }
    }
}