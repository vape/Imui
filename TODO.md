### Urgent
- ~~Add simple arena allocator~~
- ~~Implement separate control for numeric input~~
  - ~~Add +/- buttons for easier input~~
- ~~Split dropdown control into preview, dropdown button and popup menu with items~~
- ~~Sliders should have its current value drawn over the knob~~
  - ~~With custom format~~
- ~~Radio buttons~~
- ~~Make API C# properties friendly~~
- ~~Make text edit buffer generic and remove all conditions~~ - Well, nice solution is not possible yet until Unity adapts c#11
- ~~De-clutter controls~~
  - ~~Parameters order should follow:~~
    1. this ImGui
    2. ID (if present)
    3. State
    4. Visual
    5. Size
    6. Optional
  - ~~Remove duplication~~

### Better do
- Reorder demo controls from basic to more advanced
- ~~Fix all the warnings in code~~
- Handle double clicks in ImTextEdit to select words
- Implement table layout
- Implement tabs
  - Vertical tabs
  - Horizontal tabs
- ~~Implement foldable header control~~ (well, it is basically tree control)
- ~~Tree control~~
- Text drawing overhaul
  - __Word__ wrapping
  - Add support for (basic subset of) rich text
  - Optimize layout and rendering
- Fix touch keyboard in webgl
- In code documentation
- Simplify styling and fix the current themes as they look kinda ugly
- Implement slider inside numeric editor
- Show min/max value inside slider
- Make slider looking similar to sliders in other GUI libraries, dearimgui look seems off
- Implement tooltips on mouse hover
- Fix that 2px border in adjacent controls
- Plotting control
- Property drawers, like in Unity's IMGUI

### Maybe
- Unit tests
- ~~TextEdit as dropdown preview area~~
- More demo windows
  - ~~Unity console, perhaps~~
  - Inspector
- Text edit preview hint
- ~~Slider steps~~
- Better API for combined controls
- Theming simplification
  - Maybe even overall simplification, some of the controls API seems unnecessary complex
- Controls API for non-ref values (because getters are everywhere, duh)