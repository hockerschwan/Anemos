#include "NotifyIconLib.h"
#include <windowsx.h>
#include <msclr/marshal_cppstd.h>
#include <sstream>

using namespace NotifyIconLib;

NotifyIcon::NotifyIcon()
{
	CreateIcon();
	menuItems_ = new std::vector<MenuItem>();
}

NotifyIcon::!NotifyIcon()
{
	DestroyWindow(icon_->GetHwnd());
	delete icon_;
	delete menuItems_;
}

void NotifyIcon::SetMenuItems(List<MenuItemManaged^>^ items)
{
	menuItems_->clear();

	for (int i = 0; i < items->Count; ++i)
	{
		auto item = items[i];
		auto id = item->Id;
		TypeNative type = static_cast<TypeNative>(item->Type);
		auto str = item->Text;
		auto wStr = msclr::interop::marshal_as<std::wstring, String^>(str);
		auto checked = item->IsChecked;
		auto enabled = item->IsEnabled;
		auto menuitem = MenuItem{ id, type, wStr, checked, enabled };
		menuItems_->push_back(menuitem);
	}
}

void NotifyIcon::SetTooltip(String^ tooltip)
{
	tooltip_ = tooltip;
	auto wStr = msclr::interop::marshal_as<std::wstring, String^>(tooltip);
	if (!icon_->SetTooltip(wStr))
	{
		auto err = GetLastError();
		std::wostringstream ss;
		ss << "SetTooltip: " << err;
		Throw(ss.str());
	}
}

void NotifyIconLib::NotifyIcon::SetVisible(bool visible)
{
	if (!icon_->SetVisible(visible))
	{
		auto err = GetLastError();
		std::wostringstream ss;
		ss << "SetVisibility: " << err;
		Throw(ss.str());
	}
}

void NotifyIconLib::NotifyIcon::CreateIcon()
{
	delete icon_;
	icon_ = new Icon;
}

void NotifyIconLib::NotifyIcon::Throw(std::wstring message)
{
	String^ str = msclr::interop::marshal_as<String^, std::wstring>(message);
	throw gcnew Exception(str);
}

LRESULT NotifyIconLib::WndProc(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	static UINT s_uTaskbarRestart;

	switch (message)
	{
	case WM_CREATE:
		s_uTaskbarRestart = RegisterWindowMessageW(L"TaskbarCreated");
		break;
	case WM_COMMAND:
		NotifyIcon::Instance->FireItemEvent(static_cast<int>(wParam));
		break;
	case WMAPP_NOTIFYCALLBACK:
		switch (GET_X_LPARAM(lParam))
		{
		case NIN_SELECT:
			NotifyIcon::Instance->FireIconEvent();
			break;
		case WM_CONTEXTMENU:
		{
			POINT const pt = { GET_X_LPARAM(wParam), GET_Y_LPARAM(wParam) };
			NotifyIcon::Instance->GetIcon()->ShowContextMenu(hwnd, pt);
			break;
		}
		}
		break;
	default:
		if (message == s_uTaskbarRestart)
		{
			NotifyIcon::Instance->CreateIcon();
			NotifyIcon::Instance->SetTooltip(NotifyIcon::Instance->tooltip_);
			NotifyIcon::Instance->SetVisible(true);
		}
		return DefWindowProc(hwnd, message, wParam, lParam);
	}
	return 0;
}
