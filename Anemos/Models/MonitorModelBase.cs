using System.Collections.ObjectModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using Anemos.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace Anemos.Models;

public enum MonitorSourceType
{
    Fan, Curve, Sensor
}

public enum MonitorDisplayType
{
    Current, History
}

public class MonitorColorThreshold : ObservableObject
{
    public double Threshold
    {
        get; set;
    }

    public bool IsNormal => double.IsFinite(Threshold);

    public Color Color
    {
        get; set;
    }

    public SolidBrush Brush => new(Color);

    public Microsoft.UI.Xaml.Media.SolidColorBrush SolidColorBrush
        => new(Windows.UI.Color.FromArgb(Color.A, Color.R, Color.G, Color.B));
}

public class MonitorArg
{
    public MonitorSourceType SourceType;
    public MonitorDisplayType DisplayType;
    public string Id = string.Empty;
    public string SourceId = string.Empty;
    public IEnumerable<Tuple<double, string>> Colors = Enumerable.Empty<Tuple<double, string>>();
}

public abstract class MonitorModelBase : ObservableObject
{
    private readonly ISettingsService _settingsService = App.GetService<ISettingsService>();
    private protected readonly INotifyIconMonitorService _monitorService = App.GetService<INotifyIconMonitorService>();

    private readonly NotifyIconLib.NotifyIconLib _notifyIconLib = NotifyIconLib.NotifyIconLib.Instance;

    private readonly string _id = string.Empty;
    public string Id => _id;

    private protected string _sourceId = string.Empty;
    public virtual string SourceId
    {
        get => _sourceId;
        set => SetProperty(ref _sourceId, value);
    }

    public virtual MonitorSourceType SourceType
    {
        get;
    }

    private MonitorDisplayType _displayType;
    public MonitorDisplayType DisplayType
    {
        get => _displayType;
        set
        {
            if (SetProperty(ref _displayType, value))
            {
                UpdateTooltip();
                Update(true);
                _monitorService.Save();
            }
        }
    }

    private double? _value = null;
    public double? Value
    {
        get => _value;
        protected set => SetProperty(ref _value, value);
    }

    private protected string DisplayedValue { get; set; } = string.Empty;

    public LimitedQueue History { get; } = new(14);

    public ObservableCollection<MonitorColorThreshold> Colors { get; } = [];

    private readonly Guid _guid;

    public readonly NotifyIconLib.NotifyIcon NotifyIcon;

    private readonly Bitmap _bitmap = new(16, 16);
    private readonly Graphics _graphics;

    private readonly Font _fontLarge = new(FontFamily.GenericSansSerif, 8);
    private readonly Font _fontSmall = new(FontFamily.GenericSansSerif, 7);

    private readonly SolidBrush _fontBrushBlack = new(Color.Black);
    private readonly SolidBrush _fontBrushWhite = new(Color.White);

    private readonly Pen _historyFramePen = new(Color.Gray);

    private protected readonly StringBuilder _stringBuilder = new();

    private readonly DispatcherQueueHandler _updateHandler;
    private readonly DispatcherQueueHandler _forcedUpdateHandler;

    public MonitorModelBase(MonitorArg arg)
    {
        _id = arg.Id;
        if (Id == string.Empty)
        {
            throw new ArgumentException("ID can't be empty.", nameof(arg));
        }

        _guid = Helpers.Helper.GenerateGuid(App.AppLocation + Id);
        _sourceId = arg.SourceId;
        _displayType = arg.DisplayType;

        _settingsService.Settings.PropertyChanged += Settings_PropertyChanged;

        _updateHandler = Update_;
        _forcedUpdateHandler = UpdateForced_;

        var colors = CreateColors(arg).ToList();
        Sort(ref colors);
        Colors = new(colors);

        _graphics = Graphics.FromImage(_bitmap);
        _graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        _graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        NotifyIcon = _notifyIconLib.CreateIcon(_guid);
        Update(true);

        static IEnumerable<MonitorColorThreshold> CreateColors(MonitorArg arg)
        {
            foreach (var color in arg.Colors.ToList())
            {
                if (int.TryParse(
                    color.Item2,
                    System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var n))
                {
                    yield return new MonitorColorThreshold
                    {
                        Threshold = color.Item1,
                        Color = Color.FromArgb((n & 0xFFFFFF) - 16777216)
                    };
                }
                else
                {
                    yield return new MonitorColorThreshold
                    {
                        Threshold = color.Item1,
                        Color = Color.Black
                    };
                }
            }
        }
    }

    public void AddColor()
    {
        var t = Colors.Max(x => x.Threshold);
        if (!double.IsNormal(t)) { t = 0.0; }

        Colors.Add(new MonitorColorThreshold
        {
            Threshold = t + 10.0,
            Color = Color.Black
        });
        Update(true);
        _monitorService.Save();
    }

    private SolidBrush CalcFgColor(Color bgColor)
    {
        var lumBg = CalcLuminance(bgColor);

        var contrastBlack = (lumBg + 0.05) / (CalcLuminance(Color.Black) + 0.05);
        var contrastWhite = (CalcLuminance(Color.White) + 0.05) / (lumBg + 0.05);

        return contrastBlack > contrastWhite ? _fontBrushBlack : _fontBrushWhite;
    }

    private static double CalcLuminance(Color color)
    {
        var pR = color.R / 255.0;
        var pG = color.G / 255.0;
        var pB = color.B / 255.0;

        var lR = pR <= 0.03928 ? pR / 12.92 : Math.Pow((pR + 0.055) / 1.055, 2.4);
        var lG = pG <= 0.03928 ? pG / 12.92 : Math.Pow((pG + 0.055) / 1.055, 2.4);
        var lB = pB <= 0.03928 ? pB / 12.92 : Math.Pow((pB + 0.055) / 1.055, 2.4);

        return 0.2126 * lR + 0.7152 * lG + 0.0722 * lB;
    }

    public void Destroy()
    {
        _settingsService.Settings.PropertyChanged -= Settings_PropertyChanged;
        NotifyIcon.Destroy();
    }

    private void DrawCurrent()
    {
        if (Colors.Count == 0) { return; }

        // background
        var brush = LastOrDefault(this).Brush;
        _graphics.Clear(brush.Color);

        // text
        var font = DisplayedValue.Length < 3 ? _fontLarge : _fontSmall;
        var textSize = _graphics.MeasureString(DisplayedValue, font);
        var fg = CalcFgColor(brush.Color);

        var x = (int)double.Round((16 - textSize.Width) / 2, 0);
        var y = (int)double.Round((16 - textSize.Height) / 2, 0);
        if (font == _fontSmall) { y -= 1; }
        var point = new Point(x, y);

        _graphics.DrawString(DisplayedValue, font, fg, point);

        // set icon
        var icon = System.Drawing.Icon.FromHandle(_bitmap.GetHicon());
        NotifyIcon.SetIcon(icon);

        static MonitorColorThreshold LastOrDefault(MonitorModelBase @this)
        {
            for (var i = @this.Colors.Count - 1; i > 0; --i)
            {
                if (@this.Colors[i].Threshold <= @this.Value)
                {
                    return @this.Colors[i];
                }
            }
            return @this.Colors[0];
        }
    }

    private void DrawHistory()
    {
        if (Colors.Count == 0) { return; }

        // background
        _graphics.Clear(Color.Black);

        // frame
        _graphics.DrawRectangle(_historyFramePen, 0, 0, 15, 15);

        // chart
        var brush = LastOrDefault(this).Brush;
        switch (SourceType)
        {
            case MonitorSourceType.Fan:
            case MonitorSourceType.Curve:
                {
                    for (var i = 0; i < History.Count; ++i)
                    {
                        var h = (int)double.Ceiling(History[i] * 15.0 / 100.0) - 1;
                        if (h < 1) { continue; }

                        _graphics.FillRectangle(brush, i + 1, 15 - h, 1, h);
                    }

                    break;
                }
            case MonitorSourceType.Sensor:
                {
                    var max = _settingsService.Settings.CurveMaxTemp;
                    var min = _settingsService.Settings.CurveMinTemp;
                    for (var i = 0; i < History.Count; ++i)
                    {
                        var h = Math.Clamp((int)double.Ceiling((History[i] - min) * 15.0 / (max - min)) - 1, 0, 14);
                        if (h < 1) { continue; }

                        _graphics.FillRectangle(brush, i + 1, 15 - h, 1, h);
                    }

                    break;
                }
        }

        // set icon
        var icon = System.Drawing.Icon.FromHandle(_bitmap.GetHicon());
        NotifyIcon.SetIcon(icon);

        static MonitorColorThreshold LastOrDefault(MonitorModelBase @this)
        {
            for (var i = @this.Colors.Count - 1; i > 0; --i)
            {
                if (@this.Colors[i].Threshold <= @this.Value)
                {
                    return @this.Colors[i];
                }
            }
            return @this.Colors[0];
        }
    }

    public void EditColor(MonitorColorThreshold oldColor, MonitorColorThreshold newColor)
    {
        if (!Colors.Contains(oldColor)) { return; }

        Colors[Colors.IndexOf(oldColor)] = newColor;
        SortColors();

        Update(true);
        _monitorService.Save();
    }

    public void RemoveColor(MonitorColorThreshold color)
    {
        Colors.Remove(color);
        Update(true);
        _monitorService.Save();
    }

    private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settingsService.Settings.CurveMinTemp) || e.PropertyName == nameof(_settingsService.Settings.CurveMaxTemp))
        {
            Update(true);
        }
    }

    private static void Sort(ref List<MonitorColorThreshold> collection)
    {
        CollectionsMarshal.AsSpan(collection).Sort((a, b) =>
        {
            if (a.Threshold < b.Threshold)
            {
                return -1;
            }
            else if (a.Threshold > b.Threshold)
            {
                return 1;
            }
            return 0;
        });
    }

    private void SortColors()
    {
        var sorted = Colors.ToList().OrderBy(x => x.Threshold);
        for (var i = 0; i < sorted.Count(); ++i)
        {
            Colors.Move(Colors.IndexOf(sorted.ElementAt(i)), i);
        }
    }

    public void Update(bool forceRedraw = false)
    {
        App.DispatcherQueue.TryEnqueue(
            DispatcherQueuePriority.High,
            forceRedraw ? _forcedUpdateHandler : _updateHandler);
    }

    private void Update_()
    {
        UpdateValue();

        switch (DisplayType)
        {
            case MonitorDisplayType.Current:
                {
                    var text = Value.HasValue ? double.Round(Value.Value, 0).ToString() : "--";
                    if (DisplayedValue != text)
                    {
                        DisplayedValue = text;
                        DrawCurrent();
                    }
                    break;
                }
            case MonitorDisplayType.History:
                DrawHistory();
                break;
        }

        UpdateTooltip();
        NotifyIcon.SetTooltip(_stringBuilder.ToString());
    }

    private void UpdateForced_()
    {
        switch (DisplayType)
        {
            case MonitorDisplayType.Current:
                DisplayedValue = Value.HasValue ? double.Round(Value.Value, 0).ToString() : "--";
                DrawCurrent();
                break;
            case MonitorDisplayType.History:
                DrawHistory();
                break;
        }

        UpdateTooltip();
        NotifyIcon.SetTooltip(_stringBuilder.ToString());
    }

    private protected abstract void UpdateTooltip();

    private protected abstract void UpdateValue();
}
