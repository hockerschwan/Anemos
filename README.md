# Anemos

Control chassis, CPU & GPU fans on Windows 10 or later.

\**This is for desktop PCs. For laptops, try [NoteBook FanControl](https://github.com/hirschmann/nbfc).*

\**This app does not control RGB LEDs. Use [OpenRGB with hardware sync plugin](https://openrgb.org/plugins.html).*

## Screenshots

<div>
<img src="https://user-images.githubusercontent.com/80553357/238078414-fbb1b808-758b-469b-af83-4601d76776f1.png" width=300>
<img src="https://user-images.githubusercontent.com/80553357/238078419-79279cfd-f916-4de2-9589-91e03b8e8590.png" width=300>
<img src="https://user-images.githubusercontent.com/80553357/238078424-caba8b31-dae1-446e-b9c9-e1d0f1611e5e.png" width=300>
<img src="https://user-images.githubusercontent.com/80553357/238078422-003c5968-5613-4a89-bb93-236634c52dde.png" width=300>
</div>

## Usage

To run this app, you need to install .Net7 and [Windows App SDK runtime](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads).

If amdadlx64.dll is not found, update driver or download from [here](https://download.amd.com/dir/bin/amdadlx64.dll/).

## GPU Support

Multi-GPU not tested.

- AMD

    Cards supported by [ADLX](https://gpuopen.com/manuals/adlx/adlx-page_guide__compatibility/) (Tested on a RDNA2 card)

    > ADLX does not support some legacy AMD GPUs

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
- [H.OxyPlot.WinUI](https://github.com/HavenDV/H.OxyPlot)
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
