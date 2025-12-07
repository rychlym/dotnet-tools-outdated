# Changelog

## [v0.8.0]
- bump to .NET 10.0, keeping .NET 8.0 target
- configurable http client via appsettings.json in the .config. It supports some http client parameters (like UseDefaultCredentials, Proxy, Timeout).

## [v0.7.1]
- -o|--output option typo fix

## [v0.7.0]
- support for dotnet local tools
- updates and fixes for output json and xml formats
- Semver package update
- small fixes in the Readme file 

## [v0.6.2]
- .NET Core 3.1, .NET 7.0 target removed (keeping .NET 6.0, .NET 8.0)

## [v0.6.1]
- bump to .NET 8.0,.NET 7.0 (.NET 6.0, .NET Core 3.1. kept)
- fix: when no packages are specified, the program should exit with code 0 (no outdated packages found) instead of code 1 (error)
 
## [v0.6.0]
- bump to .NET 6.0 (.NET Core 3.1. kept, .NET Core 2.1, .NET 5.0 targets removed)

## [v0.5.2]
- switch to Cryptisk.Utf8Json (version 1.4.0)

## [v0.5.1]
- minor fix of the package description
- uses a different NuGet API endpoint to get the versions list
- -pre|--prerelease parameter - Check also the pre-released versions
  (by default - only stable versions are considered. That is actually a small breaking change)
- packages becoming unlisted are ignored now (no console output line for them, but mentioned in the complete json/xml output)

## [v0.5.0]
- bump to .NET 5.0 (.NET Core 3.1., .NET Core 2.1 targets kept)

## [v0.4.1]
- remove the -u shortcut option of the --utf8 (supposed to not be mixed with possible future update command)

## [v0.4.0]
- make the package smaller by getting rid of references to the Microsoft.AspNet.WebApi.Client, (Newtonsoft.Json) - using the Utf8Json package instead.
- --noIndent parameter (for the JSON output type)

## [v0.3.0] 
- fix using correct NuGet API call to get the info about latest version
- fix (and enhance) the versions comparison using Semver for semantic comparison and then fall back with kind of number comparison preceding string comparison

## [v0.2.0] (note this and the lower versions are not able to fetch correct data any more, because a NuGet API change)
- improve the performance awaiting all the running tasks together
- fixes in Readme file
- refactoring of "Installed" to "Current" (follow the "npm -g outdated" terms) 

## [v0.1.1]
- fix in Readme file, installation command line 

## [v0.1.0]
Initial release
 - Provides a standard command-line (with a simplified async flow so far)
