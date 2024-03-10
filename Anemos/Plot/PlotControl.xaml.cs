using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
using Serilog;

namespace Anemos.Plot;

public sealed partial class PlotControl : UserControl
{
    private InputSystemCursorShape _cursor = InputSystemCursorShape.Arrow;
    public InputSystemCursorShape Cursor
    {
        get => _cursor;
        set
        {
            if (_cursor != value)
            {
                _cursor = value;
                ProtectedCursor = InputSystemCursor.Create(_cursor);
            }
        }
    }

    public Plot Plot { get; } = new();

    public PlotControl()
    {
        InitializeComponent();

        canvas.Draw += Canvas_Draw;
    }

    private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        if (canvas.Device.IsDeviceLost())
        {
            Log.Error("Canvas device lost: {reason}", canvas.Device.GetDeviceLostReason());
            return;
        }
        if (canvas.IsLoaded)
        {
            Plot.Draw((float)sender.ActualWidth, (float)sender.ActualHeight, args.DrawingSession, canvas.Device);
        }
    }

    public void Close()
    {
        canvas.Draw -= Canvas_Draw;
        canvas.RemoveFromVisualTree();
    }

    public void Refresh()
    {
        canvas.Invalidate();
    }
}
