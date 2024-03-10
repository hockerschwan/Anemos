using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Anemos.Views;

internal class CursorGrid : Grid
{
    public event RoutedEventHandler? Click;

    private protected void OnClick(RoutedEventArgs e)
    {
        Click?.Invoke(this, e);
    }

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

    public CursorGrid()
    {
        KeyDown += CursorGrid_KeyDown;
        PointerPressed += CursorGrid_PointerPressed;
    }

    private void CursorGrid_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case Windows.System.VirtualKey.LeftButton:
            case Windows.System.VirtualKey.Enter:
            case Windows.System.VirtualKey.Space:
            case Windows.System.VirtualKey.GamepadA:
                OnClick(e);
                break;
        }
    }

    private void CursorGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            OnClick(e);
        }
    }
}
