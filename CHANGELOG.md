# Changelog
## [v0.4.1]
- remove the -u shortcut option of the --utf8 (supposed to not be mixed with possible future update command)

# Changelog
## [v0.4.0]
- make the package smaller by getting rid of references to the Microsoft.AspNet.WebApi.Client, (Newtonsoft.Json) - using the Utf8Json package instead.
- --noIndent parameter (for the JSON output type)

## [v0.3.0] 
- fix using correct NuGet API call to get the info about latest version
- fix (and enhance) the versions comparison using Semver for semantic comparison and then fall back with kind of number comparison preceding string comparison

## [v0.2.0] (note this and the lower versions are not able to fetch correct data any more, because a NuGet API change)
- Improve the performance awaiting all the running task together
- Fixes in Readme file
- Refactoring of "Installed" to "Current" (follow the "npm -g outdated" terms) 

## [v0.1.1]
- Fix in Readme file, installation command line 

## [v0.1.0]
Initial release
 - Provides a standard command-line (with a simplified async flow so far)