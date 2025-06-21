using System;
using System.Collections.Generic;
using Imui.Controls;
using Imui.Core;
using Imui.Rendering;
using Imui.Style;
using UnityEngine;

namespace Imui.Examples
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
        private static int selectedThemeIndex = 0;
        private static ImTheme[] themes = { CreateTheme(0), CreateTheme(1), CreateTheme(2), CreateTheme(3), CreateTheme(4), CreateTheme(5), CreateTheme(6) };

        private static string[] themeNames =
        {
            nameof(ImThemeBuiltin.Light), nameof(ImThemeBuiltin.Dark), nameof(ImThemeBuiltin.Dear), nameof(ImThemeBuiltin.Orange),
            nameof(ImThemeBuiltin.Terminal), nameof(ImThemeBuiltin.LightTouch), nameof(ImThemeBuiltin.DarkTouch)
        };

        private static ImTheme CreateTheme(int index)
        {
            return index switch
            {
                6 => ImThemeBuiltin.DarkTouch(),
                5 => ImThemeBuiltin.LightTouch(),
                4 => ImThemeBuiltin.Terminal(),
                3 => ImThemeBuiltin.Orange(),
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
        private static bool useNumericSlider = false;
        private static ImDemoEnumFlags demoFlags;
        private static int largeTableRows = 1024 * 128;
        private static int largeTableColumns = 512;
        private static float largeTableColumnSize = 150;
        private static bool largeTableScrollable = true;
        private static bool largeTableResizable = true;
        private static Vector2 vec2 = new Vector2(1.0f, 2.0f);
        private static Vector3 vec3 = new Vector3(1.0f, 2.0f, 3.0f);
        private static Vector4 vec4 = new Vector4(1.0f, 2.0f, 3.0f, 4.0f);
        private static Vector2Int vec2int = new Vector2Int(1, 2);
        private static Vector3Int vec3int = new Vector3Int(1, 2, 3);

        private static bool selectMultipleValues = false;
        private static HashSet<string> selectedNodes = new HashSet<string>(8);

        private static readonly ImDemoTreeNode[] treeNodes = new[]
        {
            new ImDemoTreeNode("Node 0",
                new ImDemoTreeNode("Node 1"),
                new ImDemoTreeNode("Node 2")),
            new ImDemoTreeNode("Node 3"), new ImDemoTreeNode("Node 4",
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

            if (gui.BeginFoldout("Tables"))
            {
                gui.BeginIndent();
                DrawTablesPage(gui);
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
            if (gui.BeginDropdown("Custom Dropdown", preview: dropdownPreview))
            {
                if (gui.MenuItem("Menu Item"))
                {
                    gui.CloseDropdown();
                }
                gui.TooltipAtLastControl("Will close dropdown on click");

                if (gui.BeginMenuItem("Sub Menu Inside Dropdown"))
                {
                    gui.Text("Hello there");
                    gui.EndMenuItem();
                }
                gui.Checkbox(ref checkboxValue, "Checkbox");
                gui.Separator("Nested dropdown, if that's want you really want");
                gui.Dropdown(ref selectedValue, values, defaultLabel: "Nothing", preview: dropdownPreview);
                gui.EndDropdown();
            }
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
            gui.BeginReadOnly(useNumericSlider);
            gui.Checkbox(ref showPlusMinusButtons, "Enable Plus/Minus buttons");
            gui.EndReadOnly();
            gui.Checkbox(ref useNumericSlider, "Enable Slider");

            var numericFlag = ImNumericEditFlag.None;
            numericFlag |= showPlusMinusButtons ? ImNumericEditFlag.PlusMinus : ImNumericEditFlag.None;
            numericFlag |= useNumericSlider ? ImNumericEditFlag.Slider : ImNumericEditFlag.None;

            gui.AddSpacingIfLayoutFrameNotEmpty();
            gui.BeginHorizontal();
            gui.BeginHorizontal(width: gui.GetLayoutWidth() * 0.6f);
            gui.NumericEdit(ref floatValue, step: 0.05f, flags: numericFlag);
            gui.EndHorizontal();
            gui.Text(Format(" floatValue = ", floatValue, "0.0######"));
            gui.EndHorizontal();

            gui.AddSpacingIfLayoutFrameNotEmpty();
            gui.BeginHorizontal();
            gui.BeginHorizontal(width: gui.GetLayoutWidth() * 0.6f);
            gui.NumericEdit(ref intValue, flags: numericFlag);
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

            gui.Separator("Tabs");
            gui.AddSpacing();
            gui.BeginTabsPane(gui.AddLayoutRect(gui.GetLayoutWidth(), gui.GetRowsHeightWithSpacing(2)));
            for (int i = 0; i < 4; ++i)
            {
                var label = gui.Formatter.Concat("Tab ", i);
                if (gui.BeginTab(label))
                {
                    gui.Text(label);
                    gui.EndTab();
                }
            }
            gui.EndTabsPane();
            
            gui.Separator("Vectors (float)");
            gui.Text("Two component vector");
            gui.Vector(ref vec2);
            gui.Text("Three component vector");
            gui.Vector(ref vec3);
            gui.Text("Four component vector");
            gui.Vector(ref vec4);
            gui.Separator("Vectors (int)");
            gui.Text("Two component vector");
            gui.Vector(ref vec2int);
            gui.Text("Three component vector");
            gui.Vector(ref vec3int);

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
            gui.SliderHeader("Size", bouncingBallSize, "0.00 px");
            gui.Slider(ref bouncingBallSize, 0.1f, gui.GetRowHeight());
            gui.TooltipAtLastControl("Size of the circles in pixels");
            gui.SliderHeader("Speed", bouncingBallSpeed, "0.##");
            gui.Slider(ref bouncingBallSpeed, -2f, 2f);
            gui.TooltipAtLastControl("Speed of moving circles");
            gui.SliderHeader("Trail Length", bouncingBallTrail);
            gui.Slider(ref bouncingBallTrail, 1, 256, step: 32, flags: ImSliderFlag.DynamicHandle);
            gui.TooltipAtLastControl("Number of circles drawn");
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
            if (gui.BeginMenuItem("Custom Menus"))
            {
                gui.BeginVertical(width: 300);
                DrawSlidersDemo(gui);
                gui.EndVertical();

                gui.EndMenuItem();
            }
            if (gui.BeginMenuItem("Recursive"))
            {
                DrawDemoMenu(gui, ref windowOpen);
                gui.EndMenuItem();
            }
            gui.Separator();
            if (gui.BeginMenuItem("Test"))
            {
                if (gui.BeginMenuItem("Same name submenu"))
                {
                    gui.MenuItem("Item");
                    gui.EndMenuItem();
                }

                gui.PushId("Next Menu");
                if (gui.BeginMenuItem("Same name submenu"))
                {
                    gui.MenuItem("Item");
                    gui.EndMenuItem();
                }
                gui.PopId();

                gui.EndMenuItem();
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

        private static void DrawStylePage(ImGui gui)
        {
            gui.Text("Theme");
            gui.BeginHorizontal();
            if (gui.Dropdown(ref selectedThemeIndex, themeNames, defaultLabel: "Unknown", size: (gui.GetLayoutWidth() * 0.6f, gui.GetRowHeight())))
            {
                gui.SetTheme(themes[selectedThemeIndex]);
            }
            gui.AddSpacing();

#if UNITY_WEBGL
            if (gui.Button("Copy"))
            {
                gui.Input.Clipboard = ImThemeEditor.BuildCodeString(in themes[selectedThemeIndex]);
            }
#endif
            
            if (gui.Button("Reset", ImSizeMode.Fill))
            {
                themes[selectedThemeIndex] = CreateTheme(selectedThemeIndex);
                gui.SetTheme(themes[selectedThemeIndex]);
            }

            gui.TooltipAtLastControl("Copies theme as code into clipboard");
            gui.EndHorizontal();

            if (ImThemeEditor.DrawEditor(gui, ref themes[selectedThemeIndex]))
            {
                gui.SetTheme(themes[selectedThemeIndex]);
            }
        }

        private static void DrawOtherPage(ImGui gui)
        {
            gui.Checkbox(ref showDebugWindow, "Show Debug Window");
            gui.Checkbox(ref showLogWindow, "Show Log Window");
        }

        private static void DrawTablesPage(ImGui gui)
        {
            if (gui.BeginTreeNode("Simple"))
            {
                gui.PrepareState(4);
                for (int row = 0; row < 5; ++row)
                {
                    gui.TableNextRow();
                    for (int col = 0; col < 4; ++col)
                    {
                        gui.TableNextColumn();
                        gui.Text(gui.Formatter.Concat("Hello At ", gui.Formatter.Format(col), ":", gui.Formatter.Format(row)));
                    }
                }
                gui.EndTable();

                gui.Separator("Resizable Columns");

                gui.PrepareState(4, flags: ImTableFlag.ResizableColumns);
                for (int row = 0; row < 5; ++row)
                {
                    gui.TableNextRow();
                    for (int col = 0; col < 4; ++col)
                    {
                        gui.TableNextColumn();
                        gui.Text(gui.Formatter.Concat("Hello At ", gui.Formatter.Format(col), ":", gui.Formatter.Format(row)), wrap: true);
                    }
                }
                gui.EndTable();

                gui.EndTreeNode();
            }

            if (gui.BeginTreeNode("With Scroll Bars"))
            {
                gui.PrepareState(4, (gui.GetLayoutWidth(), 200));
                for (int row = 0; row < 12; ++row)
                {
                    gui.TableNextRow();
                    for (int col = 0; col < 4; ++col)
                    {
                        gui.TableNextColumn();
                        gui.Text(gui.Formatter.Concat("Hello At ", gui.Formatter.Format(col), ":", gui.Formatter.Format(row)), true);
                    }
                }
                gui.EndTable();

                gui.EndTreeNode();
            }

            if (gui.BeginTreeNode("Large Tables"))
            {
                NumEditWithLabel(gui, ref largeTableRows, "Rows", min: 1, max: 1024 * 1024 * 4);
                NumEditWithLabel(gui, ref largeTableColumns, "Columns", min: 1, max: 4096);
                NumEditWithLabel(gui, ref largeTableColumnSize, "Col. Size", min: 50, max: 300);

                gui.Checkbox(ref largeTableResizable, "Resizable Columns");
                gui.Checkbox(ref largeTableScrollable, "Scrollable");

                var size = largeTableScrollable ? new ImSize(gui.GetLayoutWidth(), 300) : new ImSize(ImSizeMode.Auto);
                var flags = largeTableResizable ? ImTableFlag.ResizableColumns : ImTableFlag.None;

                ref var state = ref gui.PrepareState(largeTableColumns, size, flags);

                gui.TableSetRowsHeight(gui.GetTextLineHeight() + gui.Style.Table.CellPadding.Vertical);
                for (int i = 0; i < largeTableColumns; ++i)
                {
                    gui.TableSetColumnWidth(i, largeTableColumnSize);
                }

                var textSettings = new ImTextSettings(gui.Style.Layout.TextSize, new ImAlignment(0.5f, 0.5f), overflow: ImTextOverflow.Ellipsis);
                var rowsRange = gui.TableGetVisibleRows(largeTableRows);
                var colsRange = gui.TableGetVisibleColumns();

                for (int row = rowsRange.Min; row < rowsRange.Max; ++row)
                {
                    gui.TableSetRow(row, ref state);

                    for (int col = colsRange.Min; col < colsRange.Max; ++col)
                    {
                        gui.TableSetColumn(col, ref state);
                        gui.Text(gui.Formatter.Concat(gui.Formatter.Format(col), "x", gui.Formatter.Format(row)), textSettings);
                    }
                }

                gui.EndTable();

                gui.EndTreeNode();
            }
        }

        private static void NumEditWithLabel(ImGui gui, ref int value, ReadOnlySpan<char> label, int min, int max)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            gui.BeginHorizontal();
            gui.Text(label, gui.Layout.AddRect(150, gui.GetRowHeight()));
            gui.NumericEdit(ref value, min: min, max: max, flags: ImNumericEditFlag.PlusMinus);
            gui.EndHorizontal();
        }

        private static void NumEditWithLabel(ImGui gui, ref float value, ReadOnlySpan<char> label, float min, float max)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            gui.BeginHorizontal();
            gui.Text(label, gui.Layout.AddRect(150, gui.GetRowHeight()));
            gui.NumericEdit(ref value, min: min, max: max, flags: ImNumericEditFlag.PlusMinus);
            gui.EndHorizontal();
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
                var c = gui.Style.Text.Color.WithAlpha(Mathf.Pow((i + 1) / (float)bouncingBallTrail, 6));

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