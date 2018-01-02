using ConsoleApp1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TelephoneSensorService;

namespace TelephoneSensorService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的类名“SensorDataService1”。
    public class SensorDataService1 : ISensorDataService1
    {
        public void ClearSensorDataQueue()
        {
            //SensorDataItem sditem = new SensorDataItem();
            SensorDataItemXD sditemXD = new SensorDataItemXD();
            //throw new NotImplementedException();
            while (Program.sensorDataQueue.TryDequeue(out sditemXD))
            {
            }
        }

        public void DoWork()
        {
        }

        public String getSensorsType()
        {
            //throw new NotImplementedException();
            return Program.sensorsType;
        }

        public List<SensorDataItemXD> TryDeque()
        {
            // throw new NotImplementedException();
            List<SensorDataItemXD> sensorDataList = new List<SensorDataItemXD>();
            try
            {
                //SensorDataItem sditem = new SensorDataItem();
                SensorDataItemXD sditemXD = new SensorDataItemXD();
                while (Program.sensorDataQueue.TryDequeue(out sditemXD))
                {
                    sensorDataList.Add(sditemXD);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return sensorDataList;
        }


    }
}
