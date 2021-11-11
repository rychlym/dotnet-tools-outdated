dotnet-tools-outdated
============

[![NuGet][main-nuget-badge]][main-nuget] [![NuGet][nuget-dl-badge]][main-nuget]

[main-nuget]: https://www.nuget.org/packages/dotnet-tools-outdated/
[main-nuget-badge]: https://img.shields.io/nuget/v/dotnet-tools-outdated.svg?style=flat-square&label=nuget
[nuget-dl-badge]: https://img.shields.io/nuget/dt/dotnet-tools-outdated.svg?style=flat-square

## Overview

It might be handy to find out whether newer versions of the (.NET Core/ .NET 5/6) global tools currently installed on your machine is available.
However, the .NET command-line tools do not provide a built-in way for you to report on outdated NuGet packages of the global tools.

**dotnet-tools-outdated** is a  global tool, that allows you to quickly report on any outdated global tools currently installed on your machine. 


## Overview Detail

The out-of-box command for listing all the global tools is typed as follows:

```bash
dotnet tool list -g
```

```text
Package Id                              Version          Commands
------------------------------------------------------------------------------
apiport                                 2.8.14           ApiPort
dotnet-tools-outdated                   0.5.2            dotnet-tools-outdated
dotnet-try                              1.0.19553.4      dotnet-try
project2015to2017.migrate2019.tool      4.1.3            dotnet-migrate-2019
try-convert                             0.7.210903       try-convert
upgrade-assistant                       0.2.212405       upgrade-assistant
```

It greatly tells about the installed version. Hovewer, it does not inform if a package is outdated

The **dotnet-tools-outdated** provides such info just by typing: 
```bash
dotnet-tools-outdated
```

```text
Package Id                         Current     Available
---------------------------------------------------------
dotnet-tools-outdated              0.5.2       0.6.0
dotnet-try                         1.0.19553.4
try-convert                        0.7.210903  0.9.232202
upgrade-assistant                  0.2.212405  0.3.255803
```

It lists just the packages which are outdated in a meaning, that it can be updated to a higher version or are obsolete (marked as unlisted - e.g. dotnet-try above).

## Installation

Download and install the [.NET Core 3.1 SDK or .NET 6 SDK](https://www.microsoft.com/net/download) or newer. Once installed, run the following command:

```bash
dotnet tool install -g dotnet-tools-outdated
```

If you already have a previous version of **dotnet-tools-outdated** installed, you can uptdate to the latest version using the following command:

```bash
dotnet tool update -g dotnet-tools-outdated
```

## Usage

### Help

```bash
dotnet-tools-outdated -h
```

```text
dotnet-tools-outdated

Usage: dotnet-tools-outdated [options]

Options:
  --version         Show version information
  -?|-h|--help      Show help information
  -t|--toolPath     Custom location of the (globally) installed .NET Core tools
  -n|--name         Check just one package with the given name
  -f|--format       Output format. xml, json, or table are the valid values. (Default: table)
  -ni|--noIndent    No indent (For the json format so far)
  -o|--output       Output file path. (Default: stdout)
  -pre|--prerelease Check also pre-released versions
  --utf8            Output UTF-8 instead of system default encoding. (no bom)

```

### Outdated tools

Notes: 
 - the output is empty in case of all the installed packages up-to date
 - in case of unlisted (deprecated) NuGet package, it does not have the "Available" version column value (e.g. [dotnet-try](https://www.nuget.org/packages/dotnet-try/) below)

```bash
dotnet-tools-outdated
```

```text
Package Id                         Current     Available
---------------------------------------------------------
dotnet-tools-outdated              0.5.2       0.6.0
dotnet-try                         1.0.19553.4
try-convert                        0.7.210903  0.9.232202
upgrade-assistant                  0.2.212405  0.3.255803
```

### Outdated tools in the json format

Notes: 
 - as seen bellow it shows much more info about installed global tools including these which aren't updated. 
   - IsOutdated informs whether the installed golbal tool is/is not outdated
   - BecomeUnisted informs the installed golbal tool is/is not set as deprecated, alias unlisted at the [NuGet repo](https://www.nuget.org))
 - in case of unlisted (deprecated) NuGet package, it does not have the "Available" version column value (e.g. [dotnet-try](https://www.nuget.org/packages/dotnet-try/) below)

```bash
dotnet-tools-outdated -f json
```

```text
{
  "outdatedPackages": [
    {
      "IsOutdated": false,
      "BecomeUnlisted": false,
      "Directory": "C:\\Users\\user\\.dotnet\\tools\\.store\\apiport",
      "PackageName": "apiport",
      "CurrentVer": "2.8.14",
      "AvailableVer": "2.8.14"
    },
    {
      "IsOutdated": true,
      "BecomeUnlisted": false,
      "Directory": "C:\\Users\\user\\.dotnet\\tools\\.store\\dotnet-tools-outdated",
      "PackageName": "dotnet-tools-outdated",
      "CurrentVer": "0.5.2",
      "AvailableVer": "0.6.0"
    },
    {
      "IsOutdated": true,
      "BecomeUnlisted": true,
      "Directory": "C:\\Users\\user\\.dotnet\\tools\\.store\\dotnet-try",
      "PackageName": "dotnet-try",
      "CurrentVer": "1.0.19553.4",
      "AvailableVer": ""
    },
    {
      "IsOutdated": false,
      "BecomeUnlisted": false,
      "Directory": "C:\\Users\\user\\.dotnet\\tools\\.store\\project2015to2017.migrate2019.tool",
      "PackageName": "project2015to2017.migrate2019.tool",
      "CurrentVer": "4.1.3",
      "AvailableVer": "4.1.3"
    },
    {
      "IsOutdated": true,
      "BecomeUnlisted": false,
      "Directory": "C:\\Users\\user\\.dotnet\\tools\\.store\\try-convert",
      "PackageName": "try-convert",
      "CurrentVer": "0.7.210903",
      "AvailableVer": "0.9.232202"
    },
    {
      "IsOutdated": true,
      "BecomeUnlisted": false,
      "Directory": "C:\\Users\\user\\.dotnet\\tools\\.store\\upgrade-assistant",
      "PackageName": "upgrade-assistant",
      "CurrentVer": "0.2.212405",
      "AvailableVer": "0.3.255803"
    }
  ]
}

```


## Build

```bash
git clone https://github.com/rychlmoj/dotnet-tools-outdated
```
```bash
cd dotnet-tools-outdated/src/DotNetToolsOutdated
```
```bash
dotnet pack -c release -o nupkg
```

Output is located in ```src/DotNetToolsOutdated/nupkg```

## Uninstall

```bash
dotnet tool uninstall -g dotnet-tools-outdated
```

## Common Release Notes

The latest versions (>= 0.6.0) of the **dotnet-tools-outdated** are going to be ported just to the .NET Core (3.1) / .NET 6, which are in the LTS scheme.
This is in behalf of minimalizing the size of the package and supposing folks use this usually care about the updated versions of the global tools, 
So thy don't' need the older version of the .NET Core 2.1.
(At this moment, you can stay with 0.5.x version if you need to run it in the .NET Core 2.1. There is no functionality change/bug fixings in the 0.6.0 comparing the 0.5.2)

This aproach could continue. If the new .NET 7 comes into place, it can be ported into it and then further .NET 8 (LTS) release similarly run just under .NET 6 and .NET 8


## Useful Links

### .NET Core Global Tools

* [.NET Core Global Tools overview](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools)

* [.NET Core 2.1 Global Tools Announcement](https://devblogs.microsoft.com/dotnet/announcing-net-core-2-1-preview-1/#global-tools)
* [.NET Core Global Tools Sample](https://github.com/dotnet/core/blob/master/samples/dotnetsay/README.md)
* [.NET Core Global Tools and Gotchas](https://www.natemcmaster.com/blog/2018/02/02/dotnet-global-tool)
