# Development Notes

## Pregequisites

Please note, that to build the project, you need to have installed the .NET 10 SDK or a newer or a compatible version, depending on the version you want to build.
You can download it from the official Microsoft .NET website: https://dotnet.microsoft.com/en-us/download

## Fetching the source code

You can clone the repository using git

```bash
cd your-desired-base-directory
```

```bash
git clone https://github.com/rychlmoj/dotnet-tools-outdated
```

## Building the project
(Please check the [Changelog](DEVELOPMENT.md), if courious about more development related details)

```bash
cd dotnet-tools-outdated/src/DotNetToolsOutdated
```

## Packing the project
```bash
dotnet pack -c release -o nupkg
```

Output from the pack is located in ```./nupkg``` (```src/DotNetToolsOutdated/nupkg```)


## Install the project from the local package
```bash
dotnet tool install --global dotnet-tools-outdated --add-source ./nupkg
```

## Uninstall the dotnet-tools-outdated

This time, as usual way:
```bash
dotnet tool uninstall --global dotnet-tools-outdated
```

