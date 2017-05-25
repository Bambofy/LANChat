using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;

namespace LANChat
{
    namespace BamNet
    {
        static class Bits
        {
            public static byte[] StringToBytes(string pString)
            {
                char[] strChars = pString.ToCharArray();
                byte[] data = new byte[strChars.Length * 2];
                for (int i = 0; i < strChars.Length; i++)
                {
                    byte[] charValue = BitConverter.GetBytes(strChars[i]);
                    if (BitConverter.IsLittleEndian) charValue.Reverse();

                    data[2 * i] = charValue[0];
                    data[2 * i + 1] = charValue[1];
                }
                return data;
            }

            public static byte[] FileToBytes(string pFilename)
            {
                List<byte> _bytes = new List<byte>();

                using (StreamReader sr = new StreamReader(pFilename))
                {
                    BinaryReader br = new BinaryReader(sr.BaseStream);

                    for (int i = 0; i < sr.BaseStream.Length; i++)
                    {
                        _bytes.Add(br.ReadByte());
                    }

                    br.Close();
                }

                return _bytes.ToArray();
            }

            public static string ByteArrayToString(byte[] pBytes, int pByteOffset)
            {
                int byteCount = pBytes.Length;
                char[] data = new char[byteCount / 2];
                for (int i = 0; i < byteCount; i+=2)
                {
                    byte msb = pBytes[pByteOffset + i];
                    byte lsb = pBytes[pByteOffset + i + 1];
                    char value = BitConverter.ToChar(new byte[] { msb, lsb }, 0);
                    data[i / 2] = value;
                }

                string rstring = "";
                foreach (char c in data) rstring += c;
                return rstring;
            }

        }

        static class Networking
        {
            /// <summary>
            /// DO not close the application while this is running or else you will crash your computer.
            /// </summary>
            /// <param name="maxIpCount"></param>
            /// <returns></returns>
            public static List<IPAddress> GetLANAddresses(int maxIpCount = 10)
            {
                List<IPAddress> openIps = new List<IPAddress>();
                string baseIpAddress = "192.168.0.";

                Ping ping = new Ping();
                PingReply reply;

                for (int ipCount = 0; ipCount < maxIpCount; ipCount++)
                {
                    reply = ping.Send(baseIpAddress + ipCount);
                    if (reply.Status == IPStatus.Success)
                    {
                        openIps.Add(reply.Address);
                    }
                }
                return openIps;
            }

            public static IPAddress GetLocalAddress()
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip;
                    }
                }
                return IPAddress.Loopback;
            }
        }
    }
    
}
