using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Station
{
    class RemoteData
    {
        public struct Data
        {
            public UInt16 status;
            public Int16 luffer;
            public Int16 rotation;
            public Int16 height;
            public UInt16 controlID;
            public UInt16 remoteID;
        }
        public int length;
        public byte[] databuffer = new byte[16];
        public Data data;
        public RemoteData()
        {
            length = 16;
        }
        public void Initdata()
        {
            databuffer = new byte[16];
        }
        public void encode()
        {
            byte sum = 0;
            List<byte> lTemp = new List<byte>();
            lTemp.Add(0xAA);
            lTemp.Add(0xAA);
            lTemp.Add(0x0A);
            lTemp.AddRange(BitConverter.GetBytes(data.status));
            lTemp.AddRange(BitConverter.GetBytes(data.remoteID));
            lTemp.AddRange(BitConverter.GetBytes(data.controlID));
            lTemp.AddRange(BitConverter.GetBytes(data.luffer));
            lTemp.AddRange(BitConverter.GetBytes(data.rotation));
            lTemp.AddRange(BitConverter.GetBytes(data.height));
            for (int i = 0; i < 15; i++)
            {
                sum += lTemp[i];
            }
            lTemp.Add(sum);
            lTemp.CopyTo(databuffer);
        }
        public int decode()
        {
            byte sum = 0;
            for (int i = 0; i < 15; i++)
            {
                sum += databuffer[i];
            }
            if (databuffer[15] == sum)
            {
                data.status = BitConverter.ToUInt16(databuffer, 3);
                data.remoteID = BitConverter.ToUInt16(databuffer, 5);
                data.controlID = BitConverter.ToUInt16(databuffer, 7);
                data.luffer = BitConverter.ToInt16(databuffer, 9);
                data.rotation = BitConverter.ToInt16(databuffer, 11);
                data.height = BitConverter.ToInt16(databuffer, 13);
                return 0;
            }
            else
            {
                return -1;
            }
        }
    }
}
