using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.Models;

public class SensorModelBase : ObservableObject
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

    private protected decimal? _value;
    public decimal? Value
    {
        get => _value;
        private protected set => SetProperty(ref _value, value);
    }

    public virtual void Update()
    {
    }
}
