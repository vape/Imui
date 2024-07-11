using Imui.Controls.Styling.Themes;

namespace Imui.Controls.Styling
{
    public struct ImTheme
    {
        public static ImTheme Active = ImDefaultTheme.Create();

        public ImControlsStyle Controls;
        public ImWindowStyle Window;
        public ImTextStyle Text;
        public ImButtonStyle Button;
        public ImCheckboxStyle Checkbox;
        public ImFoldoutStyle Foldout;
        public ImPanelStyle Panel;
        public ImScrollStyle Scroll;
        public ImTextEditStyle TextEdit;
        public ImDropdownStyle Dropdown;
        public ImSliderStyle Slider;
    }
}