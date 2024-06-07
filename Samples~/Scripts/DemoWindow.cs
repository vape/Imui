using System;
using Imui.Controls;
using Imui.Controls.Layout;
using Imui.Core;
using UnityEngine;

namespace Imui.Demo
{
    [Serializable]
    public struct DemoWindowConfig
    {
        public Texture[] Textures;
    }
    
    public class DemoWindow
    {
        private static char[] formatBuffer = new char[256];
        
        private DemoWindowConfig config;
        private bool checkmarkValue;
        private int selectedValue;
        private float sliderValue;
        private string[] values = new string[]
        {
            "Value 1", "Value 2", "Value 3", "Value 4",  "Value 5",  "Value 6",
            "Value 7", "Value 8", "Value 9", "Value 10", "Value 11", "Value 12"
        };
        private string singleLineText = "Single line text edit";
        private string multiLineText = "Multiline text\nedit";

        public DemoWindow(DemoWindowConfig config)
        {
            this.config = config;
        }
        
        public void Draw(ImGui gui)
        {
            gui.BeginWindow("Demo");
            
            gui.BeginFoldout("Widgets", out var widgetsOpen);
            if (widgetsOpen)
            {
                DrawWidgetsPage(gui);
            }
            gui.EndFoldout();

            gui.BeginFoldout("Layout", out var layoutOpen);
            if (layoutOpen)
            {
                DrawLayoutPage(gui);
            }
            gui.EndFoldout();
            
            gui.BeginFoldout("Style", out var styleOpen);
            if (styleOpen)
            {
                DrawStylePage(gui);
            }
            gui.EndFoldout();
            
            gui.EndWindow();
        }

        private void DrawWidgetsPage(ImGui gui)
        {
            const string sliderLabel = "Slider value: ";

            gui.ButtonFitted("Small Button");
            gui.Button("Big Button");
            gui.Checkmark(ref checkmarkValue, "Checkmark");
            gui.Dropdown(ref selectedValue, values);
            gui.Slider(ref sliderValue, 0.0f, 1.0f);
            gui.Text(Format("Slider value: ", sliderValue, "0.00"));
            gui.TextEdit(ref singleLineText);
            gui.TextEdit(ref multiLineText, gui.GetAvailableWidth(), 200);

            gui.AddSpacing();
            
            var imageTex = config.Textures[0];
            var imageRect = gui.Layout.AddRect(imageTex.width, imageTex.height);
            gui.Image(imageRect, imageTex);
        }

        private void DrawLayoutPage(ImGui gui)
        {
            gui.AddSpacing();
            
            gui.BeginHorizontal();
            for (int i = 0; i < 3; ++i)
            {
                gui.ButtonFitted("Horizontal");
            }
            gui.EndHorizontal();
            
            gui.AddSpacing();
            
            gui.BeginVertical();
            for (int i = 0; i < 3; ++i)
            {
                gui.ButtonFitted("Vertical");
            }
            gui.EndVertical();
            
            gui.AddSpacing();
            
            var grid = gui.BeginGrid(5, gui.GetRowHeight());
            for (int i = 0; i < 12; ++i)
            {
                var cell = gui.GridNextCell(ref grid);
                gui.TextFittedSlow(Format("Grid cell ", i, "0"), in cell);
            }
            gui.EndGrid(in grid);
        }

        private void DrawStylePage(ImGui gui)
        {
            gui.Text("Text Size");
            gui.Slider(ref ImControls.Style.TextSize, 6, 128);
            
            gui.Text("Controls Spacing");
            gui.Slider(ref ImControls.Style.Spacing, 0, 32);
        }
        
        private ReadOnlySpan<char> Format(ReadOnlySpan<char> prefix, float value, ReadOnlySpan<char> format = default)
        {
            var dst = new Span<char>(formatBuffer);
            prefix.CopyTo(dst);
            value.TryFormat(dst[prefix.Length..], out var written, format);
            return dst[..(prefix.Length + written)];
        }
    }
}