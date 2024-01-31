Control chassis, CPU & GPU fans on Windows 10 or later.

\**This is for desktop PCs. For laptops, try [NoteBook FanControl](https://github.com/hirschmann/nbfc).*

\**This app does not control RGB LEDs. Use [OpenRGB with hardware sync plugin](https://openrgb.org/plugins.html).*

## Screenshots

<div>
<img src="https://github.com/hockerschwan/Anemos/assets/80553357/1a402336-bbe5-4355-bcfe-c5646eb7b486" width=300>
<img src="https://github.com/hockerschwan/Anemos/assets/80553357/0e1c7cd8-5576-49fb-8310-22f9248b1278" width=300>
<img src="https://github.com/hockerschwan/Anemos/assets/80553357/388b063b-44e6-4afc-81a3-b34a539df8ca" width=300>
<img src="https://github.com/hockerschwan/Anemos/assets/80553357/b2a144d5-8733-4b85-aef5-f8a7fc0b6950" width=300>
<img src="https://github.com/hockerschwan/Anemos/assets/80553357/d63a44c5-3bf7-423f-af33-50630c768694" width=300>
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
- [ScottPlot](https://github.com/ScottPlot/ScottPlot)
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
