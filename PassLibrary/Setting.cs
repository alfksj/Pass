using Microsoft.Win32;

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
            public static string[] keyList = { "sharing", "DeviceName", "askBeforeShare", "DefaultSave", "PingTimeout", "statusOnLaunch", "LogPageOnLaunch" };
            public static object[] defaultSetting = { true, "", false, "D:\\", 3000, false, false };
            public static RegistryValueKind[] valueType = { RegistryValueKind.String, RegistryValueKind.String, RegistryValueKind.String,
                RegistryValueKind.String, RegistryValueKind.DWord, RegistryValueKind.String, RegistryValueKind.String };
        }
        public static void checkEnvironment()
        {
            Log.log("Checking Registry environment");
            RegistryKey keyRoot = Registry.CurrentUser.OpenSubKey("SOFTWARE").OpenSubKey("Pass");
            if (keyRoot == null)
            {
                Registry.SetValue(REGED_PATH, "valid", true);
                Log.log("Created key:" + REGED_PATH);
            }
            for (int i = 0; i < Preset.Lenght; i++)
            {
                object key = Registry.GetValue(REGED_PATH, Preset.keyList[i], null);
                if (key == null)
                {
                    Log.log("Created " + REGED_PATH + Preset.keyList[i] + " = \"" + Preset.defaultSetting[i] + "\"");
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
            Log.log("Saved!");
        }
    }
}
