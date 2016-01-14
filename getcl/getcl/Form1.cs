using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;

namespace getcl
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Console.WriteLine("\u767b\u9646\u8d85\u65f6\uff0c\u8bf7\u91cd\u65b0\u767b\u9646\u3002");
        }

        private void Form1_Click(object sender, EventArgs e)
        {

        }

        private static int startid = 1200001;
        private static int endid = 1300000;
        private static string clroot = @"f:\xunlei\clsvr\cl " + startid.ToString() + "-" + endid.ToString() +"\\";
        public static string ecdp = "gb2312";
        public static string tics = "";
        public static string clsvr = "wo.yao.cl";
        //public static string clsvr = "t66y.com";
        //public static string clsvr = "cl.flag.fi";
        //public static string clsvr = "cl.yo.fi";
        private void TestBtn_Click(object sender, EventArgs e)
        {
            //for (int n = 0; n < sizeof(Path.GetInvalidFileNameChars()); n++) tics += t;
            foreach (char t in Path.GetInvalidFileNameChars()) tics += t;

            ThreadPool.SetMaxThreads(100, 100);
            //this.textBox2.Text = GetCLCnt(GetCLLocation(this.textBox1.Text));
            //ParsePages(this.textBox2.Text);
            //int ti = System.Convert.ToInt32(textBox1.Text);
            GetRootIDSInfo();
            int ti = startid;
            for (int n = 0; n <= endid - ti; n++)
            {
                if (ids.IndexOf((ti + n).ToString() + ":") >= 0)
                {
                    Console.WriteLine((ti+n).ToString() + " exist...");
                    continue;
                }
                //Console.WriteLine("CurrentID:\t"+(ti+n).ToString());
                //ParsePages(GetCLCnt(GetCLLocation((ti+n).ToString())));
                //this.textBox2.Text = GetCLCnt(GetCLLocation((ti+n).ToString()));
                //ParsePages(this.textBox2.Text);
                ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadProc), (ti + n));
                //Console.WriteLine("Iamsleep...\t" + DateTime.Now.ToString());
                //Thread.Sleep(new TimeSpan(0, 0, 5));
            }
        }

        string ids = "";
        private void GetRootIDSInfo()
        {
            if (!Directory.Exists(clroot)) return;
            foreach (string di in Directory.EnumerateDirectories(clroot))
                if (di.Length > 6+clroot.Length) ids += di.Substring(clroot.Length, 7) + ":";
        }

        static void ThreadProc(Object clid)
        {
            //Console.WriteLine("CurrentID:" + clid.ToString());
            ParsePages(GetCLCnt(GetCLLocation(clid.ToString())));
        }
        
        //------------------------------------------MYSVR-----------------------------------------------------//
        //private static Socket cls = null;
        private static void ParsePages(string page)
        {
            if (page.Length < 256) return;

            string tid = "";
            Regex rgx = new Regex("&tid=[^&\"]*", RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(page);
            if (matches.Count > 0) tid = matches[0].Value.Substring(5);

            Console.WriteLine("CurrentID:" + tid.ToString());

            string title = "";
            rgx = new Regex("\\<title\\>[^<]*", RegexOptions.IgnoreCase);
            matches = rgx.Matches(page);
            if (matches.Count > 0 && matches[0].Value.Length > 38) title = matches[0].Value.Substring(7, matches[0].Value.Length - 37);
            else return;

            string tpath = clroot + tid.ToString() + " ";
            foreach (char t in title) if (tics.IndexOf(t)<0) tpath += t;
            if (!Directory.Exists(tpath)) tpath = Directory.CreateDirectory(tpath).FullName;

            if (File.Exists(tpath + "\\info.html")) return;
            StreamWriter nfs = new StreamWriter(tpath + "\\info.html", false, Encoding.GetEncoding(ecdp));
            nfs.Write(page);

            //string downloadlist = "";
            //rgx = new Regex("input type='image' src='[^']*", RegexOptions.IgnoreCase);
            //matches = rgx.Matches(page);
            //foreach (Match match in matches) downloadlist += match.Value.Substring(24)+"\r\n";

            //StreamWriter tfs = new StreamWriter(tpath + "\\rls.txt");
            //tfs.Write(downloadlist);
            //tfs.Close();

            //StreamWriter tafs = new StreamWriter(clroot + "arls.txt", true);
            //tafs.Write(downloadlist);
            //tafs.Close();
        }

        private static string GetCLLocation(string tid)
        {
            //string cookiet = @"Cookie: 227c9_ck_info=%2F%09; 227c9_winduser=AgoKVVENPVAAUQcDV1EHAwYNAgAMBQULVwNYClUBVlVRVgABBV1e; 227c9_groupid=8; 227c9_lastfid=0; 227c9_lastvisit=0%091334218520%09%2Fread.php%3Ftid%3D712654; cnzz_a950900=43; sin950900=; rtime=2; ltime=1334218495264; cnzz_eid=8376013-1333592780-http%3A//cl.orc.st/";
            string cookiet = @"";
            string request = "GET /read.php?tid=" + tid.ToString() + " HTTP/1.1\r\nHost: " + clsvr + "\r\nUser-Agent: Mozilla/5.0 (Windows NT 5.1)\r\nConnection:keep-alive\r\n" + cookiet + "\r\n\r\n";
            string page = GetPageFromCLSvr(request);
            String[] lines = page.Split(new char[] {'\r', '\n'});
            if (lines.Length != 0 && (lines[0] == "HTTP/1.0 302 Found" || lines[0] == "HTTP/1.1 302" || lines[0] == "HTTP/1.1 302 Found"))
            {
                foreach (string tli in lines)
                {
                    if (tli.IndexOf("Location:") >= 0)
                        return tli.Substring(10);
                }

                System.Console.WriteLine("InGetCLLocation can't find location: \t" + page);
                return "";
            }
            //else if (lines.Length != 0 && lines[0] == "HTTP/1.1 200 OK")
            //{
            //    ParsePages(page);
            //    return "";
            //}
            else
            {
                //System.Console.WriteLine("InGetCLLocation errorinfos: \t" + lines);
                return "";
            }
        }

        private static string GetCLCnt(string location)
        {
            if (location == "") return "error: NULL location";
            string request = "GET " + location.Substring(clsvr.Length + 7) + " HTTP/1.1\r\nHost: " + clsvr + "\r\nUser-Agent: Mozilla/5.0 (Windows NT 5.1)\r\nConnection:keep-alive\r\n\r\n";
            string page = "";
            int errorcount = 0;
            while (page == "" && errorcount < 3)
            {
                page = GetPageFromCLSvr(request);
                errorcount++;
            }
            return page;
        }

        //private static Byte[] rbuf = null;
        private static string GetPageFromCLSvr(string request)
        {
            Byte[] bytesSent = Encoding.ASCII.GetBytes(request);
            Byte[] bytesReceived = new Byte[8192*8];
            Byte[] rbuf = null;
            if (rbuf == null) rbuf = new Byte[1024 * 1024];

            //Socket cls = ConnectSocket(clsvr, 8087);
            Socket cls = ConnectSocket(clsvr, 80);
            if (cls == null) return ("Connection failed");
            try
            {
                cls.Send(bytesSent, bytesSent.Length, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine("error:cls.Send", e);
                return "";
            }


            int bytes = 0;
            int bytescount=0;
            cls.ReceiveTimeout = 30000;
            string page = "";
            do
            {
                //bytes = cls.Receive(bytesReceived, bytesReceived.Length, 0);
                //stopWatch.Reset();
                //stopWatch.Start();
                try
                {
                    bytes = cls.Receive(bytesReceived, bytesReceived.Length, 0);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ErrorGetReq" + request, e);
                    page = "";
                    return page;
                }
                //stopWatch.Stop();
                //Console.WriteLine("cls.Receive:\t" + stopWatch.Elapsed.ToString() + "\tbytes:\t" + bytes.ToString());
                if (bytes > 0)
                {
                    System.Array.Copy(bytesReceived, 0, rbuf, bytescount, bytes);
                    bytescount += bytes;
                }
            }
            while (bytes > 0);
            page = Encoding.GetEncoding(ecdp).GetString(rbuf, 0, bytescount);
            
            return page;
        }

        //private static Stopwatch stopWatch = new Stopwatch();
        private static Socket ConnectSocket(string server, int port)
        {
            //use googleagent
            //server = "localhost";
            Socket s = null;
            IPHostEntry hostEntry = null;

            //stopWatch.Reset();
            //stopWatch.Start();
            hostEntry = Dns.GetHostEntry(server);
            //stopWatch.Stop();
            //Console.WriteLine("GetHostEntry:\t" + stopWatch.Elapsed.ToString());

            foreach (IPAddress address in hostEntry.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetworkV6) continue;

                IPEndPoint ipe = new IPEndPoint(address, port);
                Socket tempSocket =
                    new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                //tempSocket.Connect(ipe);
                //stopWatch.Reset();
                //stopWatch.Start();
                try
                {
                    tempSocket.Connect(ipe);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ErrorConnectTo:"+ipe.ToString(), e);
                }
                //stopWatch.Stop();
                //Console.WriteLine("Socket.Connect:\t" + stopWatch.Elapsed.ToString());

                if (tempSocket.Connected)
                {
                    s = tempSocket;
                    break;
                }
                else
                {
                    continue;
                }
            }
            return s;
        }
    }
}
