using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TelephoneSensorService;

namespace ConsoleApp1
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的类名“SocketService1”。
    public class SocketService1 : ISocketService1
    {
        public void DoWork()
        {
        }

        public bool sendData(string msg)
        {
           // throw new NotImplementedException();
           if(Program.clientSocket == null)
            {
                return false;//没有android设备连接到服务器
            }
            Debug.WriteLine("Socket 准备发送数据：" + msg);
            byte[] sendByte = Encoding.UTF8.GetBytes(msg);
            try
            {
                int n = Program.clientSocket.Send(sendByte, sendByte.Length, 0);
                Debug.WriteLine("发送了" + n + "bytes");
                
            }
            catch(Exception e1)
            {
                Debug.WriteLine("sensortypes 发送失败:"+e1.ToString());
                return false;
            }
            return true; 
        }
    }
}
