using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        private static byte[] result = new byte[1024];
        private static int myPort = 30000;//port
        static Socket serverSocket;

        static void Main(string[] args)
        {
            string name = Dns.GetHostName();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
            Console.WriteLine("server ip:{0}", ipadrlist[1]);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ipadrlist[1], myPort));
            serverSocket.Listen(10);
            Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());
            Thread socketThread = new Thread(ListenClientConnect);
            socketThread.IsBackground = true;
            socketThread.Start();
            Console.ReadKey();
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
            while(true)
            {
                try
                {
                    DateTime curTime = System.DateTime.Now;
                    int minute = curTime.Minute;
                    int second = curTime.Second;
                     result = new byte[1024];
                    int receiveNumber = clientSocket.Receive(result);
                    if(receiveNumber<=0)break;
                    Console.WriteLine("receiveNUm:{0}",receiveNumber);
                    Console.WriteLine(""+minute+":"+second+
                        "/接收客户端{0}消息{1}", clientSocket.RemoteEndPoint.ToString()
                        , Encoding.ASCII.GetString(result, 0, receiveNumber));
                }catch(Exception ex)
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
