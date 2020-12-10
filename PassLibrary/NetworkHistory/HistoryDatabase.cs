using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Resources;
using System.Windows.Documents;

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
                                "type BIGINTEGER NOT NULL," +
                                "time TEXT NOT NULL," +
                                "comment TEXT NOT NULL," +
                                "id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT);");
            }
        }
        public void AddHistory(Record data)
        {
            SQLiteTransaction transaction = connection.BeginTransaction();
            SQLiteCommand command = new SQLiteCommand("INSERT INTO history (ip_addr, mac_addr, type, time, comment) VALUES (@ip, @mac, @type, @time, @comment);", connection);
            command.Parameters.AddWithValue("@ip", data.Ip);
            command.Parameters.AddWithValue("@mac", data.Mac);
            command.Parameters.AddWithValue("@type", (int)data.Type);
            command.Parameters.AddWithValue("@time", data.time.ToString("yyyy.MM.dd.HH.mm.ss.fff"));
            command.Parameters.AddWithValue("@comment", data.Comment);
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        public List<Record> ReadHistory()
        {
            List<Record> ret = new List<Record>();
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM history ORDER BY id DESC;", connection);
            SQLiteDataReader reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                ret.Add(new Record((COMMU_TYPE)(long)reader["type"], (string)reader["ip_addr"], (string)reader["mac_addr"],
                    (string)reader["comment"], (string)reader["time"]));
            }
            return ret;
        }
        public void DeleteAll()
        {
            ExecuteNonquery("DELETE FROM history");
        }
        private int ExecuteNonquery(string command)
        {
            SQLiteTransaction transaction = connection.BeginTransaction();
            SQLiteCommand cmd = new SQLiteCommand(command, connection);
            transaction.Commit();
            return cmd.ExecuteNonQuery();
        }
        public static ResourceManager Rm { get; set; }
        public static string EnumToString(COMMU_TYPE type)
        {
            return Rm.GetString("commu-type-" + type.ToString());
        }
    }
}
