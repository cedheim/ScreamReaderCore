# ScreamReader Core
Is a .NET Core port of Scream Reader by @MrShoenel for Scream by @duncanthrax, hosted here: https://github.com/duncanthrax/scream.

The main purpose of this port is to wrap the Scream protocol in a Windows Service.

Because of NAudio's platform dependencies, this project is only supported on Windows.

## Building
Make sure to restore all *NuGet*-packages prior to building to avoid any errors.

## Installing as Windows Service
To install the built executable as a Windows Service, use the following command in an elevated Command Prompt (run as Administrator):

```
sc.exe create "ScreamReaderCore" binpath= "C:\Path\To\ScreamReaderCore.WindowsService.exe"
```
