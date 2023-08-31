#pragma once
#include <vector>
#include <ADLXHelper/Windows/Cpp/ADLXHelper.h>
#include <Include/IGPUManualFanTuning.h>
#include <Include/IGPUTuning.h>

class AMDGPU
{
public:
	AMDGPU(adlx::IADLXGPUTuningServicesPtr, adlx::IADLXGPUPtr);
	~AMDGPU();

	bool IsSupported() { return isSupported_; }

	ADLX_IntRange GetFanSpeedRange() { return fanSpeedRange_; }
	ADLX_IntRange GetFanTemperatureRange() { return fanTemperatureRange_; }

	// <temperature, speed>
	std::vector<std::pair<int, int>> GetFanSpeeds();
	void SetFanSpeeds(std::vector<std::pair<int, int>> collection);
	void SetFanSpeed(int speed);

	bool IsZeroRPMSupported() { return isZeroRPMSupported_; }
	bool IsZeroRPMEnabled();
	void SetZeroRPM(bool enable);

private:
	adlx::IADLXGPUTuningServices* gpuTuningService_ = nullptr;
	adlx::IADLXGPU* gpu_ = nullptr;
	adlx::IADLXInterface* fanTuningIfc_ = nullptr;
	adlx::IADLXManualFanTuning* manualFanTuning_ = nullptr;
	bool isSupported_ = false;
	bool isZeroRPMSupported_ = false;
	ADLX_IntRange fanSpeedRange_;
	ADLX_IntRange fanTemperatureRange_;
};
