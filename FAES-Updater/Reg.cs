using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IWshRuntimeLibrary;
using Microsoft.Win32;

namespace FAES_Updater
{
    public class Reg
    {
        private const string _faesInstalledSoftware = "Software\\FileAES";
        private const string _faesExtentionFileType = ".faes";
        private const string _faesShellOpenCommand = "FAES\\shell\\open\\command";
        private const string _faesContextMenuNameFolders = "Folder\\Shell\\FAES";
        private const string _faesContextCommandNameFolders = "Folder\\Shell\\FAES\\command";
        private const string _faesContextMenuNameFiles = "*\\Shell\\FAES";
        private const string _faesContextCommandNameFiles = "*\\Shell\\FAES\\command";
        private const string _faesContextMenuNamePeek = "SystemFileAssociations\\.faes\\Shell\\Peek with FileAES";
        private const string _faesContextCommandNamePeek = "SystemFileAssociations\\.faes\\Shell\\Peek with FileAES\\command";
        private static readonly string _startMenuShortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs", "FileAES");


        public void CreateSoftwareFilePath(string path, string appName)
        {
            try
            {
                Registry.SetValue($"HKEY_CURRENT_USER\\{_faesInstalledSoftware}\\{appName}", "Path", path);
            }
            catch (Exception e)
            {
                Logging.Log("Unable to create SoftwareFilePath key! Exception: " + e, Severity.ERROR);
            }
        }

        public void DeleteSoftwareFilePaths()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(_faesInstalledSoftware);
            }
            catch (Exception)
            {
                Logging.Log("Unable to delete SoftwareFilePath key! Are you sure it exists?", Severity.WARN);
            }
        }

        public string[] GetSoftwareFilePaths(out List<string> toolNames)
        {
            RegistryKey registry = Registry.CurrentUser.OpenSubKey(_faesInstalledSoftware);

            if (registry != null)
            {
                toolNames = registry.GetSubKeyNames().ToList();
                string[] filePaths = new string[toolNames.Count];

                for (int i = 0; i < toolNames.Count; i++)
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                               $"{_faesInstalledSoftware}\\{toolNames[i]}"))
                    {
                        filePaths[i] = key.GetValue("Path").ToString();
                    }
                }
                return filePaths;
            }
            toolNames = null;
            return null;
        }

        public void CreateFileTypeAssociation(string pathToTool)
        {
            DeleteFileTypeAssociation(); // Delete existing filetype associations

            try
            {
                Registry.ClassesRoot.CreateSubKey(_faesExtentionFileType)?.SetValue("", "FAES", RegistryValueKind.String);
                Registry.ClassesRoot.CreateSubKey(_faesShellOpenCommand)?.SetValue("", $"{pathToTool} \"%1\"", RegistryValueKind.String);
            }
            catch (Exception e)
            {
                Logging.Log("Unable to create FileTypeAssociation keys! Exception: " + e, Severity.ERROR);
            }
        }

        public void DeleteFileTypeAssociation()
        {
            try
            {
                Registry.ClassesRoot.DeleteSubKey(_faesExtentionFileType);
                Registry.ClassesRoot.DeleteSubKey(_faesShellOpenCommand);
            }
            catch (Exception)
            {
                Logging.Log("Unable to delete FileTypeAssociation keys! Are you sure they exist?", Severity.WARN);
            }
        }

        public void CreateStartMenuShortcut(string path, string appName, string description)
        {
            try
            {
                if (!Directory.Exists(_startMenuShortcutPath))
                    Directory.CreateDirectory(_startMenuShortcutPath);

                string shortcutLocation = Path.Combine(_startMenuShortcutPath, appName + ".lnk");

                if (System.IO.File.Exists(shortcutLocation))
                    System.IO.File.Delete(shortcutLocation);

                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

                shortcut.Description = description;
                shortcut.TargetPath = path;
                shortcut.Save();

                Logging.Log($"Created Start Menu shortcut for '{appName}'", Severity.DEBUG);
            }
            catch (Exception e)
            {
                Logging.Log($"Unable to create Start Menu shortcut for '{appName}'! Exception: {e}", Severity.WARN);
            }
        }

        public void DeleteStartMenuShortcuts()
        {
            try
            {
                if (Directory.Exists(_startMenuShortcutPath))
                    Directory.Delete(_startMenuShortcutPath, true);
            }
            catch (Exception)
            {
                Logging.Log("Unable to delete Start Menu shortcuts!", Severity.WARN);
            }
        }

        public void CreateContextMenus(string pathToTool)
        {
            DeleteContextMenus(); // Delete existing keys

            CreateContextMenusFolders(pathToTool); // Context Menus for Folders
            CreateContextMenusFiles(pathToTool); // Context Menus for Files
            CreateContextMenusPeek(pathToTool); // Context Menu for .faes files (Peeking)
        }

        public void DeleteContextMenus()
        {
            DeleteKey(Registry.ClassesRoot.OpenSubKey("Folder\\Shell", true), "FAES");
            DeleteKey(Registry.ClassesRoot.OpenSubKey("*\\Shell", true), "FAES");
            DeleteKey(Registry.ClassesRoot.OpenSubKey("SystemFileAssociations", true), ".faes");
        }

        private void CreateContextMenusFolders(string pathToTool)
        {
            RegistryKey regmenu = null;
            RegistryKey regcmd = null;
            try
            {
                regmenu = Registry.ClassesRoot.CreateSubKey(_faesContextMenuNameFolders);
                if (regmenu != null)
                {
                    regmenu.SetValue("", "Open with FileAES");
                    regmenu.SetValue("Icon", $"\"{pathToTool}\"");
                }
                regcmd = Registry.ClassesRoot.CreateSubKey(_faesContextCommandNameFolders);
                if (regcmd != null)
                    regcmd.SetValue("", $"\"{pathToTool}\" \"%1\"");

                Logging.Log("Created 'Open with FileAES' context menu (Folders)", Severity.DEBUG);
            }
            catch (Exception e)
            {
                Logging.Log($"Could not add SystemFileAssociations: {e}", Severity.ERROR);
            }
            finally
            {
                if (regmenu != null)
                    regmenu.Close();
                if (regcmd != null)
                    regcmd.Close();
            }
        }

        private void CreateContextMenusFiles(string pathToTool)
        {
            RegistryKey regmenu = null;
            RegistryKey regcmd = null;
            try
            {
                regmenu = Registry.ClassesRoot.CreateSubKey(_faesContextMenuNameFiles);
                if (regmenu != null)
                {
                    regmenu.SetValue("", "Open with FileAES");
                    regmenu.SetValue("Icon", $"\"{pathToTool}\"");
                }
                regcmd = Registry.ClassesRoot.CreateSubKey(_faesContextCommandNameFiles);
                if (regcmd != null)
                    regcmd.SetValue("", $"\"{pathToTool}\" \"%1\"");

                Logging.Log("Created 'Open with FileAES' context menu (Files)", Severity.DEBUG);
            }
            catch (Exception e)
            {
                Logging.Log($"Could not add SystemFileAssociations: {e}");
            }
            finally
            {
                if (regmenu != null)
                    regmenu.Close();
                if (regcmd != null)
                    regcmd.Close();
            }
        }

        private void CreateContextMenusPeek(string pathToTool)
        {
            RegistryKey regmenu = null;
            RegistryKey regcmd = null;
            try
            {
                regmenu = Registry.ClassesRoot.CreateSubKey(_faesContextMenuNamePeek);
                if (regmenu != null)
                {
                    regmenu.SetValue("", "Peek with FileAES");
                    regmenu.SetValue("Icon", $"\"{pathToTool}\"");
                }
                regcmd = Registry.ClassesRoot.CreateSubKey(_faesContextCommandNamePeek);
                if (regcmd != null)
                    regcmd.SetValue("", $"\"{pathToTool}\" \"%1\" \"--peek\"");

                Logging.Log("Created 'Peek with FileAES' context menu", Severity.DEBUG);
            }
            catch (Exception e)
            {
                Logging.Log($"Could not add SystemFileAssociations: {e}", Severity.ERROR);
            }
            finally
            {
                if (regmenu != null)
                    regmenu.Close();
                if (regcmd != null)
                    regcmd.Close();
            }
        }

        public void DeleteUserData()
        {
            string userData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "mullak99", "FileAES");
            if (Directory.Exists(userData))
            {
                Directory.Delete(userData, true);
                Logging.Log("Deleted userdata!");
            }
        }

        private void DeleteKey(RegistryKey regKey, string subKey)
        {
            try
            {
                regKey.DeleteSubKeyTree(subKey);
            }
            catch (Exception)
            {
                Logging.Log($"Could not delete key '{regKey.Name}'! Are you sure it exists?", Severity.WARN);
            }
        }
    }
}
