#pragma once
#include  <Windows.h>

enum MenuItemType
{
	Default, Submenu, Separator, Check, Radio
};

struct MenuItem
{
	UINT Id;
	MenuItemType Type;
	WCHAR* Text;
	BOOL IsChecked;
	BOOL IsEnabled;
	HICON Icon;
};
