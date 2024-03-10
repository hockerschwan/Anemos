using Anemos.Contracts.Services;
using Anemos.Models;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class CustomSensorSourcesDialog : ContentDialog
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();
    private readonly ISensorService _sensorService = App.GetService<ISensorService>();

    private readonly string _id = string.Empty;

    public List<SensorModelBase> Sensors = [];

    public CustomSensorSourcesDialog(string sensorId)
    {
        _id = sensorId;
        if (_sensorService.GetSensor(_id) is not CustomSensorModel sensor) { return; }

        Sensors = [.. _sensorService.Sensors];
        Sensors.Remove(sensor);

        InitializeComponent();
        DataContext = this;

        ListViewSensors.Loaded += ListViewSensors_Loaded;

        Loaded += CustomSensorSourcesDialog_Loaded;
        Unloaded += CustomSensorSourcesDialog_Unloaded;
        PrimaryButtonClick += CustomSensorSourcesDialog_PrimaryButtonClick;
    }

    private async void CustomSensorSourcesDialog_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= CustomSensorSourcesDialog_Loaded;

        await Task.Delay(100);
        Focus(FocusState.Programmatic);
    }

    private void CustomSensorSourcesDialog_Unloaded(object sender, RoutedEventArgs e)
    {
        Unloaded -= CustomSensorSourcesDialog_Unloaded;
        PrimaryButtonClick -= CustomSensorSourcesDialog_PrimaryButtonClick;

        Bindings.StopTracking();
    }

    private async void CustomSensorSourcesDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        while (IsLoaded)
        {
            await Task.Delay(100);
        }
        await Task.Delay(100);

        var res = new List<string>();
        for (var i = 0; i < ListViewSensors.SelectedItems.Count; ++i)
        {
            if (ListViewSensors.SelectedItems[i] is SensorModelBase sensor)
            {
                res.Add(sensor.Id);
            }
        }

        _messenger.Send<CustomSensorSourceEditedMessage>(new(res));
    }

    private void ListViewSensors_Loaded(object sender, RoutedEventArgs e)
    {
        ListViewSensors.Loaded -= ListViewSensors_Loaded;

        if (_sensorService.GetSensor(_id) is not CustomSensorModel sensor) { return; }
        for (var i = 0; i < Sensors.Count; i++)
        {
            if (sensor.SourceIds.Contains(Sensors[i].Id))
            {
                ListViewSensors.SelectedItems.Add(Sensors[i]);
            }
        }
    }
}
