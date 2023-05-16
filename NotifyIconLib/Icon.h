#pragma once
#include "Global.h"
#include <Windows.h>
#include <string>

namespace NotifyIconLib
{
	private class Icon
	{
	public:
		Icon();
		~Icon();
		HWND GetHwnd() { return hWnd_; };
		bool SetTooltip(std::wstring tooltip);
		bool SetVisible(bool visible);
		void ShowContextMenu(HWND hwnd, POINT pt);

	private:
		int CreateContextMenu(int idStart, HMENU& result);
		bool CreateIcon();
		bool DeleteIcon();
		void RegisterWindowClass(WNDPROC lpfnWndProc);
		void Throw(const std::wstring message, const DWORD err);

		GUID const guid_ = __uuidof(NotifyIcon);
		wchar_t* const szWindowClass_ = L"WindowClass";

		HWND hWnd_ = nullptr;
		HINSTANCE hInstance_ = nullptr;
	};
}
