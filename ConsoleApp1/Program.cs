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
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ipadrlist[0], myPort));
            serverSocket.Listen(10);
            Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());
            Thread myThread = new Thread(ListenClientConnect);
            //foreach (IPAddress ipa in ipadrlist)
            //{
            //    if (ipa.AddressFamily == AddressFamily.InterNetwork)
            //        Console.WriteLine(ipa.ToString());
            //}
            //Console.ReadKey();

        }

        private static void ListenClientConnect()
        {
            Socket clientSocket = serverSocket.Accept();
            while (true)
            {
                try
                {
                    int receiveNumber = clientSocket.Receive(result);
                    Console.WriteLine("接收客户端{0}消息{1}", clientSocket.RemoteEndPoint.ToString()
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
