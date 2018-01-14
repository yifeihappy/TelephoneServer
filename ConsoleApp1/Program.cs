using ConsoleApp1;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TelephoneSensorService
{
    class Program 
    {
        private static byte[] result;
        public static string sensorsType = null;//Android支持的传感器类型
        private static int myPort = 30000;//port
        static Socket serverSocket = null;
        public static Socket clientSocket = null;
        private static String msg = "";
        public static ConcurrentQueue<SensorDataItemXD> sensorDataQueue = new ConcurrentQueue<SensorDataItemXD>();
        static void Main(string[] args)
        {


            ServiceHost host = new ServiceHost(typeof(SensorDataService1));
            host.Open();

            ServiceHost hostSocket = new ServiceHost(typeof(SocketService1));
            hostSocket.Open();


            Console.WriteLine("CalculaorService已经启动，按任意键终止服务！");

            //socket listent
            string name = Dns.GetHostName();
            //IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
            //for (int j = 0; j < ipadrlist.Length; j++)
            //{
            //    Debug.WriteLine("server ip[{0}]:{1}", j, ipadrlist[j]);
            //}

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //serverSocket.Bind(new IPEndPoint(ipadrlist[3], myPort));
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse(GetLocalIP()), myPort));
            serverSocket.Listen(10);
            Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());
            Thread socketThread = new Thread(ListenClientConnect);
            socketThread.IsBackground = true;
            socketThread.Start();


            Console.WriteLine("press any key to teriminate...");
            Console.ReadKey();
            host.Abort();
            host.Close();
            hostSocket.Abort();
            hostSocket.Close();

            Console.WriteLine("Close......");

            socketThread.Abort();
        }

        private static void ListenClientConnect()
        {
            while (true)
            {
                clientSocket = serverSocket.Accept();
                Thread recvThread = new Thread(recvData);
                recvThread.IsBackground = true;
                recvThread.Start(clientSocket);
            }
        }
        private static void recvData(object s)
        {

            Socket clientSocket = (Socket)s;
            while (true)
            {
                try
                {
                    DateTime curTime = System.DateTime.Now;
                    int minute = curTime.Minute;
                    int second = curTime.Second;
                    result = new byte[1024];
                    
                    int receiveNumber = clientSocket.Receive(result);
                    if (receiveNumber <= 0) break;
                    Console.WriteLine(minute + ":" + second + "  receiveNUm:{0}", receiveNumber);
                    Console.WriteLine("" + minute + ":" + second +
                        "/接收客户端{0}消息\n{1}本次接收完毕", clientSocket.RemoteEndPoint.ToString()
                        , Encoding.ASCII.GetString(result, 0, receiveNumber));
                    String str = Encoding.ASCII.GetString(result, 0, receiveNumber);
                    msg += str;
                    if(str.EndsWith("\n"))
                    {
                        String[] strArr = msg.Split('\n');
                        //Console.WriteLine("strArr_L = {0}", strArr.Length);
                        if (strArr[0].StartsWith("SENSORSTYPE"))
                        {
                            //Console.WriteLine("SENSORSTYPE");
                            Program.sensorsType = strArr[0];
                        }
                        else
                        {
                            for (int i = 0; i < strArr.Length; i++)
                            {
                                if (strArr[i].Length == 0) continue;
                                string[] sArr = strArr[i].Split(',');

                                SensorDataItemXD sditemXD = new SensorDataItemXD();
                                try
                                {
                                    sditemXD.Type = Convert.ToInt32(sArr[0]);
                                    sditemXD.Timestamp = Convert.ToUInt64(sArr[1]);
                                    sditemXD.Dimension = Convert.ToInt32(sArr[2]);
                                    if (sditemXD.Dimension > 16)
                                    {
                                        Console.WriteLine("TTPE:" + sArr[0] + " 数据维度大于16，目前无法处理！");
                                        continue;
                                    }
                                    for (int d = 0; d < sditemXD.Dimension; d++)
                                    {
                                        sditemXD.SensorsArr[d] = Convert.ToDouble(sArr[3 + d]);
                                    }
                                }
                                catch(Exception econv)
                                {
                                    Console.WriteLine(econv.Message);
                                    continue;
                                }

                                sensorDataQueue.Enqueue(sditemXD);

                            }
                        }
                        msg = "";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    break;
                }
            }
        }

        /// <summary>  
        /// 获取当前使用的IP  
        /// </summary>  
        /// <returns></returns>  
        public static string GetLocalIP()
        {
            string result = RunApp("route", "print", true);
            Match m = Regex.Match(result, @"0.0.0.0\s+0.0.0.0\s+(\d+.\d+.\d+.\d+)\s+(\d+.\d+.\d+.\d+)");
            if (m.Success)
            {
                return m.Groups[2].Value;
            }
            else
            {
                try
                {
                    System.Net.Sockets.TcpClient c = new System.Net.Sockets.TcpClient();
                    c.Connect("www.baidu.com", 80);
                    string ip = ((System.Net.IPEndPoint)c.Client.LocalEndPoint).Address.ToString();
                    c.Close();
                    return ip;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary>  
        /// 获取本机主DNS  
        /// </summary>  
        /// <returns></returns>  
        public static string GetPrimaryDNS()
        {
            string result = RunApp("nslookup", "", true);
            Match m = Regex.Match(result, @"\d+\.\d+\.\d+\.\d+");
            if (m.Success)
            {
                return m.Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>  
        /// 运行一个控制台程序并返回其输出参数。  
        /// </summary>  
        /// <param name="filename">程序名</param>  
        /// <param name="arguments">输入参数</param>  
        /// <returns></returns>  
        public static string RunApp(string filename, string arguments, bool recordLog)
        {
            try
            {
                if (recordLog)
                {
                    Trace.WriteLine(filename + " " + arguments);
                }
                Process proc = new Process();
                proc.StartInfo.FileName = filename;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.Arguments = arguments;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.Start();

                using (System.IO.StreamReader sr = new System.IO.StreamReader(proc.StandardOutput.BaseStream, Encoding.Default))
                {

                    //上面标记的是原文，下面是我自己调试错误后自行修改的  
                    Thread.Sleep(100);           //貌似调用系统的nslookup还未返回数据或者数据未编码完成，程序就已经跳过直接执行  
                                                 //txt = sr.ReadToEnd()了，导致返回的数据为空，故睡眠令硬件反应  
                    if (!proc.HasExited)         //在无参数调用nslookup后，可以继续输入命令继续操作，如果进程未停止就直接执行  
                    {                            //txt = sr.ReadToEnd()程序就在等待输入，而且又无法输入，直接掐住无法继续运行  
                        proc.Kill();
                    }
                    string txt = sr.ReadToEnd();
                    sr.Close();
                    if (recordLog)
                        Trace.WriteLine(txt);
                    return txt;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return ex.Message;
            }
        }

    }
}
