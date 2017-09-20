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
            SensorDataItem sditem = new SensorDataItem();
            //throw new NotImplementedException();
            while (Program.sensorDataQueue.TryDequeue(out sditem))
            {

            }
        }

        public void DoWork()
        {
        }

        public List<SensorDataItem> TryDeque()
        {
            // throw new NotImplementedException();
            List<SensorDataItem> sensorDataList = new List<SensorDataItem>();
            try
            {
                SensorDataItem sditem = new SensorDataItem();
                while (Program.sensorDataQueue.TryDequeue(out sditem))
                {
                    sensorDataList.Add(sditem);
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
