#include "pch.hpp"
#include "NotifyIconLib.hpp"

BOOL APIENTRY DllMain(
	HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		g_hInstance_ = GetModuleHandleW(NULL);
		g_windowClass_.append(std::to_wstring(GetCurrentProcessId()));
		OnInit();
		break;
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
		break;
	case DLL_PROCESS_DETACH:
		OnExit();
		break;
	}
	return TRUE;
}
