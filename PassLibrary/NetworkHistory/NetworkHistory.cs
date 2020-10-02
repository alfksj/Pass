using System;
using System.Collections.Generic;

namespace PassLibrary.NetworkHistory
{
    public partial class NetworkHistory
    {
        private static readonly List<Record> history = new List<Record>();
        public static void RegisterHistory(COMMU_TYPE type, string ip)
        {
            //database commit

            //list append
            history.Add(new Record(type, ip));
        }
    }
    public enum COMMU_TYPE
    {
        PING,
        PING_REPLY,
        INCOMMING_SHARE,
        OUTGOING_SHARE
    };
    public class Record
    {
        public COMMU_TYPE Type { get; set; }
        public string Ip { get; set; }
        public string Mac { get; set; }
        public DateTime time;
        public Record(COMMU_TYPE Type, string ip)
        {
            this.Type = Type;
            Ip = ip;
            Mac = Internet.GetMacFromIp(ip);
            time = DateTime.Now;
        }
    }
}
