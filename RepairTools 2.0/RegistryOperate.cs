using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace RepairTools_2._0
{
    class RegistryOperate
    {
        private static RegistryKey hklm = Registry.LocalMachine;
        private static RegistryKey software = hklm.OpenSubKey("SOFTWARE");
        private static RegistryKey microsoft = software.OpenSubKey("Microsoft");
        private static RegistryKey windows = microsoft.OpenSubKey("Windows");
        private static RegistryKey windowsnt = microsoft.OpenSubKey("Windows NT"); 
        private static RegistryKey currentversion = windows.OpenSubKey("CurrentVersion");
        private static RegistryKey nt_currentversion = windowsnt.OpenSubKey("CurrentVersion");
        private static RegistryKey image = nt_currentversion.OpenSubKey("Image File Execution Options",true);
        private static RegistryKey explorer = currentversion.OpenSubKey("Explorer");
        private static RegistryKey advanced = explorer.OpenSubKey("Advanced");
        private static RegistryKey folder = advanced.OpenSubKey("Folder");
        private static RegistryKey hidden = folder.OpenSubKey("Hidden");
        private static RegistryKey showall = hidden.OpenSubKey("SHOWALL",true);
        private static RegistryKey run = currentversion.OpenSubKey("Run",true);
        private static RegistryKey hkcu = Registry.CurrentUser;
        private static RegistryKey hkcu_software = hkcu.OpenSubKey("SOFTWARE");
        private static RegistryKey hkcu_microsoft = hkcu_software.OpenSubKey("Microsoft");
        private static RegistryKey hkcu_windows = hkcu_microsoft.OpenSubKey("Windows");
        private static RegistryKey hkcu_currentversion = hkcu_windows.OpenSubKey("CurrentVersion");
        private static RegistryKey hkcu_run = currentversion.OpenSubKey("Run");

        public static RegistryKey getImage()
        {
            return image;
        }

        public static RegistryKey getShowall()
        {
            return showall;
        }

        public static RegistryKey getRun()
        {
            return run;
        }

        public static bool registryRead()
        {
            if ((int)showall.GetValue("CheckedValue") == 1)
            {
                return true;
            }
            else
                return false;
        }

        public static bool registrySetValue(string key, int value)
        {
            try
            {
                showall.SetValue(key, value,RegistryValueKind.DWord);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string[] strRegistry()
        {
            return image.GetSubKeyNames();
        }

        public static string[] strRunRegistry()
        {
            return run.GetValueNames();
        }

        public static void deleteSubKey(string str)
        {
            try
            {
                image.DeleteSubKey(str);
                image.Close();
            }
            catch
            {
 
            }
        }

        public static bool checkImageValue(string str)
        {
            string[] subkeyNames;
            RegistryKey check = image.OpenSubKey(str);
            subkeyNames = check.GetValueNames();
            bool status = false;
            foreach (string keyName in subkeyNames)
            {
                if (keyName == "debugger" || keyName=="Debugger")
                {
                    status = true;
                }   
            }
            return status;
        }

        public static void deleteStartupFiles(string str)
        {
            run.DeleteValue(str);
            run.Close();
        }

        public static void setRunKey(string path)
        {
            try
            {
                run.SetValue("RepairTools", path, RegistryValueKind.String);
            }
            catch
            {
 
            }
        }

        public static void deleteRunKey()
        {
            try
            {
                run.DeleteValue("RepairTools");
            }
            catch
            {
 
            }
        }
    }
}