using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Control
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
            data.controlID = 0;
            data.height = 10;
            data.incontrolID = 0;
            data.lean = 11;
            data.luffer = 1;
            data.remoteID = 0;
            data.rotation = 13;
            data.torque = 1;
            data.windspeed = 1;
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
            lTemp.AddRange(BitConverter.GetBytes(data.incontrolID));
            lTemp.AddRange(BitConverter.GetBytes(data.remoteID));
            lTemp.AddRange(BitConverter.GetBytes(data.controlID));
            lTemp.AddRange(BitConverter.GetBytes(data.windspeed));
            lTemp.AddRange(BitConverter.GetBytes(data.torque));
            lTemp.AddRange(BitConverter.GetBytes(data.luffer));
            lTemp.AddRange(BitConverter.GetBytes(data.height));
            lTemp.AddRange(BitConverter.GetBytes(data.lean));
            lTemp.AddRange(BitConverter.GetBytes(data.rotation));
            lTemp.Add(0x1f);
            for (int i = 0; i < 33; i++)
            {
                sum += lTemp[i];
            }
            lTemp.Add(sum);
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
                data.incontrolID = BitConverter.ToUInt16(databuffer, 2);
                data.remoteID = BitConverter.ToUInt16(databuffer, 4);
                data.controlID = BitConverter.ToUInt16(databuffer, 6);
                data.windspeed = BitConverter.ToSingle(databuffer, 7);
                data.torque = BitConverter.ToSingle(databuffer, 12);
                data.luffer = BitConverter.ToSingle(databuffer, 16);
                data.height = BitConverter.ToSingle(databuffer, 20);
                data.lean = BitConverter.ToSingle(databuffer, 24);
                data.rotation = BitConverter.ToSingle(databuffer, 28);
                return 0;
            }
            else
            {
                return -1;
            }
        }
    }
}
