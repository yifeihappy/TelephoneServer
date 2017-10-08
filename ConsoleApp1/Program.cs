﻿using ConsoleApp1;
using System;
using System.Collections.Concurrent;
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
        private static byte[] result;
        public static string sensorsType = null;//Android支持的传感器类型
        private static int myPort = 30000;//port
        static Socket serverSocket = null;
        public static Socket clientSocket = null;
        private static String msg = "";
        public static ConcurrentQueue<SensorDataItem> sensorDataQueue = new ConcurrentQueue<SensorDataItem>();
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(SensorDataService1));
            host.Open();

            ServiceHost hostSocket = new ServiceHost(typeof(SocketService1));
            hostSocket.Open();


            Console.WriteLine("CalculaorService已经启动，按任意键终止服务！");

            //socket listent
            string name = Dns.GetHostName();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
            for (int j = 0; j < ipadrlist.Length; j++)
            {
                Debug.WriteLine("server ip[{0}]:{1}", j, ipadrlist[j]);
            }

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ipadrlist[1], myPort));
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

            //socketThread.Abort();
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
                    result = new byte[1024*100];
                    
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
                                //Console.WriteLine("sARR.L={0}", sArr.Length);
                                //if (sArr.Length != 5) continue;//目前只能获取数据类型为(x, y, z)的传感器数据
                                SensorDataItem sditem = new SensorDataItem();
                                sditem.Type = Convert.ToInt32(sArr[0]);
                                sditem.Timestamp = Convert.ToUInt64(sArr[1]);
                                sditem.X = Convert.ToDouble(sArr[2]);
                                if (sArr.Length > 3)
                                {
                                    sditem.Y = Convert.ToDouble(sArr[3]);
                                }
                                if (sArr.Length > 4)
                                {
                                    sditem.Z = Convert.ToDouble(sArr[4]);
                                }
                                sensorDataQueue.Enqueue(sditem);
                            }
                        }
                        msg = "";
                    }

                    //String[] strArr = str.Split('\n');
                    ////Console.WriteLine("strArr_L = {0}", strArr.Length);
                    //if(strArr[0].StartsWith("SENSORSTYPE"))
                    //{
                    //    //Console.WriteLine("SENSORSTYPE");
                    //    Program.sensorsType = strArr[0];
                    //}
                    //else
                    //{
                    //    for (int i = 0; i < strArr.Length; i++)
                    //    { 
                    //        if (strArr[i].Length == 0) continue;
                    //        string[] sArr = strArr[i].Split(',');
                    //        //Console.WriteLine("sARR.L={0}", sArr.Length);
                    //        //if (sArr.Length != 5) continue;//目前只能获取数据类型为(x, y, z)的传感器数据
                    //        SensorDataItem sditem = new SensorDataItem();
                    //        sditem.Type = Convert.ToInt32(sArr[0]);
                    //        sditem.Timestamp = Convert.ToUInt64(sArr[1]);
                    //        sditem.X = Convert.ToDouble(sArr[2]);
                    //        if(sArr.Length>3)
                    //        {
                    //            sditem.Y = Convert.ToDouble(sArr[3]);
                    //        }
                    //        if(sArr.Length>4)
                    //        {
                    //            sditem.Z = Convert.ToDouble(sArr[4]);
                    //        }
                    //        sensorDataQueue.Enqueue(sditem);
                    //    }
                    //}
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
