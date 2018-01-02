using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    [DataContract]
    public class SensorDataItemXD
    {
        private ulong timestamp;
        private int dimension;//sensor data 的维度
        private int type;
        public double[] sensorsArr = new double[16]; //sensor 的最大dimension

        [DataMember]
        public ulong Timestamp { get => timestamp; set => timestamp = value; }
        [DataMember]
        public int Dimension { get => dimension; set => dimension = value; }
        [DataMember]
        public int Type { get => type; set => type = value; }
        [DataMember]
        public double[] SensorsArr { get => sensorsArr; set => sensorsArr = value; }
    }
}
