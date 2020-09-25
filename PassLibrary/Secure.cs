using System;
using System.Data;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PassLibrary
{
    public class Secure
    {
        public Secure(string Key)
        {
            KEY = Key;
        }
        public Secure() {}
        private string KEY { get; set; }
        private string IV;
        public string Key
        {
            get
            {
                return KEY;
            }
            set
            {
                KEY = GetRandomText(Int32.Parse(value));
            }
        }
        public void SetIV(string iv)
        {
            IV = iv;
        }
        private string GetRandomText(int len)
        {
            Random random = new Random();
            byte[] key = new byte[len];
            random.NextBytes(key);
            return Bytes.ADTS(key);
        }
        private RijndaelManaged InitializeAES()
        {
            RijndaelManaged aes = new RijndaelManaged
            {
                KeySize = 256,
                BlockSize = 256,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                Key = Encoding.ASCII.GetBytes(Key),
                IV = Encoding.ASCII.GetBytes(IV)
            };
            return aes;
        }
        public string AES256Encrypt(string msg)
        {
            RijndaelManaged aes = InitializeAES();
            var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] buf = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                {
                    byte[] xXml = Encoding.UTF8.GetBytes(msg);
                    cs.Write(xXml, 0, xXml.Length);
                }
                buf = ms.ToArray();
            }
            string Output = Convert.ToBase64String(buf);
            return Output;
        }
        public string AES256Decrypt(string msg)
        {
            RijndaelManaged aes = InitializeAES();
            var decrypt = aes.CreateDecryptor();
            byte[] buf = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                {
                    byte[] xXml = Convert.FromBase64String(msg);
                    cs.Write(xXml, 0, xXml.Length);
                }
                buf = ms.ToArray();
            }
            string Output = Encoding.UTF8.GetString(buf);
            return Output;
        }
        public byte[] AES256Encrypt(byte[] msg)
        {
            RijndaelManaged aes = new RijndaelManaged();
            var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] buf = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                {
                    cs.Write(msg, 0, msg.Length);
                }
                buf = ms.ToArray();
            }
            return buf;
        }
        public byte[] AES256Decrypt(byte[] msg)
        {
            RijndaelManaged aes = new RijndaelManaged();
            var decrypt = aes.CreateDecryptor();
            byte[] buf = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                {
                    cs.Write(msg, 0, msg.Length);
                }
                buf = ms.ToArray();
            }
            return buf;
        }
        public class RSASystem
        {
            private string PriKey { get; set; }
            public string PubKey { get; }
            public RSASystem()
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                //Public Key
                RSAParameters privateKey = RSA.Create().ExportParameters(true);
                rsa.ImportParameters(privateKey);
                PriKey = rsa.ToXmlString(true);
                //Private Key
                RSAParameters publicKey = new RSAParameters
                {
                    Modulus = privateKey.Modulus,
                    Exponent = privateKey.Exponent
                };
                rsa.ImportParameters(publicKey);
                PubKey = rsa.ToXmlString(false);
            }
            public RSASystem(string pubKey)
            {
                this.PubKey = pubKey;
            }
            public string Encrypt(string plainText)
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(PubKey);

                byte[] inbuf = (new UTF8Encoding()).GetBytes(plainText);

                byte[] encbuf = rsa.Encrypt(inbuf, false);

                return System.Convert.ToBase64String(encbuf);
            }
            public string RSADecrypt(string encryptedText)
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(PriKey);

                byte[] srcbuf = System.Convert.FromBase64String(encryptedText);

                byte[] decbuf = rsa.Decrypt(srcbuf, false);

                string sDec = (new UTF8Encoding()).GetString(decbuf, 0, decbuf.Length);
                return sDec;
            }

        }
    }
}
