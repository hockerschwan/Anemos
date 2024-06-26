#include "pch.hpp"
#include "NotifyIconLib.hpp"

void CreateNotifyIcon(const char* guid, HICON hIcon)
{
	g_mutex_.lock();
	DeleteNotifyIcon(guid);

	auto icon = new NotifyIcon(guid, hIcon);
	g_map_hwnd_guid_[icon->GetHwnd()] = guid;
	g_map_guid_icon_[guid] = icon;
	g_mutex_.unlock();
}

void DeleteNotifyIcon(const char* guid)
{
	auto ni = FindNotifyIcon(guid);
	g_mutex_.lock();
	if (ni == nullptr)
	{
		NOTIFYICONDATA nid = { sizeof(nid) };
		nid.uFlags = NIF_GUID;
		nid.guidItem = GetGuid(guid);
		Shell_NotifyIconW(NIM_DELETE, &nid);
	}
	else
	{
		g_map_hwnd_guid_.erase(ni->GetHwnd());
		PostMessageW(ni->GetHwnd(), WMAPP_DESTROYICON, 0, 0);
		delete ni;
	}

	g_map_guid_icon_.erase(guid);
	g_mutex_.unlock();
}

void SetIcon(const char* guid, HICON hIcon)
{
	auto ni = FindNotifyIcon(guid);
	if (ni == nullptr) return;

	g_mutex_.lock();
	ni->SetIcon(hIcon);
	g_mutex_.unlock();
}

void SetMenuItems(const char* guid, int count, MenuItem* items[])
{
	auto ni = FindNotifyIcon(guid);
	if (ni == nullptr) return;

	g_mutex_.lock();
	auto menu = std::vector<MenuItem*>(items, items + count);
	ni->SetMenuItems(menu);
	g_mutex_.unlock();
}

void SetTooltip(const char* guid, const wchar_t* tooltip)
{
	auto ni = FindNotifyIcon(guid);
	if (ni == nullptr) return;

	g_mutex_.lock();
	ni->SetTooltip(tooltip);
	g_mutex_.unlock();
}

void SetVisibility(const char* guid, BOOL visible)
{
	auto ni = FindNotifyIcon(guid);
	if (ni == nullptr) return;

	ni->SetVisibility(visible);
}

void SetChecked(const char* guid, UINT id, BOOL checked)
{
	auto item = FindMenuItem(guid, id);
	if (item == nullptr) return;

	g_mutex_.lock();
	item->IsChecked = checked;
	g_mutex_.unlock();
}

void SetEnabled(const char* guid, UINT id, BOOL enabled)
{
	auto item = FindMenuItem(guid, id);
	if (item == nullptr) return;

	g_mutex_.lock();
	item->IsEnabled = enabled;
	g_mutex_.unlock();
}

void SetCallback_IconClick(callback_function_guid callback)
{
	g_mutex_.lock();
	g_callback_icon_click_ = callback;
	g_mutex_.unlock();
}

void SetCallback_ItemClick(callback_function_uint callback)
{
	g_mutex_.lock();
	g_callback_item_click_ = callback;
	g_mutex_.unlock();
}

void OnInit()
{
	WNDCLASSEX wcex = { sizeof(WNDCLASSEX) };
	wcex.cbSize = sizeof(WNDCLASSEX);
	wcex.style = CS_HREDRAW | CS_VREDRAW;
	wcex.lpfnWndProc = WndProc;
	wcex.hInstance = g_hInstance_;
	wcex.hCursor = LoadCursorW(NULL, IDC_ARROW);
	wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
	wcex.lpszMenuName = NULL;
	wcex.lpszClassName = g_windowClass_.c_str();
	if (RegisterClassExW(&wcex) == 0)
	{
		auto err = GetLastError();
		Throw("RegisterClassExW", err);
	}

	Gdiplus::GdiplusStartupInput gdiplusStartupInput;
	if (Gdiplus::GdiplusStartup(&g_gdiplusToken_, &gdiplusStartupInput, NULL) != Gdiplus::Status::Ok) return;
}

void OnExit()
{
	for (const auto& [hwnd, guid] : g_map_hwnd_guid_)
	{
		DeleteNotifyIcon(guid.c_str());
	}

	UnregisterClassW(g_windowClass_.c_str(), g_hInstance_);

	Gdiplus::GdiplusShutdown(g_gdiplusToken_);
}

HBITMAP GetBitmapFromIcon(HICON hIcon)
{
	auto bitmap = std::unique_ptr<Gdiplus::Bitmap>(Gdiplus::Bitmap::FromHICON(hIcon));
	HBITMAP res{};
	auto status = bitmap->GetHBITMAP(Gdiplus::Color::AlphaMask, &res);
	if (status == Gdiplus::Status::Ok) return res;
	return nullptr;
}

GUID GetGuid(const char str[36])
{
	std::ostringstream ss;
	ss << std::hex;

	GUID guid{};

	for (size_t i = 0; i < 8; ++i)
	{
		ss << str[i];
	}
	guid.Data1 = std::stoul(ss.str(), nullptr, 16);

	ss.str("");
	ss.clear();
	for (size_t i = 9; i < 13; ++i)
	{
		ss << str[i];
	}
	guid.Data2 = std::stoi(ss.str(), nullptr, 16);

	ss.str("");
	ss.clear();
	for (size_t i = 14; i < 18; ++i)
	{
		ss << str[i];
	}
	guid.Data3 = std::stoi(ss.str(), nullptr, 16);

	for (size_t i = 19; i < 36; i += 2)
	{
		if (i == 23)
		{
			i -= 1;
			continue;
		}

		ss.str("");
		ss.clear();
		ss << str[i] << str[i + 1];
		guid.Data4[((i - 1) / 2) - 9] = std::stoi(ss.str(), nullptr, 16);
	}

	return guid;
}

std::string GetGuidString(GUID guid)
{
	std::ostringstream ss;
	ss << std::hex;

	ss << std::setw(8) << std::setfill('0') << guid.Data1 << "-";
	ss << std::setw(4) << std::setfill('0') << guid.Data2 << "-";
	ss << std::setw(4) << std::setfill('0') << guid.Data3 << "-";
	for (size_t i = 0; i < 8; ++i)
	{
		ss << std::setw(2) << std::setfill('0') << guid.Data4[i] + 0u;
		if (i == 1) ss << "-";
	}

	return ss.str();
}

GUID FindGuid(HWND hWnd)
{
	auto it = g_map_hwnd_guid_.find(hWnd);
	if (it == g_map_hwnd_guid_.end()) return GUID_NULL;

	return GetGuid((it->second).c_str());
}

MenuItem* FindMenuItem(std::string guid, UINT id)
{
	auto ni = FindNotifyIcon(guid);
	if (ni == nullptr) return nullptr;

	auto menu = ni->GetMenuItems();
	if (menu.empty()) return nullptr;

	auto it = std::find_if(
		menu.begin(),
		menu.end(),
		[&id](MenuItem* item) { return item->Id == id; });
	if (it == menu.end()) return nullptr;

	return *it;
}

NotifyIcon* FindNotifyIcon(std::string guid)
{
	auto it = g_map_guid_icon_.find(guid);
	if (it == g_map_guid_icon_.end()) return nullptr;

	return it->second;
}

void Throw(const char* message, int error)
{
	std::ostringstream ss;
	ss << message << ' ' << error;
	throw std::runtime_error(ss.str());
}

LRESULT WndProc(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	static UINT s_uTaskbarRestart;

	switch (message)
	{
	case WM_CREATE:
		s_uTaskbarRestart = RegisterWindowMessageW(L"TaskbarCreated");
		break;
	case WM_COMMAND:
	{
		if (g_callback_item_click_ != NULL)
		{
			auto guid = FindGuid(hwnd);
			auto id = static_cast<UINT>(wParam);
			g_callback_item_click_(GetGuidString(guid).c_str(), id);
		}
		break;
	}
	case WMAPP_NOTIFYCALLBACK:
		switch (GET_X_LPARAM(lParam))
		{
		case NIN_SELECT:
		{
			POINT const pt = { GET_X_LPARAM(wParam), GET_Y_LPARAM(wParam) };

			auto guid = FindGuid(hwnd);
			if (guid == GUID_NULL) break;

			if (g_callback_icon_click_ != NULL)
			{
				g_callback_icon_click_(GetGuidString(guid).c_str());
			}
			break;
		}
		case WM_CONTEXTMENU:
		{
			POINT const pt = { GET_X_LPARAM(wParam), GET_Y_LPARAM(wParam) };

			auto guid = FindGuid(hwnd);
			if (guid == GUID_NULL) break;

			auto ni = FindNotifyIcon(GetGuidString(guid));
			if (ni == nullptr) break;

			ni->ShowContextMenu(pt);
			break;
		}
		}
		break;
	default:
		if (message == s_uTaskbarRestart)
		{
			auto guid = FindGuid(hwnd);
			if (guid == GUID_NULL) break;

			auto guidStr = GetGuidString(guid);
			auto ni_old = FindNotifyIcon(guidStr);
			if (ni_old == nullptr) break;

			g_mutex_.lock();

			HICON hicon = CopyIcon(ni_old->GetHIcon());
			std::vector<MenuItem*> menuitems = ni_old->GetMenuItems();
			std::wstring tooltip = ni_old->GetTooltip();

			DeleteNotifyIcon(guidStr.c_str());

			auto ni_new = new NotifyIcon(guidStr.c_str(), hicon);
			g_map_hwnd_guid_[ni_new->GetHwnd()] = guidStr;
			g_map_guid_icon_[guidStr] = ni_new;

			if (menuitems.size() > 0)
			{
				ni_new->SetMenuItems(menuitems);
			}
			if (!tooltip.empty())
			{
				ni_new->SetTooltip(tooltip.c_str());
			}

			g_mutex_.unlock();

			ni_new->SetVisibility(true);
		}
		return DefWindowProc(hwnd, message, wParam, lParam);
	}
	return 0;
}
