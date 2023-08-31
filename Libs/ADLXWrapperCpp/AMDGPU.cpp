#include "AMDGPU.hpp"
#include <sstream>

AMDGPU::AMDGPU(adlx::IADLXGPUTuningServicesPtr service, adlx::IADLXGPUPtr gpu)
{
	gpuTuningService_ = service;
	gpu_ = gpu;

	ADLX_RESULT res = ADLX_FAIL;
	adlx_bool supported = false;
	res = gpuTuningService_->IsSupportedManualFanTuning(gpu_, &supported);
	if (ADLX_FAILED(res) || supported == false)
	{
		std::ostringstream ss;
		ss << "IsSupportedManualFanTuning " << (int)res;
		throw std::runtime_error(ss.str());
	}

	res = gpuTuningService_->GetManualFanTuning(gpu_, &fanTuningIfc_);
	if (ADLX_FAILED(res) || fanTuningIfc_ == nullptr)
	{
		std::ostringstream ss;
		ss << "GetManualFanTuning " << (int)res;
		throw std::runtime_error(ss.str());
	}

	manualFanTuning_ = adlx::IADLXManualFanTuningPtr(fanTuningIfc_);
	if (manualFanTuning_ == nullptr)
		throw("IADLXManualFanTuningPtr");

	isSupported_ = true;
	manualFanTuning_->GetFanTuningRanges(&fanSpeedRange_, &fanTemperatureRange_);
	manualFanTuning_->IsSupportedZeroRPM(&isZeroRPMSupported_);
}

AMDGPU::~AMDGPU()
{
	(*fanTuningIfc_).Release();
	(*manualFanTuning_).Release();
	(*gpuTuningService_).Release();
}

std::vector<std::pair<int, int>> AMDGPU::GetFanSpeeds()
{
	std::vector<std::pair<int, int>> speeds;

	adlx::IADLXManualFanTuningStateListPtr states;
	adlx::IADLXManualFanTuningStatePtr state;
	ADLX_RESULT res;
	res = manualFanTuning_->GetFanTuningStates(&states);
	for (adlx_uint i = states->Begin(); i != states->End(); ++i)
	{
		res = states->At(i, &state);
		adlx_int speed = 0, temperature = 0;
		state->GetFanSpeed(&speed);
		state->GetTemperature(&temperature);
		std::pair<int, int> item(temperature, speed);
		speeds.push_back(item);
	}

	return speeds;
}

void AMDGPU::SetFanSpeeds(std::vector<std::pair<int, int>> collection)
{
	adlx::IADLXManualFanTuningStateListPtr states;
	adlx::IADLXManualFanTuningStatePtr state;
	ADLX_RESULT res = manualFanTuning_->GetEmptyFanTuningStates(&states);
	for (adlx_uint i = states->Begin(); i != states->End() || i < collection.size(); ++i)
	{
		res = states->At(i, &state);
		state->SetTemperature(collection[i].first);
		state->SetFanSpeed(collection[i].second);
	}

	adlx_int errorIndex;
	res = manualFanTuning_->IsValidFanTuningStates(states, &errorIndex);
	if (ADLX_SUCCEEDED(res))
		manualFanTuning_->SetFanTuningStates(states);
}

void AMDGPU::SetFanSpeed(int speed)
{
	adlx::IADLXManualFanTuningStateListPtr states;
	adlx::IADLXManualFanTuningStatePtr state;
	ADLX_RESULT res = manualFanTuning_->GetEmptyFanTuningStates(&states);

	int fanTemperatureStep = (fanTemperatureRange_.maxValue - fanTemperatureRange_.minValue) / (states->Size() - 1);
	for (adlx_uint i = states->Begin(); i != states->End(); ++i)
	{
		res = states->At(i, &state);
		state->SetTemperature(fanTemperatureRange_.minValue + fanTemperatureStep * i);
		state->SetFanSpeed(speed);
	}

	adlx_int errorIndex;
	res = manualFanTuning_->IsValidFanTuningStates(states, &errorIndex);
	if (ADLX_SUCCEEDED(res))
		manualFanTuning_->SetFanTuningStates(states);
}

bool AMDGPU::IsZeroRPMEnabled()
{
	if (!this->IsZeroRPMSupported())
		return false;

	adlx_bool zeroRPMState = false;
	manualFanTuning_->GetZeroRPMState(&zeroRPMState);
	return zeroRPMState;
}

void AMDGPU::SetZeroRPM(bool enable)
{
	if (!this->IsZeroRPMSupported())
		return;

	manualFanTuning_->SetZeroRPMState(enable);
}
