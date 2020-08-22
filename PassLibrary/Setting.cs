using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Globalization;

namespace PassLibrary
{
    public class Setting
    {
        private const string REGED_PATH = "HKEY_CURRENT_USER\\SOFTWARE\\Pass";
        public static bool sharing { get; set; }
        public static string name { get; set; }
        public static bool askBeforeShare { get; set; }
        public static string defaultSave { get; set; }
        public static int pingTimeout { get; set; }
        public static bool statusOnLaunch { get; set; }
        public static bool logOnLaunch { get; set; }
        public static bool autoStartOnBoot { get; set; }
        public static bool autoScanOnLaunch { get; set; }
        public static bool autoScanOnVisible { get; set; }
        public static string language { get; set; }
        public static bool stealthMode { get; set; }
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
            public static object[] defaultSetting = { true, "", false, "USER_DOWNLOAD_PATH", 3000, false, false, true, false, true, "DEFAULT_LANG", false, "[DEFAULT]"};
            public static RegistryValueKind[] valueType = { RegistryValueKind.String, RegistryValueKind.String, RegistryValueKind.String,
                RegistryValueKind.String, RegistryValueKind.DWord, RegistryValueKind.String, RegistryValueKind.String, RegistryValueKind.String,
                RegistryValueKind.String, RegistryValueKind.String, RegistryValueKind.String, RegistryValueKind.String, RegistryValueKind.String };
        }
        public static void checkEnvironment()
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
        public static void load()
        {
            Log.log("Loading");
            sharing = bool.Parse(Registry.GetValue(REGED_PATH, "sharing", true).ToString());
            name = Registry.GetValue(REGED_PATH, "DeviceName", "").ToString();
            askBeforeShare = bool.Parse(Registry.GetValue(REGED_PATH, "askBeforeShare", false).ToString());
            defaultSave = Registry.GetValue(REGED_PATH, "DefaultSave", "").ToString();
            pingTimeout = (int)Registry.GetValue(REGED_PATH, "PingTimeout", 0);
            statusOnLaunch = bool.Parse(Registry.GetValue(REGED_PATH, "statusOnLaunch", false).ToString());
            logOnLaunch = bool.Parse(Registry.GetValue(REGED_PATH, "LogPageOnLaunch", false).ToString());
            autoStartOnBoot = bool.Parse(Registry.GetValue(REGED_PATH, "AutoStartOnInit", true).ToString());
            autoScanOnVisible = bool.Parse(Registry.GetValue(REGED_PATH, "autoScanOnVisible", true).ToString());
            autoScanOnLaunch = bool.Parse(Registry.GetValue(REGED_PATH, "autoScanOnLaunch", false).ToString());
            language = Registry.GetValue(REGED_PATH, "language", "en-US").ToString();
            stealthMode = bool.Parse(Registry.GetValue(REGED_PATH, "stealthMode", false).ToString());
            Aes_Initial_Value = Registry.GetValue(REGED_PATH, "ENCRYPTION_IV", "[DEFAULT]").ToString();
            if(Aes_Initial_Value.Equals("[DEFAULT]"))
            {
                Actual_IV = "23fwe0gwegwdgofdogofdhg";
            }
            Log.log("Loaded!");
        }
        /// <summary>
        /// Save setting
        /// configure env if it's not configured.
        /// </summary>
        public static void save()
        {
            Log.log("Saving");
            Registry.SetValue(REGED_PATH, "sharing", sharing);
            Registry.SetValue(REGED_PATH, "DeviceName", name);
            Registry.SetValue(REGED_PATH, "askBeforeShare", askBeforeShare);
            Registry.SetValue(REGED_PATH, "DefaultSave", defaultSave);
            Registry.SetValue(REGED_PATH, "PingTimeout", pingTimeout);
            Registry.SetValue(REGED_PATH, "statusOnLaunch", statusOnLaunch);
            Registry.SetValue(REGED_PATH, "LogPageOnLaunch", logOnLaunch);
            Registry.SetValue(REGED_PATH, "autoScanOnVisible", autoScanOnVisible);
            Registry.SetValue(REGED_PATH, "autoScanOnLaunch", autoScanOnLaunch);
            Registry.SetValue(REGED_PATH, "AutoStartOnInit", autoStartOnBoot);
            Registry.SetValue(REGED_PATH, "language", language);
            Registry.SetValue(REGED_PATH, "stealthMode", stealthMode);
            Registry.SetValue(REGED_PATH, "ENCRYPTION_IV", Aes_Initial_Value);
            Log.log("Saved!");
        }
        /// <summary>
        /// Clear all settings as first settings.
        /// </summary>
        public static void clearEverything()
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
