Control chassis, CPU & GPU fans on Windows 10 or later.

\**This is for desktop PCs. For laptops, try [NoteBook FanControl](https://github.com/hirschmann/nbfc).*

\**This app does not control RGB LEDs. Use [OpenRGB with hardware sync plugin](https://openrgb.org/plugins.html).*

## Screenshots

<div>
<img src="https://github.com/hockerschwan/Anemos/assets/80553357/9bcb43e0-98b8-47b1-8f8b-0f949b2628ca" width=300>
<img src="https://github.com/hockerschwan/Anemos/assets/80553357/38c0645e-42d0-4337-9ad1-e2cead7613b4" width=300>
<img src="https://github.com/hockerschwan/Anemos/assets/80553357/3ae32364-f7b1-4f93-bae4-d345fee34c75" width=300>
<img src="https://github.com/hockerschwan/Anemos/assets/80553357/87741a0e-ebb4-489c-ae0f-255c4a081bae" width=300>
<img src="https://github.com/hockerschwan/Anemos/assets/80553357/550fd230-eb50-423d-80b1-fa520eaf6dbf" width=300>
<img src="https://github.com/hockerschwan/Anemos/assets/80553357/95686806-bd45-44bb-9c70-683bab4975a6">
</div>

## Features

- Switch profiles automatically based on customizable rules
    - Process
        - Process name ("steam")
        - Memory used  ("python" && Memory > 4000MB)
    - Sensor
    - Time of day
- Show current value and history of fan speed, curve value and sensor value in tray

## Usage

To run this app, you need to install [.NET8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
and [Windows App SDK runtime](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads).

(You don't need to if you use "Self-Contained" version.)

## GPU Support

Multi-GPU not tested.

- AMD

    Cards supported by [ADLX](https://gpuopen.com/manuals/adlx/adlx-page_guide__compatibility/) (Tested on a RDNA2 card)

    > ADLX does not support some legacy AMD GPUs

- Intel

    No (I don't have Arc cards so can't test them.)

- NVIDIA

    Cards supported by [LibreHardwareMonitorLib](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) (Tested on an Ampere card)

## Building

1. [Install Visual Studio components](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment#required-workloads-and-components) for WinUI development.
1. Install "Desktop development with C++" and "Windows 11 SDK (10.0.22621.0)" in Visual Studio Installer
1. Download [ADLX](https://github.com/GPUOpen-LibrariesAndSDKs/ADLX)
1. Copy `SDK` folder of ADLX to `Libs/ADLXWrapperCpp` (even if you don't have AMD cards, otherwise build fails.)

## Libraries used

- [.NET Community Toolkit MVVM](https://github.com/CommunityToolkit/dotnet)
- [AMD Device Library eXtra](https://github.com/GPUOpen-LibrariesAndSDKs/ADLX)
- [LibreHardwareMonitorLib](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)
- [PlainlyIpc](https://github.com/chrbaeu/PlainlyIpc)
- [Serilog](https://github.com/serilog/serilog)
- [Windows Community Toolkit](https://github.com/CommunityToolkit/Windows)
- [WinUIEx](https://github.com/dotMorten/WinUIEx)

## Contributing

Please create an issue / discussion before making a pull request.

Use issues for issues (bug reports) only.

## License

[GPL 3](https://github.com/hockerschwan/Anemos/blob/main/LICENSE)

## Credits

- App icon: [Origami pack from Flaticon](https://www.flaticon.com/packs/origami-32)
- [Azeret Mono](https://github.com/displaay/Azeret)
- [TablerIcons](https://github.com/tabler/tabler-icons)
