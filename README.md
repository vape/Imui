# Imui

Immediate mode GUI framework made specifically for Unity. Written in pure C#, has zero per-frame allocations, is somewhat performant, has no external dependencies, and works on basically any platform that Unity supports and that has either a touchscreen or mouse and keyboard. A WebGL demo can be seen [here](https://vape.github.io/imui_demo_060/).

## Screenshot

![Screenshot](https://raw.githubusercontent.com/vape/Imui/screenshots/Screenshots/screenshot_2.png)

## How it works

The basic principle is the same as in Dear ImGui: every frame, we generate a mesh with all the controls in it, split into different draw calls when needed, and render it to the render texture. For simple cases, the UI can be drawn with a single draw call (when no masks or different materials are involved). Using the `ImuiUnityGUIBackend` component, the whole UI can be integrated into the `Canvas` hierarchy and used like any other UGUI component.

## Installation

You can install Imui as a git package in Unity Package Manager

## Supported Controls

* Window
* Button
* Checkbox
* Slider
* Label
* Image
* Text/Number Editor
* Dropdown
* Scroll Bar
* Foldout
* Separator
* Tree
* Radio Group
* Listbox
* Menu
* Tooltip
* Color Picker
* Tabs Pane
* Table

## Notes

At this stage, it's not ready for, well, anything except poking and tweaking around. Any existing API could be changed anytime if I have the desire or time to continue developing it.
