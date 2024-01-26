using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();

        var task = Task.Run(SettingsViewModel.TaskExists);
        task.Wait();
        TaskCheckBox.IsChecked = task.Result;
    }

    [RelayCommand]
    private async Task CreateTask(bool createTask)
    {
        await SettingsViewModel.CreateTask(createTask);

        if (createTask != await SettingsViewModel.TaskExists())
        {
            TaskCheckBox.IsChecked = !createTask;
        }
    }
}
