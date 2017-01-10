using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace test
{
    class ControlData
    {
        public struct Data
        {
            public UInt16 incontrolID;
            public UInt16 remoteID;
            public UInt16 controlID;
            public float windspeed;
            public float torque;
            public float luffer;
            public float height;
            public float lean;
            public float rotation;
        }
        public int length;
        public Data data;
        public byte[] databuffer;
        public ControlData()
        {
            length = 34;
            databuffer = new byte[34];
            data.controlID = 1;
            data.height = 10;
            data.incontrolID = 1;
            data.lean = 11;
            data.luffer = 12;
            data.remoteID = 1;
            data.rotation = 13;
            data.torque = 14;
            data.windspeed = 15;
        }
        public void Initdata()
        {
            databuffer = new byte[34];
        }
        public void encode()
        {
            byte sum = 0;
            List<byte> lTemp = new List<byte>();
            lTemp.Add(0xAA);
            lTemp.Add(0xAA);
            lTemp.Add(0x1f);
            lTemp.AddRange(BitConverter.GetBytes(data.incontrolID));
            lTemp.AddRange(BitConverter.GetBytes(data.remoteID));
            lTemp.AddRange(BitConverter.GetBytes(data.controlID));
            lTemp.AddRange(BitConverter.GetBytes(data.windspeed));
            lTemp.AddRange(BitConverter.GetBytes(data.torque));
            lTemp.AddRange(BitConverter.GetBytes(data.luffer));
            lTemp.AddRange(BitConverter.GetBytes(data.height));
            lTemp.AddRange(BitConverter.GetBytes(data.lean));
            lTemp.AddRange(BitConverter.GetBytes(data.rotation));
            for (int i = 0; i < 33; i++)
            {
                sum += lTemp[i];
            }
            lTemp.AddRange(BitConverter.GetBytes(sum));
            lTemp.CopyTo(databuffer);
        }
        public int decode()
        {
            byte sum = 0;
            for (int i = 0; i < 33; i++)
            {
                sum += databuffer[i];
            }
            if (databuffer[33] == sum)
            {
                data.incontrolID = BitConverter.ToUInt16(databuffer, 3);
                data.remoteID = BitConverter.ToUInt16(databuffer, 5);
                data.controlID = BitConverter.ToUInt16(databuffer, 7);
                data.windspeed = BitConverter.ToSingle(databuffer, 9);
                data.torque = BitConverter.ToSingle(databuffer, 13);
                data.luffer = BitConverter.ToSingle(databuffer, 17);
                data.height = BitConverter.ToSingle(databuffer, 21);
                data.lean = BitConverter.ToSingle(databuffer, 25);
                data.rotation = BitConverter.ToSingle(databuffer, 29);
                return 0;
            }
            else
            {
                return -1;
            }
        }
    }
}
