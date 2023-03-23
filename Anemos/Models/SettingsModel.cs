using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.Models;

public partial class SettingsModel : ObservableObject
{
    public Settings_Window Window { get; set; } = new();

    private bool _startMinimized = false;
    public bool StartMinimized
    {
        get => _startMinimized;
        set => SetProperty(ref _startMinimized, value);
    }

    private bool _startWithLogIn = false;
    public bool StartWithLogIn
    {
        get => _startWithLogIn;
        set => SetProperty(ref _startWithLogIn, value);
    }

    private int _updateInterval = 2;
    public int UpdateInterval
    {
        get => _updateInterval;
        set => SetProperty(ref _updateInterval, value);
    }
}

public class Settings_Window
{
    public bool Maximized { get; set; } = false;
    public int X { get; set; } = 100;
    public int Y { get; set; } = 100;
    public int Width { get; set; } = 900;
    public int Height { get; set; } = 720;
}
