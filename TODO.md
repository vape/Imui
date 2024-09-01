### Urgent
- ~~Add simple arena allocator~~
- ~~Implement separate control for numeric input~~
  - ~~Add +/- buttons for easier input~~
- ~~Split dropdown control into preview, dropdown button and popup menu with items~~
- ~~Sliders should have its current value drawn over the knob~~
  - ~~With custom format~~
- ~~Radio buttons~~

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
- Unit tests

### Maybe
- ~~TextEdit as dropdown preview area~~
- More demo windows
  - ~~Unity console, perhaps~~
  - Inspector
- Plotting, at least very basic
- Text edit preview hint
- ~~Slider steps~~
- Better API for combined controls
- Theming simplification
  - Maybe even overall simplification, some of the controls API seems unnecessary complex
- Controls API for non-ref values (because getters are everywhere, duh)