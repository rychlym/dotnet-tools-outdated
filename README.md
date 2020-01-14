dotnet-tools-outdated
============

[![NuGet][main-nuget-badge]][main-nuget] [![NuGet][nuget-dl-badge]][main-nuget]

[main-nuget]: https://www.nuget.org/packages/dotnet-tools-outdated/
[main-nuget-badge]: https://img.shields.io/nuget/v/dotnet-tools-outdated.svg?style=flat-square&label=nuget
[nuget-dl-badge]: https://img.shields.io/nuget/dt/dotnet-tools-outdated.svg?style=flat-square

Checks if any of installed .NET Core CLI tools are outdated

## Installation

### .NET Core 2.1 or .NET Core 3.1 & higher
```
dotnet tool install -g dotnet-tools-outdated
```
## Usage

### Help

```
$ dotnet-tools-outdated --help

dotnet-tools-outdated

Usage: dotnet-tools-outdated [options]

Options:
  --version             Show version information.
  -?|-h|--help          Show help information.
  -t|--toolPath         Custom path to the installed .NET CLI packages.
  -n|--name             Check just one package with the given name.
  -f|--format           Output format. xml, json, or table are the valid values. (Default: table)
  -u|--utf8             Output UTF-8 instead of system default encoding. (no bom)
  -o|--output           Output file path. (Default: stdout)

DODO:
  --incl-prerelease  Include prerelease packages
```

### Outdated tools

Note: The output is empty in case of all the installed packages up-to date.

```
$ dotnet-tools-outdated

Package Id                         Installed Available
------------------------------------------------------
findref                            0.2.0     0.2.1
project2015to2017.migrate2017.tool 3.0.0     4.1.2
volo.abp.cli                       1.0.0     2.0.0
```

### Outdated tools in the json format

```
$ dotnet-tools-outdated -f json

{
  "outdatedPackages": [

    {
      "name": "findref",
      "installedVersion": "0.2.0",
      "availableVersion": "0.2.1"
    },
    {
      "name": "project2015to2017.migrate2017.tool",
      "installedVersion": "3.0.0",
      "availableVersion": "4.1.2"
    },
    {
      "name": "volo.abp.cli",
      "installedVersion": "1.0.0",
      "availableVersion": "2.0.0"
    }
  ]
}

```


## Build

```
git clone https://github.com/rychlmoj/dotnet-tools-outdated
```
```
cd dotnet-tools-outdated/src/DotNetToolsOutdated
```
```
dotnet pack -c release -o nupkg
```

Output is located in ```src/DotNetToolsOutdated/nupkg```

### Uninstall

```
dotnet tools uninstall -g dotnet-tools-outdated
```

## Useful Links

* [.NET Core Global Tools overview](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools)

* [.NET Core 2.1 Global Tools Announcement](https://devblogs.microsoft.com/dotnet/announcing-net-core-2-1-preview-1/#global-tools)
* [.NET Core Global Tools Sample](https://github.com/dotnet/core/blob/master/samples/dotnetsay/README.md)
* [.NET Core Global Tools and Gotchas](https://www.natemcmaster.com/blog/2018/02/02/dotnet-global-tool)
