using Anemos.Contracts.Services;
using Anemos.ViewModels;
using Anemos.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = [];

    public PageService()
    {
        Configure<FansViewModel, FansPage>();
        Configure<CurvesViewModel, CurvesPage>();
        Configure<SensorsViewModel, SensorsPage>();
        Configure<RulesViewModel, RulesPage>();
        Configure<MonitorsViewModel, MonitorsPage>();
        Configure<SettingsViewModel, SettingsPage>();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }

    private void Configure<VM, V>()
        where VM : ObservableObject
        where V : Page
    {
        lock (_pages)
        {
            var key = typeof(VM).FullName!;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            var type = typeof(V);
            if (_pages.ContainsValue(type))
            {
                var k = First(this, type);
                throw new ArgumentException($"This type is already configured with key {k}");

                static string First(PageService @this, Type? type)
                {
                    foreach (var p in @this._pages)
                    {
                        if (p.Value == type) { return p.Key; }
                    }
                    return string.Empty;
                }
            }

            _pages.Add(key, type);
        }
    }
}
