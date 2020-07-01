using System.Resources;
using System.Threading;

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
            string URL = "" + lang + '/' + type;

        }
    }
}
