using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassLibrary
{
    public class Bytes
    {
        public static String ADTS(byte[] data)
        {
            return ASCIIEncoding.ASCII.GetString(data);
        }
        public static byte[] AETB(String data)
        {
            return ASCIIEncoding.ASCII.GetBytes(data);
        }
        public static String UDTS(byte[] data)
        {
            return ASCIIEncoding.UTF8.GetString(data);
        }
        public static byte[] UETB(String data)
        {
            return ASCIIEncoding.UTF8.GetBytes(data);
        }
    }
}
