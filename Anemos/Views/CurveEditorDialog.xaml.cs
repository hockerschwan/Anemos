using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Globalization.NumberFormatting;

namespace Anemos.Views;

public sealed partial class CurveEditorDialog : ContentDialog
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    public CurveEditorViewModel ViewModel
    {
        get;
    }

    public CurveEditorDialog()
    {
        ViewModel = App.GetService<CurveEditorViewModel>();
        App.MainWindow.SizeChanged += MainWindow_SizeChanged;
        Loaded += CurveEditorDialog_Loaded;
        Closing += CurveEditorDialog_Closing;
        InitializeComponent();

        SetNumberFormatter();
    }

    private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        if (Visibility == Visibility.Visible)
        {
            SetPageSize(args.Size.Width, args.Size.Height);
        }
    }

    private void CurveEditorDialog_Loaded(object sender, RoutedEventArgs e)
    {
        SetPageSize(App.MainWindow.Width, App.MainWindow.Height);
    }

    private void SetPageSize(double windowWidth, double windowHeight)
    {
        Page.Width = Math.Min(1000, windowWidth - 200);
        Page.Height = Math.Min(1000, windowHeight - 300);
    }

    private void CurveEditorDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        ViewModel.Unselect();
        _messenger.Send(new CurveEditorResultMessage(ViewModel.GetLineData()));
    }

    private void SetNumberFormatter()
    {
        IncrementNumberRounder rounder = new()
        {
            Increment = 0.1,
            RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp
        };

        DecimalFormatter formatter = new()
        {
            FractionDigits = 1,
            NumberRounder = rounder
        };
        NumberBoxX.NumberFormatter = NumberBoxY.NumberFormatter = formatter;
    }
}
