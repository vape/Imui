using System;
using Imui.Controls.Styling;
using Imui.Core;

namespace Imui.Controls.Windows
{
    public static class ImDemoWindow
    {
        private static char[] formatBuffer = new char[256];
        
        private static bool checkmarkValue;
        private static int selectedValue;
        private static float sliderValue;
        private static string[] values = {
            "Value 1", "Value 2", "Value 3", "Value 4",  "Value 5",  "Value 6",
            "Value 7", "Value 8", "Value 9", "Value 10", "Value 11", "Value 12"
        };
        private static string singleLineText = "Single line text edit";
        private static string multiLineText = "Multiline text\nedit";
        private static float floatValue;
        private static int intValue;
        private static bool isReadOnly;
        private static bool[] checkboxes = new bool[8];

        public static void Draw(ImGui gui)
        {
            gui.BeginWindow("Demo", width: 700, height: 700);
            
            gui.BeginFoldout("Widgets", out var widgetsOpen);
            if (widgetsOpen)
            {
                DrawWidgetsPage(gui);
            }
            gui.EndFoldout();

            gui.BeginReadOnly(isReadOnly);
            
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
            
            gui.EndReadOnly();
            
            gui.EndWindow();
        }

        private static void DrawWidgetsPage(ImGui gui)
        {
            gui.Checkbox(ref isReadOnly, "Read Only");

            gui.BeginReadOnly(isReadOnly);
            gui.BeginDropdown("Click me", out var open);
            if (open)
            {
                gui.AddSpacing();
                gui.BeginHorizontal();
                for (int i = 0; i < checkboxes.Length; ++i)
                {
                    gui.Checkbox(ref checkboxes[i]);
                }
                gui.EndHorizontal();
            }
            gui.EndDropdown();
            
            gui.Button("Small Button", ImSizeType.Fit);
            gui.Button("Big Button");
            gui.Checkbox(ref checkmarkValue, "Checkmark");
            gui.Dropdown(ref selectedValue, values);
            gui.Slider(ref sliderValue, 0.0f, 1.0f);
            gui.Text(Format("Slider value: ", sliderValue, "0.00"));
            gui.TextEdit(ref singleLineText);
            gui.TextEdit(ref multiLineText, (gui.GetLayoutWidth(), 200));
            
            gui.Text("Float TextEdit");
            gui.AddSpacing();
            gui.BeginHorizontal();
            gui.BeginHorizontal(width: gui.GetLayoutWidth() * 0.5f);
            gui.TextEdit(ref floatValue);
            gui.EndHorizontal();
            gui.Text(Format("", floatValue));
            gui.EndHorizontal();
            
            gui.Text("Float TextEdit (limited precision)");
            gui.AddSpacing();
            gui.BeginHorizontal();
            gui.BeginHorizontal(width: gui.GetLayoutWidth() * 0.5f);
            gui.TextEdit(ref floatValue, format: "0.0");
            gui.EndHorizontal();
            gui.Text(Format("", floatValue));
            gui.EndHorizontal();
            
            gui.Text("Integer TextEdit");
            gui.AddSpacing();
            gui.BeginHorizontal();
            gui.BeginHorizontal(width: gui.GetLayoutWidth() * 0.5f);
            gui.TextEdit(ref intValue);
            gui.EndHorizontal();
            gui.Text(Format("", intValue));
            gui.EndHorizontal();

            gui.AddSpacing();
            
            gui.EndReadOnly();
        }

        private static void DrawLayoutPage(ImGui gui)
        {
            gui.AddSpacing();
            
            gui.BeginHorizontal();
            for (int i = 0; i < 3; ++i)
            {
                gui.Button("Horizontal", ImSizeType.Fit);
            }
            gui.EndHorizontal();
            
            gui.AddSpacing();
            
            gui.BeginVertical();
            for (int i = 0; i < 3; ++i)
            {
                gui.Button("Vertical", ImSizeType.Fit);
            }
            gui.EndVertical();
            
            gui.AddSpacing();
            
            var grid = gui.BeginGrid(5, gui.GetRowHeight());
            for (int i = 0; i < 12; ++i)
            {
                var cell = gui.GridNextCell(ref grid);
                gui.TextAutoSize(Format("Grid cell ", i, "0"), cell);
            }
            gui.EndGrid(in grid);
        }

        private static void DrawStylePage(ImGui gui)
        {
            gui.Text("Text Size");
            gui.Slider(ref ImControls.Style.TextSize, 6, 128);
            
            gui.Text("Spacing");
            gui.Slider(ref ImControls.Style.ControlsSpacing, 0, 32);
            
            gui.Text("Padding Left");
            gui.Slider(ref ImControls.Style.Padding.Left, 0, 32);
            
            gui.Text("Padding Right");
            gui.Slider(ref ImControls.Style.Padding.Right, 0, 32);
            
            gui.Text("Padding Top");
            gui.Slider(ref ImControls.Style.Padding.Top, 0, 32);
            
            gui.Text("Padding Bottom");
            gui.Slider(ref ImControls.Style.Padding.Bottom, 0, 32);
        }
        
        private static ReadOnlySpan<char> Format(ReadOnlySpan<char> prefix, float value, ReadOnlySpan<char> format = default)
        {
            var dst = new Span<char>(formatBuffer);
            prefix.CopyTo(dst);
            value.TryFormat(dst[prefix.Length..], out var written, format);
            return dst[..(prefix.Length + written)];
        }
    }
}