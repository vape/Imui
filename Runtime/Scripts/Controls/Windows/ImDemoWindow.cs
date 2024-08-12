using System;
using System.Collections.Generic;
using Imui.Controls.Styling;
using Imui.Controls.Styling.Themes;
using Imui.Core;
using UnityEngine;

namespace Imui.Controls.Windows
{
    public static class ImDemoWindow
    {
        private static char[] formatBuffer = new char[256];
        
        private static bool checkmarkValue;
        private static int selectedValue = -1;
        private static float sliderValue;
        private static float stepSliderValue;
        private static int intSliderValue;
        private static int selectedTheme;
        private static string[] themes = { ImLightTheme.NAME, ImDarkTheme.NAME };
        private static string[] values = 
        {
            "Value 1", "Value 2", "Value 3", "Value 4",  "Value 5",  "Value 6",
            "Value 7", "Value 8", "Value 9", "Value 10", "Value 11", "Value 12"
        };
        private static string singleLineText = "Single line text edit";
        private static string multiLineText = "Multiline text\nedit";
        private static float floatValue;
        private static int intValue;
        private static bool isReadOnly;
        private static bool customDropdownOpen;
        private static bool[] checkboxes = new bool[4];
        private static bool showDebugWindow = false;
        private static int clicks;
        private static int nestedFoldouts;
        private static bool showPlusMinusButtons = true;
        private static HashSet<int> selectedValues = new HashSet<int>(values.Length);

        public static void Draw(ImGui gui)
        {
            gui.BeginWindow("Demo", width: 700, height: 700);
            
            gui.BeginFoldout("Controls", out var controlsOpen);
            gui.BeginIndent();
            if (controlsOpen)
            {
                DrawControlsPage(gui);
            }
            gui.EndIndent();
            gui.EndFoldout();

            gui.BeginReadOnly(isReadOnly);
            
            gui.BeginFoldout("Layout", out var layoutOpen);
            gui.BeginIndent();
            if (layoutOpen)
            {
                DrawLayoutPage(gui);
            }
            gui.EndIndent();
            gui.EndFoldout();
            
            gui.BeginFoldout("Style", out var styleOpen);
            gui.BeginIndent();
            if (styleOpen)
            {
                DrawStylePage(gui);
            }
            gui.EndIndent();
            gui.EndFoldout();
            
            gui.BeginFoldout("Other", out var otherOpen);
            gui.BeginIndent();
            if (otherOpen)
            {
                DrawOtherPage(gui);
            }
            gui.EndIndent();
            gui.EndFoldout();
            
            gui.EndReadOnly();
            
            gui.EndWindow();
            
            if (showDebugWindow)
            {
                gui.PushId("DemoDebugWindow");
                ImDebugWindow.Draw(gui);
                gui.PopId();
            }
        }

        private static void DrawControlsPage(ImGui gui)
        {
            void DrawNestedFoldout(ImGui gui, int current, ref int total)
            {
                const int MAX = 8;
                
                var label = current == 0 ? "Nested Foldout" : Format("Nested Foldout ", current, "0");
                
                gui.BeginFoldout(label, out var nestedFoldoutOpen);
                gui.BeginIndent();
                if (nestedFoldoutOpen)
                {
                    if (current < total)
                    {
                        DrawNestedFoldout(gui, current + 1, ref total);
                    }
                    else if (current == total)
                    {
                        if (total == MAX)
                        {
                            gui.Text("Let's just stop here");
                            if (gui.Button("Reset"))
                            {
                                total = 0;
                            }
                        }
                        else if (gui.Button("Add one more"))
                        {
                            total++;
                        }
                    }
                }
                gui.EndIndent();
                gui.EndFoldout();
            }
            
            gui.Checkbox(ref isReadOnly, "Read Only");

            gui.BeginReadOnly(isReadOnly);

            var customDropdownId = gui.GetNextControlId();

            gui.AddSpacingIfLayoutFrameNotEmpty();
            
            ImDropdown.Begin(gui);
            ImDropdown.BeginPreview(gui);
            ImDropdown.PreviewButton(gui, customDropdownId, default, ref customDropdownOpen);
            var textSettings = new ImTextSettings(ImTheme.Active.Controls.TextSize, 0.0f, 0.5f);
            for (int i = 0; i < checkboxes.Length; ++i)
            {
                gui.Text(checkboxes[i] ? " X " : " O ", textSettings);
            }
            ImDropdown.EndPreview(gui, customDropdownId, ref customDropdownOpen);
            if (customDropdownOpen)
            {
                ImDropdown.BeginList(gui, customDropdownId, 1);
                var allTrue = true;
                
                gui.BeginHorizontal();
                for (int i = 0; i < checkboxes.Length; ++i)
                {
                    gui.Checkbox(ref checkboxes[i]);
                    allTrue &= checkboxes[i];
                }
                if (allTrue)
                {
                    gui.Text("Bingo!", textSettings);
                }
                gui.EndHorizontal();
                ImDropdown.EndList(gui, ref customDropdownOpen);
            }
            ImDropdown.End(gui);

            DrawNestedFoldout(gui, 0, ref nestedFoldouts);

            if (gui.Button(Format("Clicks ", clicks, "0"), ImSizeType.Fit))
            {
                clicks++;
            }

            if (gui.Button("Reset Clicks"))
            {
                clicks = 0;
            }

            gui.Checkbox(ref checkmarkValue, "Checkmark");
            gui.Dropdown(ref selectedValue, values, defaultLabel: "Not Selected");
            gui.Slider(ref sliderValue, -Mathf.PI * 2, Mathf.PI * 2, format: "0.000 rad.");
            gui.Slider(ref stepSliderValue, -5.0f, 5.0f, step: 0.1f);
            gui.Slider(ref intSliderValue, -10, 10);
            gui.TextEdit(ref singleLineText, multiline: false);
            gui.TextEdit(ref multiLineText, (gui.GetLayoutWidth(), 200));
            gui.BeginList((gui.GetLayoutWidth(), ImList.GetEnclosingHeight(gui.GetRowsHeightWithSpacing(3))));
            for (int i = 0; i < values.Length; ++i)
            {
                var isSelected = selectedValues.Contains(i);
                if (gui.ListItem(values[i], ref isSelected))
                {
                    if (isSelected)
                    {
                        selectedValues.Add(i);
                    }
                    else
                    {
                        selectedValues.Remove(i);
                    }
                }
            }
            gui.EndList();
            
            gui.AddSpacing(gui.GetRowHeight() * 0.5f);
            gui.Checkbox(ref showPlusMinusButtons, "Show +/-");
            
            gui.Text("Float TextEdit");
            gui.AddSpacing();
            gui.BeginHorizontal();
            gui.BeginHorizontal(width: gui.GetLayoutWidth() * 0.6f);
            gui.FloatEdit(ref floatValue, format: "0.00#####", step: showPlusMinusButtons ? 0.01f : 0.0f);
            gui.EndHorizontal();
            gui.Text(Format(" floatValue = ", floatValue, "0.0######"));
            gui.EndHorizontal();

            gui.Text("Integer TextEdit");
            gui.AddSpacing();
            gui.BeginHorizontal();
            gui.BeginHorizontal(width: gui.GetLayoutWidth() * 0.6f);
            gui.IntEdit(ref intValue, step: showPlusMinusButtons ? 1 : 0);
            gui.EndHorizontal();
            gui.Text(Format(" intValue = ", intValue));
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
                gui.TextAutoSize(Format("Grid cell ", i, "0"), gui.GridNextCell(ref grid));
            }
            gui.EndGrid(in grid);
        }

        private static void DrawStylePage(ImGui gui)
        {
            selectedTheme = GetThemeIndex(ImTheme.Active.Name);
            
            gui.Text("Theme");
            if (gui.Dropdown(ref selectedTheme, themes, defaultLabel: "Unknown"))
            {
                ImTheme.Active = CreateTheme(selectedTheme);
            }
            
            gui.Text(Format("Text Size: ", ImTheme.Active.Controls.TextSize));
            gui.Slider(ref ImTheme.Active.Controls.TextSize, 6, 128);
            
            gui.Text(Format("Spacing: ", ImTheme.Active.Controls.ControlsSpacing));
            gui.Slider(ref ImTheme.Active.Controls.ControlsSpacing, 0, 32);
            
            gui.Text( Format("Extra Row Size: ", ImTheme.Active.Controls.ExtraRowHeight));
            gui.Slider(ref ImTheme.Active.Controls.ExtraRowHeight, 0, 32);

            gui.Text( Format("Indent: ", ImTheme.Active.Controls.Indent));
            gui.Slider(ref ImTheme.Active.Controls.Indent, 0, 32);
            
            if (gui.Button("Reset"))
            {
                ImTheme.Active = CreateTheme(selectedTheme);
            }
        }

        private static void DrawOtherPage(ImGui gui)
        {
            gui.Checkbox(ref showDebugWindow, "Show Debug Window");
        }

        private static int GetThemeIndex(string name)
        {
            for (int i = 0; i < themes.Length; ++i)
            {
                if (themes[i] == name)
                {
                    return i;
                }
            }

            return -1;
        }

        private static ImTheme CreateTheme(int index)
        {
            return index switch
            {
                1 => ImDarkTheme.Create(),
                _ => ImLightTheme.Create()
            };
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