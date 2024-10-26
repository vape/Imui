using System;
using System.Collections.Generic;
using Imui.Core;
using Imui.Style;
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
        Flag1And3 = Flag1 | Flag3
    }

    internal struct ImDemoTreeNode
    {
        public string Name;
        public ImDemoTreeNode[] Childrens;

        public ImDemoTreeNode(string name, params ImDemoTreeNode[] childrens)
        {
            Name = name;
            Childrens = childrens;
        }
    }

    public static class ImDemoWindow
    {
        private static string[] themes =
        {
            nameof(ImThemeBuiltin.Light), 
            nameof(ImThemeBuiltin.Dark), 
            nameof(ImThemeBuiltin.Dear)
        };

        private static ImTheme CreateTheme(int index)
        {
            return index switch
            {
                2 => ImThemeBuiltin.Dear(),
                1 => ImThemeBuiltin.Dark(),
                _ => ImThemeBuiltin.Light()
            };
        }
        
        private static char[] formatBuffer = new char[256];

        private static bool checkboxValue;
        private static int selectedValue = -1;
        private static float bouncingBallSize = 22;
        private static float bouncingBallSpeed = 1;
        private static int bouncingBallTrail = 32;
        private static float bouncingBallTime;
        private static int selectedTheme;

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

        private static bool selectMultipleValues = false;
        private static HashSet<string> selectedNodes = new HashSet<string>(8);
        private static readonly ImDemoTreeNode[] treeNodes = new[]
        {
            new ImDemoTreeNode("Node 0", 
                new ImDemoTreeNode("Node 1"), 
                new ImDemoTreeNode("Node 2")),
            new ImDemoTreeNode("Node 3"),
            new ImDemoTreeNode("Node 4", 
                new ImDemoTreeNode("Node 5", 
                    new ImDemoTreeNode("Node 6"), 
                    new ImDemoTreeNode("Node 7")))
        };

        private static HashSet<int> selectedValues = new HashSet<int>(values.Length);
        private static ImConsoleWindow consoleWindow;

        public static void Draw(ImGui gui, ref bool open)
        {
            if (!gui.BeginWindow("Demo", ref open, (700, 700), ImWindowFlag.HasMenuBar))
            {
                return;
            }

            gui.BeginWindowMenuBar();
            DrawMenuBarItems(gui, ref open);
            gui.EndWindowMenuBar();

            if (gui.BeginFoldout("Controls"))
            {
                gui.BeginIndent();
                DrawControlsPage(gui, ref open);
                gui.EndIndent();
                
                gui.EndFoldout();
            }

            gui.BeginReadOnly(isReadOnly);

            if (gui.BeginFoldout("Layout"))
            {
                gui.BeginIndent();
                DrawLayoutPage(gui);
                gui.EndIndent();
                
                gui.EndFoldout();
            }

            if (gui.BeginFoldout("Style"))
            {
                gui.BeginIndent();
                DrawStylePage(gui);
                gui.EndIndent();
                
                gui.EndFoldout();
            }

            if (gui.BeginFoldout("Other"))
            {
                gui.BeginIndent();
                DrawOtherPage(gui);
                gui.EndIndent();
                gui.EndFoldout();
            }

            gui.EndReadOnly();

            gui.EndWindow();

            gui.PushId("DemoDebugWindow");
            ImDebugWindow.Draw(gui, ref showDebugWindow);
            gui.PopId();
            
            if (showLogWindow && consoleWindow == null)
            {
                consoleWindow = new ImConsoleWindow();
            }

            if (consoleWindow != null)
            {
                gui.PushId("DemoLogWindow");
                consoleWindow.Draw(gui, ref showLogWindow);
                gui.PopId();
            }
        }

        private static void DrawControlsPage(ImGui gui, ref bool open)
        {
            gui.Checkbox(ref isReadOnly, "Read Only");

            gui.BeginReadOnly(isReadOnly);

            gui.AddSpacingIfLayoutFrameNotEmpty();
            gui.BeginHorizontal();
            if (gui.Button(Format("Clicks ", clicks, "0"), ImSizeMode.Fit))
            {
                clicks++;
            }

            if (gui.Button("Reset Clicks", ImSizeMode.Auto))
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
            gui.Separator("Text editors");
            gui.TextEdit(ref singleLineText, multiline: false);
            gui.TextEdit(ref multiLineText, multiline: true);
            gui.Separator("Sliders (with tooltips)");
            DrawSlidersDemo(gui);
            gui.Separator("Selection list (you can select multiple values)");
            gui.BeginList((gui.GetLayoutWidth(), ImList.GetEnclosingHeight(gui, gui.GetRowsHeightWithSpacing(3))));
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

            gui.Separator("Numeric editors");
            gui.Checkbox(ref showPlusMinusButtons, "Show +/-");

            gui.AddSpacingIfLayoutFrameNotEmpty();
            gui.BeginHorizontal();
            gui.BeginHorizontal(width: gui.GetLayoutWidth() * 0.6f);
            gui.NumericEdit(ref floatValue, format: "0.00#####", step: showPlusMinusButtons ? 0.05f : 0.0f);
            gui.EndHorizontal();
            gui.Text(Format(" floatValue = ", floatValue, "0.0######"));
            gui.EndHorizontal();

            gui.AddSpacingIfLayoutFrameNotEmpty();
            gui.BeginHorizontal();
            gui.BeginHorizontal(width: gui.GetLayoutWidth() * 0.6f);
            gui.NumericEdit(ref intValue, step: showPlusMinusButtons ? 1 : 0);
            gui.EndHorizontal();
            gui.Text(Format(" intValue = ", intValue));
            gui.EndHorizontal();

            gui.AddSpacingIfLayoutFrameNotEmpty();
            gui.Separator("Radio buttons (enum flags)");
            gui.Radio(ref demoFlags);

            gui.Separator("Trees");
            DrawTreeDemo(gui);

            gui.Separator("Nested Foldout");
            NestedFoldout(gui, 0, ref nestedFoldouts);

            gui.Separator("Floating menu");
            gui.BeginMenuBar();
            DrawMenuBarItems(gui, ref open);
            gui.EndMenuBar();

            gui.EndReadOnly();
        }

        private static void DrawSelectableTreeDemo(ImGui gui)
        {
            gui.Checkbox(ref selectMultipleValues, "Select multiple values");
            
            gui.BeginHorizontal();
            gui.Text("Selected nodes: ");
            foreach (var name in selectedNodes)
            {
                gui.Text(name);
                gui.AddSpacing();
            }
            gui.EndHorizontal();

            void SetSelected(string name, bool selected)
            {
                if (selected)
                {
                    if (!selectMultipleValues)
                    {
                        selectedNodes.Clear();
                    }

                    selectedNodes.Add(name);
                }
                else
                {
                    selectedNodes.Remove(name);
                }
            }
            
            void Node(ref ImDemoTreeNode node)
            {
                var flags = selectMultipleValues ? ImTreeNodeFlags.UnselectOnClick : ImTreeNodeFlags.None;
                var isSelected = selectedNodes.Contains(node.Name);

                if (node.Childrens.Length == 0)
                {
                    gui.TreeNode(ref isSelected, node.Name, flags: flags);
                    SetSelected(node.Name, isSelected);
                    return;
                }

                if (!gui.BeginTreeNode(ref isSelected, node.Name, flags: flags))
                {
                    SetSelected(node.Name, isSelected);
                    return;
                }
                
                SetSelected(node.Name, isSelected);
                    
                for (int i = 0; i < node.Childrens.Length; ++i)
                {
                    Node(ref node.Childrens[i]);
                }
                    
                gui.EndTreeNode();
            }

            for (int i = 0; i < treeNodes.Length; ++i)
            {
                Node(ref treeNodes[i]);
            }
        }

        private static void DrawTreeDemo(ImGui gui)
        {
            gui.TreeNode("Node 0");
            if (gui.BeginTreeNode("Node 1"))
            {
                gui.TreeNode("Node 3");
                if (gui.BeginTreeNode("Node 4"))
                {
                    gui.TreeNode("Node 5");
                    gui.EndTreeNode();
                }
                gui.EndTreeNode();
            }
            gui.TreeNode("Node 5");
            if (gui.BeginTreeNode("Selectable nodes demo"))
            {
                DrawSelectableTreeDemo(gui);
                gui.EndTreeNode();
            }
        }

        private static void DrawSlidersDemo(ImGui gui)
        {
            DrawBouncingBall(gui);
            gui.Slider(ref bouncingBallSize, 0.1f, gui.GetRowHeight(), format: "0.00 px");
            gui.TooltipAtControl("Size of the circles in pixels");
            gui.Slider(ref bouncingBallSpeed, -2f, 2f, format: "0.0# speed");
            gui.TooltipAtControl("Speed for circles moving");
            gui.Slider(ref bouncingBallTrail, 1, 256, format: "0 trail length", flags: ImSliderFlag.DynamicHandle);
            gui.TooltipAtControl("Number of circles drawn");
        }

        private static void DrawMenuBarItems(ImGui gui, ref bool windowOpen)
        {
            if (gui.BeginMenuBarItem("Demo"))
            {
                DrawDemoMenu(gui, ref windowOpen);
                gui.EndMenuBarItem();
            }

            if (gui.BeginMenuBarItem("Windows"))
            {
                DrawExamplesMenu(gui);
                gui.EndMenuBarItem();
            }
        }

        private static void DrawDemoMenu(ImGui gui, ref bool windowOpen)
        {
            if (gui.BeginSubMenu("Custom Menus"))
            {
                gui.BeginVertical(width: 300);
                DrawSlidersDemo(gui);
                gui.EndVertical();
                
                gui.EndSubMenu();
            }
            if (gui.BeginSubMenu("Recursive"))
            {
                DrawDemoMenu(gui, ref windowOpen);
                gui.EndSubMenu();
            }
            gui.Separator();
            if (gui.BeginSubMenu("Test"))
            {           
                if (gui.BeginSubMenu("Same name submenu"))
                {
                    gui.MenuItem("Item");
                    gui.EndSubMenu();
                }

                gui.PushId("Next Menu");
                if (gui.BeginSubMenu("Same name submenu"))
                {
                    gui.MenuItem("Item");
                    gui.EndSubMenu();
                }
                gui.PopId();
                
                gui.EndSubMenu();
            }
            gui.Separator();
            if (gui.MenuItem("Close"))
            {
                windowOpen = false;
            }
        }

        private static void DrawExamplesMenu(ImGui gui)
        {
            if (gui.MenuItem("Console"))
            {
                showLogWindow = true;
            }

            if (gui.MenuItem("Debug"))
            {
                showDebugWindow = true;
            }
        }
        
        private static void DrawLayoutPage(ImGui gui)
        {
            gui.AddSpacing();

            gui.BeginHorizontal();
            for (int i = 0; i < 3; ++i)
            {
                gui.Button("Horizontal", ImSizeMode.Fit);
            }

            gui.EndHorizontal();

            gui.AddSpacing();

            gui.BeginVertical();
            for (int i = 0; i < 3; ++i)
            {
                gui.Button("Vertical", ImSizeMode.Fit);
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

        // TODO (artem-s): this does not reflect actual changes in theme
        private static void DrawStylePage(ImGui gui)
        {
            gui.Text("Theme");
            if (gui.Dropdown(ref selectedTheme, themes, defaultLabel: "Unknown"))
            {
                gui.SetTheme(CreateTheme(selectedTheme));
            }

            gui.Text(Format("Text Size: ", gui.Style.Layout.TextSize));
            gui.Slider(ref gui.Style.Layout.TextSize, 6, 128);

            gui.Text(Format("Spacing: ", gui.Style.Layout.Spacing));
            gui.Slider(ref gui.Style.Layout.Spacing, 0, 32);

            gui.Text(Format("Extra Row Size: ", gui.Style.Layout.ExtraRowHeight));
            gui.Slider(ref gui.Style.Layout.ExtraRowHeight, 0, 32);

            gui.Text(Format("Indent: ", gui.Style.Layout.Indent));
            gui.Slider(ref gui.Style.Layout.Indent, 0, 32);

            if (gui.Button("Reset"))
            {
                gui.SetTheme(CreateTheme(selectedTheme));
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

            var bounds = gui
                         .AddLayoutRectWithSpacing(gui.GetLayoutWidth(), gui.GetRowHeight() * 1.25f)
                         .WithPadding(left: bouncingBallSize / 2.0f, right: bouncingBallSize / 2.0f);
            var dt = Time.deltaTime * bouncingBallSpeed;

            bouncingBallTime += dt;

            for (int i = 0; i < bouncingBallTrail; ++i)
            {
                var t = mod((bouncingBallTime + (i * 0.01f * bouncingBallSpeed)), 2.0f);
                var x = t <= 1.0f ? t : 1 - (t - 1);
                var y = 0.5f + Mathf.Sin((bouncingBallTime + (i * 0.01f * bouncingBallSpeed)) * Mathf.PI * 2) * 0.25f;
                var p = bounds.GetPointAtNormalPosition(x, y);
                var c = gui.Style.Text.Color.WithAlphaF(Mathf.Pow((i + 1) / (float)bouncingBallTrail, 6));

                gui.Canvas.Circle(p, bouncingBallSize * 0.5f, c);
            }
        }
        
        public static void NestedFoldout(ImGui gui, int current, ref int total)
        {
            const int MAX = 8;

            var label = current == 0 ? "Nested Foldout" : Format("Nested Foldout ", current, "0");

            if (!gui.BeginFoldout(label))
            {
                return;
            }
            
            gui.BeginIndent();
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
            gui.EndIndent();
                
            gui.EndFoldout();
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