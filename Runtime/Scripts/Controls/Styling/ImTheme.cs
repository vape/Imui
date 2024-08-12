using System;
using Imui.Controls.Styling.Themes;

namespace Imui.Controls.Styling
{
    [Serializable]
    public struct ImTheme
    {
        public static ImTheme Active = ImLightTheme.Create();

        public string Name;
        public ImControlsStyle Controls;
        public ImWindowStyle Window;
        public ImTextStyle Text;
        public ImButtonStyle Button;
        public ImCheckboxStyle Checkbox;
        public ImFoldoutStyle Foldout;
        public ImScrollStyle Scroll;
        public ImTextEditStyle TextEdit;
        public ImDropdownStyle Dropdown;
        public ImSliderStyle Slider;
        public ImListStyle List;
        public ImRadioStyle Radio;
    }
}