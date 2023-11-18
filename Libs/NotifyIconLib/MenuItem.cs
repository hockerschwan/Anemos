using System.Drawing;

namespace NotifyIconLib;

public class MenuItem
{
    // uIDNewItem for AppendMenuW
    internal uint? ItemId_ { get; set; } = null;

    public MenuItemType Type { get; set; } = MenuItemType.Default;

    public string Text { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public bool IsChecked { get; set; } = false;

    public int RadioGroup { get; set; } = 0;

    public Icon? Icon { get; set; } = null;

    public List<MenuItem> Children { get; set; } = [];

    ~MenuItem()
    {
        Children.Clear();
    }
}
