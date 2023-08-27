#include "ADLXWrapper.hpp"

using namespace adlx;

void Initialize()
{
	// When OnInit is called on DLL_PROCESS_ATTACH in dllmain
	// g_ADLXHelp.Initialize() takes 10 seconds.

	OnInit();
}

int GetId(const CHAR* pnpString)
{
	for (auto i = g_gpuList->Begin(); i != g_gpuList->End(); ++i)
	{
		ADLX_RESULT res = ADLX_FAIL;
		IADLXGPUPtr gpu;
		res = g_gpuList->At(i, &gpu);
		if (ADLX_FAILED(res) || gpu == nullptr)
			continue;

		const char* pnp = nullptr;
		res = gpu->PNPString(&pnp);
		if (ADLX_SUCCEEDED(res) && strcmp(pnp, pnpString) == 0)
		{
			int id;
			res = gpu->UniqueId(&id);
			if (ADLX_SUCCEEDED(res))
				return id;
		}
	}
	return -1;
}

void GetGPUs(int* result[], int* count)
{
	*count = (int)g_gpu_ids.size();

	auto size = *count * sizeof(int);
	*result = static_cast<int*>(CoTaskMemAlloc(size));
	if (*result != nullptr)
	{
		memcpy(*result, g_gpu_ids.data(), size);
	}
}

BOOL IsSupported(int id)
{
	auto gpu = FindGPU(id);
	if (gpu == nullptr)
		return FALSE;
	return (BOOL)gpu->IsSupported();
}

BOOL GetFanRange(int id, FanRange* result)
{
	auto gpu = FindGPU(id);
	if (gpu == nullptr)
		return FALSE;

	auto fanSpeedRange = gpu->GetFanSpeedRange();
	auto fanTemperatureRange = gpu->GetFanTemperatureRange();
	auto newResult = FanRange();
	newResult.minTemperature = fanTemperatureRange.minValue;
	newResult.maxTemperature = fanTemperatureRange.maxValue;
	newResult.minSpeed = fanSpeedRange.minValue;
	newResult.maxSpeed = fanSpeedRange.maxValue;

	memcpy(result, &newResult, sizeof(newResult));
	return TRUE;
}

BOOL GetFanSpeeds(int id, FanSpeeds* result)
{
	auto gpu = FindGPU(id);
	if (gpu == nullptr)
		return FALSE;

	auto s = gpu->GetFanSpeeds();
	auto speeds = FanSpeeds();
	speeds.P1 = s.at(0);
	speeds.P2 = s.at(1);
	speeds.P3 = s.at(2);
	speeds.P4 = s.at(3);
	speeds.P5 = s.at(4);

	memcpy(result, &speeds, sizeof(speeds));
	return TRUE;
}

void SetFanSpeeds(int id, FanSpeeds* speeds)
{
	auto gpu = FindGPU(id);
	if (gpu == nullptr) return;

	std::vector<std::pair<int, int>> items
	{
		std::pair<int, int>(speeds->P1.first, speeds->P1.second),
		std::pair<int, int>(speeds->P2.first, speeds->P2.second),
		std::pair<int, int>(speeds->P3.first, speeds->P3.second),
		std::pair<int, int>(speeds->P4.first, speeds->P4.second),
		std::pair<int, int>(speeds->P5.first, speeds->P5.second)
	};
	gpu->SetFanSpeeds(items);
}

void SetFanSpeed(int id, int speed)
{
	auto gpu = FindGPU(id);
	if (gpu == nullptr) return;

	gpu->SetFanSpeed(speed);
}

BOOL IsZeroRPMSupported(int id)
{
	auto gpu = FindGPU(id);
	if (gpu == nullptr)
		return FALSE;

	return gpu->IsZeroRPMSupported() ? TRUE : FALSE;
}

BOOL IsZeroRPMEnabled(int id)
{
	auto gpu = FindGPU(id);
	if (gpu == nullptr)
		return FALSE;

	return gpu->IsZeroRPMEnabled() ? TRUE : FALSE;
}

void SetZeroRPM(int id, BOOL enable)
{
	auto gpu = FindGPU(id);
	if (gpu == nullptr) return;

	gpu->SetZeroRPM(enable == 0 ? false : true);
}

void OnInit()
{
	ADLX_RESULT res = ADLX_FAIL;
	res = g_ADLXHelp.Initialize();
	if (ADLX_FAILED(res))
		throw("g_ADLXHelp initialize failed");

	res = g_ADLXHelp.GetSystemServices()->GetGPUTuningServices(&g_gpuTuningService);
	if (ADLX_FAILED(res))
		throw("GetGPUTuningServices failed");

	res = g_ADLXHelp.GetSystemServices()->GetGPUs(&g_gpuList);
	if (ADLX_FAILED(res))
		throw("GetGPUs failed");

	for (auto i = g_gpuList->Begin(); i != g_gpuList->End(); ++i)
	{
		IADLXGPUPtr gpu;
		res = g_gpuList->At(i, &gpu);
		if (ADLX_FAILED(res) || gpu == nullptr)
			continue;

		int uniqueId;
		res = gpu->UniqueId(&uniqueId);

		g_gpu_map.emplace(uniqueId, new AMDGPU(g_gpuTuningService, gpu));
		g_gpu_ids.push_back(uniqueId);
	}
}

void OnExit()
{
	for (auto& pair : g_gpu_map)
	{
		AMDGPU* gpu = pair.second;
		delete gpu;
	}
	g_gpuList->Release();
	g_gpuTuningService->Release();
	g_ADLXHelp.Terminate();
}


AMDGPU* FindGPU(int id)
{
	auto it = g_gpu_map.find(id);
	if (it == g_gpu_map.end() || !it->second->IsSupported())
		return nullptr;
	return it->second;
}
