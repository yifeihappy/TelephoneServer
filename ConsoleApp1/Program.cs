using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TelephoneSensorService
{
    class Program 
    {
        private static byte[] result = new byte[1024];
        private static int myPort = 30000;//port
        static Socket serverSocket;
        public static ConcurrentQueue<SensorDataItem> sensorDataQueue = new ConcurrentQueue<SensorDataItem>();
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(SensorDataService1));
            host.Open();

            Console.WriteLine("CalculaorService已经启动，按任意键终止服务！");






            //socket listent
            string name = Dns.GetHostName();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
            //for(int j = 0;j<ipadrlist.Length;j++)
            //{
            //    Debug.WriteLine("server ip[{0}]:{1}",j, ipadrlist[j]);
            //}

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ipadrlist[3], myPort));
            serverSocket.Listen(10);
            Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());
            Thread socketThread = new Thread(ListenClientConnect);
            socketThread.IsBackground = true;
            socketThread.Start();


            Console.WriteLine("press any key to teriminate...");
            Console.ReadKey();
            host.Abort();
            host.Close();
            Console.WriteLine("Close......");

            //socketThread.Abort();
        }

        private static void ListenClientConnect()
        {
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
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
                        "/接收客户端{0}消息{1}", clientSocket.RemoteEndPoint.ToString()
                        , Encoding.ASCII.GetString(result, 0, receiveNumber));
                    String str = Encoding.ASCII.GetString(result, 0, receiveNumber);
                    String[] strArr = str.Split('\n');
                    //Console.WriteLine("strArr_L = {0}", strArr.Length);
                    for (int i = 0; i < strArr.Length; i++)
                    {
                        if (strArr[i].Length == 0) continue;
                        string[] sArr = strArr[i].Split(',');
                        //Console.WriteLine("sARR.L={0}", sArr.Length);
                        if (sArr.Length != 5) continue;
                        SensorDataItem sditem = new SensorDataItem();
                        sditem.Type = Convert.ToInt32(sArr[0]);
                        sditem.Timestamp = Convert.ToUInt64(sArr[1]);
                        sditem.X = Convert.ToDouble(sArr[2]);
                        sditem.Y = Convert.ToDouble(sArr[3]);
                        sditem.Z = Convert.ToDouble(sArr[4]);

                        sensorDataQueue.Enqueue(sditem);


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
    }
}
