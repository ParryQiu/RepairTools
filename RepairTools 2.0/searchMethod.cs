using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Management;

namespace RepairTools_2._0
{
    class searchMethod
    {
        public static string[] getDrivers()
        {
            string[] strDisks = new string[26];
            int i = 0;
            foreach (DriveInfo temp in DriveInfo.GetDrives())
            {
                if (temp.DriveType == DriveType.Fixed || temp.DriveType == DriveType.Removable)
                {
                    strDisks[i++] = temp.Name;
                }
            }
            return strDisks;
         }

        public static string[] getRemovableDrivers()
        {
            string[] strDisks = new string[26];
            int i = 0;
            foreach (DriveInfo temp in DriveInfo.GetDrives())
            {
                if (temp.DriveType == DriveType.Removable)
                {
                    strDisks[i++] = temp.Name;
                }
            }
            return strDisks;
         }
     }
}
