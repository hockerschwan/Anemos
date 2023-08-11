#pragma once
#include "MenuItem.hpp"
#include <string>

class NotifyIcon
{
public:
	NotifyIcon(const NotifyIcon&) = delete;
	NotifyIcon(const char* guidStr, HICON hIcon);
	~NotifyIcon();
	HICON GetHIcon();
	HWND GetHwnd();
	std::vector<MenuItem*> GetMenuItems();
	void SetIcon(HICON hIcon);
	void SetMenuItems(std::vector<MenuItem*>& menuItems);
	void SetTooltip(const WCHAR* tooltip);
	void SetVisibility(bool visible);
	void ShowContextMenu(POINT pt);

private:
	UINT CreateContextMenu(UINT idStart, HMENU& result);

	HWND hWnd_;
	std::wstring windowClass_;
	GUID guid_;
	std::string guidString_;
	HICON hIcon_;
	std::vector<MenuItem*> menuItems_;
	WCHAR* tooltip_;
};
