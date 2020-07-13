using System.Resources;
using PassLibrary;

namespace Pass
{
    class Support
    {
        public const string NONE_VISIBLE = "none-visible";

        private ResourceManager rm;
        public void setResourceManager(ResourceManager rm)
        {
            this.rm = rm;
        }
        public void go(string type)
        {
            string lang = rm.GetString("LANG_CODE");
            string URL = "https://supportpass.netlify.app/" + lang + '/' + type;
            Log.log("OpenSupport: " + URL);
            System.Diagnostics.Process.Start(URL);
        }
    }
}
