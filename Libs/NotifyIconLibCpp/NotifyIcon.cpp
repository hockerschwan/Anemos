#include "pch.hpp"
#include "NotifyIcon.hpp"
#include "NotifyIconLib.hpp"

NotifyIcon::NotifyIcon(const char* guidStr, HICON hIcon)
{
	guidString_ = guidStr;
	guid_ = GetGuid(guidStr);
	hIcon_ = hIcon;

	auto t = std::thread(&NotifyIcon::CreateWindowAndRun, this);
	t.detach();

	while (!windowCreated_)
	{
		Sleep(10);
	}
}

NotifyIcon::~NotifyIcon()
{
	NOTIFYICONDATA nid = { sizeof(nid) };
	nid.uFlags = NIF_GUID;
	nid.guidItem = guid_;
	Shell_NotifyIconW(NIM_DELETE, &nid);

	DestroyWindow(hWnd_);

	g_map_hwnd_guid_.erase(hWnd_);
	g_map_guid_icon_.erase(guidString_);
}

HICON NotifyIcon::GetHIcon()
{
	return hIcon_;
}

HWND NotifyIcon::GetHwnd()
{
	return hWnd_;
}

std::vector<MenuItem*> NotifyIcon::GetMenuItems()
{
	return menuItems_;
}

void NotifyIcon::SetIcon(HICON hIcon)
{
	hIcon_ = hIcon;
	NOTIFYICONDATA nid = { sizeof(nid) };
	nid.uFlags = NIF_GUID | NIF_ICON | NIF_STATE;
	nid.guidItem = guid_;
	nid.hIcon = hIcon_;
	Shell_NotifyIconW(NIM_MODIFY, &nid);
}

void NotifyIcon::SetMenuItems(std::vector<MenuItem*>& menuItems)
{
	menuItems_ = menuItems;
}

void NotifyIcon::SetTooltip(const WCHAR* tooltip)
{
	NOTIFYICONDATA nid = { sizeof(nid) };
	nid.uFlags = NIF_GUID | NIF_TIP | NIF_SHOWTIP;
	nid.guidItem = guid_;
	StringCchCopyW(nid.szTip, ARRAYSIZE(nid.szTip), tooltip);
	Shell_NotifyIconW(NIM_MODIFY, &nid);
}

void NotifyIcon::SetVisibility(bool visible)
{
	NOTIFYICONDATA nid = { sizeof(nid) };
	nid.uFlags = NIF_GUID | NIF_STATE | NIF_SHOWTIP;
	nid.guidItem = guid_;
	nid.dwState = visible ? 0 : NIS_HIDDEN;
	nid.dwStateMask = NIS_HIDDEN;
	Shell_NotifyIconW(NIM_MODIFY, &nid);
}

void NotifyIcon::ShowContextMenu(POINT pt)
{
	if (menuItems_.empty()) return;

	HMENU hmenu;
	CreateContextMenu(menuItems_.front()->Id, hmenu);
	SetForegroundWindow(hWnd_);
	TrackPopupMenuEx(hmenu, NULL, pt.x, pt.y, hWnd_, NULL);
	DestroyMenu(hmenu);
}

UINT NotifyIcon::CreateContextMenu(UINT idStart, HMENU& result)
{
	HMENU hmenu = CreatePopupMenu();

	UINT diff = 0;
	auto depth = idStart / g_nItemsPerDepth_;
	auto it = std::find_if(menuItems_.begin(), menuItems_.end(), [&](MenuItem* item) { return item->Id == idStart; });

	for (; it != menuItems_.end(); ++it)
	{
		auto item = (*it);
		auto id = item->Id;
		auto dp = id / g_nItemsPerDepth_;
		if (dp < depth) break;
		if (dp > depth) continue;
		++diff;

		switch (item->Type)
		{
		case Default:
		{
			auto flag = MF_STRING;
			if (!item->IsEnabled) flag |= MF_DISABLED;
			AppendMenuW(hmenu, flag, item->Id, item->Text);

			if (item->Icon == nullptr) { break; }

			auto bmp = GetBitmapFromIcon(item->Icon);
			if (bmp == nullptr) { break; }

			if (SetMenuItemBitmaps(hmenu, item->Id, MF_BYCOMMAND, bmp, NULL) == FALSE)
			{
				auto err = GetLastError();
				std::ostringstream ss;
				ss << "SetMenuItemBitmaps " << err;
				throw std::runtime_error(ss.str());
			}

			break;
		}
		case Separator:
			AppendMenuW(hmenu, MF_SEPARATOR, item->Id, NULL);
			break;
		case Submenu:
		{
			auto flag = MF_STRING | MF_POPUP;
			if (!item->IsEnabled) flag |= MF_DISABLED;

			HMENU submenu = nullptr;
			if ((*(it + 1))->Id / g_nItemsPerDepth_ <= depth)
			{
				submenu = CreatePopupMenu();
				AppendMenuW(hmenu, flag, (UINT_PTR)submenu, item->Text);
			}
			else
			{
				int n = CreateContextMenu((*(it + 1))->Id, submenu);
				AppendMenuW(hmenu, flag, (UINT_PTR)submenu, item->Text);
				it += n;
				diff += n;
			}
			break;
		}
		case Check:
		{
			auto flag = MF_STRING;
			if (item->IsChecked) flag |= MF_CHECKED;
			if (!item->IsEnabled) flag |= MF_DISABLED;
			AppendMenuW(hmenu, flag, item->Id, item->Text);
			break;
		}
		case Radio:
		{
			auto flag = MF_STRING | MF_USECHECKBITMAPS;
			if (item->IsChecked) flag |= MF_CHECKED;
			if (!item->IsEnabled) flag |= MF_DISABLED;
			AppendMenuW(hmenu, flag, item->Id, item->Text);
			break;
		}
		}
	}

	result = hmenu;
	return diff;
}

void NotifyIcon::CreateWindowAndRun()
{
	hWnd_ = CreateWindowW(g_windowClass_.c_str(), NULL, WS_OVERLAPPEDWINDOW, 0, 0, 200, 200, NULL, NULL, g_hInstance_, NULL);
	if (hWnd_ == NULL)
	{
		auto err = GetLastError();
		Throw("CreateWindowW", err);
	}

	NOTIFYICONDATA nid = { sizeof(nid) };
	nid.hWnd = hWnd_;
	nid.uFlags = NIF_GUID | NIF_ICON | NIF_MESSAGE | NIF_SHOWTIP | NIF_STATE;
	nid.dwState = NIS_HIDDEN;
	nid.dwStateMask = NIS_HIDDEN;
	nid.guidItem = guid_;
	nid.hIcon = hIcon_;
	nid.uCallbackMessage = WMAPP_NOTIFYCALLBACK;
	if (!Shell_NotifyIconW(NIM_ADD, &nid))
	{
		auto err = GetLastError();
		Throw("Shell_NotifyIcon(NIM_ADD)", err);
	}

	nid.uVersion = NOTIFYICON_VERSION_4;
	if (!Shell_NotifyIconW(NIM_SETVERSION, &nid))
	{
		auto err = GetLastError();
		Throw("Shell_NotifyIcon(NIM_SETVERSION)", err);
	}

	windowCreated_ = true;

	MSG msg;
	while (true)
	{
		if (PeekMessageW(&msg, NULL, 0, 0, PM_REMOVE))
		{
			TranslateMessage(&msg);
			DispatchMessageW(&msg);
			if (msg.message == WMAPP_DESTROYICON) { break; }
		}
		else
		{
			WaitMessage();
		}
	}
}
