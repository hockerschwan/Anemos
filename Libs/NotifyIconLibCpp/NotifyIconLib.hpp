#pragma once
#include "MenuItem.hpp"
#include "NotifyIcon.hpp"
#include <map>
#include <string>

#ifdef NOTIFYICONLIBCPP_EXPORTS
#define DLL __declspec(dllexport)
#else
#define DLL __declspec(dllimport)
#endif

typedef void (*callback_function_guid)(const char* guid);
typedef void (*callback_function_uint)(const char* guid, UINT id_item);

const UINT g_nItemsPerDepth_ = 10000u;
const UINT WMAPP_NOTIFYCALLBACK = WM_APP + 1;
const UINT WMAPP_DESTROYICON = WM_APP + 2;

static HINSTANCE g_hInstance_;
static ULONG_PTR g_gdiplusToken_;
static std::wstring g_windowClass_ = L"WindowClass_";

static callback_function_guid g_callback_icon_click_ = NULL;
static callback_function_uint g_callback_item_click_ = NULL;

static std::map<HWND, std::string> g_map_hwnd_guid_{};
static std::map<std::string, NotifyIcon*> g_map_guid_icon_{};


extern "C" DLL void CreateNotifyIcon(const char* guid, HICON hIcon);
extern "C" DLL void DeleteNotifyIcon(const char* guid);

extern "C" DLL void SetIcon(const char* guid, HICON hIcon);
extern "C" DLL void SetMenuItems(const char* guid, int count, MenuItem * items[]);
extern "C" DLL void SetTooltip(const char* guid, const wchar_t* tooltip);
extern "C" DLL void SetVisibility(const char* guid, BOOL visible);

extern "C" DLL void SetChecked(const char* guid, UINT id, BOOL checked);
extern "C" DLL void SetEnabled(const char* guid, UINT id, BOOL enabled);

extern "C" DLL void SetCallback_IconClick(callback_function_guid callback);
extern "C" DLL void SetCallback_ItemClick(callback_function_uint callback);

void OnInit();
void OnExit();

HBITMAP GetBitmapFromIcon(HICON hIcon);

GUID GetGuid(const char str[36]);
std::string GetGuidString(GUID guid);

GUID FindGuid(HWND hWnd);
MenuItem* FindMenuItem(std::string guid, UINT id);
NotifyIcon* FindNotifyIcon(std::string guid);

void Throw(const char* message, int error);

LRESULT CALLBACK WndProc(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam);
