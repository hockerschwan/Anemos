﻿using System.Diagnostics.CodeAnalysis;

using Anemos.Contracts.Services;
using Anemos.Helpers;

using Microsoft.UI.Xaml.Controls;

namespace Anemos.Services;

public class NavigationViewService(INavigationService navigationService, IPageService pageService) : INavigationViewService
{
    private readonly INavigationService _navigationService = navigationService;

    private readonly IPageService _pageService = pageService;

    private NavigationView? _navigationView;

    public IList<object>? MenuItems => _navigationView?.MenuItems;

    public object? SettingsItem => _navigationView?.SettingsItem;

    [MemberNotNull(nameof(_navigationView))]
    public void Initialize(NavigationView navigationView)
    {
        _navigationView = navigationView;
        _navigationView.BackRequested += OnBackRequested;
        _navigationView.ItemInvoked += OnItemInvoked;
    }

    public void UnregisterEvents()
    {
        if (_navigationView != null)
        {
            _navigationView.BackRequested -= OnBackRequested;
            _navigationView.ItemInvoked -= OnItemInvoked;
        }
    }

    public NavigationViewItem? GetSelectedItem(Type pageType)
    {
        if (_navigationView != null)
        {
            return GetSelectedItem(_navigationView.MenuItems, pageType) ?? GetSelectedItem(_navigationView.FooterMenuItems, pageType);
        }

        return null;
    }

    private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args) => _navigationService.GoBack();

    private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            // Navigate to the settings page.
        }
        else
        {
            var selectedItem = args.InvokedItemContainer as NavigationViewItem;

            if (selectedItem?.GetValue(NavigationHelper.NavigateToProperty) is string pageKey)
            {
                _navigationService.NavigateTo(pageKey);
            }
        }
    }

    private NavigationViewItem? GetSelectedItem(IEnumerable<object> menuItems, Type pageType)
    {
        foreach (var item in menuItems.OfType<NavigationViewItem>().ToList())
        {
            if (IsMenuItemForPageType(item, pageType))
            {
                return item;
            }

            var selectedChild = GetSelectedItem(item.MenuItems, pageType);
            if (selectedChild != null)
            {
                return selectedChild;
            }
        }

        return null;
    }

    private bool IsMenuItemForPageType(NavigationViewItem menuItem, Type sourcePageType)
    {
        if (menuItem.GetValue(NavigationHelper.NavigateToProperty) is string pageKey)
        {
            return _pageService.GetPageType(pageKey) == sourcePageType;
        }

        return false;
    }
}
