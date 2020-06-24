using Newtonsoft.Json.Linq;
using System;
using System.Dynamic;
using System.IO;
using System.Net;

namespace PassLibrary
{
    public class Setting
    {
        private static string ROOT = Environment.GetEnvironmentVariable("appdata") + "/Pass";
        private static string PATH = Environment.GetEnvironmentVariable("appdata")+"/Pass/setting.json";

        private static bool sharing { get; set; }
        private static string iam { get; set; }
        public static bool Sharing
        {
            get
            {
                return sharing;
            }
            set
            {
                sharing = value;
            }
        }
        public static string Iam
        {
            get
            {
                return iam;
            }
            set
            {
                iam = value;
            }
        }

        /// <summary>
        /// Load setting
        /// configure env if it's not configured.
        /// </summary>
        public static void load()
        {
            Log.log("Loading");
            if(!Directory.Exists(ROOT) || !File.Exists(PATH))
            {
                Directory.CreateDirectory(ROOT);
                File.WriteAllText(PATH, "{\n" +
                    "    \"allowShare\":true,\n" +
                    "    \"iam\":\"DefaultName\"\n" +
                    "}");
                Log.log("Environment configured");
            }
            string raw = File.ReadAllText(PATH);
            JObject RootObj = JObject.Parse(raw);
            Sharing = (bool)RootObj.GetValue("allowShare");
            Iam = RootObj.GetValue("iam").ToString();
            Log.log("Loaded!");
        }
        /// <summary>
        /// Save setting
        /// configure env if it's not configured.
        /// </summary>
        public static void save()
        {
            Log.log("Saving");
            if (!Directory.Exists(ROOT))
            {
                Directory.CreateDirectory(ROOT);
                Log.log("Environment configured");
            }
            File.WriteAllText(PATH, "{\n" +
                    "    \"allowShare\":"+Sharing.ToString()+"\n" +
                    "    \"iam\":\""+Iam+"\"" +
                    "}");
            Log.log("Saved!");
        }
    }
}
