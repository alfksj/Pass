using System;
using System.Windows.Controls;

namespace PassLibrary
{
    public class Log
    {
        private static Action<string> logger;
        public static void setLogVisualizer(Action<string> setter)
        {
            logger = setter;
        }
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
            logger.Invoke(msg);
            log("SERVER > " + msg);
        }
        public static void clientLog(string msg)
        {
            logger.Invoke(msg);
            log("CLIENT > " + msg);
        }
        public static void serverLog(string msg, int code)
        {
            logger.Invoke(msg);
            log("SERVER > " + msg, code);
        }
        public static void clientLog(string msg, int code)
        {
            logger.Invoke(msg);
            log("CLIENT > " + msg, code);
        }
        public static void pingSender(string msg)
        {
            logger.Invoke(msg);
            log("PingSender > " + msg);
        }
        public static void pingReceiver(string msg)
        {
            logger.Invoke(msg);
            log("PingReceiver > " + msg);
        }
        public static void pingSender(string msg, int code)
        {
            logger.Invoke(msg);
            log("PingSender > " + msg, code);
        }
        public static void pingReceiver(string msg, int code)
        {
            logger.Invoke(msg);
            log("PingReceiver > " + msg, code);
        }
    }
}
