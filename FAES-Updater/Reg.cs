using System;
using System.IO;
using IWshRuntimeLibrary;
using Microsoft.Win32;

namespace FAES_Updater
{
    public class Reg
    {
        public void AssociateFileTypes(string pathToTool)
        {
            Registry.ClassesRoot.CreateSubKey(".faes").SetValue("", "FAES", RegistryValueKind.String);
            Registry.ClassesRoot.CreateSubKey("FAES\\shell\\open\\command").SetValue("", String.Format("{0} \"%1\"", pathToTool), RegistryValueKind.String);
        }

        public void StartShortcut(string path, string appName, string description)
        {
            string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            string appStartMenuPath = Path.Combine(commonStartMenuPath, "Programs", "FileAES");

            if (!Directory.Exists(appStartMenuPath))
                Directory.CreateDirectory(appStartMenuPath);

            string shortcutLocation = Path.Combine(appStartMenuPath, appName + ".lnk");

            if (System.IO.File.Exists(shortcutLocation))
                System.IO.File.Delete(shortcutLocation);

            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

            shortcut.Description = description;
            shortcut.TargetPath = path;
            shortcut.Save();

            Logging.Log(String.Format("Created Start Menu shortcut for '{0}'", appName), Severity.DEBUG);
        }

        public void ContextMenus(string pathToTool)
        {
            {
                string MenuName = "Folder\\Shell\\FAES";
                string Command = "Folder\\Shell\\FAES\\command";

                RegistryKey regmenu = null;
                RegistryKey regcmd = null;
                try
                {
                    regmenu = Registry.ClassesRoot.CreateSubKey(MenuName);
                    if (regmenu != null)
                    {
                        regmenu.SetValue("", "Open with FileAES");
                        regmenu.SetValue("Icon", String.Format("\"{0}\"", pathToTool));
                    }
                    regcmd = Registry.ClassesRoot.CreateSubKey(Command);
                    if (regcmd != null)
                        regcmd.SetValue("", String.Format("\"{0}\" \"%1\"", pathToTool));

                    Logging.Log("Created 'Open with FileAES' context menu (Folders)", Severity.DEBUG);
                }
                catch (Exception e)
                {
                    Logging.Log(String.Format("Could not add SystemFileAssociations: {0}", e), Severity.ERROR);
                }
                finally
                {
                    if (regmenu != null)
                        regmenu.Close();
                    if (regcmd != null)
                        regcmd.Close();
                }
            }

            DeleteKey(Registry.ClassesRoot.OpenSubKey("*\\Shell", true), "FAES");

            {
                string MenuName = "*\\Shell\\FAES";
                string Command = "*\\Shell\\FAES\\command";

                RegistryKey regmenu = null;
                RegistryKey regcmd = null;
                try
                {
                    regmenu = Registry.ClassesRoot.CreateSubKey(MenuName);
                    if (regmenu != null)
                    {
                        regmenu.SetValue("", "Open with FileAES");
                        regmenu.SetValue("Icon", String.Format("\"{0}\"", pathToTool));
                    }
                    regcmd = Registry.ClassesRoot.CreateSubKey(Command);
                    if (regcmd != null)
                        regcmd.SetValue("", String.Format("\"{0}\" \"%1\"", pathToTool));

                    Logging.Log("Created 'Open with FileAES' context menu (Files)", Severity.DEBUG);
                }
                catch (Exception e)
                {
                    Logging.Log(String.Format("Could not add SystemFileAssociations: {0}", e));
                }
                finally
                {
                    if (regmenu != null)
                        regmenu.Close();
                    if (regcmd != null)
                        regcmd.Close();
                }
            }

            DeleteKey(Registry.ClassesRoot.OpenSubKey("SystemFileAssociations", true), ".faes");

            {
                string MenuName = "SystemFileAssociations\\.faes\\Shell\\Peek with FileAES";
                string Command = "SystemFileAssociations\\.faes\\Shell\\Peek with FileAES\\command";

                RegistryKey regmenu = null;
                RegistryKey regcmd = null;
                try
                {
                    regmenu = Registry.ClassesRoot.CreateSubKey(MenuName);
                    if (regmenu != null)
                    {
                        regmenu.SetValue("", "Peek with FileAES");
                        regmenu.SetValue("Icon", String.Format("\"{0}\"", pathToTool));
                    }
                    regcmd = Registry.ClassesRoot.CreateSubKey(Command);
                    if (regcmd != null)
                        regcmd.SetValue("", String.Format("\"{0}\" \"%1\" \"--peek\"", pathToTool));

                    Logging.Log("Created 'Peek with FileAES' context menu", Severity.DEBUG);
                }
                catch (Exception e)
                {
                    Logging.Log(String.Format("Could not add SystemFileAssociations: {0}", e), Severity.ERROR);
                }
                finally
                {
                    if (regmenu != null)
                        regmenu.Close();
                    if (regcmd != null)
                        regcmd.Close();
                }
            }
        }

        private void DeleteKey(RegistryKey regKey, string subKey)
        {
            regKey.DeleteSubKeyTree(subKey);
        }
    }
}
