using System;

namespace PassLibrary
{
    public class Log
    {
        public const int INFO = 0, WARN = 1, ERR = 2, FATAL = 3;
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
        public static void serverLog(string msg)
        {
            log("SERVER > " + msg);
        }
        public static void clientLog(string msg)
        {
            log("CLIENT > " + msg);
        }
        public static void serverLog(string msg, int code)
        {
            log("SERVER > " + msg, code);
        }
        public static void clientLog(string msg, int code)
        {
            log("CLIENT > " + msg, code);
        }
    }
}
