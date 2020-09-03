# FAES-Updater
An updater used to automatically handle installing and updating the various FAES-based tools.

## Usage
Directly using the FAES-Updater executable is not needed, or recommended.

The tool will be downloaded, executed and deleted automatically by the FAES-based tools to ensure
they are automatically updated.

FAES-Updater can be used by advanced users manually to download FAES tools automatically or to
force update tools. This requires use of the various launch arguments available in FAES-Updater.

### Launch Arguments 
- `--branch <stable/beta/dev>`: Sets the release branch used when downloading the FAES tool(s) (Defaults to 'stable')
- `--tool <faes/faes_gui/faes_cli/faes_legacy>`: Sets which tool should be installed (Defaults to 'faes_gui')
- `--version <version>`: Sets the version of the FAES tool(s) to be installed (Defaults to 'latest')
- `--silent`: Toggles whether the installation should be perfored silently (Hide console window)
- `--run`: Toggles whether the FAES tool will be automatically executed after install
- `--delay <time(MS)>`: Sets installation delay to give time for the host application to be exited
- `--preserveself`: Disables self-deletion of FAES-Updater.exe after the installation process is completed
- `--dir <path>`: Sets the install path for the FAES tool(s)
- `--fastmode`: Uses multithreading where possible (currently only used installing the suite)
- `--suite`: Installs all the latest FAES-powered tools (FileAES, FileAES-Legacy and FileAES-CLI) for the requested branch
- `--fullinstall`: Toggles whether the FAES tool(s) are going to be installed in 'full-install' mode (currently not supported)
- `--portable`: Toggles whether the FAES tool(s) are going to be installed in 'portable' mode (Used by default)
- `--verbose`: Enables verbose logging. This verbosely shows what the updater is doing.
- `--faeslib <both/netf/netc>`: Sets whether the NET4.5 version, NETCore2.1 version or both versions of FAES should be installed
- `--netcore`: Sets that the NetCore2.1 version of FAES should be installed
- `--netframework`: Sets that the NET4.5 version of FAES should be installed
- `--pure`: Disables the writing of extra files during installation

#### Single-Tool Install Example:  
`FAES-Updater.exe --tool faes_legacy --branch beta --version latest --dir "InstallPath"`  
Will install the latest build of FileAES-Legacy in either the stable or beta branch (depending on which is newer) to the '/InstallPath' directory.

#### Suite Install Example:
`FAES-Updater.exe --suite --branch dev --version latest --dir "InstallPath"`  
Will install the latest build of FileAES, FileAES-Legacy and FileAES-CLI in either the stable, beta or dev branch (depending on which is newer) to the '/InstallPath' directory.
