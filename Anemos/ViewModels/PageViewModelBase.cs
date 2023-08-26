using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.ViewModels;

public class PageViewModelBase : ObservableObject
{
    public virtual bool IsVisible
    {
        get; set;
    }
}
