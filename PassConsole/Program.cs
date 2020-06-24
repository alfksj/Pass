using System;
using System.Globalization;
using System.Resources;
using System.Threading;
using PassLibrary;

namespace PassConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ko-KR");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ko-KR");
            ResourceManager rm = new ResourceManager("Pass.Localization", typeof(MainWindow).Assembly);
            Setting.load();
            Internet internet = new Internet(rm);
            internet.serverStart();
            Console.ReadKey();
            internet.wannaSendTo("172.30.1.1", "");
        }
    }
}
