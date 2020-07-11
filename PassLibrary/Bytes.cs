using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassLibrary
{
    public class Bytes
    {
        public static string ADTS(byte[] data)
        {
            return Encoding.ASCII.GetString(data);
        }
        public static byte[] AETB(string data)
        {
            return Encoding.ASCII.GetBytes(data);
        }
        public static string UDTS(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
        public static byte[] UETB(string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }
        public static string getRawString(byte[] data)
        {
            return BitConverter.ToString(data).Replace(" ", string.Empty);
        }
    }
}
