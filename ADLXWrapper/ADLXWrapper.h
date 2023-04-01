#pragma once
#include <map>
#include <string>
#include <msclr/marshal_cppstd.h>
#include <ADLXHelper/Windows/Cpp/ADLXHelper.h>
#include <Include/IGPUManualFanTuning.h>
#include <Include/IGPUTuning.h>

using namespace System;
using namespace System::Collections::Generic;

using namespace adlx;

namespace ADLXWrapper
{
	static ADLXHelper g_ADLXHelp;

	class AMDGPU;
	ref class FanRangeResult;

	public ref class ADLX
	{
	public:
		ADLX();
		~ADLX();
		!ADLX();

		int GetId(String^ pnpString);
		bool Exists(int id);
		bool IsSupported(int id);
		List<int>^ GetGPUs();

		bool GetFanRange(int id, FanRangeResult^** result);

		// <temperature, speed>
		bool GetFanSpeeds(int id, List<Tuple<int, int>^>^** result);
		bool SetFanSpeed(int id, int speed);

		bool IsZeroRPMSupported(int id);
		bool IsZeroRPMEnabled(int id);
		bool SetZeroRPM(int id, bool enable);

	internal:
		void Throw(String^ message);

	private:
		bool GetGPU(int id, AMDGPU** gpu);
		std::string ConvertStringToStd(String^ managedStr);
		String^ ConvertStringToManaged(std::string stdStr);

		IADLXGPUTuningServices* gpuTuningService_ = nullptr;
		IADLXGPUList* gpuList_ = nullptr;
		std::map<int, AMDGPU*>* gpus_;
	};

	private class AMDGPU
	{
	public:
		AMDGPU(IADLXGPUTuningServicesPtr, IADLXGPUPtr);
		~AMDGPU();

		bool IsSupported() { return isSupported_; }

		ADLX_IntRange GetFanSpeedRange() { return fanSpeedRange_; }
		ADLX_IntRange GetFanTemperatureRange() { return fanTemperatureRange_; }

		// <temperature, speed>
		std::vector<std::pair<int, int>> GetFanSpeeds();
		void SetFanSpeed(int speed);

		bool IsZeroRPMSupported() { return isZeroRPMSupported_; }
		bool IsZeroRPMEnabled();
		void SetZeroRPM(bool enable);

	private:
		IADLXGPUTuningServices* gpuTuningService_ = nullptr;
		IADLXGPU* gpu_ = nullptr;
		IADLXInterface* fanTuningIfc_ = nullptr;
		IADLXManualFanTuning* manualFanTuning_ = nullptr;
		bool isSupported_ = false;
		bool isZeroRPMSupported_ = false;
		ADLX_IntRange fanSpeedRange_;
		ADLX_IntRange fanTemperatureRange_;
	};

	public ref class FanRangeResult
	{
	public:
		int minTemperature = 0;
		int maxTemperature = 0;
		int minSpeed = 0;
		int maxSpeed = 0;
	};
}
