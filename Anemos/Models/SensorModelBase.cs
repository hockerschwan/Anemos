using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.Models;

[DebuggerDisplay("{LongName}")]
public abstract class SensorModelBase : ObservableObject
{
    private protected string _id = string.Empty;
    public string Id => _id;

    private protected string _name = string.Empty;
    public virtual string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public virtual string LongName => Name;

    private protected double? _value;
    public double? Value
    {
        get => _value;
        private protected set => SetProperty(ref _value, value);
    }

    public virtual void Update()
    {
    }
}
