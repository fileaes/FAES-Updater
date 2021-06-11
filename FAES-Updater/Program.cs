using System;
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
        private static UInt16 _delayStart = 0;
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
        private static bool _onlyShowInstalled = false;

        private const string preReleaseTag = "";

        static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            IntPtr handle = GetConsoleWindow();

            for (int i = 0; i < args.Length; i++)
            {
                string strippedArg = args[i].ToLower();

                if (Directory.Exists(args[i])) _directory = args[i];

                strippedArg = strippedArg.TrimStart('-', '/', '\\');

                if (strippedArg == "verbose" || strippedArg == "v" || strippedArg == "developer" || strippedArg == "dev" || strippedArg == "debug") _verbose = true;
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
                else if ((strippedArg == "delay" || strippedArg == "delaystart" || strippedArg == "delayinstall" || strippedArg == "delayupdater")
                    && args.Length > i + 1 && !String.IsNullOrEmpty(args[i + 1]) && UInt16.TryParse(args[i + 1], out _delayStart)) { }
                else if ((strippedArg == "directory" || strippedArg == "d" || strippedArg == "dir" || strippedArg == "installdir")
                    && args.Length > i + 1 && !String.IsNullOrEmpty(args[i + 1]))
                {
                    if (!Directory.Exists(args[i + 1])) Directory.CreateDirectory(args[i + 1]);
                    _directory = args[i + 1];
                }
                else if ((strippedArg == "ver" || strippedArg == "v" || strippedArg == "version" || strippedArg == "toolversion" || strippedArg == "toolver")
                    && args.Length > i + 1 && !String.IsNullOrEmpty(args[i + 1]))
                {
                    if (args[i + 1].ToLower() == "latest") _installVer = "latest";
                    else _installVer = args[i + 1].ToLower();
                }
                else if ((strippedArg == "branch" || strippedArg == "b") && args.Length > i + 1 && !String.IsNullOrEmpty(args[i + 1]))
                {
                    if (args[i + 1].ToLower() == "stable") _branch = "stable";
                    else if (args[i + 1].ToLower() == "beta") _branch = "beta";
                    else if (args[i + 1].ToLower() == "dev") _branch = "dev";
                }
                else if ((strippedArg == "tool" || strippedArg == "t") && args.Length > i + 1 && !String.IsNullOrEmpty(args[i + 1]))
                {
                    if (args[i + 1].ToLower() == "faes") _tool = "faes";
                    else if (args[i + 1].ToLower() == "faes_gui") _tool = "faes_gui";
                    else if (args[i + 1].ToLower() == "faes_cli") _tool = "faes_cli";
                    else if (args[i + 1].ToLower() == "faes_legacy") _tool = "faes_legacy";
                }
                else if ((strippedArg == "faeslib" || strippedArg == "l") && args.Length > i + 1 && !String.IsNullOrEmpty(args[i + 1])) _faesLib = args[i + 1].ToLower();
                else if (strippedArg == "noextrafiles" || strippedArg == "pure" || strippedArg == "noextras") _writeExtraFiles = false;
                else if (strippedArg == "showinstalled" || strippedArg == "installed" || strippedArg == "installedtools") _onlyShowInstalled = true;
                else if (strippedArg == "uninstall") _uninstall = true;
            }

            try
            {
                if (_showUpdaterVer)
                {
                    Logging.Log(String.Format("FAES-Updater Version: {0}\nBuild Date: {1}", GetVersion(), GetBuildDateFormatted()));
                }
                else if (_onlyShowInstalled)
                {
                    string[] softwarePaths = _regControl.GetSoftwareFilePaths(out List<string> toolNames);

                    for (int i = 0; i < softwarePaths.Length; i++)
                    {
                        Logging.Log(String.Format("Tool: {0}, Path: {1}", toolNames[i], softwarePaths[i]));
                    }
                }
                else
                {
                    if (_delayStart > 0)
                    {
                        Logging.Log(String.Format("Update delay requested. Delaying any I/O operations for {0}ms...", _delayStart));
                        Thread.Sleep(_delayStart);
                    }

                    if (!_uninstall)
                    {
                        if (_installSuite)
                        {
                            if (_useThreadedWherePossible)
                            {
                                Logging.Log(String.Format("Install Suite is running in multi-threadded mode. Logging WILL NOT appear in a logical order."), Severity.WARN);

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
                    }
                    else
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
                                        Logging.Log(String.Format("Could not delete '{0}' at path '{1}'. Exception: {2}", toolName, toolPath, e), Severity.ERROR);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logging.Log("An unexpected error occurred when uninstalling one or more FAES tools! Exception: " + e, Severity.ERROR);
                                }
                            }
                        }
                        else
                        {
                            Logging.Log("Cannot find any installed FAES tools! If you are certain you have them installed please delete them manually.", Severity.WARN);
                        }
                        _regControl.DeleteSoftwareFilePaths();
                    }

                    if (_deleteSelf) SelfDelete();
                }
            }
            catch (SecurityException)
            {
                Logging.Log(String.Format("Please run as an administrator!"), Severity.ERROR);
            }
        }

        private static int KillTool(string toolName, int attemptNumber)
        {
            switch (attemptNumber)
            {
                case 1:
                    Logging.Log(String.Format("Attempt 1: '{0}' could not be deleted! Attempting to find and end any FAES related processes before reattempting deletion...", toolName), Severity.WARN);
                    KillFAES(toolName);
                    break;
                case 2:
                    Logging.Log(String.Format("Attempt 2: '{0}' could not be deleted! Going nuclear on all FAES related processes before reattempting deletion...", toolName), Severity.WARN);
                    KillFAES(null);
                    break;
                case 3:
                    Logging.Log(String.Format("Attempt 3: '{0}' could not be deleted! Aborting installation...", toolName), Severity.WARN);
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
            if (String.IsNullOrWhiteSpace(directory)) directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:", "").TrimStart(':', '/', '\\');

            string toolNameFormatted;

            if (tool == "faes") toolNameFormatted = "FAES";
            else if (tool == "faes_gui") toolNameFormatted = "FAES_GUI";
            else if (tool == "faes_cli") toolNameFormatted = "FAES_CLI";
            else if (tool == "faes_legacy") toolNameFormatted = "FAES_Legacy";
            else toolNameFormatted = tool.ToUpper();

            Logging.Log(String.Format("{0} Update Selected!", toolNameFormatted), Severity.DEBUG);

            string downloadLink = String.Format("https://api.mullak99.co.uk/FAES/GetDownload.php?app={0}&ver={1}&branch={2}", tool, _installVer, _branch);
            string installPath = Path.Combine(directory, "FAES_Updater_Temp");
            string fileName;

            if (_installVer != "latest") fileName = String.Format("{0}-{1}.zip", toolNameFormatted, _installVer);
            else fileName = String.Format("{0}-{1}-{2}.zip", toolNameFormatted, _branch, _installVer);

            try
            {
                if (DoesVersionExist(tool, _installVer, _branch))
                {
                    if (DownloadFile(downloadLink, installPath, fileName))
                    {
                        string toolFinalName = null;

                        if (tool == "faes_gui") toolFinalName = "FileAES.exe";
                        else if (tool == "faes_cli") toolFinalName = "FileAES-CLI.exe";
                        else if (tool == "faes_legacy") toolFinalName = "FileAES_Legacy.exe";
                        else toolFinalName = "FAES.dll";

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
                                    switch (tool)
                                    {
                                        case "faes_gui":
                                            {
                                                if (_associateFileTypes)
                                                    _regControl.CreateFileTypeAssociation(finalToolFilePath);
                                                if (_contextMenus)
                                                    _regControl.CreateContextMenus(finalToolFilePath);
                                                if(_startMenuShortcuts)
                                                    _regControl.CreateStartMenuShortcut(finalToolFilePath, "FileAES", "A GUI application for encrypting and decrypting files using FAES.");

                                                string process = Path.Combine(directory, toolFinalName);
                                                Logging.Log(String.Format("Starting process '{0}' to enable FullInstall...", toolFinalName), Severity.DEBUG);
                                                Process p = new Process
                                                {
                                                    StartInfo =
                                                    {
                                                        FileName = process,
                                                        Arguments = String.Format("--genFullInstallConfig --installBranch {0} {1}", _branch, string.Join(" ", DumpInstallerOptions())),
                                                        UseShellExecute = false,
                                                        CreateNoWindow = true
                                                    }
                                                };
                                                p.Start();
                                                Thread.Sleep(500);
                                                Logging.Log(String.Format("FullInstall enabled for '{0}'!", toolFinalName), Severity.DEBUG);
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
                                                File.WriteAllText(launchParamsFilePath, String.Format("--fullinstall\n--{0}\n{1}", _branch, string.Join("\n", DumpInstallerOptions())));
                                                Logging.Log(String.Format("FullInstall enabled for '{0}'!", toolFinalName), Severity.DEBUG);
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
                                            Logging.Log(String.Format("FullInstall enabled for '{0}'!", toolFinalName), Severity.DEBUG);
                                            break;
                                            }
                                    }
                                }
                                catch
                                {
                                    Logging.Log(String.Format("FullInstall could not be enabled for '{0}'!", toolFinalName), Severity.WARN);
                                }
                            }

                            if (_runPost && toolFinalName != "FAES.dll")
                            {
                                string process = Path.Combine(directory, toolFinalName);

                                if (!String.IsNullOrEmpty(toolFinalName) && File.Exists(process))
                                {
                                    Logging.Log(String.Format("Starting process '{0}'...", toolFinalName), Severity.DEBUG);
                                    Process.Start(process);
                                }
                                else Logging.Log(String.Format("'{0}' executible could not be found. Aborting auto-run.", toolFinalName), Severity.WARN);
                            }
                        }
                    }
                }
                else Logging.Log(String.Format("'{0}' on branch '{1}' does not exist!", _installVer, _branch), Severity.ERROR);
            }
            catch (WebException)
            {
                Logging.Log("A connection to the download server could not be made! Please ensure you are connected to the internet.", Severity.ERROR);
            }
            catch (Exception e)
            {
                Logging.Log(String.Format("An unexpected error occurred while downloading the install files! Exception: {0}", e.ToString()), Severity.ERROR);
            }
            finally
            {
                CleanupInstallFiles(installPath);
            }
        }

        private static bool DoesVersionExist(string tool, string installVer, string branch)
        {
            Logging.Log(String.Format("Checking if '{0}' version '{1}' exists in branch '{2}'", tool, installVer, branch), Severity.DEBUG);

            string downloadLink = String.Format("https://api.mullak99.co.uk/FAES/DoesVersionExist.php?app={0}&ver={1}&branch={2}", tool, installVer, branch);

            WebClient webClient = new WebClient();
            string doesExist = webClient.DownloadString(new Uri(downloadLink));

            Logging.Log(String.Format("{0}|{2}|{1}: {3}", tool, installVer, branch, doesExist), Severity.DEBUG);

            return (doesExist == "VersionExists");
        }

        private static void ExtractZip(string sourceZipPath, string destinationPath, string tempPath)
        {
            Logging.Log(String.Format("Extracting ZIP '{0}'", sourceZipPath), Severity.DEBUG);

            if (_tool == "faes" && _faesLib != "all")
            {
                Logging.Log(String.Format("Library-based ZIP extraction selected."), Severity.DEBUG);

                if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
                ZipFile.ExtractToDirectory(sourceZipPath, tempPath);

                List<string> libPaths = Directory.GetDirectories(tempPath).ToList();
                string[] dirs = Directory.GetDirectories(tempPath);

                foreach(string dir in dirs)
                {
                    Logging.Log(String.Format("Library Subpath added! '{0}'.", dir), Severity.DEBUG);
                    libPaths.Add(dir.Replace(tempPath, ""));
                }

                List<string> allFiles;

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
                    Logging.Log(String.Format("NetFramework45-based library requested. Selecting '{0}'.", net4xPath), Severity.DEBUG);
                    allFiles = Directory.GetFiles(net4xPath, "*.*", SearchOption.AllDirectories).ToList();
                }

                foreach (string file in allFiles)
                {
                    string fileNameDest = Path.Combine(destinationPath, Path.GetFileName(file));
                    if (File.Exists(fileNameDest)) File.Delete(fileNameDest);

                    Logging.Log(String.Format("Moving file '{0}' to '{1}'.", Path.GetFileName(file), destinationPath), Severity.DEBUG);
                    File.Move(file, fileNameDest);
                }
            }
            else
            {
                Logging.Log(String.Format("Standard ZIP extraction selected."), Severity.DEBUG);
                ZipFile.ExtractToDirectory(sourceZipPath, destinationPath);
            }
        }

        private static bool DownloadFile(string webLink, string dir, string fileName)
        {
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string fullPath = Path.Combine(dir, fileName);

            if (!File.Exists(fullPath))
            {
                Logging.Log(String.Format("Downloading install files to '{0}' from '{1}'...", fullPath, webLink));

                WebClient webClient = new WebClient();
                string downloadLink = webClient.DownloadString(new Uri(webLink));

                if (!String.IsNullOrWhiteSpace(downloadLink))
                {
                    webClient.DownloadFile(new Uri(downloadLink), fullPath);

                    if (File.Exists(fullPath))
                    {
                        Logging.Log(String.Format("Download of '{0}' Complete!", fileName), Severity.DEBUG);
                        return true;
                    }
                    else Logging.Log("An unexpected error occurred while downloading the install files!", Severity.ERROR);
                }
                else Logging.Log("The requested file could not be found!", Severity.ERROR);
            }
            else Logging.Log(String.Format("Cannot update since install files already exist in '{0}'!", fullPath), Severity.ERROR);

            return false;
        }

        private static bool KillFAES(string tool)
        {
            Logging.Log(String.Format("Searching for any FAES related processes..."), Severity.DEBUG);

            if (tool != "faes_gui" || tool != "faes_cli" || tool != "faes_legacy" || tool != "faes") tool = null;

            try
            {
                if (tool == "faes_gui" || tool == null)
                {
                    foreach (var process in Process.GetProcessesByName("FileAES"))
                    {
                        process.Kill();
                        Logging.Log(String.Format("Killed '{0}' (PID: {1})!", process.ProcessName, process.Id), Severity.DEBUG);
                    }
                    foreach (var process in Process.GetProcessesByName("FAES_GUI"))
                    {
                        process.Kill();
                        Logging.Log(String.Format("Killed '{0}' (PID: {1})!", process.ProcessName, process.Id), Severity.DEBUG);
                    }
                }
                if (tool == "faes_cli" || tool == null)
                {
                    foreach (var process in Process.GetProcessesByName("FileAES-CLI"))
                    {
                        process.Kill();
                        Logging.Log(String.Format("Killed '{0}' (PID: {1})!", process.ProcessName, process.Id), Severity.DEBUG);
                    }
                    foreach (var process in Process.GetProcessesByName("FileAES_CLI"))
                    {
                        process.Kill();
                        Logging.Log(String.Format("Killed '{0}' (PID: {1})!", process.ProcessName, process.Id), Severity.DEBUG);
                    }
                    foreach (var process in Process.GetProcessesByName("FAES_CLI"))
                    {
                        process.Kill();
                        Logging.Log(String.Format("Killed '{0}' (PID: {1})!", process.ProcessName, process.Id), Severity.DEBUG);
                    }
                }
                if (tool == "faes_legacy" || tool == null)
                {
                    foreach (var process in Process.GetProcessesByName("FileAES_Legacy"))
                    {
                        process.Kill();
                        Logging.Log(String.Format("Killed '{0}' (PID: {1})!", process.ProcessName, process.Id), Severity.DEBUG);
                    }
                    foreach (var process in Process.GetProcessesByName("FileAES-Legacy"))
                    {
                        process.Kill();
                        Logging.Log(String.Format("Killed '{0}' (PID: {1})!", process.ProcessName, process.Id), Severity.DEBUG);
                    }
                    foreach (var process in Process.GetProcessesByName("FAES_Legacy"))
                    {
                        process.Kill();
                        Logging.Log(String.Format("Killed '{0}' (PID: {1})!", process.ProcessName, process.Id), Severity.DEBUG);
                    }
                }
                if (tool == "faes" || tool == null)
                {
                    foreach (var process in Process.GetProcessesByName("FAES"))
                    {
                        process.Kill();
                        Logging.Log(String.Format("Killed '{0}'!", process.ToString()), Severity.DEBUG);
                    }
                }
                Logging.Log(String.Format("Finished searching for any FAES related processes."), Severity.DEBUG);
                return true;
            }
            catch
            {
                Logging.Log(String.Format("FAES related processed found, but FAES-Updater was unable to kill them. Manually close FAES related processes, or try running FAES-Updater as admin."), Severity.ERROR);
                return false;
            }
        }

        private static void CleanupInstallFiles(string updaterTempPath)
        {
            try
            {
                if (Directory.Exists(updaterTempPath)) Directory.Delete(updaterTempPath, true);
                Logging.Log("Updater Temp directory cleared!", Severity.DEBUG);
            }
            catch
            {
                Logging.Log("Install files could not be cleaned up automatically! Something else may be using the Updater Temp directory.", Severity.WARN);
            }
        }

        private static void CleanupMiscFiles(string dir)
        {
            if (File.Exists(Path.Combine(dir, "LICENSE"))) File.Delete(Path.Combine(dir, "LICENSE"));
        }

        private static bool SafeDeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                Logging.Log(String.Format("'{0}' deleted!", path), Severity.DEBUG);
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

                if (tool == "faes")
                    licenseDownloadLink = "https://builds.mullak99.co.uk/FAES/FAES/LICENSE.txt";
                else if (tool == "faes_gui")
                    licenseDownloadLink = "https://builds.mullak99.co.uk/FAES/FAES_GUI/LICENSE.txt";
                else if (tool == "faes_cli")
                    licenseDownloadLink = "https://builds.mullak99.co.uk/FAES/FAES_CLI/LICENSE.txt";
                else if (tool == "faes_legacy")
                    licenseDownloadLink = "https://builds.mullak99.co.uk/FAES/FAES_Legacy/LICENSE.txt";

                WebClient webClient = new WebClient();
                webClient.DownloadFile(new Uri(licenseDownloadLink), Path.Combine(dir, "LICENSE.txt"));

                Logging.Log(String.Format("Added extra files to '{0}'!", dir), Severity.DEBUG);
            }
        }

        private static void SelfDelete()
        {
            string thisFile = Assembly.GetExecutingAssembly().Location;
            Logging.Log(String.Format("Initiating self-destruct ({0}).", thisFile), Severity.DEBUG);
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
            string[] ver = (typeof(FAES_Updater.Program).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version).Split('.');
            if (String.IsNullOrEmpty(preReleaseTag))
                return "v" + ver[0] + "." + ver[1] + "." + ver[2];
            else
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
