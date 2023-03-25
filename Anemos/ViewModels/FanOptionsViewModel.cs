using Anemos.Contracts.Services;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.ViewModels;

public partial class FanOptionsViewModel : ObservableObject
{
    private readonly IFanService _fanService = App.GetService<IFanService>();

    private string _fanId = string.Empty;
    public string FanId
    {
        get => _fanId;
        set => SetProperty(ref _fanId, value);
    }

    public FanModelBase? Model => _fanService.GetFanModel(FanId);

    private int _maxSpeed = 100;
    public int MaxSpeed
    {
        get => _maxSpeed;
        set
        {
            if (value < MinSpeed)
            {
                value = MinSpeed;
            }
            SetProperty(ref _maxSpeed, value);
        }
    }

    private int _minSpeed = 0;
    public int MinSpeed
    {
        get => _minSpeed;
        set
        {
            if (value > MaxSpeed)
            {
                value = MaxSpeed;
            }
            SetProperty(ref _minSpeed, value);
        }
    }

    private int _deltaLimitUp = 0;
    public int DeltaLimitUp
    {
        get => _deltaLimitUp;
        set => SetProperty(ref _deltaLimitUp, value);
    }

    private int _deltaLimitDown = 0;
    public int DeltaLimitDown
    {
        get => _deltaLimitDown;
        set => SetProperty(ref _deltaLimitDown, value);
    }

    public void SetId(string fanId)
    {
        FanId = fanId;
        OnPropertyChanged(nameof(Model));
        if (Model == null) { return; }

        MaxSpeed = Model.MaxSpeed;
        MinSpeed = Model.MinSpeed;
        DeltaLimitUp = Model.DeltaLimitUp;
        DeltaLimitDown = Model.DeltaLimitDown;
    }
}
