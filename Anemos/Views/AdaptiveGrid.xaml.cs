using System.Collections;
using System.Collections.Specialized;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace Anemos.Views;

public sealed partial class AdaptiveGrid : Grid
{
    public IList ItemsSource
    {
        get => (IList)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(IList),
        typeof(AdaptiveGrid),
        new PropertyMetadata(null, new PropertyChangedCallback(OnItemsSourcePropertyChanged)));

    private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AdaptiveGrid ag)
        {
            ag.OnItemsSourceChanged((IList)e.OldValue, (IList)e.NewValue);
        }
    }

    private void OnItemsSourceChanged(IList oldValue, IList newValue)
    {
        if (oldValue is INotifyCollectionChanged oldIncc)
        {
            oldIncc.CollectionChanged -= ItemsSource_CollectionChanged;
            Children.Clear();
        }

        if (newValue is INotifyCollectionChanged newIncc)
        {
            newIncc.CollectionChanged += ItemsSource_CollectionChanged;
            foreach (var item in newValue)
            {
                if (item is FrameworkElement elm)
                {
                    var heightBinding = new Binding()
                    {
                        Source = this,
                        Path = new PropertyPath("ItemHeight"),
                        Mode = BindingMode.OneWay
                    };
                    elm.SetBinding(HeightProperty, heightBinding);
                    elm.VerticalAlignment = VerticalAlignment.Stretch;
                    Children.Add(elm);
                }
            }
        }
    }

    private double _minItemWidth = 100;
    public double MinItemWidth
    {
        get => _minItemWidth;
        set
        {
            if (value < 1) { return; }
            if (_minItemWidth != value)
            {
                _minItemWidth = value;
                CalcColumns();
            }
        }
    }

    private double _itemHeight = 100;
    public double ItemHeight
    {
        get => _itemHeight;
        set
        {
            if (_itemHeight != value)
            {
                _itemHeight = value;
                SetupRowDefinitions();

                InvalidateMeasure();
            }
        }
    }

    public int Rows { get; private set; } = 1;
    public int Columns { get; private set; } = 1;


    public AdaptiveGrid()
    {
        InitializeComponent();
        DataContext = this;
        SizeChanged += AdaptiveGrid_SizeChanged;
        Unloaded += AdaptiveGrid_Unloaded;
    }

    private void AdaptiveGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.PreviousSize.Width != e.NewSize.Width)
        {
            CalcColumns();
        }
    }

    private void AdaptiveGrid_Unloaded(object sender, RoutedEventArgs e)
    {
        SizeChanged -= AdaptiveGrid_SizeChanged;
        Unloaded -= AdaptiveGrid_Unloaded;
        if (ItemsSource is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged -= ItemsSource_CollectionChanged;
        }
    }

    private void AssignRowAndColumn()
    {
        for (var i = 0; i < ItemsSource.Count; ++i)
        {
            if (ItemsSource[i] is FrameworkElement elm)
            {
                elm.SetValue(RowProperty, i / Columns);
                elm.SetValue(ColumnProperty, i % Columns);
            }
        }
        InvalidateArrange();
    }

    private void CalcColumns()
    {
        var width = ActualWidth - Padding.Left - Padding.Right;
        var sp = ColumnSpacing;

        var colPrev = Columns;
        var col = 2;
        while (true)
        {
            if ((width - (col - 1) * sp) / col < MinItemWidth)
            {
                if (--col != colPrev)
                {
                    Columns = col;
                    SetupColumnDefinitions();

                    Rows = (int)double.Ceiling((double)ItemsSource.Count / Columns);
                    SetupRowDefinitions();

                    AssignRowAndColumn();
                }
                return;
            }
            ++col;
        }
    }

    private void ItemsSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var item in e.NewItems!)
                {
                    if (item is FrameworkElement elm)
                    {
                        Children.Add(elm);
                    }
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                Children.RemoveAt(e.OldStartingIndex);
                break;
            case NotifyCollectionChangedAction.Replace:
                {
                    if (e.NewItems![0] is FrameworkElement elm)
                    {
                        Children[e.NewStartingIndex] = elm;
                    }
                    break;
                }
            case NotifyCollectionChangedAction.Move:
                {
                    var elm = Children[e.OldStartingIndex];
                    Children.RemoveAt(e.OldStartingIndex);
                    Children.Insert(e.NewStartingIndex, elm);
                    break;
                }
            case NotifyCollectionChangedAction.Reset:
                Children.Clear();
                foreach (var item in e.NewItems!)
                {
                    if (item is FrameworkElement elm)
                    {
                        Children.Add(elm);
                    }
                }
                break;
        }

        Rows = (int)double.Ceiling((double)ItemsSource.Count / Columns);
        SetupRowDefinitions();

        AssignRowAndColumn();
    }

    private void SetupColumnDefinitions()
    {
        ColumnDefinitions.Clear();
        for (var i = 0; i < Columns; i++)
        {
            ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        }
    }

    private void SetupRowDefinitions()
    {
        RowDefinitions.Clear();
        for (var i = 0; i < Rows; i++)
        {
            RowDefinitions.Add(new() { Height = new GridLength(ItemHeight) });
        }
    }
}
