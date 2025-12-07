# Development Notes

## Pregequisites

Please note, that to build the project, you need to have installed the .NET 10 SDK or a newer or a compatible version, depending on the version you want to build.
You can download it from the official Microsoft .NET website: https://dotnet.microsoft.com/en-us/download

## Fetching the source code

You can clone the repository using git

```bash
cd a-base-directory-you-want-to-clone-to
```

```bash
git clone https://github.com/rychlmoj/dotnet-tools-outdated
```

## Building the project

```bash
cd dotnet-tools-outdated/src/DotNetToolsOutdated
```

## Packing the project
```bash
dotnet pack -c release -o nupkg
```

The NuGet package created by the pack command is located at ```./nupkg``` or at ```src/DotNetToolsOutdated/nupkg``` directories.


## Install the project from the local package
```bash
dotnet tool install -g dotnet-tools-outdated --add-source ./nupkg
```

## Uninstall the dotnet-tools-outdated

This time, as usual way:
```bash
dotnet tool uninstall -g dotnet-tools-outdated
```

## Update dotnet-tools-outdated

Alternatively, if you want update from NuGet package having newer version, you don't need to uninstall, but just update :
```bash
dotnet tool update -g dotnet-tools-outdated --add-source ./nupkg
```

