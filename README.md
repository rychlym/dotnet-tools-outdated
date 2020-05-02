dotnet-tools-outdated
============

[![NuGet][main-nuget-badge]][main-nuget] [![NuGet][nuget-dl-badge]][main-nuget]

[main-nuget]: https://www.nuget.org/packages/dotnet-tools-outdated/
[main-nuget-badge]: https://img.shields.io/nuget/v/dotnet-tools-outdated.svg?style=flat-square&label=nuget
[nuget-dl-badge]: https://img.shields.io/nuget/dt/dotnet-tools-outdated.svg?style=flat-square

## Overview

It might be handy to find out whether newer versions of the .NET Core global tools currently installed on your machine is available.
However, the .NET Core command-line tools do not provide a built-in way for you to report on outdated NuGet packages of the .NET Core global tools.

**dotnet-tools-outdated** is a .NET Core Global tool that allows you to quickly report on any outdated .NET Core global tools currently installed on your machine. 

## Installation

Download and install the [.NET Core 2.1 SDK or .NET Core 3.1 SDK](https://www.microsoft.com/net/download) or newer. Once installed, run the following command:

```bash
dotnet tool install -g dotnet-tools-outdated
```

If you already have a previous version of **dotnet-tools-outdated** installed, you can upgrade to the latest version using the following command:

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
  --version       Show version information
  -?|-h|--help    Show help information
  -t|--toolPath   Custom location of the (globally) installed .NET Core tools
  -n|--name       Check just one package with the given name
  -f|--format     Output format. xml, json, or table are the valid values. (Default: table)
  -ni|--noIndent  No indent (For the json format so far)
  -o|--output     Output file path. (Default: stdout)
  --utf8          Output UTF-8 instead of system default encoding. (no bom)

TODO:
  --incl-prerelease  Include prerelease packages
```

### Outdated tools

Note: The output is empty in case of all the installed packages up-to date.

```bash
dotnet-tools-outdated
```

```text
Package Id                         Current Available
----------------------------------------------------
findref                            0.2.0   0.2.1
project2015to2017.migrate2017.tool 3.0.0   4.1.2
volo.abp.cli                       2.0.0   2.1.0
```

### Outdated tools in the json format

```bash
dotnet-tools-outdated -f json
```

```text
{
  "outdatedPackages": [

    {
      "name": "findref",
      "currentVersion": "0.2.0",
      "availableVersion": "0.2.1"
    },
    {
      "name": "project2015to2017.migrate2017.tool",
      "currentVersion": "3.0.0",
      "availableVersion": "4.1.2"
    },
    {
      "name": "volo.abp.cli",
      "currentVersion": "1.0.0",
      "availableVersion": "2.0.0"
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

## Useful Links

### .NET Core Global Tools

* [.NET Core Global Tools overview](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools)

* [.NET Core 2.1 Global Tools Announcement](https://devblogs.microsoft.com/dotnet/announcing-net-core-2-1-preview-1/#global-tools)
* [.NET Core Global Tools Sample](https://github.com/dotnet/core/blob/master/samples/dotnetsay/README.md)
* [.NET Core Global Tools and Gotchas](https://www.natemcmaster.com/blog/2018/02/02/dotnet-global-tool)
