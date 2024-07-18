# Imui

Immediate mode GUI framework made specifically for Unity. It has zero per-frame allocations, is somewhat performant, has no external dependencies, and works on basically any platform that Unity supports and that has either a touchscreen or mouse and keyboard. A WebGL demo can be seen [here](https://vape.github.io/imui_demo/).

## Screenshot

![Screenshot](https://raw.githubusercontent.com/vape/Imui/screenshots/Screenshots/screenshot_1.png)

## How it works

The basic principle is the same as in Dear ImGui: every frame, we generate a mesh with all the controls in it, split into different draw calls when needed, and render it to the render texture. For simple cases, the UI can be drawn with a single draw call (when no masks or different materials are involved). Using the `ImuiGraphic` component, the whole UI can be integrated into the `Canvas` hierarchy and used like any other UGUI component.

## Installation

You can install Imui as a git package in Unity Package Manager

## Themes

Dark and light themes are supported, check out demo [here](https://vape.github.io/imui_demo/).

## Controls

Supports a very basic set of controls:
* Window
* Button
* Checkbox
* Slider
* Label
* Image
* Text Editor
* Dropdown
* Scroll Bar

## Layout

Implements an automatic layout system with vertical, horizontal, and grid layout groups.

## Notes

At this stage, it's not ready for, well, anything except poking and tweaking around. Any existing API could be changed anytime if I have the desire or time to continue developing it.
