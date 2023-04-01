#include "ADLXWrapper.h"

ADLXWrapper::ADLX::ADLX()
{
	ADLX_RESULT res = ADLX_FAIL;
	res = g_ADLXHelp.Initialize();
	if (ADLX_FAILED(res))
		Throw("g_ADLXHelp initialize failed");

	pin_ptr<IADLXGPUTuningServices*> pService = &gpuTuningService_;
	res = g_ADLXHelp.GetSystemServices()->GetGPUTuningServices(pService);
	if (ADLX_FAILED(res))
		Throw("Get GPU tuning services failed");

	pin_ptr<IADLXGPUList*> pList = &gpuList_;
	res = g_ADLXHelp.GetSystemServices()->GetGPUs(pList);
	if (ADLX_FAILED(res))
		Throw("Get GPU list failed");
}

ADLXWrapper::ADLX::~ADLX()
{
	this->!ADLX();
}

ADLXWrapper::ADLX::!ADLX()
{
	for (auto& pair : *gpus_)
	{
		AMDGPU* gpu = pair.second;
		delete gpu;
	}
	delete gpus_;
	(*gpuList_).Release();
	(*gpuTuningService_).Release();

	g_ADLXHelp.Terminate();
}

int ADLXWrapper::ADLX::GetId(String^ pnpString)
{
	for (auto i = gpuList_->Begin(); i != gpuList_->End(); ++i)
	{
		ADLX_RESULT res = ADLX_FAIL;
		IADLXGPUPtr gpu;
		res = gpuList_->At(i, &gpu);
		if (ADLX_FAILED(res) || gpu == nullptr)
			continue;

		const char* pnp = nullptr;
		res = gpu->PNPString(&pnp);
		if (ADLX_SUCCEEDED(res) && ConvertStringToManaged(pnp) == pnpString)
		{
			int id;
			res = gpu->UniqueId(&id);
			if (ADLX_SUCCEEDED(res))
				return id;
		}
	}
	return -1;
}

bool ADLXWrapper::ADLX::Exists(int id)
{
	return gpus_->find(id) != gpus_->end();
}

bool ADLXWrapper::ADLX::IsSupported(int id)
{
	AMDGPU* gpu = nullptr;
	if (!GetGPU(id, &gpu))
		return false;
	return gpu->IsSupported();
}

List<int>^ ADLXWrapper::ADLX::GetGPUs()
{
	auto list = gcnew List<int>();
	for (const auto& pair : *gpus_)
	{
		list->Add(pair.first);
		pair.second;
	}
	return list;
}

bool ADLXWrapper::ADLX::GetFanRange(int id, FanRangeResult^** result)
{
	AMDGPU* gpu = nullptr;
	if (!GetGPU(id, &gpu))
		return false;

	auto fanSpeedRange = gpu->GetFanSpeedRange();
	auto fanTemperatureRange = gpu->GetFanTemperatureRange();
	auto newResult = gcnew FanRangeResult();
	newResult->minTemperature = fanTemperatureRange.minValue;
	newResult->maxTemperature = fanTemperatureRange.maxValue;
	newResult->minSpeed = fanSpeedRange.minValue;
	newResult->maxSpeed = fanSpeedRange.maxValue;

	*result = &newResult;
	return true;
}

bool ADLXWrapper::ADLX::GetFanSpeeds(int id, List<Tuple<int, int>^>^** result)
{
	AMDGPU* gpu = nullptr;
	if (!GetGPU(id, &gpu))
		return false;

	auto speeds = gcnew List<Tuple<int, int>^>();
	for (const auto& pair : gpu->GetFanSpeeds())
	{
		speeds->Add(gcnew Tuple<int, int>(pair.first, pair.second));
	}
	*result = &speeds;
	return true;
}

bool ADLXWrapper::ADLX::SetFanSpeed(int id, int speed)
{
	AMDGPU* gpu = nullptr;
	if (!GetGPU(id, &gpu))
		return false;

	gpu->SetFanSpeed(speed);
	return true;
}

bool ADLXWrapper::ADLX::IsZeroRPMSupported(int id)
{
	AMDGPU* gpu = nullptr;
	if (!GetGPU(id, &gpu))
		return false;
	return gpu->IsZeroRPMSupported();
}

bool ADLXWrapper::ADLX::IsZeroRPMEnabled(int id)
{
	AMDGPU* gpu = nullptr;
	if (!GetGPU(id, &gpu))
		return false;
	return gpu->IsZeroRPMEnabled();
}

bool ADLXWrapper::ADLX::SetZeroRPM(int id, bool enable)
{
	AMDGPU* gpu = nullptr;
	if (!GetGPU(id, &gpu))
		return false;

	gpu->SetZeroRPM(enable);
	return true;
}

void ADLXWrapper::ADLX::Throw(String^ message)
{
	if (gpus_ != nullptr)
	{
		for (auto& pair : *gpus_)
		{
			auto gpu = pair.second;
			delete gpu;
		}
	}
	delete gpus_;
	delete gpuList_;
	delete gpuTuningService_;
	ADLXWrapper::g_ADLXHelp.Terminate();
	throw gcnew Exception(message);
}

bool ADLXWrapper::ADLX::GetGPU(int id, AMDGPU** gpu)
{
	auto it = gpus_->find(id);
	if (it == gpus_->end() || !it->second->IsSupported())
		return false;

	*gpu = *(&it->second);
	return true;
}

std::string ADLXWrapper::ADLX::ConvertStringToStd(String^ managedStr)
{
	return msclr::interop::marshal_as<std::string, String^>(managedStr);
}

String^ ADLXWrapper::ADLX::ConvertStringToManaged(std::string stdStr)
{
	return msclr::interop::marshal_as<String^, std::string>(stdStr);
}

/////////////////////////////////////////////////

ADLXWrapper::AMDGPU::AMDGPU(IADLXGPUTuningServicesPtr tuning, IADLXGPUPtr gpu)
{
	gpuTuningService_ = tuning;
	gpu_ = gpu;

	ADLX_RESULT res = ADLX_FAIL;
	adlx_bool supported = false;
	res = gpuTuningService_->IsSupportedManualFanTuning(gpu_, &supported);
	if (ADLX_FAILED(res) || supported == false)
		return;

	pin_ptr<IADLXInterface*> pIfc = &fanTuningIfc_;
	res = gpuTuningService_->GetManualFanTuning(gpu_, pIfc);
	if (ADLX_FAILED(res) || fanTuningIfc_ == nullptr)
		return;

	manualFanTuning_ = IADLXManualFanTuningPtr(fanTuningIfc_);
	if (manualFanTuning_ == nullptr)
		return;

	isSupported_ = true;
	manualFanTuning_->GetFanTuningRanges(&fanSpeedRange_, &fanTemperatureRange_);
	manualFanTuning_->IsSupportedZeroRPM(&isZeroRPMSupported_);
}

ADLXWrapper::AMDGPU::~AMDGPU()
{
	(*fanTuningIfc_).Release();
	(*manualFanTuning_).Release();
	(*gpuTuningService_).Release();
}

std::vector<std::pair<int, int>> ADLXWrapper::AMDGPU::GetFanSpeeds()
{
	std::vector<std::pair<int, int>> speeds;

	ADLX_RESULT res;
	IADLXManualFanTuningStateListPtr states;
	IADLXManualFanTuningStatePtr state;
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

void ADLXWrapper::AMDGPU::SetFanSpeed(int speed)
{
	IADLXManualFanTuningStateListPtr states;
	IADLXManualFanTuningStatePtr state;
	ADLX_RESULT res = manualFanTuning_->GetEmptyFanTuningStates(&states);

	int fanTemperatureStep = (fanTemperatureRange_.maxValue - fanTemperatureRange_.minValue) / (states->Size() + 1);
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

bool ADLXWrapper::AMDGPU::IsZeroRPMEnabled()
{
	if (!this->IsZeroRPMSupported())
		return false;

	adlx_bool zeroRPMState = false;
	manualFanTuning_->GetZeroRPMState(&zeroRPMState);
	return zeroRPMState;
}

void ADLXWrapper::AMDGPU::SetZeroRPM(bool enable)
{
	if (!this->IsZeroRPMSupported())
		return;

	manualFanTuning_->SetZeroRPMState(enable);
}
