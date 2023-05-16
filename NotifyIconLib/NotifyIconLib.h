#pragma once
#include "Global.h"
#include "Icon.h"
#include <vector>

using namespace System;
using namespace System::Collections::Generic;

namespace NotifyIconLib
{
	public ref class NotifyIcon
	{
	public:
		static property NotifyIcon^ Instance
		{
			NotifyIcon^ get() { return instance_->Value; }
		}
		~NotifyIcon() { this->!NotifyIcon(); };
		!NotifyIcon();
		void SetMenuItems(List<MenuItemManaged^>^ items);
		void SetTooltip(String^ tooltip);
		void SetVisible(bool visible);

		delegate void IconClickEventHandler();
		event IconClickEventHandler^ IconClick;

		delegate void ItemClickEventHandler(int id);
		event ItemClickEventHandler^ ItemClick;

	internal:
		void CreateIcon();
		Icon* GetIcon() { return icon_; };
		std::vector<MenuItem>* GetMenuItems() { return menuItems_; };
		void FireIconEvent() { IconClick(); };
		void FireItemEvent(int id) { ItemClick(id); };
		void Throw(std::wstring message);

		String^ tooltip_;

	private:
		NotifyIcon();
		static NotifyIcon^ CreateInstance() { return gcnew NotifyIcon(); };
		static Lazy<NotifyIcon^>^ instance_ = gcnew Lazy<NotifyIcon^>(gcnew Func<NotifyIcon^>(CreateInstance));

		Icon* icon_ = nullptr;
		std::vector<MenuItem>* menuItems_ = nullptr;
	};
}
