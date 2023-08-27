#pragma once
#include "AMDGPU.hpp"
#include <map>
#include <vector>
#include <ADLXHelper/Windows/Cpp/ADLXHelper.h>

#ifdef ADLXWRAPPERCPP_EXPORTS
#define DLL __declspec(dllexport)
#else
#define DLL __declspec(dllimport)
#endif

#pragma region Structs
struct FanRange
{
	int minTemperature;
	int maxTemperature;
	int minSpeed;
	int maxSpeed;
};

struct FanSpeeds
{
	// C, %
	std::pair<int, int> P1;
	std::pair<int, int> P2;
	std::pair<int, int> P3;
	std::pair<int, int> P4;
	std::pair<int, int> P5;
};
#pragma endregion

#pragma region Exported functions
extern "C" DLL void Initialize();

extern "C" DLL int  GetId(const CHAR * pnpString);
extern "C" DLL void GetGPUs(int* result[], int* count);
extern "C" DLL BOOL IsSupported(int id);

extern "C" DLL BOOL GetFanRange(int id, FanRange * result);

extern "C" DLL BOOL GetFanSpeeds(int id, FanSpeeds * result);
extern "C" DLL void SetFanSpeeds(int id, FanSpeeds * speeds);
extern "C" DLL void SetFanSpeed(int id, int speed);

extern "C" DLL BOOL IsZeroRPMSupported(int id);
extern "C" DLL BOOL IsZeroRPMEnabled(int id);
extern "C" DLL void SetZeroRPM(int id, BOOL enable);
#pragma endregion

#pragma region Private functions
void OnInit();
void OnExit();

AMDGPU* FindGPU(int id);
#pragma endregion

#pragma region Members
static ADLXHelper g_ADLXHelp;
static adlx::IADLXGPUTuningServices* g_gpuTuningService = nullptr;
static adlx::IADLXGPUList* g_gpuList = nullptr;
static std::map<int, AMDGPU*> g_gpu_map{};
static std::vector<int> g_gpu_ids{};
#pragma endregion
