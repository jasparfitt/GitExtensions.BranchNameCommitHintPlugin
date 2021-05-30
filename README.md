# GitExtensions Branch Name Commit Hint Plugin
A plugin for GitExtenstions that uses the branch name to create a set of commit templates.

The package is published on [NuGet.org](https://www.nuget.org/packages/GitExtensions.BranchNameCommitHintPlugin) feed.

### Csproj
 - I'm using powershell script to download a selected version of Git Extensions from GitHub releases. This script runs before every build and checks if Git Extensions binaries are donwloaded.
 - CSproj references selected binaries from the downloaded Git Extensions.
 - After build a newly created binaries of the plugin is copied to Git Extensions plugins directory.
 - F5 is setup to start downloaded `GitExtensions.exe` for easy debugging.