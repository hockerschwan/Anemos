#include "Icon.h"
#include "NotifyIconLib.h"
#include <strsafe.h>
#include <algorithm>
#include <sstream>

using namespace NotifyIconLib;

Icon::Icon()
{
	hInstance_ = GetModuleHandleW(L"Anemos");

	RegisterWindowClass(WndProc);

	hWnd_ = CreateWindowW(szWindowClass_, NULL, WS_OVERLAPPEDWINDOW, 0, 0, 200, 200, NULL, NULL, hInstance_, NULL);
	if (hWnd_)
	{
		DeleteIcon();
		CreateIcon();
	}
	else
	{
		auto err = GetLastError();
		Throw(L"CreateWindowW", err);
	}
}

Icon::~Icon()
{
	DeleteIcon();
	DestroyWindow(hWnd_);
	UnregisterClassW(szWindowClass_, hInstance_);
}

bool Icon::SetTooltip(std::wstring tooltip)
{
	NOTIFYICONDATA nid = { sizeof(nid) };
	nid.uFlags = NIF_GUID | NIF_TIP | NIF_SHOWTIP;
	nid.guidItem = guid_;
	StringCchCopyW(nid.szTip, ARRAYSIZE(nid.szTip), tooltip.c_str());

	return Shell_NotifyIconW(NIM_MODIFY, &nid);
}

bool Icon::SetVisible(bool visible)
{
	NOTIFYICONDATA nid = { sizeof(nid) };
	nid.uFlags = NIF_GUID | NIF_STATE | NIF_SHOWTIP;
	nid.guidItem = guid_;
	nid.dwState = visible ? 0 : NIS_HIDDEN;
	nid.dwStateMask = NIS_HIDDEN;

	return Shell_NotifyIconW(NIM_MODIFY, &nid);
}

void Icon::ShowContextMenu(HWND hwnd, POINT pt)
{
	HMENU hmenu = nullptr;
	CreateContextMenu(NotifyIcon::Instance->GetMenuItems()->front().Id, hmenu);
	SetForegroundWindow(hwnd);
	TrackPopupMenuEx(hmenu, NULL, pt.x, pt.y, hwnd, NULL);
	DestroyMenu(hmenu);
}

int Icon::CreateContextMenu(int idStart, HMENU& result)
{
	HMENU hmenu = CreatePopupMenu();

	int diff = 0;
	auto depth = idStart / 1000;
	auto menuItems = NotifyIcon::Instance->GetMenuItems();
	auto it = std::find_if(menuItems->begin(), menuItems->end(), [&](const MenuItem& item) { return item.Id == idStart; });

	for (; it != menuItems->end(); ++it)
	{
		auto id = it->Id;
		auto dp = id / 1000;
		if (dp < depth) break;
		if (dp > depth) continue;
		++diff;

		switch (it->Type)
		{
		case Default:
		{
			auto flag = MF_STRING;
			if (!it->IsEnabled)
				flag |= MF_DISABLED;
			AppendMenuW(hmenu, flag, it->Id, it->Text.c_str());
			break;
		}
		case Separator:
			AppendMenuW(hmenu, MF_SEPARATOR, it->Id, NULL);
			break;
		case Submenu:
		{
			auto flag = MF_STRING | MF_POPUP;
			if (!it->IsEnabled)
				flag |= MF_DISABLED;

			HMENU submenu = nullptr;
			if ((it + 1)->Id / 1000 <= depth)
			{
				submenu = CreatePopupMenu();
				AppendMenuW(hmenu, flag, (UINT_PTR)submenu, it->Text.c_str());
			}
			else
			{
				int n = CreateContextMenu((it + 1)->Id, submenu);
				AppendMenuW(hmenu, flag, (UINT_PTR)submenu, it->Text.c_str());
				it += n;
				diff += n;
			}
			break;
		}
		case Check:
		{
			auto flag = MF_STRING;
			if (it->IsChecked)
				flag |= MF_CHECKED;
			if (!it->IsEnabled)
				flag |= MF_DISABLED;
			AppendMenuW(hmenu, flag, it->Id, it->Text.c_str());
			break;
		}
		case Radio:
		{
			auto flag = MF_STRING | MF_USECHECKBITMAPS;
			if (it->IsChecked)
				flag |= MF_CHECKED;
			if (!it->IsEnabled)
				flag |= MF_DISABLED;
			AppendMenuW(hmenu, flag, it->Id, it->Text.c_str());
			break;
		}
		}
	}

	result = hmenu;
	return diff;
}

bool Icon::CreateIcon()
{
	NOTIFYICONDATA nid = { sizeof(nid) };
	nid.hWnd = hWnd_;
	nid.uFlags = NIF_GUID | NIF_ICON | NIF_MESSAGE | NIF_SHOWTIP | NIF_STATE;
	nid.dwState = NIS_HIDDEN;
	nid.dwStateMask = NIS_HIDDEN;
	nid.guidItem = guid_;
	nid.hIcon = LoadIconW(hInstance_, MAKEINTRESOURCE(32512));
	nid.uCallbackMessage = WMAPP_NOTIFYCALLBACK;
	if (!Shell_NotifyIconW(NIM_ADD, &nid))
	{
		auto err = GetLastError();
		Throw(L"Shell_NotifyIconW(NIM_ADD)", err);
	}

	nid.uVersion = NOTIFYICON_VERSION_4;
	return Shell_NotifyIconW(NIM_SETVERSION, &nid);
}

bool Icon::DeleteIcon()
{
	NOTIFYICONDATA nid = { sizeof(nid) };
	nid.uFlags = NIF_GUID;
	nid.guidItem = guid_;
	return Shell_NotifyIconW(NIM_DELETE, &nid);
}

void Icon::RegisterWindowClass(WNDPROC lpfnWndProc)
{
	WNDCLASSEX wcex = { sizeof(wcex) };
	wcex.style = CS_HREDRAW | CS_VREDRAW;
	wcex.lpfnWndProc = lpfnWndProc;
	wcex.hInstance = hInstance_;
	wcex.hCursor = LoadCursorW(NULL, IDC_ARROW);
	wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
	wcex.lpszMenuName = NULL;
	wcex.lpszClassName = szWindowClass_;
	if (RegisterClassExW(&wcex) == 0)
	{
		auto err = GetLastError();
		Throw(L"RegisterClassExW", err);
	}
}

void NotifyIconLib::Icon::Throw(const std::wstring message, const DWORD err)
{
	std::wostringstream ss;
	ss << message << ": " << err;
	NotifyIcon::Instance->Throw(ss.str());
}
