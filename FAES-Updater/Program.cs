﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;


namespace FAES_Updater
{
    class Program
    {
        private static readonly Reg _regControl = new Reg();

        private static string _directory = "";
        private static string _branch = "stable";
        private static string _tool = "faes_gui";
        private static string _faesLib = "all";
        private static string _installVer = "latest";
        private static ushort _delayStart = 0;
        private static bool _installSuite = false;
        private static bool _useThreadedWherePossible = false;
        private static bool _verbose = false;
        private static bool _fullInstall = false;
        private static bool _runPost = false;
        private static bool _writeExtraFiles = true;
        private static bool _showUpdaterVer = false;
        private static bool _deleteSelf = true;
        private static bool _associateFileTypes = false;
        private static bool _startMenuShortcuts = false;
        private static bool _contextMenus = false;
        private static bool _uninstall = false;
        private static bool _deleteUserData = false;
        private static bool _showInstalled = false;

        private const string preReleaseTag = "";

        private static int Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            IntPtr handle = GetConsoleWindow();

            for (int i = 0; i < args.Length; i++)
            {
                string strippedArg = args[i].ToLower();

                if (Directory.Exists(args[i])) _directory = args[i];

                strippedArg = strippedArg.TrimStart('-', '/', '\\');

                if (strippedArg == "verbose" || strippedArg == "v" || strippedArg == "debug") _verbose = true;
                else if (strippedArg == "suite" || strippedArg == "installsuite" || strippedArg == "all") _installSuite = true;
                else if (strippedArg == "threadded" || strippedArg == "threaddedaspossible" || strippedArg == "fastmode") _useThreadedWherePossible = true;
                else if (strippedArg == "updaterversion" || strippedArg == "updaterver" || strippedArg == "uver" || strippedArg == "showver" || strippedArg == "showversion") _showUpdaterVer = true;
                else if (strippedArg == "fullinstall" || strippedArg == "full") _fullInstall = true;
                else if (strippedArg == "associatefiletypes" || strippedArg == "filetypes") _associateFileTypes = true;
                else if (strippedArg == "startmenushortcuts" || strippedArg == "startmenu") _startMenuShortcuts = true;
                else if (strippedArg == "contextmenus" || strippedArg == "context") _contextMenus = true;
                else if (strippedArg == "portableinstall" || strippedArg == "p" || strippedArg == "portable") _fullInstall = false;
                else if (strippedArg == "run" || strippedArg == "r" || strippedArg == "runpost") _runPost = true;
                else if (strippedArg == "silent" || strippedArg == "s" || strippedArg == "headless") ShowWindow(handle, SW_HIDE);
                else if (strippedArg == "preserveself" || strippedArg == "preserve" || strippedArg == "nodelete" || strippedArg == "nodel") _deleteSelf = false;
                else if ((strippedArg == "delay" || strippedArg == "delaystart" || strippedArg == "delayinstall" ||
                          strippedArg == "delayupdater") && args.Length > i + 1 && !string.IsNullOrEmpty(args[i + 1]) && UInt16.TryParse(args[i + 1], out _delayStart))
                {
                    i++;
                }
                else if ((strippedArg == "directory" || strippedArg == "d" || strippedArg == "dir" || strippedArg == "installdir")
                    && args.Length > i + 1 && !string.IsNullOrEmpty(args[i + 1]))
                {
                    if (!Directory.Exists(args[i + 1])) Directory.CreateDirectory(args[i + 1]);
                    _directory = args[i + 1];
                    i++;
                }
                else if ((strippedArg == "ver" || strippedArg == "v" || strippedArg == "version" || strippedArg == "toolversion" || strippedArg == "toolver")
                    && args.Length > i + 1 && !string.IsNullOrEmpty(args[i + 1]))
                {
                    if (args[i + 1].ToLower() == "latest") _installVer = "latest";
                    else _installVer = args[i + 1].ToLower();
                    i++;
                }
                else if ((strippedArg == "branch" || strippedArg == "b") && args.Length > i + 1 && !string.IsNullOrEmpty(args[i + 1]))
                {
                    if (args[i + 1].ToLower() == "stable") _branch = "stable";
                    else if (args[i + 1].ToLower() == "beta") _branch = "beta";
                    else if (args[i + 1].ToLower() == "dev") _branch = "dev";
                    i++;
                }
                else if ((strippedArg == "tool" || strippedArg == "t") && args.Length > i + 1 && !string.IsNullOrEmpty(args[i + 1]))
                {
                    if (args[i + 1].ToLower() == "faes") _tool = "faes";
                    else if (args[i + 1].ToLower() == "faes_gui") _tool = "faes_gui";
                    else if (args[i + 1].ToLower() == "faes_cli") _tool = "faes_cli";
                    else if (args[i + 1].ToLower() == "faes_legacy") _tool = "faes_legacy";
                    i++;
                }
                else if ((strippedArg == "faeslib" || strippedArg == "l") && args.Length > i + 1 && !string.IsNullOrEmpty(args[i + 1]))
                {
                    _faesLib = args[i + 1].ToLower();
                    i++;
                }
                else if (strippedArg == "noextrafiles" || strippedArg == "pure" || strippedArg == "noextras") _writeExtraFiles = false;
                else if (strippedArg == "showinstalled" || strippedArg == "installed" || strippedArg == "installedtools") _showInstalled = true;
                else if (strippedArg == "uninstall") _uninstall = true;
                else if (strippedArg == "deleteuserdata") _deleteUserData = true;
            }

            try
            {
                if (_showUpdaterVer)
                {
                    try
                    {
                        Logging.Log($"FAES-Updater Version: {GetVersion()}\r\nBuild Date: {GetBuildDateFormatted()}");
                    }
                    catch (Exception e)
                    {
                        Logging.Log($"Unable to show FAES-Updater information! Exception: {e}", Severity.ERROR);
                    }
                }
                if (_showInstalled)
                {
                    try
                    {
                        string[] softwarePaths = _regControl.GetSoftwareFilePaths(out List<string> toolNames);

                        if (softwarePaths != null && softwarePaths.Length > 0)
                            for (int i = 0; i < softwarePaths.Length; i++)
                            {
                                Logging.Log($"Tool: {toolNames[i]}, Path: {softwarePaths[i]}");
                            }
                    }
                    catch (Exception e)
                    {
                        Logging.Log($"Unable to show currently installed tools! Exception: {e}", Severity.ERROR);
                    }
                }

                {
                    if (_delayStart > 0)
                    {
                        Logging.Log($"Update delay requested. Delaying any I/O operations for {_delayStart}ms...");
                        Thread.Sleep(_delayStart);
                    }

                    if (!_uninstall)
                    {
                        if (_installSuite)
                        {
                            if (_useThreadedWherePossible)
                            {
                                Logging.Log("Install Suite is running in multi-threadded mode. Logging WILL NOT appear in a logical order.", Severity.WARN);

                                Task updateToolFAESGUI = Task.Factory.StartNew(() => UpdateTool("faes_gui", Path.Combine(_directory, "FileAES")));
                                Task updateToolFAESLegacy = Task.Factory.StartNew(() => UpdateTool("faes_legacy", Path.Combine(_directory, "FileAES_Legacy")));
                                Task updateToolFAESCLI = Task.Factory.StartNew(() => UpdateTool("faes_cli", Path.Combine(_directory, "FileAES_CLI")));

                                Task.WaitAll(updateToolFAESGUI, updateToolFAESLegacy, updateToolFAESCLI);
                                Logging.Log("Multi-Threadded Install Suite completed!");
                            }
                            else
                            {
                                UpdateTool("faes_gui", Path.Combine(_directory, "FileAES"));
                                UpdateTool("faes_legacy", Path.Combine(_directory, "FileAES_Legacy"));
                                UpdateTool("faes_cli", Path.Combine(_directory, "FileAES_CLI"));
                            }
                        }
                        else UpdateTool(_tool, _directory);
                        return 0;
                    }

                    {
                        _regControl.DeleteContextMenus();
                        _regControl.DeleteFileTypeAssociation();
                        _regControl.DeleteStartMenuShortcuts();

                        string[] softwarePaths = _regControl.GetSoftwareFilePaths(out List<string> toolNames);
                        if (softwarePaths != null && softwarePaths.Length > 0)
                        {
                            for (int i = 0; i < softwarePaths.Length; i++)
                            {
                                try
                                {
                                    string toolName = toolNames[i];
                                    string toolPath = softwarePaths[i];

                                    try
                                    {
                                        DeleteTool(toolName, toolPath);
                                    }
                                    catch (Exception e)
                                    {
                                        Logging.Log(
                                            $"Could not delete '{toolName}' at path '{toolPath}'. Exception: {e}", Severity.ERROR);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logging.Log("An unexpected error occurred when uninstalling one or more FAES tools! Exception: " + e, Severity.ERROR);
                                }
                            }

                            if (_deleteUserData) _regControl.DeleteUserData();
                            _regControl.DeleteSoftwareFilePaths();
                            if (_deleteSelf) SelfDelete();
                            return 0;
                        }

                        {
                            Logging.Log("Cannot find any installed FAES tools! If you are certain you have them installed please delete them manually.", Severity.WARN);
                            _regControl.DeleteSoftwareFilePaths();
                            if (_deleteSelf) SelfDelete();
                            return 1;
                        }
                    }
                }
            }
            catch (SecurityException)
            {
                Logging.Log("Permission Denied! Please run as an administrator.", Severity.ERROR);
                return 2;
            }
        }

        private static int KillTool(string toolName, int attemptNumber)
        {
            switch (attemptNumber)
            {
                case 1:
                    Logging.Log(
                        $"Attempt 1: '{toolName}' could not be deleted! Attempting to find and end any FAES related processes before reattempting deletion...", Severity.WARN);
                    KillFAES(toolName);
                    break;
                case 2:
                    Logging.Log(
                        $"Attempt 2: '{toolName}' could not be deleted! Going nuclear on all FAES related processes before reattempting deletion...", Severity.WARN);
                    KillFAES(null);
                    break;
                case 3:
                    Logging.Log($"Attempt 3: '{toolName}' could not be deleted! Aborting installation...", Severity.WARN);
                    break;
                default:
                    attemptNumber = 1;
                    KillTool(toolName, attemptNumber);
                    break;
            }
            return ++attemptNumber;
        }

        private static void DeleteTool(string toolName, string toolFilePath)
        {
            int attemptNumber = 1;
            while (attemptNumber <= 3)
            {
                try
                {
                    SafeDeleteFile(toolFilePath);
                    return;
                }
                catch (UnauthorizedAccessException)
                {
                    attemptNumber = KillTool(toolName, attemptNumber);
                }
            }
        }

        private static void UpdateTool(string tool, string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:", "").TrimStart(':', '/', '\\');

            string toolNameFormatted;

            switch (tool)
            {
                case "faes":
                    toolNameFormatted = "FAES";
                    break;
                case "faes_gui":
                    toolNameFormatted = "FAES_GUI";
                    break;
                case "faes_cli":
                    toolNameFormatted = "FAES_CLI";
                    break;
                case "faes_legacy":
                    toolNameFormatted = "FAES_Legacy";
                    break;
                default:
                    toolNameFormatted = tool.ToUpper();
                    break;
            }

            Logging.Log($"{toolNameFormatted} Update Selected!", Severity.DEBUG);

            string downloadLink = $"https://api.mullak99.co.uk/FAES/GetDownload.php?app={tool}&ver={_installVer}&branch={_branch}";
            string installPath = Path.Combine(directory, "FAES_Updater_Temp");
            string fileName = _installVer != "latest" ? $"{toolNameFormatted}-{_installVer}.zip" : $"{toolNameFormatted}-{_branch}-{_installVer}.zip";

            try
            {
                if (DoesVersionExist(tool, _installVer, _branch))
                {
                    if (DownloadFile(downloadLink, installPath, fileName))
                    {
                        string toolFinalName = null;

                        switch (tool)
                        {
                            case "faes_gui":
                                toolFinalName = "FileAES.exe";
                                break;
                            case "faes_cli":
                                toolFinalName = "FileAES-CLI.exe";
                                break;
                            case "faes_legacy":
                                toolFinalName = "FileAES_Legacy.exe";
                                break;
                            default:
                                toolFinalName = "FAES.dll";
                                break;
                        }

                        string finalFilePath = Path.Combine(directory, toolFinalName);

                        DeleteTool(toolFinalName, finalFilePath);

                        if (!File.Exists(finalFilePath))
                        {
                            string finalToolFilePath = Path.Combine(directory, toolFinalName);
                            ExtractZip(Path.Combine(installPath, fileName), directory, installPath);

                            CleanupMiscFiles(directory);
                            if (_writeExtraFiles) AddExtraFiles(tool, directory);

                            if (_fullInstall)
                            {
                                try
                                {
                                    _regControl.CreateSoftwareFilePath(finalToolFilePath, tool);
                                    switch (tool.ToLower())
                                    {
                                        case "faes_gui":
                                            {
                                                if (_associateFileTypes)
                                                    _regControl.CreateFileTypeAssociation(finalToolFilePath);
                                                if (_contextMenus)
                                                    _regControl.CreateContextMenus(finalToolFilePath);
                                                if (_startMenuShortcuts)
                                                    _regControl.CreateStartMenuShortcut(finalToolFilePath, "FileAES", "A GUI application for encrypting and decrypting files using FAES.");

                                                string process = Path.Combine(directory, toolFinalName);
                                                Logging.Log(
                                                    $"Starting process '{toolFinalName}' to enable FullInstall...", Severity.DEBUG);
                                                Process p = new Process
                                                {
                                                    StartInfo =
                                                    {
                                                        FileName = process,
                                                        Arguments =
                                                            $"--genFullInstallConfig --installBranch {_branch} {string.Join(" ", DumpInstallerOptions())}",
                                                        UseShellExecute = false,
                                                        CreateNoWindow = true
                                                    }
                                                };
                                                p.Start();
                                                Thread.Sleep(500);
                                                Logging.Log($"FullInstall enabled for '{toolFinalName}'!", Severity.DEBUG);
                                                break;
                                            }
                                        case "faes_legacy":
                                            {
                                                if (_associateFileTypes)
                                                    _regControl.CreateFileTypeAssociation(finalToolFilePath);
                                                if (_contextMenus)
                                                    _regControl.CreateContextMenus(finalToolFilePath);
                                                if (_startMenuShortcuts)
                                                    _regControl.CreateStartMenuShortcut(finalToolFilePath, "FileAES Legacy", "A GUI application for encrypting and decrypting files using FAES.");

                                                string launchParamsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"mullak99\FileAES\config\FileAES_Legacy-launchParams.cfg");
                                                File.WriteAllText(launchParamsFilePath,
                                                    $"--fullinstall\n--{_branch}\n{string.Join("\n", DumpInstallerOptions())}");
                                                Logging.Log($"FullInstall enabled for '{toolFinalName}'!", Severity.DEBUG);
                                                break;
                                            }
                                        case "faes_cli":
                                        {
                                            if (_associateFileTypes)
                                                _regControl.CreateFileTypeAssociation(finalToolFilePath);
                                            if (_contextMenus)
                                                _regControl.CreateContextMenus(finalToolFilePath);
                                            if (_startMenuShortcuts)
                                                _regControl.CreateStartMenuShortcut(finalToolFilePath, "FileAES CLI", "A CLI application for encrypting and decrypting files using FAES.");

                                            // Start process and automatically create full-install config, similar to "FAES_GUI".
                                            Logging.Log($"FullInstall enabled for '{toolFinalName}'!", Severity.DEBUG);
                                            break;
                                            }
                                    }
                                }
                                catch
                                {
                                    Logging.Log($"FullInstall could not be enabled for '{toolFinalName}'!", Severity.WARN);
                                }
                            }

                            if (_runPost && toolFinalName != "FAES.dll")
                            {
                                string process = Path.Combine(directory, toolFinalName);

                                if (!string.IsNullOrEmpty(toolFinalName) && File.Exists(process))
                                {
                                    Logging.Log($"Starting process '{toolFinalName}'...", Severity.DEBUG);
                                    Process.Start(process);
                                }
                                else Logging.Log($"'{toolFinalName}' executable could not be found. Aborting auto-run.", Severity.WARN);
                            }
                        }
                    }
                }
                else Logging.Log($"'{_installVer}' on branch '{_branch}' does not exist!", Severity.ERROR);
            }
            catch (WebException)
            {
                Logging.Log("A connection to the download server could not be made! Please ensure you are connected to the internet.", Severity.ERROR);
            }
            catch (Exception e)
            {
                Logging.Log($"An unexpected error occurred while downloading the install files! Exception: {e.ToString()}", Severity.ERROR);
            }
            finally
            {
                CleanupInstallFiles(installPath);
            }
        }

        private static bool DoesVersionExist(string tool, string installVer, string branch)
        {
            Logging.Log($"Checking if '{tool}' version '{installVer}' exists in branch '{branch}'", Severity.DEBUG);

            string downloadLink = $"https://api.mullak99.co.uk/FAES/DoesVersionExist.php?app={tool}&ver={installVer}&branch={branch}";

            WebClient webClient = new WebClient();
            string doesExist = webClient.DownloadString(new Uri(downloadLink));

            Logging.Log(String.Format("{0}|{2}|{1}: {3}", tool, installVer, branch, doesExist), Severity.DEBUG);

            return (doesExist == "VersionExists");
        }

        private static void ExtractZip(string sourceZipPath, string destinationPath, string tempPath)
        {
            if (_verbose)
                Logging.Log($"Extracting ZIP '{sourceZipPath}'", Severity.DEBUG);
            else
                Logging.Log("Extracting tool...");

            if (_tool == "faes" && _faesLib != "all")
            {
                Logging.Log("Library-based ZIP extraction selected.", Severity.DEBUG);

                if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
                ZipFile.ExtractToDirectory(sourceZipPath, tempPath);

                List<string> libPaths = Directory.GetDirectories(tempPath).ToList();
                string[] dirs = Directory.GetDirectories(tempPath);

                foreach(string dir in dirs)
                {
                    Logging.Log($"Library Subpath added! '{dir}'.", Severity.DEBUG);
                    libPaths.Add(dir.Replace(tempPath, ""));
                }

                List<string> allFiles;

                //TODO: Entire section needs updating
                if (_faesLib.Contains("netstandard"))
                {
                    string faesStandLib = _faesLib.Replace("netstandard", "");
                    string netStandardPath = libPaths.FirstOrDefault(path => path.Split('/', '\\').Last().Contains("netstandard" + faesStandLib));

                    Logging.Log(String.Format("NetStandard{1}-based library requested. Selecting '{0}'.", netStandardPath, faesStandLib), Severity.DEBUG);
                    allFiles = Directory.GetFiles(netStandardPath, "*.*", SearchOption.AllDirectories).ToList();
                }
                else if (_faesLib.Contains("net4"))
                {
                    string faesStandLib = _faesLib.Replace("net", "");
                    string net4xPath = libPaths.FirstOrDefault(path => path.Split('/', '\\').Last().Contains("net" + faesStandLib));
                    Logging.Log(String.Format("NetFramework{1}-based library requested. Selecting '{0}'.", net4xPath, faesStandLib), Severity.DEBUG);
                    allFiles = Directory.GetFiles(net4xPath, "*.*", SearchOption.AllDirectories).ToList();
                }
                else
                {
                    string net4xPath = libPaths.FirstOrDefault(path => path.Split('/', '\\').Last().Contains("net45"));
                    Logging.Log($"NetFramework45-based library requested. Selecting '{net4xPath}'.", Severity.DEBUG);
                    allFiles = Directory.GetFiles(net4xPath, "*.*", SearchOption.AllDirectories).ToList();
                }

                foreach (string file in allFiles)
                {
                    string fileNameDest = Path.Combine(destinationPath, Path.GetFileName(file));
                    if (File.Exists(fileNameDest)) File.Delete(fileNameDest);

                    Logging.Log($"Moving file '{Path.GetFileName(file)}' to '{destinationPath}'.", Severity.DEBUG);
                    File.Move(file, fileNameDest);
                }
                Logging.Log("Extraction completed!");
            }
            else
            {
                Logging.Log("Standard ZIP extraction selected.", Severity.DEBUG);
                ZipFile.ExtractToDirectory(sourceZipPath, destinationPath);
            }
        }

        private static bool DownloadFile(string webLink, string dir, string fileName)
        {
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string fullPath = Path.Combine(dir, fileName);

            if (!File.Exists(fullPath))
            {
                if (_verbose)
                    Logging.Log($"Downloading installation files to '{fullPath}' from '{webLink}'...", Severity.DEBUG);
                else
                    Logging.Log("Downloading installation files...");

                WebClient webClient = new WebClient();
                string downloadLink = webClient.DownloadString(new Uri(webLink));

                if (!string.IsNullOrWhiteSpace(downloadLink))
                {
                    webClient.DownloadFile(new Uri(downloadLink), fullPath);

                    if (File.Exists(fullPath))
                    {
                        if (_verbose)
                            Logging.Log($"Download of '{fileName}' complete!", Severity.DEBUG);
                        else
                            Logging.Log("Finished downloading installation files.");

                        return true;
                    }
                    Logging.Log("An unexpected error occurred while downloading the install files!", Severity.ERROR);
                }
                else Logging.Log("The requested file could not be found!", Severity.ERROR);
            }
            else Logging.Log($"Cannot update since install files already exist in '{fullPath}'!", Severity.ERROR);

            return false;
        }

        private static bool KillFAES(string tool)
        {
            Logging.Log("Searching for any FAES related processes...", Severity.DEBUG);

            if (tool != "faes_gui" || tool != "faes_cli" || tool != "faes_legacy" || tool != "faes") tool = null;

            try
            {
                if (tool == "faes_gui" || tool == null)
                {
                    foreach (var process in Process.GetProcessesByName("FileAES"))
                    {
                        process.Kill();
                        Logging.Log($"Killed '{process.ProcessName}' (PID: {process.Id})!", Severity.DEBUG);
                    }
                    foreach (var process in Process.GetProcessesByName("FAES_GUI"))
                    {
                        process.Kill();
                        Logging.Log($"Killed '{process.ProcessName}' (PID: {process.Id})!", Severity.DEBUG);
                    }
                }
                if (tool == "faes_cli" || tool == null)
                {
                    foreach (var process in Process.GetProcessesByName("FileAES-CLI"))
                    {
                        process.Kill();
                        Logging.Log($"Killed '{process.ProcessName}' (PID: {process.Id})!", Severity.DEBUG);
                    }
                    foreach (var process in Process.GetProcessesByName("FileAES_CLI"))
                    {
                        process.Kill();
                        Logging.Log($"Killed '{process.ProcessName}' (PID: {process.Id})!", Severity.DEBUG);
                    }
                    foreach (var process in Process.GetProcessesByName("FAES_CLI"))
                    {
                        process.Kill();
                        Logging.Log($"Killed '{process.ProcessName}' (PID: {process.Id})!", Severity.DEBUG);
                    }
                }
                if (tool == "faes_legacy" || tool == null)
                {
                    foreach (var process in Process.GetProcessesByName("FileAES_Legacy"))
                    {
                        process.Kill();
                        Logging.Log($"Killed '{process.ProcessName}' (PID: {process.Id})!", Severity.DEBUG);
                    }
                    foreach (var process in Process.GetProcessesByName("FileAES-Legacy"))
                    {
                        process.Kill();
                        Logging.Log($"Killed '{process.ProcessName}' (PID: {process.Id})!", Severity.DEBUG);
                    }
                    foreach (var process in Process.GetProcessesByName("FAES_Legacy"))
                    {
                        process.Kill();
                        Logging.Log($"Killed '{process.ProcessName}' (PID: {process.Id})!", Severity.DEBUG);
                    }
                }
                if (tool == "faes" || tool == null)
                {
                    foreach (var process in Process.GetProcessesByName("FAES"))
                    {
                        process.Kill();
                        Logging.Log($"Killed '{process.ToString()}'!", Severity.DEBUG);
                    }
                }
                Logging.Log("Finished searching for any FAES related processes.", Severity.DEBUG);
                return true;
            }
            catch
            {
                Logging.Log("FAES related processed found, but FAES-Updater was unable to kill them. Manually close FAES related processes, or try running FAES-Updater as admin.", Severity.ERROR);
                return false;
            }
        }

        private static void CleanupInstallFiles(string updaterTempPath)
        {
            try
            {
                if (Directory.Exists(updaterTempPath)) Directory.Delete(updaterTempPath, true);
                if (_verbose)
                    Logging.Log("Updater Temp directory cleared!", Severity.DEBUG);
                else
                    Logging.Log("Installation cleanup completed!");
            }
            catch
            {
                Logging.Log("Install files could not be cleaned up automatically! Something else may be using the Updater Temp directory.", Severity.WARN);
            }
        }

        private static void CleanupMiscFiles(string dir)
        {
            if (File.Exists(Path.Combine(dir, "LICENSE"))) File.Delete(Path.Combine(dir, "LICENSE"));
            Logging.Log("Cleaned up misc files!");
        }

        private static bool SafeDeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                Logging.Log($"'{path}' deleted!", Severity.DEBUG);
                return true;
            }
            return false;
        }

        private static void AddExtraFiles(string tool, string dir)
        {
            if (Directory.Exists(dir) && !File.Exists(Path.Combine(dir, "LICENSE.txt")))
            {
                string licenseDownloadLink = "https://builds.mullak99.co.uk/FAES/FAES/LICENSE.txt";

                if (tool != "faes_gui" || tool != "faes_cli" || tool != "faes_legacy" || tool != "faes") tool = "faes";

                switch (tool)
                {
                    case "faes":
                        licenseDownloadLink = "https://builds.mullak99.co.uk/FAES/FAES/LICENSE.txt";
                        break;
                    case "faes_gui":
                        licenseDownloadLink = "https://builds.mullak99.co.uk/FAES/FAES_GUI/LICENSE.txt";
                        break;
                    case "faes_cli":
                        licenseDownloadLink = "https://builds.mullak99.co.uk/FAES/FAES_CLI/LICENSE.txt";
                        break;
                    case "faes_legacy":
                        licenseDownloadLink = "https://builds.mullak99.co.uk/FAES/FAES_Legacy/LICENSE.txt";
                        break;
                }

                WebClient webClient = new WebClient();
                webClient.DownloadFile(new Uri(licenseDownloadLink), Path.Combine(dir, "LICENSE.txt"));

                Logging.Log($"Added extra files to '{dir}'!", Severity.DEBUG);
            }
        }

        private static void SelfDelete()
        {
            string thisFile = Assembly.GetExecutingAssembly().Location;
            Logging.Log($"Initiating self-destruct ({thisFile}).", Severity.DEBUG);
            Process.Start(new ProcessStartInfo()
            {
                Arguments = "/C choice /C Y /N /D Y /T 1 & Del \"" + thisFile + "\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            });
        }

        private static string[] DumpInstallerOptions()
        {
            List<string> options = new List<string>();

            if (_associateFileTypes)
                options.Add("--associatefiletypes");
            if (_startMenuShortcuts)
                options.Add("--startmenushortcuts");
            if (_contextMenus)
                options.Add("--contextmenus");

            return options.ToArray();
        }

        public static bool GetVerbose()
        {
            return _verbose;
        }

        public static string GetVersion()
        {
            string[] ver = (typeof(Program).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version).Split('.');
            if (string.IsNullOrEmpty(preReleaseTag))
                return "v" + ver[0] + "." + ver[1] + "." + ver[2];
            return "v" + ver[0] + "." + ver[1] + "." + ver[2] + " (" + preReleaseTag + ")";
        }

        public static string GetBuildDateFormatted()
        {
            return GetBuildDate().ToString("yyyy-MM-dd hh:mm:ss tt");
        }

        public static DateTime GetBuildDate()
        {
            return new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
    }
}
