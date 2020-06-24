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

namespace PassLibrary
{
    public class Internet
    {
        private const int PING_PORT = 24689;
        private const int CONTROL_PORT = 24690;
        private const string VERSION = "1.0.0";
        public string myIP = "-1";
        private ResourceManager rm;
        public Internet(ResourceManager rm)
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
            this.rm = rm;
        }
        /// <summary>
        /// Send ping to local network machine  through broatcasting
        /// </summary>
        /// <returns>return transferable IP addresses with list</returns>
        [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
        public List<string> scanLocalIp()
        {
            List<string> okIp = new List<string>();
            Log.log("Scanning private network");
            Socket updSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            updSocket.EnableBroadcast = true;
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
                        Action<Socket> response = delegate (Socket socket)
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
                            else
                            {
                                Log.log("Denied: " + ((IPEndPoint)socket.RemoteEndPoint).Address.ToString() + " msg=\"" + r_msg + '\"');
                            }
                        };
                        Task.Run(() => response(respon));
                        Thread.Sleep(1000);
                        respon.Close();
                    } catch(ThreadAbortException)
                    {
                        Log.log("Abort Replies receiving");
                        return;
                    } catch(SocketException)
                    {
                        return;
                    }
                }
            });
            receiving.Start();
            receiving.Join(5000);
            avls.Close();
            Log.log("Searching finished");
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
                string[] div = code.Split(new char[] { '+' });
                if(div[0].Equals("Hi"))
                {
                    //Prepare to receive to sender's server.
                    TcpClient cli = new TcpClient(origin, PING_PORT);
                    NetworkStream stream = cli.GetStream();
                    if (Setting.Sharing && div[1].Equals(VERSION))
                    {
                        byte[] tosend = ASCIIEncoding.ASCII.GetBytes("love");
                        stream.Write(tosend, 0, tosend.Length);
                        Log.log("Accept: " + endPoint.Address.ToString());
                    }
                    else
                    {
                        if(!div[1].Equals(VERSION))
                        {
                            byte[] tosend = ASCIIEncoding.ASCII.GetBytes("vers");
                            stream.Write(tosend, 0, tosend.Length);
                            Log.log("Version Mismatch: " + endPoint.Address.ToString());
                        }
                        else
                        {
                            byte[] tosend = ASCIIEncoding.ASCII.GetBytes("hate");
                            stream.Write(tosend, 0, tosend.Length);
                            Log.log("Deny: " + endPoint.Address.ToString());
                        }
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
        private String receive(Socket s)
        {
            byte[] buf = new byte[s.ReceiveBufferSize];
            s.Receive(buf);
            String sx = Bytes.ADTS(buf);
            StringBuilder bid = new StringBuilder();
            foreach (char x in sx.ToCharArray())
            {
                if (x == 0x1a) break;
                else bid.Append(x);
            }
            //Log.log("received \"" + bid.ToString() + "\"");
            return bid.ToString();
        }
        private void send(String msg, Socket s)
        {
            if (msg.Length >= 8192)
            {
                return;
            }
            String mmsg = msg + '';
            byte[] toSend = new byte[s.ReceiveBufferSize];
            toSend = Bytes.AETB(mmsg);
            //Log.log("Send \""+mmsg+"\"");
            s.Send(toSend);
        }
        private String receive(Socket s, Secure secure)
        {
            byte[] buf = new byte[s.ReceiveBufferSize];
            s.Receive(buf);
            String sx = Bytes.ADTS(buf);
            StringBuilder bid = new StringBuilder();
            foreach (char x in sx.ToCharArray())
            {
                if (x == 0x1a) break;
                else bid.Append(x);
            }
            String sxe = bid.ToString();
            String bidx = secure.AES256Decrypt(sxe);
            //Log.log("received \"" + bidx + "\"");
            return bidx;
        }
        private void send(String msg, Socket s, Secure secure)
        {
            if (msg.Length >= 8192)
            {
                return;
            }
            String enc = secure.AES256Encrypt(msg)+ "";
            byte[] toSend = new byte[s.ReceiveBufferSize];
            toSend = Bytes.AETB(enc);
            //Log.log("Send \""+toSend+"\"");
            s.Send(toSend);
        }
        public void mainServer()
        {
            try
            {
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint iPEnd = new IPEndPoint(IPAddress.Any, CONTROL_PORT);
                server.Bind(iPEnd);
                server.Listen(10);
                Log.log("Server Listening");
                while (true)
                {
                    Socket socket = server.Accept();
                    Action<Socket> serve = delegate (Socket sock)
                    {
                        string currentIP = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
                        Log.log("New connection: " + currentIP);
                        //HAND SHAKE
                        String msg;
                        msg = receive(sock);
                        if(msg.Equals("handshake")) // Share request
                        {
                            if(!Setting.Sharing)
                            {
                                send("{\"reply\":\"deny\",\"reason\":\"notSharing\"}", sock);
                                Log.log("Denied request: You're currently not sharing");
                                MessageBox.Show("notSharing", "Pass", MessageBoxButton.OK);
                                return;
                            }
                            else
                            {
                                send("{\"reply\":\"approve\"}", sock);
                                Log.log("Approved: "+((IPEndPoint)sock.RemoteEndPoint).Address.ToString());
                            }
                            Log.log("Preparing");
                            //Security
                            String AES;
                            Secure.RSASystem rsa = new Secure.RSASystem();
                            send(rsa.PubKey, sock);
                            Log.log("Sent RSA public key");
                            AES = rsa.RSADecrypt(receive(sock));
                            Secure secure = new Secure(AES);
                            Log.log("AES key was replied");
                            JObject json = JObject.Parse(receive(sock, secure));
                            Log.log("File Info Received");
                            if(!json.Value<String>("reply").Equals("OK"))
                            {
                                Log.log("Not OK!");
                                MessageBox.Show(rm.GetString(json.Value<String>("reply")), "Pass", MessageBoxButton.OK);
                                return;
                            }
                            Log.log("File info displayed");
                            Int64 bytes = Int64.Parse(json.Value<String>("size"));
                            String unit = " Byte";
                            double bas;
                            //Byte to suitable unit
                            if(bytes>1024)
                            {
                                if(bytes>1024000)
                                {
                                    if(bytes>1024000000)
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
                            bas = Math.Round(bas*100)/100;
                            MessageBoxResult res = MessageBox.Show(((IPEndPoint)sock.RemoteEndPoint).Address.ToString() +
                                rm.GetString("appv1") + json.Value<String>("name") + rm.GetString("appv2") +
                                " (" + bas + unit+')', "Pass", MessageBoxButton.OKCancel);
                        }
                        else if(msg.Equals("ping"))
                        {

                        }
                        else
                        {
                            Log.log("Invalid Message: \"" + msg+"\"");
                        }
                    };
                    Task.Run(() => serve(socket));
                }
            }
            catch(Exception e)
            {
                Log.log("Server Stopped: " + e.Message);
                return;
            }
            
        }
        public static String reason;
        public static int SUCESS = 0;
        public static int DENIED = 1;
        public static int ERROR = 2;
        public int wannaSendTo(string ip, string path)
        {
            Log.log("Connecting to " + ip);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint iPEnd = new IPEndPoint(IPAddress.Parse(ip), CONTROL_PORT);
            socket.Connect(iPEnd);
            send("handshake", socket);
            String reply = receive(socket);
            var j1 = JObject.Parse(reply);
            if(j1.Value<String>("reply").Equals("deny"))
            {
                Log.log("Denied: "+j1.Value<String>("reason"));
                reason = j1.Value<String>("reason").ToString();
                return DENIED;
            }
            else if(!j1.Value<String>("reply").Equals("approve"))
            {
                Log.log("Invalid message: " + reply);
                reason = "invalid";
                return ERROR;
            }
            Log.log("RSA public key was received");
            String pubKey = receive(socket);
            Secure.RSASystem rsa = new Secure.RSASystem(pubKey);
            Secure secure = new Secure();
            secure.Key = "32";
            send(rsa.encrypt(secure.Key), socket);
            Log.log("Replied AES key");
            //Get Allow
            JObject pack = new JObject();
            if(!File.Exists(path))
            {
                Log.log("File not exists: "+path);
                pack.Add("reply", "FileErr");
                send(pack.ToString(), socket, secure);
                reason = "FileNotFound";
                return ERROR;
            }
            FileInfo file = new FileInfo(path);
            pack.Add("reply", "OK");
            pack.Add("name", file.Name);
            pack.Add("size", file.Length);
            Log.log("File data: "+file.Name+" "+file.Length+" Bytes");
            send(pack.ToString(), socket, secure);
            return SUCESS;
        }
        public void serverStart()
        {
            Task.Run(() =>
            {
                mainServer();
            });
        }
    }
}
