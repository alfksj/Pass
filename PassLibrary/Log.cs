using System;

namespace PassLibrary
{
    public class Log
    {
        /// <summary>
        /// make log
        /// </summary>
        public static void log(string msg, int code)
        {
            string now = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
            string stat = null;
            switch (code)
            {
                case 0:
                    stat = "INFO";
                    break;
                case 1:
                    stat = "WARN";
                    break;
                case 2:
                    stat = "ERR";
                    break;
                case 3:
                    stat = "FATAL";
                    break;
            }
            Console.WriteLine(now + " |" + stat + "| " + msg);
        }
        /// <summary>
        /// make info log
        /// </summary>
        /// <param name="msg"></param>
        public static void log(string msg)
        {
            string now = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
            Console.WriteLine(now + " |INFO| " + msg);
        }
    }
}
