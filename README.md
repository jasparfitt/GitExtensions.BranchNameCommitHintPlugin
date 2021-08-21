# GitExtensions Branch Name Commit Hint Plugin
A plugin for GitExtenstions that uses the branch name to create a set of commit templates.

By default the plugin is set to find the first occurence of a Jira reference from the branch name but the regex can be changed to find any part of branch name.

The key extracted from the branch name can be set on it's own as a commit template, as a set of semantic commit templates, or used in custom templates.


The package is published on [NuGet.org](https://www.nuget.org/packages/GitExtensions.BranchNameCommitHintPlugin) feed.
This package is based on the plugin template [here](https://github.com/gitextensions/gitextensions.plugintemplate)
### Installation Instructions
Open the plugin manager in GitExtensions and search for this plugin, click install. Once installed open the "Plugins" menu and click "Branch Commit Hint" to open the settings.

Here you can enable/disable and configure the plugin. You can set the regex used to parse the branch name, by default it is set to find JIRA references. You can also choose which templates you want enabled, by default a set of semantic messages are enabled but you can also add a plain message or custom messages.
### For Development
 - There's powershell script to download a selected version of Git Extensions from GitHub releases. This script runs before every build and checks if Git Extensions binaries are donwloaded.
 - CSproj references selected binaries from the downloaded Git Extensions.
 - After build a newly created binaries of the plugin is copied to Git Extensions plugins directory.
 - F5 is setup to start downloaded `GitExtensions.exe` for easy debugging.