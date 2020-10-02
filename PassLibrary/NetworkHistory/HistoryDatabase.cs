using System;
using System.Data.SQLite;
using System.IO;

namespace PassLibrary.NetworkHistory
{
    public partial class NetworkHistory
    {
        public static string DB_PATH = Environment.GetEnvironmentVariable("appdata") + "\\Pass\\History.db";
        public SQLiteConnection connection = null;
        public void OpenHistory()
        {
            bool flag = false;
            if (!File.Exists(DB_PATH))
            {
                Directory.CreateDirectory(Environment.GetEnvironmentVariable("appdata") + "\\Pass");
                SQLiteConnection.CreateFile(DB_PATH);
                flag = true;
            }
            connection = new SQLiteConnection("Data Source=" + DB_PATH + ";Version=3;");
            connection.Open();
            if(flag)
            {
                ExecuteNonquery("CREATE TABLE history (" +
                                "ip_addr TEXT NOT NULL," +
                                "mac_addr TEXT NOT NULL," +
                                "type INTEGER NOT NULL," +
                                "time TEXT NOT NULL," +
                                "id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT)");
            }
        }
        public void AddHistory(Record data)
        {
            SQLiteCommand command = new SQLiteCommand("INSERT INTO history (ip_addr, mac_addr, type, time) VALUES (@ip, @mac, @type, @time)", connection);
            command.Parameters.AddWithValue("@ip", data.Ip);
            command.Parameters.AddWithValue("@mac", data.Mac);
            command.Parameters.AddWithValue("@type", (int)data.Type);
            command.Parameters.AddWithValue("@time", data.time.ToString("yyyy.MM.dd.HH.mm.ss.fff"));
            command.ExecuteNonQuery();
        }
        private int ExecuteNonquery(string command)
        {
            SQLiteCommand cmd = new SQLiteCommand(command, connection);
            return cmd.ExecuteNonQuery();
        }
    }
}
