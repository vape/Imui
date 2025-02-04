### TODO
- Plotting control
- Inspector example window
- Controls optimization
- Text edit preview hint
- Draw without UnityUI's Canvas

### Later
- Adapt to mobile touch input
- Tables
- In code documentation
- Rich text
- Fix touch keyboard in webgl
  - Apparently, it is bug in unity that prevents touchkeybaord to open in WebGL builds in versions prior to 2022.1 https://discussions.unity.com/t/touch-screen-keyboard-in-web-gl/892662/18 
- Better demo window organization 
- Unit tests
- Property drawers, like in Unity's IMGUI
- Show min/max value inside slider
- Range slider
- Progress bar
- Text clipping without additional draw call
- Draw inside unity's editor window

### 0.5
- ~~Color picker~~
- ~~Style using palettes~~
- ~~Context menus~~
- ~~Use menus instead of list in dropdown~~
- ~~Text truncation~~
- ~~Tabs~~
- ~~Do not allow to resize windows to negative values~~
- ~~Do not allow to move title bar outside screen rect~~
- ~~Controls should have their own reasonable minimal size~~
- ~~Theme editor~~
- ~~Implement slider inside numeric editor~~

### 0.4
- ~~Handle double clicks in ImTextEdit to select words~~
- ~~__Word__ wrapping~~
- ~~Optimize layout and rendering~~
- ~~Simplify styling and fix the current themes as they look kinda ugly~~
- ~~Make slider looking similar to sliders in other GUI libraries, dearimgui look seems off~~
- ~~Implement tooltips on mouse hover~~
- ~~Fix that 2px border in adjacent controls~~
- ~~Closeable windows~~
- ~~Menu Control~~
- ~~Pushing same InvColorMul as current should not result in additional draw call~~
- ~~No control should not modify passed value unless user interacts with it~~
  - ~~Slider~~
- ~~Separator with label~~
- ~~Control scopes~~
- ~~Fix memory alignment~~
  - ~~In ImStorage~~
  - ~~In ImArena~~