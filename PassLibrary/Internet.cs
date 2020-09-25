using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Windows;
using System.Threading.Tasks;
using System.Resources;
using PassLibrary.Box;
using System.Windows.Forms;
using System.Security.Cryptography;
using MessageBox = System.Windows.MessageBox;
using System.Diagnostics.Contracts;

namespace PassLibrary
{
    public class Internet
    {
        private const int PING_PORT = 24689;
        private const int CONTROL_PORT = 24690;
        private const string VERSION = "1.1.0";
        public string myIP = "-1";
        public List<Response> lastResponse = new List<Response>();
        private ResourceManager Rm { get; set; }
        public Internet()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    myIP = ip.ToString();
                    break;
                }
            }
        }
        /// <summary>
        /// Get local ip address
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapter with an IPv4 address in this system!");
        }
        /// <summary>
        /// Send ping to local network machine  through broatcasting
        /// </summary>
        /// <returns>return transferable IP addresses with list</returns>
        [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
        public List<string> ScanLocalIp()
        {
            determined.Invoke(true);
            List<string> okIp = new List<string>();
            Log.log("Scanning private network");
            Socket updSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                EnableBroadcast = true
            };
            byte[] sendbuf = Encoding.ASCII.GetBytes("Hi+"+VERSION);
            EndPoint targetPoint = new IPEndPoint(IPAddress.Broadcast, PING_PORT);
            updSocket.SendTo(sendbuf, targetPoint);
            Log.log("Sent broadcasting ping");
            updSocket.Close();
            Socket avls = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, PING_PORT);
            avls.Bind(serverEP);
            avls.Listen(10);
            Thread receiving = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        Socket respon = avls.Accept();
                        void response(Socket socket)
                        {
                            byte[] msg = new byte[4];
                            int flg = socket.Receive(msg, 4, SocketFlags.None);
                            string r_msg = ASCIIEncoding.ASCII.GetString(msg);
                            if (r_msg.Equals("love"))
                            {
                                Log.log("Accepted: " + ((IPEndPoint)socket.RemoteEndPoint).Address.ToString() + " msg=\"" + r_msg + '\"');
                                okIp.Add(((IPEndPoint)socket.RemoteEndPoint).Address.ToString());
                            }
                            else if (r_msg.Equals("vers"))
                            {
                                Log.log("Version Mismatched: " + ((IPEndPoint)socket.RemoteEndPoint).Address.ToString() + " msg=\"" + r_msg + '\"');
                            }
                            else if (r_msg.Equals("self"))
                            {
                                Log.log("Myself: " + ((IPEndPoint)socket.RemoteEndPoint).Address.ToString() + " msg=\"" + r_msg + '\"');
                            }
                            else
                            {
                                Log.log("Denied: " + ((IPEndPoint)socket.RemoteEndPoint).Address.ToString() + " msg=\"" + r_msg + '\"');
                            }
                            lastResponse.Add(new Response()
                            {
                                IP = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString(),
                                response = r_msg
                            });
                        }
                        Task.Run(() => response(respon));
                        Thread.Sleep(1000);
                        respon.Close();
                    } catch(ThreadAbortException)
                    {
                        Log.log("Abort Replies receiving");
                        determined.Invoke(false);
                        return;
                    } catch(SocketException)
                    {
                        determined.Invoke(false);
                        return;
                    } catch(ThreadInterruptedException)
                    {
                        return;
                    }
                }
            });
            receiving.Start();
            receiving.Join(Setting.pingTimeout);
            avls.Close();
            Log.log("Searching finished");
            determined.Invoke(false);
            return okIp;
        }
        /// <summary>
        /// Initialize ping receiver.
        /// </summary>
        public void PingReceiver()
        {
            Log.log("Ping Receiver Started");
            while(true)
            {
                UdpClient pinger = new UdpClient(PING_PORT);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, PING_PORT);
                byte[] received = pinger.Receive(ref endPoint);
                string origin = endPoint.Address.ToString();
                pinger.Close();
                string code = ASCIIEncoding.ASCII.GetString(received);
                Log.log("Ping received!");
                if(Setting.stealthMode)
                {
                    Log.log("You're currently in stealth mode. Ignore ping.");
                    continue;
                }
                string[] div = code.Split(new char[] { '+' });
                if(div[0].Equals("Hi"))
                {
                    //Prepare to receive to sender's server.
                    TcpClient cli = new TcpClient(origin, PING_PORT);
                    NetworkStream stream = cli.GetStream();
                    if(((IPEndPoint)cli.Client.RemoteEndPoint).Address.ToString().Equals(myIP))
                    {
                        byte[] tosend = ASCIIEncoding.ASCII.GetBytes("self");
                        stream.Write(tosend, 0, tosend.Length);
                        Log.log("Ping from me myself: " + endPoint.Address.ToString());
                    }
                    else if(!div[1].Equals(VERSION))
                    {
                        byte[] tosend = ASCIIEncoding.ASCII.GetBytes("vers");
                        stream.Write(tosend, 0, tosend.Length);
                        Log.log("Version Mismatch: " + endPoint.Address.ToString());
                    }
                    else if(!Setting.sharing)
                    {
                        byte[] tosend = ASCIIEncoding.ASCII.GetBytes("hate");
                        stream.Write(tosend, 0, tosend.Length);
                        Log.log("Deny: " + endPoint.Address.ToString());
                    }
                    else
                    {
                        byte[] tosend = ASCIIEncoding.ASCII.GetBytes("love");
                        stream.Write(tosend, 0, tosend.Length);
                        Log.log("Accept: " + endPoint.Address.ToString());
                    }
                    stream.Flush();
                    stream.Close();
                    cli.Close();
                }
                else
                {
                    Log.log("Invalid ping message: " + code, 2);
                }
            }
        }
        private Socket server;
        private string Receive(Socket s)
        {
            byte[] buf = new byte[s.ReceiveBufferSize];
            s.Receive(buf);
            string sx = Bytes.ADTS(buf);
            StringBuilder bid = new StringBuilder();
            foreach (char x in sx.ToCharArray())
            {
                if (x == 0x1a) break;
                else bid.Append(x);
            }
//Log.log("received \"" + bid.ToString() + "\"");
            return bid.ToString();
        }
        private void Send(string msg, Socket s)
        {
            /*
            if (msg.Length >= 8192)
            {
                Log.log("Maximum sending size over.", Log.ERR);
                return;
            }
            */
            string mmsg = msg + '';
            byte[] toSend;
            toSend = Bytes.AETB(mmsg);
//Log.log("Send \"" + Bytes.getRawString(toSend) + "\"");
            s.Send(toSend);
        }
        private string Receive(Socket s, Secure secure)
        {
            byte[] buf = new byte[s.ReceiveBufferSize];
            s.Receive(buf);
            string sx = Bytes.ADTS(buf);
            StringBuilder bid = new StringBuilder();
            foreach (char x in sx.ToCharArray())
            {
                if (x == 0x1a) break;
                else bid.Append(x);
            }
            string sxe = bid.ToString();
            string bidx = secure.AES256Decrypt(sxe);
//Log.log("received \"" + bidx + "\"");
            return bidx;
        }
        private void Send(string msg, Socket s, Secure secure)
        {
            if (msg.Length >= 8192)
            {
                return;
            }
            string enc = secure.AES256Encrypt(msg)+ "";
//Log.log("Send \""+ Bytes.getRawString(Bytes.AETB(enc)) + "\"");
            s.Send(Bytes.AETB(enc));
        }
        private Action<bool> frameControl;
        public void MainServer()
        {
            try
            {
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint iPEnd = new IPEndPoint(IPAddress.Any, CONTROL_PORT);
                server.Bind(iPEnd);
                server.Listen(10);
                Log.serverLog("Server Listening");
                while (true)
                {
                    Socket socket = server.Accept();
                    void serve(Socket sock)
                    {
                        sock.SendBufferSize = 262144;
                        sock.ReceiveBufferSize = 262144;
                        string currentIP = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
                        Log.serverLog("New connection: " + currentIP);
                        //HAND SHAKE
                        string msg;
                        msg = Receive(sock);
                        if (msg.Substring(0, 9).Equals("handshake")) // Share request
                        {
                            if (!Setting.sharing)
                            {
                                Send("{\"reply\":\"deny\",\"reason\":\"notSharing\"}", sock);
                                Log.serverLog("Denied request: You're currently not sharing", Log.WARN);
                                MessageBox.Show("notSharing", "Pass", MessageBoxButton.OK);
                                return;
                            }
                            if (!msg.Substring(9, msg.Length - 9).Equals(VERSION))
                            {
                                Send("{\"reply\":\"deny\",\"reason\":\"VersionMismatch\"}", sock);
                                Log.serverLog("Denied request: FROM" + msg.Substring(9, msg.Length - 9) + " You're: " + VERSION, Log.WARN);
                                MessageBox.Show("versMismatch", "Pass", MessageBoxButton.OK);
                                return;
                            }
                            else
                            {
                                Send("{\"reply\":\"approve\"}", sock);
                                Log.serverLog("Approved: " + ((IPEndPoint)sock.RemoteEndPoint).Address.ToString(), Log.WARN);
                            }
                            Log.serverLog("Preparing");
                            //Security
                            string AES;
                            Secure.RSASystem rsa = new Secure.RSASystem();
                            Send(rsa.PubKey, sock);
                            Log.serverLog("Sent RSA public key");
                            //get AES key
                            AES = rsa.RSADecrypt(Receive(sock));
                            Secure secure = new Secure(AES);
                            Log.serverLog("AES key was replied");
                            //Set IV
                            string ivx = rsa.RSADecrypt(Receive(sock));
                            secure.SetIV(ivx);
                            Log.serverLog("IV value was received");
                            //File Go.
                            JObject json = JObject.Parse(Receive(sock, secure));
                            Log.serverLog("File Info Received");
                            if (!json.Value<string>("reply").Equals("OK"))
                            {
                                Log.serverLog("Not OK!", Log.ERR);
                                MessageBox.Show(Rm.GetString(json.Value<string>("reply")), "Pass", MessageBoxButton.OK);
                                return;
                            }
                            Log.serverLog("File info displayed");
                            Int64 bytes = Int64.Parse(json.Value<string>("size"));
                            string unit = " Byte";
                            double bas;
                            //Byte to suitable unit
                            if (bytes > 1024)
                            {
                                if (bytes > 1024000)
                                {
                                    if (bytes > 1024000000)
                                    {
                                        unit = "GB";
                                        bas = bytes / 1000000000.0;
                                    }
                                    else
                                    {
                                        unit = "MB";
                                        bas = bytes / 1000000.0;
                                    }
                                }
                                else
                                {
                                    unit = "KB";
                                    bas = bytes / 1000.0;
                                }
                            }
                            else
                            {
                                bas = bytes;
                            }
                            bas = Math.Round(bas * 100) / 100;
                            frameControl.Invoke(true);
                            DialogResult result = MessageBoxClass.Show(((IPEndPoint)sock.RemoteEndPoint).Address.ToString() +
                                Rm.GetString("appv1") + json.Value<string>("name") + Rm.GetString("appv2") +
                                " (" + bas + unit + ')', "Pass", Rm.GetString("allow"), Rm.GetString("deny"));
                            if (result == DialogResult.Yes)
                            {
                                Send("approve", sock, secure);
                            }
                            else
                            {
                                Send("deny", sock, secure);
                                frameControl.Invoke(false);
                                return;
                            }
                            string hashValue = Receive(sock, secure);
                            Log.serverLog("Received hash");
                            string targetPath = Setting.defaultSave + '\\' + json.Value<string>("name");
                            Log.serverLog("Write to " + targetPath);
                            //file download start;
                            Log.serverLog("Downloading started");
                            MD5 md5 = MD5.Create();
                            int packets = 0;
                            FileStream stream = File.OpenWrite(targetPath);
                            Log.serverLog("Writing stream connected");
                            while (true)
                            {
                                //Log.serverLog("Waiting for data piece");
                                string dataSegment = Receive(sock);
                                JObject data = JObject.Parse(dataSegment);
                                string segment = (String)data.GetValue("segment");
                                string hash = (String)data.GetValue("hash");
                                bool isEnd = (bool)data.GetValue("isEnd");
                                byte[] finalSegment = secure.AES256Decrypt(Convert.FromBase64String(segment));

                                //Log.serverLog("Got data piece: " + hash);

                                string hashed = Bytes.getRawString(md5.ComputeHash(finalSegment));
                                if (hash.Equals(hashed))
                                {
                                    Send("ok", sock, secure);
                                    //Log.serverLog("Wrote " + finalSegment.Length + " bytes");
                                    stream.Write(finalSegment, 0, finalSegment.Length);
                                    packets++;
                                    if (isEnd) break;
                                }
                                else
                                {
                                    Send("retry", sock, secure);
                                    Log.serverLog("HASH MISMATCH! requested resend(Hash=" + hashed + ')', Log.ERR);
                                }
                            }
                            stream.Close();
                            //Hash check
                            byte[] hashValueCheck;
                            string stringHashValue;
                            Log.serverLog("Hash check in progress");
                            using (FileStream hashStream = File.OpenRead(targetPath))
                            {
                                hashValueCheck = md5.ComputeHash(hashStream);
                                stringHashValue = Bytes.getRawString(hashValueCheck);
                            }
                            if (!hashValue.Equals(stringHashValue))
                            {
                                Log.serverLog("FILE HASH MISMATCH! \"" + stringHashValue + "\" vs. n\"" + hashValue + '\"');
                                Send("hash", sock, secure);
                            }
                            else
                            {
                                Log.serverLog("FILE HASH MATCH! " + stringHashValue);
                                Send("goodbye", sock, secure);
                            }
                            Log.serverLog("Done: received " + packets + " file pieces");
                        }
                        else if (msg.Equals("ping"))
                        {

                        }
                        else
                        {
                            Log.serverLog("Invalid Message: \"" + msg + "\"");
                        }
                        sock.Close();
                    }
                    Task.Run(() => serve(socket));
                }
            }
            catch(Exception e)
            {
                Log.serverLog("Server Stopped: " + e.Message);
                return;
            }
            
        }
        private Action<bool> determined;
        private Action<int> initializer;
        private Action adder;
        public void SetFunction(Action<bool> determined, Action<int> initializer, Action adder, Action<bool> FrameControl)
        {
            this.determined = determined;
            this.initializer = initializer;
            this.adder = adder;
            this.frameControl = FrameControl;
        }
        public static string reason;
        public static int SUCESS = 0;
        public static int DENIED = 1;
        public static int ERROR = 2;
        public int WannaSendTo(string ip, string path)
        {
            try
            {
                determined.Invoke(true);
                Log.clientLog("Connecting to " + ip);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint iPEnd = new IPEndPoint(IPAddress.Parse(ip), CONTROL_PORT);
                socket.Connect(iPEnd);
                socket.SendBufferSize = 262144;
                socket.ReceiveBufferSize = 262144;
                Send("handshake" + VERSION, socket);
                string reply = Receive(socket);
                var j1 = JObject.Parse(reply);
                if (j1.Value<string>("reply").Equals("deny"))
                {
                    Log.clientLog("Denied: " + j1.Value<string>("reason"), Log.WARN);
                    reason = j1.Value<string>("reason").ToString();
                    determined.Invoke(false);
                    return DENIED;
                }
                else if (!j1.Value<string>("reply").Equals("approve"))
                {
                    Log.clientLog("Invalid message: " + reply, Log.ERR);
                    reason = "invalid: "+reply;
                    determined.Invoke(false);
                    return ERROR;
                }
                Log.clientLog("RSA public key was received");
                string pubKey = Receive(socket);
                Secure.RSASystem rsa = new Secure.RSASystem(pubKey);
                Secure secure = new Secure
                {
                    Key = "32"
                };
                Send(rsa.Encrypt(secure.Key), socket);
                Log.clientLog("Replied AES key");
                //get IV
                Send(rsa.Encrypt(Setting.Actual_IV), socket);
                secure.SetIV(Setting.Actual_IV);
                Log.clientLog("IV was sent");
                //Get Allow
                JObject pack = new JObject();
                if (!File.Exists(path))
                {
                    Log.clientLog("File not exists(or it is directory): " + path, Log.ERR);
                    pack.Add("reply", "FileErr");
                    Send(pack.ToString(), socket, secure);
                    reason = "FileNotFound";
                    determined.Invoke(false);
                    return ERROR;
                }
                FileInfo file = new FileInfo(path);
                pack.Add("reply", "OK");
                pack.Add("name", file.Name);
                pack.Add("size", file.Length);
                Log.clientLog("File data: " + file.Name + " " + file.Length + " Bytes");
                Send(pack.ToString(), socket, secure);
                string received = Receive(socket, secure);
                if (received.Equals("deny"))
                {
                    Log.clientLog("User denied", Log.WARN);
                    reason = "Peer denied";
                    determined.Invoke(false);
                    return DENIED;
                }
                else if (!received.Equals("approve"))
                {
                    Log.clientLog("User invalid reply", Log.ERR);
                    reason = "invalid: " + received;
                    determined.Invoke(false);
                    return ERROR;
                }
                Log.clientLog("Received Allow Sign!");
                //Get hash and send: MD5;
                Log.clientLog("Computing hash");
                byte[] hashValue;
                string stringHashValue;
                MD5 hash = MD5.Create();
                using (FileStream stream = File.OpenRead(file.FullName))
                {
                    hashValue = hash.ComputeHash(stream);
                    stringHashValue = Bytes.getRawString(hashValue);
                }
                Log.clientLog("Sending hash: "+stringHashValue);
                Send(stringHashValue, socket, secure);
                //Send start;
                Log.clientLog("Local file stream connected");
                FileStream fileStream = file.OpenRead();
                //const int sendingBufferSize = 320;  //320B
                const int sendingBufferSize = 131072; //128KB
                                                      //const int sendingBufferSize = 1048576; //1MB
                                                      //const int sendingBufferSize = 524288; //0.5MB
                Log.clientLog("Sending pieces");
                initializer.Invoke((int)((file.Length + (sendingBufferSize - (file.Length % sendingBufferSize))) / sendingBufferSize));
                Log.log((file.Length + (sendingBufferSize - (file.Length % sendingBufferSize))) / sendingBufferSize + " Pieces");
                int packets = 0;
                while (true)
                {
                    byte[] sendingBuffer = new byte[sendingBufferSize];
                    byte[] encryptedSendingBuffer = new byte[sendingBufferSize + 16];
                    int read = fileStream.Read(sendingBuffer, 0, sendingBufferSize);
                    
//Log.clientLog("Read " + read + " bytes");
//Log.clientLog("Encryption started");
                    
                    byte[] fitByteBuffer = new byte[read];
                    Array.Copy(sendingBuffer, fitByteBuffer, read);
                    encryptedSendingBuffer = secure.AES256Encrypt(fitByteBuffer);
                    string thisData = Convert.ToBase64String(encryptedSendingBuffer);
//Log.clientLog("Encryption done");
                    JObject packet = new JObject();
                    string packetHash = Bytes.getRawString(hash.ComputeHash(fitByteBuffer));
                    packet.Add("segment", thisData);
                    packet.Add("hash", packetHash);
                    packet.Add("isEnd", read < sendingBufferSize);
                    thisData = null;
                    encryptedSendingBuffer = null;
                    string replied;
                    int attemp = 0;
                    do
                    {
                        attemp++;
//Log.clientLog("Sending data piece: " + (String)packet.GetValue("hash"));
                        Send(packet.ToString(), socket);
//Log.clientLog("Waiting for response");
                        replied = Receive(socket, secure);
//Log.clientLog("File segment reply: \"" + replied + "\", Attemp: "+attemp);
                    } while (!replied.Equals("ok")); // Retry if hash is not correct
                    adder.Invoke();
                    packets++;
                    if (read < sendingBufferSize) break;
                }
                Log.clientLog("Waiting for hash test result");
                string hashReply = Receive(socket, secure);
                if (hashReply.Equals("hash"))
                {
                    Log.clientLog("Hash Error: originHash=\"" + stringHashValue + '\"');
                }
                else if (!hashReply.Equals("goodbye"))
                {
                    Log.clientLog("Invalid hash reply: " + hashReply);
                    return ERROR;
                }
                socket.Close();
                fileStream.Close();
                Log.clientLog("Done: sent " + packets + " file pieces");
                return SUCESS;
            } catch(Exception e)
            {
                Log.clientLog(e.Message+" client error", Log.ERR);
                return ERROR;
            }
        }
        public Task ServerStart()
        {
            return Task.Run(() =>
            {
                MainServer();
            });
        }
    }
}
