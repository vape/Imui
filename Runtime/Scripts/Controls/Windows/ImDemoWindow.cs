using System;
using System.Buffers;
using System.Collections.Generic;
using Imui.Controls.Styling;
using Imui.Controls.Styling.Themes;
using Imui.Core;
using Imui.IO.Events;
using UnityEngine;

namespace Imui.Controls.Windows
{
    [Flags]
    internal enum ImDemoEnumFlags
    {
        None = 0,
        Flag1 = 1,
        Flag2 = 2,
        Flag3 = 4,
        Flag1_And_3 = Flag1 | Flag3
    }

    internal struct ImDemoTreeNode
    {
        public string Name;
        public bool Root;
        public bool Expanded;
        public int[] Nodes;

        public ImDemoTreeNode(string name, bool root, params int[] nodes)
        {
            Name = name;
            Root = root;
            Nodes = nodes;
            Expanded = false;
        }
    }

    public static class ImDemoWindow
    {
        private static char[] formatBuffer = new char[256];

        private static bool checkboxValue;
        private static int selectedValue = -1;
        private static float bouncingBallSize = 22;
        private static float bouncingBallSpeed = 1;
        private static int bouncingBallTrail = 32;
        private static float bouncingBallTime;
        private static int selectedTheme;
        private static string[] themes = { ImLightTheme.NAME, ImDarkTheme.NAME };

        private static string[] values =
        {
            "Value 1", "Value 2", "Value 3", "Value 4", "Value 5", "Value 6", "Value 7", "Value 8", "Value 9", "Value 10", "Value 11", "Value 12"
        };

        private static string singleLineText = "Single line text edit";
        private static string multiLineText = "Multiline text\nedit";
        private static float floatValue;
        private static int intValue;
        private static bool isReadOnly;
        private static bool customDropdownOpen;
        private static ImDropdownPreviewType dropdownPreview;
        private static bool[] checkboxes = new bool[4];
        private static bool showDebugWindow;
        private static bool showLogWindow;
        private static int clicks;
        private static int nestedFoldouts;
        private static bool showPlusMinusButtons = true;
        private static ImDemoEnumFlags demoFlags;

        private static int selectedNode = -1;
        private static ImDemoTreeNode[] treeNodes = new[]
        {
            new ImDemoTreeNode("Node 0", true, 1, 2),
            new ImDemoTreeNode("Node 1", false),
            new ImDemoTreeNode("Node 2", false, 3),
            new ImDemoTreeNode("Node 3", false),
            new ImDemoTreeNode("Node 4", true)
        };

        private static HashSet<int> selectedValues = new HashSet<int>(values.Length);
        private static ImConsoleWindow consoleWindow;

        public static void Draw(ImGui gui)
        {
            gui.BeginWindow("Demo", width: 700, height: 700);

            gui.BeginFoldout(out var controlsOpen, "Controls");
            gui.BeginIndent();
            if (controlsOpen)
            {
                DrawControlsPage(gui);
            }

            gui.EndIndent();
            gui.EndFoldout();

            gui.BeginReadOnly(isReadOnly);

            gui.BeginFoldout(out var layoutOpen, "Layout");
            gui.BeginIndent();
            if (layoutOpen)
            {
                DrawLayoutPage(gui);
            }

            gui.EndIndent();
            gui.EndFoldout();

            gui.BeginFoldout(out var styleOpen, "Style");
            gui.BeginIndent();
            if (styleOpen)
            {
                DrawStylePage(gui);
            }

            gui.EndIndent();
            gui.EndFoldout();

            gui.BeginFoldout(out var otherOpen, "Other");
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

            if (showLogWindow)
            {
                if (consoleWindow == null)
                {
                    consoleWindow = new ImConsoleWindow();
                }

                gui.PushId("DemoLogWindow");
                consoleWindow.Draw(gui);
                gui.PopId();
            }
        }

        private static void DrawControlsPage(ImGui gui)
        {
            gui.Checkbox(ref isReadOnly, "Read Only");

            gui.BeginReadOnly(isReadOnly);

            gui.AddSpacingIfLayoutFrameNotEmpty();
            gui.BeginHorizontal();
            if (gui.Button(Format("Clicks ", clicks, "0"), ImSizeType.Fit))
            {
                clicks++;
            }

            if (gui.Button("Reset Clicks", ImSizeType.Auto))
            {
                clicks = 0;
            }

            gui.EndHorizontal();

            gui.Checkbox(ref checkboxValue, "Checkbox");
            gui.AddSpacingIfLayoutFrameNotEmpty();
            gui.BeginHorizontal();
            gui.Text("Dropdown preview mode: ");
            gui.Radio(ref dropdownPreview);
            gui.EndHorizontal();
            gui.Dropdown(ref selectedValue, values, defaultLabel: "Dropdown without value selected", preview: dropdownPreview);
            gui.Text("Text editors");
            gui.TextEdit(ref singleLineText, multiline: false);
            gui.TextEdit(ref multiLineText, multiline: true);
            gui.Text("Sliders");
            DrawBouncingBall(gui);
            gui.Slider(ref bouncingBallSize, 0.1f, gui.GetRowHeight(), format: "0.00 px");
            gui.Slider(ref bouncingBallSpeed, -2f, 2f, format: "0.0# speed");
            gui.Slider(ref bouncingBallTrail, 1, 128, format: "0 trail length");
            gui.Text("Selection list (you can select multiple values)");
            gui.BeginList((gui.GetLayoutWidth(), ImList.GetEnclosingHeight(gui.GetRowsHeightWithSpacing(3))));
            for (int i = 0; i < values.Length; ++i)
            {
                var wasSelected = selectedValues.Contains(i);
                if (gui.ListItem(wasSelected, values[i]))
                {
                    if (wasSelected)
                    {
                        selectedValues.Remove(i);
                    }
                    else
                    {
                        selectedValues.Add(i);
                    }
                }
            }

            gui.EndList();

            gui.Text("Numeric editors");
            gui.Checkbox(ref showPlusMinusButtons, "Show +/-");

            gui.AddSpacingIfLayoutFrameNotEmpty();
            gui.BeginHorizontal();
            gui.BeginHorizontal(width: gui.GetLayoutWidth() * 0.6f);
            gui.FloatEdit(ref floatValue, format: "0.00#####", step: showPlusMinusButtons ? 0.01f : 0.0f);
            gui.EndHorizontal();
            gui.Text(Format(" floatValue = ", floatValue, "0.0######"));
            gui.EndHorizontal();

            gui.AddSpacingIfLayoutFrameNotEmpty();
            gui.BeginHorizontal();
            gui.BeginHorizontal(width: gui.GetLayoutWidth() * 0.6f);
            gui.IntEdit(ref intValue, step: showPlusMinusButtons ? 1 : 0);
            gui.EndHorizontal();
            gui.Text(Format(" intValue = ", intValue));
            gui.EndHorizontal();

            gui.AddSpacingIfLayoutFrameNotEmpty();
            gui.Text("Radio buttons (enum flags)");
            gui.Radio(ref demoFlags);

            gui.Text("Tree");

            void Node(ImGui gui, int index, ImDemoTreeNode[] nodes)
            {
                ref var node = ref nodes[index];
                var selected = selectedNode == index ? ImTreeNodeState.Selected : ImTreeNodeState.None;
                var expanded = node.Expanded ? ImTreeNodeState.Expanded : ImTreeNodeState.None;
                var state = selected | expanded;
                var changed = gui.BeginTreeNode(node.Name, ref state, node.Nodes?.Length == 0 ? ImTreeNodeFlags.NonExpandable : ImTreeNodeFlags.None);
                if (changed)
                {
                    node.Expanded = state.HasFlag(ImTreeNodeState.Expanded);
                    
                    if ((state & ImTreeNodeState.Selected) != 0)
                    {
                        selectedNode = index;
                    }
                }
                
                if (node is { Expanded: true, Nodes: not null })
                {
                    for (int i = 0; i < node.Nodes.Length; ++i)
                    {
                        Node(gui, node.Nodes[i], nodes);
                    }
                }
                gui.EndTreeNode();
            }

            for (int i = 0; i < treeNodes.Length; ++i)
            {
                if (treeNodes[i].Root)
                {
                    Node(gui, i, treeNodes);
                }
            }

            if (selectedNode >= 0 && selectedNode < treeNodes.Length)
            {
                gui.Text(gui.Formatter.Join("Selected node: ", treeNodes[selectedNode].Name));
            }

            gui.Text("Some custom controls");
            CustomDropdown(gui);
            NestedFoldout(gui, 0, ref nestedFoldouts);

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

            gui.Text(Format("Extra Row Size: ", ImTheme.Active.Controls.ExtraRowHeight));
            gui.Slider(ref ImTheme.Active.Controls.ExtraRowHeight, 0, 32);

            gui.Text(Format("Indent: ", ImTheme.Active.Controls.Indent));
            gui.Slider(ref ImTheme.Active.Controls.Indent, 0, 32);

            if (gui.Button("Reset"))
            {
                ImTheme.Active = CreateTheme(selectedTheme);
            }
        }

        private static void DrawOtherPage(ImGui gui)
        {
            gui.Checkbox(ref showDebugWindow, "Show Debug Window");
            gui.Checkbox(ref showLogWindow, "Show Log Window");
        }

        public static void DrawBouncingBall(ImGui gui)
        {
            float mod(float x, float y)
            {
                return ((x % y) + y) % y;
            }

            var bounds = gui.AddLayoutRectWithSpacing(gui.GetLayoutWidth(), gui.GetRowHeight());
            var dt = Time.deltaTime * bouncingBallSpeed;

            bouncingBallTime += dt;

            for (int i = 0; i < bouncingBallTrail; ++i)
            {
                var t = mod((bouncingBallTime + (i * 0.01f * bouncingBallSpeed)), 2.0f);
                var x = t <= 1.0f ? t : 1 - (t - 1);
                var p = bounds.GetPointAtNormalPosition(x, 0.5f);
                var c = ImTheme.Active.Text.Color.WithAlpha((byte)(255 * Mathf.Pow((i + 1) / (float)bouncingBallTrail, 2)));

                gui.Canvas.Circle(p, bouncingBallSize * 0.5f, c);
            }
        }

        public static void CustomDropdown(ImGui gui)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            var controlId = gui.GetNextControlId();

            gui.BeginDropdown(controlId, ref customDropdownOpen, default);
            {
                var textSettings = new ImTextSettings(ImTheme.Active.Controls.TextSize, 0.0f, 0.5f);

                gui.Text("Boxes ticked: ", textSettings);

                for (int i = 0; i < checkboxes.Length; ++i)
                {
                    gui.Text(checkboxes[i] ? "X" : "-", textSettings);
                }

                if (customDropdownOpen)
                {
                    ImDropdown.BeginList(gui, 1);
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

                    ImDropdown.EndList(gui, out var closeClicked);

                    if (closeClicked)
                    {
                        customDropdownOpen = false;
                    }
                }
            }
            gui.EndDropdown();
        }

        public static void NestedFoldout(ImGui gui, int current, ref int total)
        {
            const int MAX = 8;

            var label = current == 0 ? "Nested Foldout" : Format("Nested Foldout ", current, "0");

            gui.BeginFoldout(out var nestedFoldoutOpen, label);
            gui.BeginIndent();
            if (nestedFoldoutOpen)
            {
                if (current < total)
                {
                    NestedFoldout(gui, current + 1, ref total);
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