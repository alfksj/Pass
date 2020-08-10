using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Resources;

namespace PassLibrary
{
    public class Regedit
    {
        public static ResourceManager rm { get; set; }
        public static string originated { get; set; }
        public static void checkEnvironment(bool isRegedEditMod)
        {
            try
            {
                RegistryKey rightMenuKey = 
                    RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Default)
                        .OpenSubKey("*", true)
                        .OpenSubKey("shell", true)
                        .OpenSubKey("Pass", true);
                if (rightMenuKey==null || rightMenuKey.OpenSubKey("command")==null)
                {
                    RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Default)
                        .OpenSubKey("*", true)
                        .OpenSubKey("shell", true)
                        .CreateSubKey("Pass", true);
                    rightMenuKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Default)
                        .OpenSubKey("*", true)
                        .OpenSubKey("shell", true)
                        .OpenSubKey("Pass", true);
                    rightMenuKey.CreateSubKey("command");
                    rightMenuKey.SetValue("", rm.GetString("shareWith"));
                    rightMenuKey.SetValue("icon", originated);
                    rightMenuKey.OpenSubKey("command", true).SetValue("", '\"' + originated + "\" \"%1\"");
                    Log.log("Wrote env reged values");
                }
            } catch(UnauthorizedAccessException)
            {
                Log.log("Cannot modify registry: Permission denied", Log.ERR);
                if (isRegedEditMod) return;
                Log.log("Make other process of Pass with elevated privileges");
                ProcessStartInfo newInstance = new ProcessStartInfo(originated, "null -RegisEdit");
                newInstance.Verb = "runas";
                try
                {
                    Process.Start(newInstance);
                } catch(Exception)
                {
                    Log.log("User Permission Denied. Cannot elevate privileges.", Log.FATAL);
                }
            }
        }
    }
}
