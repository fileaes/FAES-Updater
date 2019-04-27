﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FAES_Updater
{
    class Program
    {
        private static string _directory = "";
        private static string _branch = "stable";
        private static string _tool = "faes_gui";
        private static string _faesLib = "both";
        private static string _installVer = "latest";
        private static bool _installSuite = false;
        private static bool _verbose = false;
        private static bool _fullInstall = false;
        private static bool _runPost = false;
        private static bool _writeExtraFiles = true;

        static void Main(string[] args)
        {
            IntPtr handle = GetConsoleWindow();

            for (int i = 0; i < args.Length; i++)
            {
                string strippedArg = args[i].ToLower();

                if (Directory.Exists(args[i])) _directory = args[i];

                strippedArg = strippedArg.TrimStart('-', '/', '\\');

                if (strippedArg == "verbose" || strippedArg == "v" || strippedArg == "developer" || strippedArg == "dev" || strippedArg == "debug") _verbose = true;
                else if (strippedArg == "suite" || strippedArg == "installsuite" || strippedArg == "all") _installSuite = true;
                else if (strippedArg == "fullinstall" || strippedArg == "full") _fullInstall = true;
                else if (strippedArg == "portableinstall" || strippedArg == "p" || strippedArg == "portable") _fullInstall = false;
                else if (strippedArg == "run" || strippedArg == "r" || strippedArg == "runpost") _runPost = true;
                else if (strippedArg == "silent" || strippedArg == "s" || strippedArg == "headless") ShowWindow(handle, SW_HIDE);
                else if ((strippedArg == "directory" || strippedArg == "d" || strippedArg == "dir" || strippedArg == "installdir") && args.Length > i + 1 && !String.IsNullOrEmpty(args[i + 1]))
                {
                    if (!Directory.Exists(args[i + 1])) Directory.CreateDirectory(args[i + 1]);
                    _directory = args[i + 1];
                }
                else if ((strippedArg == "ver" || strippedArg == "v" || strippedArg == "version") && args.Length > i + 1 && !String.IsNullOrEmpty(args[i + 1]))
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
                else if ((strippedArg == "faeslib" || strippedArg == "l") && args.Length > i + 1 && !String.IsNullOrEmpty(args[i + 1]))
                {
                    if (args[i + 1].ToLower() == "both") _faesLib = "both";
                    else if (args[i + 1].ToLower() == "netframework") _faesLib = "netf";
                    else if (args[i + 1].ToLower() == "netf") _faesLib = "netf";
                    else if (args[i + 1].ToLower().Contains("net4")) _faesLib = "netf";
                    else if (args[i + 1].ToLower().Contains("netcore")) _faesLib = "netc";
                    else if (args[i + 1].ToLower() == "netc") _faesLib = "netc";
                }
                else if (strippedArg == "netcore" || strippedArg == "c" || strippedArg == "core") _faesLib = "netc";
                else if (strippedArg == "netframework" || strippedArg == "f" || strippedArg == "framework") _faesLib = "netf";
                else if (strippedArg == "noextrafiles" || strippedArg == "pure" || strippedArg == "noextras") _writeExtraFiles = false;
            }

            if (_installSuite)
            {
                UpdateTool("faes_gui", Path.Combine(_directory, "FileAES"));
                UpdateTool("faes_legacy", Path.Combine(_directory, "FileAES_Legacy"));
                UpdateTool("faes_cli", Path.Combine(_directory, "FileAES_CLI"));
            }
            else UpdateTool(_tool, _directory);
        }

        private static void UpdateTool(string tool, string directory)
        {
            if (String.IsNullOrWhiteSpace(directory)) directory = Environment.CurrentDirectory;

            string toolNameFormatted;

            if (tool == "faes") toolNameFormatted = "FAES";
            else if (tool == "faes_gui") toolNameFormatted = "FAES_GUI";
            else if (tool == "faes_cli") toolNameFormatted = "FAES_CLI";
            else if (tool == "faes_legacy") toolNameFormatted = "FAES_Legacy";
            else toolNameFormatted = tool.ToUpper();

            Logging.Log(String.Format("{0} Update Selected!", toolNameFormatted), Severity.DEBUG);
            KillFAES(tool);

            string downloadLink = String.Format("https://api.mullak99.co.uk/FAES/GetDownload.php?app={0}&ver={1}&branch={2}", tool, _installVer, _branch);
            string installPath = Path.Combine(directory, "FAES_Updater_Temp");
            string fileName;

            if (_installVer != "latest") fileName = String.Format("{0}-{1}.zip", toolNameFormatted, _installVer);
            else fileName = String.Format("{0}-{1}-{2}.zip", toolNameFormatted, _branch, _installVer);

            try
            {
                if (DoesVersionExist())
                {
                    if (DownloadFile(downloadLink, installPath, fileName))
                    {
                        ExtractZip(Path.Combine(installPath, fileName), directory, installPath);

                        CleanupMiscFiles(directory);
                        if (_writeExtraFiles) AddExtraFiles(tool, directory);

                        if (_runPost)
                        {
                            string exeToRun = null;

                            if (tool == "faes_gui") exeToRun = "FileAES.exe";
                            else if (tool == "faes_cli") exeToRun = "FileAES-CLI.exe";
                            else if (tool == "faes_legacy") exeToRun = "FileAES_Legacy.exe";

                            string process = Path.Combine(directory, exeToRun);

                            if (!String.IsNullOrEmpty(exeToRun) && File.Exists(process))
                            {
                                Logging.Log(String.Format("Starting process '{0}'...", exeToRun), Severity.DEBUG);
                                Process.Start(process);
                            }
                            else Logging.Log(String.Format("'{0}' executible could not be found. Aborting auto-run.", exeToRun), Severity.WARN);
                        }
                    }
                }
                else
                {
                    Logging.Log(String.Format("'{0}' on branch '{1}' does not exist!", _installVer, _branch), Severity.ERROR);
                }
            }
            catch (WebException)
            {
                Logging.Log("A connection to the download server could not be made! Please ensure you are connected to the internet.", Severity.ERROR);
            }
            catch (Exception e)
            {
                Logging.Log(String.Format("An unexpected error occured while downloading the install files! Exception: {0}", e.ToString()), Severity.ERROR);
            }
            finally
            {
                CleanupInstallFiles(installPath);
            }
        }

        private static bool DoesVersionExist()
        {
            Logging.Log(String.Format("Checking if '{0}' version '{1}' exists in branch '{2}'", _tool, _installVer, _branch), Severity.DEBUG);

            string downloadLink = String.Format("https://api.mullak99.co.uk/FAES/DoesVersionExist.php?app={0}&ver={1}&branch={2}", _tool, _installVer, _branch);

            WebClient webClient = new WebClient();
            string doesExist = webClient.DownloadString(new Uri(downloadLink));

            Logging.Log(String.Format("{0}|{2}|{1}: {3}", _tool, _installVer, _branch, doesExist), Severity.DEBUG);

            return (doesExist == "VersionExists");
        }

        private static void ExtractZip(string sourceZipPath, string destinationPath, string tempPath)
        {
            Logging.Log(String.Format("Extracting ZIP '{0}'", sourceZipPath), Severity.DEBUG);

            if (_tool == "faes" && _faesLib != "both")
            {
                Logging.Log(String.Format("Library-based ZIP extaction selected."), Severity.DEBUG);

                if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
                ZipFile.ExtractToDirectory(sourceZipPath, tempPath);

                List<string> libPaths = Directory.GetDirectories(tempPath).ToList();
                string[] dirs = Directory.GetDirectories(tempPath);

                foreach(string dir in dirs)
                {
                    Logging.Log(String.Format("Library Subpath added! '{0}'.", dir), Severity.DEBUG);
                    libPaths.Add(dir.Replace(tempPath, ""));
                }

                string netCorePath = libPaths.FirstOrDefault(path => path.Split('/', '\\').Last().Contains("netcoreapp"));
                string net4xPath = libPaths.FirstOrDefault(path => path.Split('/', '\\').Last().Contains("net4"));

                List<string> allFiles;

                if (_faesLib == "netc")
                {
                    Logging.Log(String.Format("NetCore-based library requested. Selecting '{0}'.", netCorePath), Severity.DEBUG);
                    allFiles = Directory.GetFiles(netCorePath, "*.*", SearchOption.AllDirectories).ToList();
                }
                else
                {
                    Logging.Log(String.Format("NetFramework-based library requested. Selecting '{0}'.", net4xPath), Severity.DEBUG);
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
                Logging.Log(String.Format("Standard ZIP extaction selected."), Severity.DEBUG);
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
                    else Logging.Log("An unexpected error occured while downloading the install files!", Severity.ERROR);
                }
                else Logging.Log("The requested file could not be found!", Severity.ERROR);
            }
            else Logging.Log(String.Format("Cannot update since install files already exist in '{0}'!", fullPath), Severity.ERROR);

            return false;
        }

        private static void KillFAES(string tool)
        {
            Logging.Log(String.Format("Searching for any FAES related processes..."), Severity.DEBUG);

            if (tool != "faes_gui" || tool != "faes_cli" || tool != "faes_legacy" || tool != "faes") tool = null;

            if (tool == "faes_gui" || tool == null)
            {
                foreach (var process in Process.GetProcessesByName("FileAES"))
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
            }
            if (tool == "faes_legacy" || tool == null)
            {
                foreach (var process in Process.GetProcessesByName("FileAES_Legacy"))
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

        public static bool GetVerbose()
        {
            return _verbose;
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
    }
}