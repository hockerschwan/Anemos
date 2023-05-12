using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class FansPage : Page
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    private readonly IFanService _fanService = App.GetService<IFanService>();

    private FanOptionsResult? _optionsResult;

    private string _newName = string.Empty;

    private bool _isDialogShown = false;

    private bool _hasLoaded = false;

    public FansViewModel ViewModel
    {
        get;
    }

    public FansPage()
    {
        _messenger.Register<OpenFanOptionsMessage>(this, OpenFanOptionsMessageHandler);
        _messenger.Register<FanOptionsResultMessage>(this, FanOptionsResultMessageHandler);
        _messenger.Register<OpenFanProfileNameEditorMessage>(this, OpenFanProfileNameEditorMessageHandler);
        _messenger.Register<FanProfileNameEditorResultMessage>(this, FanProfileNameEditorResultMessageHandler);

        ViewModel = App.GetService<FansViewModel>();
        Loaded += FansPage_Loaded;
        Unloaded += FansPage_Unloaded;
        InitializeComponent();
    }

    private async void OpenFanOptionsMessageHandler(object recipient, OpenFanOptionsMessage message)
    {
        if (_isDialogShown) { return; }

        var fanId = message.Value;
        var model = _fanService.GetFanModel(fanId);
        if (model == null) { return; }

        ViewModel.OptionsDialog.ViewModel.SetId(fanId);

        SetOptionsDialogStyle();

        _isDialogShown = true;

        var result = await ViewModel.OptionsDialog.ShowAsync();
        if (result == ContentDialogResult.Primary && _optionsResult != null)
        {
            model.SetOptions(_optionsResult);
        }

        _optionsResult = null;
        _isDialogShown = false;
    }

    private void FanOptionsResultMessageHandler(object recipient, FanOptionsResultMessage message)
    {
        _optionsResult = message.Value;
    }

    private async void OpenFanProfileNameEditorMessageHandler(object recipient, OpenFanProfileNameEditorMessage message)
    {
        if (_isDialogShown || ViewModel.SelectedProfile == null)
        {
            return;
        }

        ViewModel.ProfileNameEditorDialog.SetText(ViewModel.SelectedProfile.Name);

        SetProfileNameEditorDialogStyle();

        _isDialogShown = true;

        var result = await ViewModel.ProfileNameEditorDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            _fanService.CurrentProfile.Name = _newName;
            _fanService.UpdateCurrentProfile();

            App.GetService<INotifyIconService>().SetupMenu();
        }

        _newName = string.Empty;
        _isDialogShown = false;
    }

    private void FanProfileNameEditorResultMessageHandler(object recipient, FanProfileNameEditorResultMessage message)
    {
        _newName = message.Value;
    }

    private void SetOptionsDialogStyle()
    {
        var dialog = ViewModel.OptionsDialog;
        dialog.XamlRoot = XamlRoot;
        dialog.Style = App.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = "FanOptionsDialog_Title".GetLocalized();
        dialog.PrimaryButtonText = "Dialog_OK".GetLocalized();
        dialog.CloseButtonText = "Dialog_Cancel".GetLocalized();
        dialog.DefaultButton = ContentDialogButton.Primary;
    }

    private void SetProfileNameEditorDialogStyle()
    {
        var dialog = ViewModel.ProfileNameEditorDialog;
        dialog.XamlRoot = XamlRoot;
        dialog.Style = App.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = "FanProfileNameEditorDialog_Title".GetLocalized();
        dialog.PrimaryButtonText = "Dialog_OK".GetLocalized();
        dialog.CloseButtonText = "Dialog_Cancel".GetLocalized();
        dialog.DefaultButton = ContentDialogButton.Primary;
    }

    private void FansPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_hasLoaded)
        {
            ViewModel.IsVisible = true;
        }
        else
        {
            if (!App.GetService<ISettingsService>().Settings.StartMinimized)
            {
                ViewModel.IsVisible = true;
            }

            _hasLoaded = true;
        }
    }

    private void FansPage_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.IsVisible = false;
    }
}
