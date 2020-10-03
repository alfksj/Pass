using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PassLibrary.NetworkHistory
{
    public partial class NetworkHistory
    {
        private readonly List<Record> history = new List<Record>();
        public void RegisterHistory(COMMU_TYPE type, string ip, string comment)
        {
            //list append
            history.Add(new Record(type, ip, comment));
            //database commit
            AddHistory(history.ElementAt(history.Count-1));
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
        public string Comment { get; set; }
        public DateTime time;
        public Record(COMMU_TYPE Type, string ip, string comment)
        {
            this.Type = Type;
            Ip = ip;
            Comment = comment;
            Mac = Internet.GetMacFromIp(ip);
            time = DateTime.Now;
        }
        public Record(COMMU_TYPE Type, string ip, string mac, string comment, string time)
        {
            this.Type = Type;
            Ip = ip;
            Mac = mac;
            Comment = comment;
            this.time = DateTime.ParseExact(time, "yyyy.MM.dd.HH.mm.ss.fff", CultureInfo.InvariantCulture);
        }
    }
}
