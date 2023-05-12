#pragma once
#include <Windows.h>
#include <string>

using namespace System;

namespace NotifyIconLib
{
#if _WIN64
#if _DEBUG
	ref class __declspec(uuid("0dbc5ce8-0daf-4ea8-a438-99cd1cbf845d")) NotifyIcon;
#else
	ref class __declspec(uuid("ec484b15-4025-4f90-bbbf-acc8c2d621ca")) NotifyIcon;
#endif
#else
#if _DEBUG
	ref class __declspec(uuid("becdc068-fa1f-4f7c-9645-cf2e145045cc")) NotifyIcon;
#else
	ref class __declspec(uuid("fba272cf-7524-49f5-9add-972b745fe1b4")) NotifyIcon;
#endif
#endif

	UINT const WMAPP_NOTIFYCALLBACK = WM_APP + 1;
	LRESULT CALLBACK WndProc(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam);

	public enum class TypeManaged
	{
		Default, Submenu, Separator, Check, Radio
	};

	public enum TypeNative
	{
		Default, Submenu, Separator, Check, Radio
	};

	public ref struct MenuItemManaged
	{
		int Id;
		TypeManaged Type;
		String^ Text;
		bool IsChecked;
		bool IsEnabled;
	};

	public struct MenuItem
	{
		int Id;
		TypeNative Type;
		std::wstring Text;
		bool IsChecked;
		bool IsEnabled;
	};
}
