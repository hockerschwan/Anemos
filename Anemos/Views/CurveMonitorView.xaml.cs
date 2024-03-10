using Anemos.Models;
using Anemos.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Anemos.Views;

public sealed partial class CurveMonitorView : MonitorViewBase
{
    public new CurveMonitorViewModel ViewModel
    {
        get;
    }

    public CurveMonitorView(CurveMonitorViewModel viewModel) : base(viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        CreateMenuFlyout();
    }

    private void ListView_ContextRequested(UIElement sender, ContextRequestedEventArgs e)
    {
        if (Resources["ContextMenu_Colors"] is not MenuFlyout menu) { return; }
        if (e.OriginalSource is not FrameworkElement elm) { return; }

        if (elm.DataContext is MonitorColorThreshold color1) // right click on item
        {
            DeleteColor.IsEnabled = color1.IsNormal;
            EditColor.IsEnabled = true;
            DeleteColor.CommandParameter = EditColor.CommandParameter = color1;

            if (e.TryGetPosition(elm, out var point))
            {
                menu.ShowAt(elm, point);
                goto End;
            }
        }
        else if (elm is ListViewItem lvi && lvi.Content is MonitorColorThreshold color2) // menu key on item
        {
            DeleteColor.IsEnabled = color2.IsNormal;
            EditColor.IsEnabled = true;
            DeleteColor.CommandParameter = EditColor.CommandParameter = color2;
        }
        else // right click on empty space
        {
            DeleteColor.IsEnabled = EditColor.IsEnabled = false;
            DeleteColor.CommandParameter = EditColor.CommandParameter = null;

            if (e.TryGetPosition(sender, out var point))
            {
                menu.ShowAt(sender, point);
                goto End;
            }
        }

        menu.ShowAt(elm);

    End:
        e.Handled = true;
    }
}
