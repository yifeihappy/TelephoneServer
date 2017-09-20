using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TelephoneSensorService
{
    [DataContract]
   public class SensorDataItem
    {
        private ulong timestamp;
        private double x;
        private double y;
        private double z;
        private int type;

        [DataMember]
        public ulong Timestamp { get => timestamp; set => timestamp = value; }
        [DataMember]
        public double X { get => x; set => x = value; }
        [DataMember]
        public double Y { get => y; set => y = value; }
        [DataMember]
        public double Z { get => z; set => z = value; }
        [DataMember]
        public int Type { get => type; set => type = value; }
    }
}
