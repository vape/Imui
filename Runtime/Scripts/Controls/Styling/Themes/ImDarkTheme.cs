using UnityEngine;

namespace Imui.Controls.Styling.Themes
{
    public static class ImDarkTheme
    {
        public static ImTheme Create()
        {
            var theme = ImDefaultTheme.Create();
            
            theme.Window.Box.BackColor = new Color32(41, 41, 41, 255);
            theme.Window.Box.BorderColor = new Color32(31, 31, 31, 255);
            theme.Window.Box.BorderWidth = 2.0f;
            theme.Window.ResizeHandleColor = new Color32(255, 255, 255, 128);
            theme.Window.TitleBar.BackColor = new Color32(51, 51, 51, 255);
            theme.Window.TitleBar.FrontColor = new Color32(204, 204, 204, 255);
            theme.Text.Color = new Color32(204, 204, 204, 255);
            theme.Button.Normal.BackColor = new Color32(71, 71, 71, 255);
            theme.Button.Normal.FrontColor = new Color32(204, 204, 204, 255);
            theme.Button.Normal.BorderColor = new Color32(20, 20, 20, 255);
            theme.Button.Hovered.BackColor = new Color32(77, 77, 77, 255);
            theme.Button.Hovered.FrontColor = new Color32(230, 230, 230, 255);
            theme.Button.Hovered.BorderColor = new Color32(21, 21, 21, 255);
            theme.Button.Pressed.BackColor = new Color32(56, 56, 56, 255);
            theme.Button.Pressed.FrontColor = new Color32(179, 179, 179, 255);
            theme.Button.Pressed.BorderColor = new Color32(10, 10, 10, 255);
            theme.Panel.Box.BackColor = new Color32(41, 41, 41, 255);
            theme.Panel.Box.BorderColor = new Color32(20, 20, 20, 255);
            theme.Scroll.NormalState.BackColor = new Color32(20, 20, 20, 255);
            theme.Scroll.NormalState.FrontColor = new Color32(76, 76, 76, 255);
            theme.Scroll.HoveredState.BackColor = new Color32(20, 20, 20, 255);
            theme.Scroll.HoveredState.FrontColor = new Color32(102, 102, 102, 255);
            theme.Scroll.PressedState.BackColor = new Color32(20, 20, 20, 255);
            theme.Scroll.PressedState.FrontColor = new Color32(89, 89, 89, 255);
            theme.TextEdit.Normal.Box.BackColor = new Color32(31, 31, 31, 255);
            theme.TextEdit.Normal.Box.FrontColor = new Color32(153, 153, 153, 255);
            theme.TextEdit.Normal.Box.BorderColor = new Color32(20, 20, 20, 255);
            theme.TextEdit.Normal.SelectionColor = new Color32(0, 0, 0, 0);
            theme.TextEdit.Selected.Box.BackColor = new Color32(36, 36, 36, 255);
            theme.TextEdit.Selected.Box.FrontColor = new Color32(230, 230, 230, 255);
            theme.TextEdit.Selected.Box.BorderColor = new Color32(82, 82, 82, 255);
            theme.TextEdit.Selected.SelectionColor = new Color32(0, 153, 255, 82);
            theme.Dropdown.OptionButton.Normal.BackColor = new Color32(255, 255, 255, 10);
            theme.Dropdown.OptionButton.Normal.FrontColor = new Color32(179, 179, 179, 255);
            theme.Dropdown.OptionButton.Normal.BorderColor = new Color32(0, 0, 0, 0);
            theme.Dropdown.OptionButton.Hovered.BackColor = new Color32(255, 255, 255, 20);
            theme.Dropdown.OptionButton.Hovered.FrontColor = new Color32(204, 204, 204, 255);
            theme.Dropdown.OptionButton.Hovered.BorderColor = new Color32(0, 0, 0, 0);
            theme.Dropdown.OptionButton.Pressed.BackColor = new Color32(255, 255, 255, 5);
            theme.Dropdown.OptionButton.Pressed.FrontColor = new Color32(204, 204, 204, 255);
            theme.Dropdown.OptionButton.Pressed.BorderColor = new Color32(0, 0, 0, 0);
            theme.Dropdown.OptionButtonSelected.Normal.BackColor = new Color32(0, 107, 179, 255);
            theme.Dropdown.OptionButtonSelected.Normal.FrontColor = new Color32(204, 204, 204, 255);
            theme.Dropdown.OptionButtonSelected.Normal.BorderColor = new Color32(0, 0, 0, 0);
            theme.Dropdown.OptionButtonSelected.Hovered.BackColor = new Color32(1, 114, 191, 255);
            theme.Dropdown.OptionButtonSelected.Hovered.FrontColor = new Color32(230, 230, 230, 255);
            theme.Dropdown.OptionButtonSelected.Hovered.BorderColor = new Color32(0, 0, 0, 0);
            theme.Dropdown.OptionButtonSelected.Pressed.BackColor = new Color32(0, 91, 153, 255);
            theme.Dropdown.OptionButtonSelected.Pressed.FrontColor = new Color32(204, 204, 204, 255);
            theme.Dropdown.OptionButtonSelected.Pressed.BorderColor = new Color32(0, 0, 0, 0);
            theme.Slider.Box.BackColor = new Color32(33, 33, 33, 255);
            theme.Slider.Box.BorderColor = new Color32(20, 20, 20, 255);
            theme.Slider.Handle.Normal.BackColor = new Color32(71, 71, 71, 255);
            theme.Slider.Handle.Normal.FrontColor = new Color32(137, 146, 155, 255);
            theme.Slider.Handle.Normal.BorderColor = new Color32(0, 0, 0, 0);
            theme.Slider.Handle.Hovered.BackColor = new Color32(77, 77, 77, 255);
            theme.Slider.Handle.Hovered.FrontColor = new Color32(151, 161, 171, 255);
            theme.Slider.Handle.Hovered.BorderColor = new Color32(0, 0, 0, 0);
            theme.Slider.Handle.Pressed.BackColor = new Color32(102, 102, 102, 255);
            theme.Slider.Handle.Pressed.FrontColor = new Color32(123, 131, 140, 255);
            theme.Slider.Handle.Pressed.BorderColor = new Color32(0, 0, 0, 0);
            
            return theme;
        }
    }
}