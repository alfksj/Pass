using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace PassLibrary
{
    public class Setting
    {
        private const string REGED_PATH = "HKEY_CURRENT_USER\\SOFTWARE\\Pass";
        public static bool Sharing { get; set; }
        public static string Name { get; set; }
        public static bool AskBeforeShare { get; set; }
        public static string DefaultSave { get; set; }
        public static int PingTimeout { get; set; }
        public static bool StatusOnLaunch { get; set; }
        public static bool LogOnLaunch { get; set; }
        public static bool AutoStartOnBoot { get; set; }
        public static bool AutoScanOnLaunch { get; set; }
        public static bool AutoScanOnVisible { get; set; }
        public static string Language { get; set; }
        public static bool StealthMode { get; set; }
        public static string Aes_Initial_Value { get; set; }
        public static string Actual_IV { get; set; }
        private class Preset
        {
            public static int Lenght
            {
                get
                {
                    int a = keyList.Length, b = defaultSetting.Length;
                    if (a != b)
                    {
                        return 0;
                    }
                    else return a;
                }
            }
            public static string[] keyList = { "sharing", "DeviceName", "askBeforeShare", "DefaultSave", "PingTimeout", "statusOnLaunch", "LogPageOnLaunch",
            "AutoStartOnInit", "autoScanOnLaunch", "autoScanOnVisible", "language", "stealthMode", "ENCRYPTION_IV"};
            public static object[] defaultSetting = { true, "", false, "USER_DOWNLOAD_PATH", 3000, false, false, true, false, true, "DEFAULT_LANG", false, "[RANDOM]"};
            public static RegistryValueKind[] valueType = { RegistryValueKind.String, RegistryValueKind.String, RegistryValueKind.String,
                RegistryValueKind.String, RegistryValueKind.DWord, RegistryValueKind.String, RegistryValueKind.String, RegistryValueKind.String,
                RegistryValueKind.String, RegistryValueKind.String, RegistryValueKind.String, RegistryValueKind.String, RegistryValueKind.String };
        }
        public static void CheckEnvironment()
        {
            Log.log("Checking Registry environment");
            RegistryKey keyRoot = Registry.CurrentUser.OpenSubKey("SOFTWARE").OpenSubKey("Pass");
            if (keyRoot == null)
            {
                RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default)
                    .OpenSubKey("SOFTWARE", true)
                    .CreateSubKey("Pass");
                Log.log("Created key: " + REGED_PATH);
            }
            for (int i = 0; i < Preset.Lenght; i++)
            {                
                object key = Registry.GetValue(REGED_PATH, Preset.keyList[i], null);
                if (key == null)
                {
                    if (i == 3)
                    {
                        Preset.defaultSetting[3] = "C:\\Users\\"
                            + Environment.GetEnvironmentVariable("USERNAME")
                            + "\\Downloads";
                    }
                    if(i == 10)
                    {
                        Preset.defaultSetting[10] = CultureInfo.InstalledUICulture.Name;
                    }
                    Log.log("Created value: " + REGED_PATH + Preset.keyList[i] + " = \"" + Preset.defaultSetting[i] + "\"");
                    Registry.SetValue(REGED_PATH, Preset.keyList[i], Preset.defaultSetting[i], Preset.valueType[i]);
                }
            }
        }
        /// <summary>
        /// Load setting
        /// configure env if it's not configured.
        /// </summary>
        public static void Load()
        {
            Log.log("Loading");
            Sharing = bool.Parse(Registry.GetValue(REGED_PATH, "sharing", true).ToString());
            Name = Registry.GetValue(REGED_PATH, "DeviceName", "").ToString();
            AskBeforeShare = bool.Parse(Registry.GetValue(REGED_PATH, "askBeforeShare", false).ToString());
            DefaultSave = Registry.GetValue(REGED_PATH, "DefaultSave", "").ToString();
            PingTimeout = (int)Registry.GetValue(REGED_PATH, "PingTimeout", 0);
            StatusOnLaunch = bool.Parse(Registry.GetValue(REGED_PATH, "statusOnLaunch", false).ToString());
            LogOnLaunch = bool.Parse(Registry.GetValue(REGED_PATH, "LogPageOnLaunch", false).ToString());
            AutoStartOnBoot = bool.Parse(Registry.GetValue(REGED_PATH, "AutoStartOnInit", true).ToString());
            AutoScanOnVisible = bool.Parse(Registry.GetValue(REGED_PATH, "autoScanOnVisible", true).ToString());
            AutoScanOnLaunch = bool.Parse(Registry.GetValue(REGED_PATH, "autoScanOnLaunch", false).ToString());
            Language = Registry.GetValue(REGED_PATH, "language", "en-US").ToString();
            StealthMode = bool.Parse(Registry.GetValue(REGED_PATH, "stealthMode", false).ToString());
            Aes_Initial_Value = Registry.GetValue(REGED_PATH, "ENCRYPTION_IV", "[DEFAULT]").ToString();
            if(Aes_Initial_Value.Equals("[DEFAULT]"))
            {
                Actual_IV = "e0gwegwdgofdogofdhgfuysdkfjdkejd";
            }
            else if(Aes_Initial_Value.Equals("[RANDOM]"))
            {
                RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();
                byte[] iv = new byte[32];
                rand.GetBytes(iv, 0, 32);
                Actual_IV = Encoding.ASCII.GetString(iv);
            }
            else
            {
                Actual_IV = Aes_Initial_Value;
            }
            Log.log("Loaded!");
        }
        /// <summary>
        /// Save setting
        /// configure env if it's not configured.
        /// </summary>
        public static void Save()
        {
            Log.log("Saving");
            Registry.SetValue(REGED_PATH, "sharing", Sharing);
            Registry.SetValue(REGED_PATH, "DeviceName", Name);
            Registry.SetValue(REGED_PATH, "askBeforeShare", AskBeforeShare);
            Registry.SetValue(REGED_PATH, "DefaultSave", DefaultSave);
            Registry.SetValue(REGED_PATH, "PingTimeout", PingTimeout);
            Registry.SetValue(REGED_PATH, "statusOnLaunch", StatusOnLaunch);
            Registry.SetValue(REGED_PATH, "LogPageOnLaunch", LogOnLaunch);
            Registry.SetValue(REGED_PATH, "autoScanOnVisible", AutoScanOnVisible);
            Registry.SetValue(REGED_PATH, "autoScanOnLaunch", AutoScanOnLaunch);
            Registry.SetValue(REGED_PATH, "AutoStartOnInit", AutoStartOnBoot);
            Registry.SetValue(REGED_PATH, "language", Language);
            Registry.SetValue(REGED_PATH, "stealthMode", StealthMode);
            Registry.SetValue(REGED_PATH, "ENCRYPTION_IV", Aes_Initial_Value);
            Log.log("Saved!");
        }
        /// <summary>
        /// Clear all settings as first settings.
        /// </summary>
        public static void ClearEverything()
        {
            Log.log("Clear all settings");
            for (int i = 0; i < Preset.Lenght; i++)
            {
                if (i == 3)
                {
                    Preset.defaultSetting[3] = "C:\\Users\\"
                        + Environment.GetEnvironmentVariable("USERNAME")
                        + "\\Downloads";
                }
                if (i == 10)
                {
                    Preset.defaultSetting[10] = CultureInfo.InstalledUICulture.Name;
                }
                Registry.SetValue(REGED_PATH, Preset.keyList[i], Preset.defaultSetting[i], Preset.valueType[i]);
                Log.log("Set value: " + REGED_PATH + Preset.keyList[i] + " = \"" + Preset.defaultSetting[i] + "\"");
            }
        }
    }
}
