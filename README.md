dotnet-tools-outdated
============

[![NuGet][main-nuget-badge]][main-nuget] [![NuGet][nuget-dl-badge]][main-nuget]

[main-nuget]: https://www.nuget.org/packages/dotnet-tools-outdated/
[main-nuget-badge]: https://img.shields.io/nuget/v/dotnet-tools-outdated.svg?style=flat-square&label=nuget
[nuget-dl-badge]: https://img.shields.io/nuget/dt/dotnet-tools-outdated.svg?style=flat-square


## News

Version 8.0.0 is targeting .NET 10, 8 and has a configurable http client via the appsettings.json file, located in the user profile directory under the path: 
```.config/dotnet-tools-outdated/appsettings.json```

Since the dotnet-tools-outdated ver. 0.7.0 is supported checking of **the locally instaled packages**.
To see more, please go the [chapter bellow](#localy-installed-net-command-line-tools).
There are also several breaking changes and fixes, especially regarding the JSON and XML output.
There are also small updates of the options. 
I will provide a list of the changes at the [NuGet package web site](https://www.nuget.org/packages/dotnet-tools-outdated/0.7.0).


## Overview

It might be handy to find out whether newer versions of the .NET global tools currently installed on your machine is available.
However, the .NET command-line tools do not provide a built-in way for you to report on outdated NuGet packages of the global tools.

**dotnet-tools-outdated** is a global tool, that allows you to quickly report on any outdated global tools currently installed on your machine. 


## Detail

The out-of-box command for listing all the global tools is typed as follows:

```bash
dotnet tool list -g
```

```text
Package Id                              Version          Commands
------------------------------------------------------------------------------
apiport                                 2.8.14           ApiPort
dotnet-tools-outdated                   0.7.0            dotnet-tools-outdated
dotnet-try                              1.0.19553.4      dotnet-try
project2015to2017.migrate2019.tool      4.1.3            dotnet-migrate-2019
try-convert                             0.7.210903       try-convert
upgrade-assistant                       0.2.212405       upgrade-assistant
```

It greatly tells about the installed version. Hovewer, it does not inform if a package is outdated.

The **dotnet-tools-outdated** provides such info just by typing: 
```bash
dotnet-tools-outdated
```

```text
Package Id                         Current     Available
---------------------------------------------------------
dotnet-tools-outdated              0.5.2       0.7.0
dotnet-try                         1.0.19553.4
try-convert                        0.7.210903  0.9.232202
upgrade-assistant                  0.2.212405  0.3.255803
```

It lists just the packages which are outdated in a meaning, that it can be updated to a higher version or are obsolete (marked as unlisted - e.g. dotnet-try above). 
Or the output is just empty, if there is no outdated global tool.

<span name="localy-installed-net-command-line-tools"></span>
## Locally installed .NET command-line tools

In a case of running the **dotnet-tools-outdated** from a directory covered by localy installed .NET tools, the locally installed tools are also checked.
If the local tools are outdated, the console shows an additional and slightly different table preceding the table with the globally installed tools.

```bash
dotnet-tools-outdated -s
```
(the -s option shows one statistics row at the end)
```text
Package Id            Current Available Manifest
-----------------------------------------------------------------------------------------------------
dotnet-tools-outdated 0.3.0   0.7.0     C:\Users\mr\source\repos\my\console\.config\dotnet-tools.json
dotnetsay             2.0.1   2.1.7     C:\Users\mr\source\repos\.config\dotnet-tools.json

Package Id            Current Available
---------------------------------------
dotnet-tools-outdated 0.6.1   0.7.0

2 local and 3 global packages available. Found 3 outdated packages.
```
The table of locally installed tools is recognizable by the additional Manifest table column.  
The locally installed tools are always tight with the .config/dotnet-tools.json manifest file somewhere in the current directory structure - to get to more detail, please check the 
[.NET Local Tools](#net-local-tools) links in the [Useful links](#useful-links)


## Installation

Download and install the [.NET 8 SDK or .NET 10 SDK](https://www.microsoft.com/net/download) currently in LTS (Long Term Support) or a newer (or a non LTS between). 
Once installed, run the following command:

```bash
dotnet tool install -g dotnet-tools-outdated
```
This will install the dotnet-tools-outdated as the global .NET tool.

If you already have a previous version of **dotnet-tools-outdated** installed, you can uptdate to the latest version using the following command:

```bash
dotnet tool update -g dotnet-tools-outdated
```

Supposing rarelly, but in case of the older [.NET Core SDK](https://dotnet.microsoft.com/download/dotnet) (up to .NET Core 2.1), please install/update to the version lesser than 0.6.2.
e.g:

```bash
dotnet tool install -g --version 0.5.2 dotnet-tools-outdated
```


## Usage

### Help

```bash
dotnet-tools-outdated -h
```

```text
dotnet-tools-outdated

Checks whether any of installed .NET command-line tools is outdated.

Usage: dotnet-tools-outdated [options]

Options:
  --version                Show version information.
  -?|-h|--help             Show help information.
  -n|--name                Check (and show) only one particular package
  -f|--format              Output format. Valid values are xml, json, or table. (Default: table)
  --outPkgRegardlessState  Otput the both up-to-date/outdated state packages for the json and xml format. (The table
                           format can show only the outdated packages)
  -ni|--noIndent           No indenttation (for the json and xml format)
  -o|--output              Output file path. (Default: stdout)
  --utf8                   Output with UTF-8 instead of the system default encoding. (no bom)
  -pre|--prerelease        Check also pre-released versions
  -s|--showStat            Show statistics info row (sums of available and outdated packages)
  -gt|--globalToolsPath    Use custom location of the globally installed .NET tools
```

### Outdated tools

Notes: 
 - the output is empty in case of all the installed packages up-to date (the -s option can at least provide an info if any packages have been checked, if a need).
 - in case of unlisted (deprecated) NuGet package, it does not have the "Available" version column value (e.g. [dotnet-try](https://www.nuget.org/packages/dotnet-try/) below)

```bash
dotnet-tools-outdated
```

```text
Package Id                         Current     Available
---------------------------------------------------------
dotnet-tools-outdated              0.5.2       0.7.0
dotnet-try                         1.0.19553.4
try-convert                        0.7.210903  0.9.232202
upgrade-assistant                  0.2.212405  0.3.255803
```

### Output to JSON/ XML

Notes:
 - it shows little more info about installed tools 
 - it allows to show also installed packages, which are up-to-date (if the --outPkgRegardlessState option is specified)
   - isOutdated informs whether the installed golbal tool is/is not outdated
   - becomeUnlisted informs the installed golbal tool is/is not set as deprecated, alias unlisted at the [NuGet repo](https://www.nuget.org))
 - in case of unlisted (deprecated) NuGet package, it does not have the "Available" version column value
 - the ver at the root level is the version of the dotnet-tools-outdated itself

#### Example of a JSON output

```bash
dotnet-tools-outdated -f json --outPkgRegardlessState
```

```text
{
  "ver": "0.7.0",
  "outPkgRegardlessState": true,
  "dotnet-tools-outdated": [
    {
      "packageName": "dotnet-tools-outdated",
      "isOutdated": true,
      "currentVer": "0.3.0",
      "availableVer": "0.7.0",
      "becomeUnlisted": false,
      "directory": "C:\\Users\\user\\source\\repos\\my\\console",
      "localManifestRefInfo": {
        "filePath": "C:\\Users\\user\\source\\repos\\my\\console\\.config\\dotnet-tools.json",
        "version": 1,
        "isRoot": false
      }
    },
    {
      "packageName": "dotnetsay",
      "isOutdated": true,
      "currentVer": "2.0.1",
      "availableVer": "2.1.7",
      "becomeUnlisted": false,
      "directory": "C:\\Users\\user\\source\\repos",
      "localManifestRefInfo": {
        "filePath": "C:\\Users\\user\\source\\repos\\.config\\dotnet-tools.json",
        "version": 1,
        "isRoot": true
      }
    },
    {
      "packageName": "dotnet-tools-outdated",
      "isOutdated": true,
      "currentVer": "0.6.1",
      "availableVer": "0.7.0",
      "becomeUnlisted": false,
      "directory": "C:\\Users\\user\\.dotnet\\tools\\.store\\dotnet-tools-outdated"
    },
    {
      "packageName": "try-convert",
      "isOutdated": false,
      "currentVer": "0.9.232202",
      "availableVer": "0.9.232202",
      "becomeUnlisted": false,
      "directory": "C:\\Users\\user\\.dotnet\\tools\\.store\\try-convert"
    },
    {
      "packageName": "upgrade-assistant",
      "isOutdated": false,
      "currentVer": "0.5.568",
      "availableVer": "0.5.568",
      "becomeUnlisted": false,
      "directory": "C:\\Users\\user\\.dotnet\\tools\\.store\\upgrade-assistant"
    }
  ]
}
```
#### Example of a XML output

```bash
dotnet-tools-outdated -f xml
```

```
<?xml version="1.0" encoding="utf-8"?>
<dotnet-tools-outdated version="0.7.0" outPkgRegardlessState="false">
  <package name="dotnet-tools-outdated" outdated="true">
    <currentVer>0.3.0</currentVer>
    <availableVer>0.7.0</availableVer>
    <becomeUnlisted>false</becomeUnlisted>
    <directory>C:\Users\mr\source\repos\my\console</directory>
    <localManifestRefInfo filePath="C:\Users\mr\source\repos\my\console\.config\dotnet-tools.json" isRoot="false" version="1" />
  </package>
  <package name="dotnetsay" outdated="true">
    <currentVer>2.0.1</currentVer>
    <availableVer>2.1.7</availableVer>
    <becomeUnlisted>false</becomeUnlisted>
    <directory>C:\Users\mr\source\repos</directory>
    <localManifestRefInfo filePath="C:\Users\mr\source\repos\.config\dotnet-tools.json" isRoot="true" version="1" />
  </package>
  <package name="dotnet-tools-outdated" outdated="true">
    <currentVer>0.6.1</currentVer>
    <availableVer>0.7.0</availableVer>
    <becomeUnlisted>false</becomeUnlisted>
    <directory>C:\Users\mr\.dotnet\tools\.store\dotnet-tools-outdated</directory>
  </package>
</dotnet-tools-outdated>
```

## Build

Please note, that to build the project, you need to have installed the .NET 10 SDK or a newer.
(Please check the [Development](DEVELOPMENT.md), if cuourious about more development related details)

There we go the basic steps to build and pack the project:

```bash
git clone https://github.com/rychlmoj/dotnet-tools-outdated
```
```bash
cd dotnet-tools-outdated/src/DotNetToolsOutdated
```
```bash
dotnet pack -c release -o nupkg
```

Output is located in ```./nupkg```

## Configuration

Usually, you don't need to configure anything. However, if you experience any issues with accessing the NuGet repository,
it might help to change the tool's http client settings .e.g. a proxy or other settings.

The configuration file is located in the user profile directory under the path: 
```.config/dotnet-tools-outdated/appsettings.json```

Please run the ```dotnet-tools-outdated``` at least once to create the configuration files. The newly created appsettings.json is currently an empty json file. All the setting items are optional and as they are not specified, the default values are used.
Note, the default settings is bacwards compatible with the previous versions of the tool.

The configuration directory also contains a file called:
```.config/dotnet-tools-outdated/appsettings-=fulltemplate.json```
The file is read-only and contains all the possible configuration settings with its default values and comments. So you can use it as a reference.


## Changelog
Please check the [Changelog](CHANGELOG.md).

## Uninstall

```bash
dotnet tool uninstall -g dotnet-tools-outdated
```
Note:
 Since the tool is run, it ensures that the .config/dotnet-tools-outdated (if not created yet) is created in the user profile directory.
 It contains the appsettings. This directoru si not removed by the above uninstall command. So if a need, please remove it manually. 

## Common Release Notes

Since the versions 0.6.0> the **dotnet-tools-outdated** are going to be ported for the available .NET [LTS scheme](https://dotnet.microsoft.com/platform/support/policy/dotnet-core)
. (Currently the .NET 8 and .NET 10)
This is on behalf of minimizing the size of the package. It is also supposed, folks who using this usually care about the updated versions of the global tools, 
and  don't' need the older version of the .NET SDK.

However if a relly a neeed of environment with an older .NET SDK, then please install the following lower version 
(There is no functionality change/bug fixings in the current version  comparing up to the 0.5.2 version)
- 0.7.0 - if a need to run under the .NET 6.0 (and no .NET 8> SDK is present.)
- 0.6.0 - if a need to run under the .NET 3.1 (and no .NET 6> SDK is present.)
- 0.5.x - if a need to run under the .NET Core 2.1

## Useful Links

### .NET Global Tools
(Since .NET Core 2.1 SDK)
* [How to manage .NET tools](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools)

* [.NET Core 2.1 Global Tools Announcement](https://devblogs.microsoft.com/dotnet/announcing-net-core-2-1-preview-1/#global-tools)
* [.NET Core Global Tools Sample](https://github.com/dotnet/core/blob/master/samples/dotnetsay/README.md)
* [.NET Core Global Tools and Gotchas](https://www.natemcmaster.com/blog/2018/02/02/dotnet-global-tool)

### .NET Local Tools
(Since .NET Core 3.0 SDK)
* [Install .NET Local Tool](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools#install-a-local-tool)

### .NET
[.NET download page](https://www.microsoft.com/net/download).