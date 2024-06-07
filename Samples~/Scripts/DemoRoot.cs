using Imui.Core;
using Imui.IO.UGUI;
using UnityEngine;

namespace Imui.Demo
{
    public class DemoRoot : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private ImuiGraphic graphic;
        [SerializeField] private Font font;
        [SerializeField] private DemoWindowConfig config;

        private ImGui gui;
        private DemoWindow window;

        private void Awake()
        {
            gui = new ImGui(graphic, graphic);
            window = new DemoWindow(config);
            gui.TextDrawer.LoadFont(font);
        }

        private void Update()
        {
            gui.UiScale = canvas.scaleFactor;
            gui.BeginFrame();
            window.Draw(gui);
            gui.EndFrame();
            gui.Render();
        }
    }
}