using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class SensorDataItem
    {
        private ulong timestamp;
        private double x;
        private double y;
        private double z;
        private int type;

        public ulong Timestamp { get => timestamp; set => timestamp = value; }
        public double X { get => x; set => x = value; }
        public double Y { get => y; set => y = value; }
        public double Z { get => z; set => z = value; }
        public int Type { get => type; set => type = value; }
    }
}
