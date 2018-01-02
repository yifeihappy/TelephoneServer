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
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“ISensorDataService1”。
    [ServiceContract]
    public interface ISensorDataService1
    {
        [OperationContract]
        void DoWork();

        [OperationContract]
        List<SensorDataItemXD> TryDeque();

        [OperationContract]
        void ClearSensorDataQueue();

        [OperationContract]
        String getSensorsType();//获取Android支持的传感器类型
    }
}
