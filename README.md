# Anemos

Control chassis, CPU & GPU fans on Windows 10 or later.

\**This is for desktop PCs. For laptops, try [NoteBook FanControl](https://github.com/hirschmann/nbfc).*

\**This app does not control RGB LEDs. Use [OpenRGB with hardware sync plugin](https://openrgb.org/plugins.html).*

## Screenshots

<div>
<img src="https://user-images.githubusercontent.com/80553357/234269155-ae4615f5-4366-4591-834c-8157454d6f41.PNG" width=300>
<img src="https://user-images.githubusercontent.com/80553357/234269167-7eb35b24-0dcf-4b3b-ad87-593ce694f361.PNG" width=300>
<img src="https://user-images.githubusercontent.com/80553357/234269184-f3476c61-0703-4f31-b106-fdefc38a94f2.PNG" width=300>
<img src="https://user-images.githubusercontent.com/80553357/234269194-73c74ed9-29e3-47d0-b3c8-dd0194d221e3.PNG" width=300>
</div>

## Usage

To run this app, you need to install .Net7 and [Windows App SDK runtime](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads).

If amdadlx64.dll is not found, download from [here](https://download.amd.com/dir/bin/amdadlx64.dll/)

## GPU Support

Multi-GPU not tested.

- AMD

    Cards supported by [ADLX](https://gpuopen.com/manuals/adlx/adlx-page_guide__compatibility/) (Tested on a RDNA2 card)

    > ADLX does not support some legacy AMD GPUs

    Caution: This is rather CPU intensive.

- Intel

    No (I don't have Arc cards so can't test them.)

- NVIDIA

    Cards supported by [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) (Tested on an Ampere card)

## Building

1. [Install Visual Studio components](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment#required-workloads-and-components) for WinUI development.

    Add C++/CLI support (latest)

1. Download [ADLX](https://github.com/GPUOpen-LibrariesAndSDKs/ADLX)
1. Copy `SDK` folder to `ADLXWrapper` (even if you don't have AMD cards, otherwise build fails.)

## Libraries used

- [Community Toolkit](https://github.com/CommunityToolkit/dotnet)
- [FontAwesome6.Fonts.WinUI](https://github.com/MartinTopfstedt/FontAwesome6)
- [H.NotifyIcon.WinUI](https://github.com/HavenDV/H.NotifyIcon)
- [H.OxyPlot.WinUI](https://github.com/oxyplot/oxyplot)
- [LibreHardwareMonitorLib](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)
- [PInvoke.User32](https://github.com/dotnet/pinvoke)
- [Serilog](https://github.com/serilog/serilog)
- [ZetaIpc](https://github.com/UweKeim/ZetaIpc)

## Contributing

Please create an issue / discussion before making a pull request.

Use issues for issues (bug reports) only.

## License

[GPL 3](https://github.com/hockerschwan/Anemos/blob/main/LICENSE)

## Credits

App icon: [Origami pack from Flaticon](https://www.flaticon.com/packs/origami-32)
