using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LANChat
{
    public partial class formLANChat : Form
    {
        private string _filePath;
        private Thread _listenThread;

        public formLANChat()
        {
            InitializeComponent();
        }

        public void Log(string message)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Log), new object[] { message });
                return;
            }

            richTextBoxLog.Text = message + richTextBoxLog.Text;
        }
        
        public void LogDownload(string message)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(LogDownload), new object[] { message });
                return;
            }

            richTextBox2.Text = message + "\n" + richTextBox2.Text;
        }

        private void formLANChat_Load(object sender, EventArgs e)
        {
            _listenThread = new Thread(Listen);
            _listenThread.Start();
        }

        private void Listen()
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 1337);
            Socket windowsSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            windowsSocket.Bind(serverEndPoint);

            while (true)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint remote = (EndPoint)(sender);

                // Receive the window.
                byte[] buffer = new byte[2048000];
                int recv = windowsSocket.ReceiveFrom(buffer, ref remote);

                // header
                // 1 bytes : containsFile
                // 4 bytes : messageLength
                // 4 bytes : filenameLength

                // message
                // filenameLength bytes : filename
                // messageLength bytes : name + message
                // fileData

                bool containsFile = BitConverter.ToBoolean(buffer.SubArray(0, 1), 0);
                int messageLength = BitConverter.ToInt32(buffer.SubArray(0 + 1, 4), 0);
                int filenameLength = BitConverter.ToInt32(buffer.SubArray(0 + 1 + 4, 4), 0);
                int filedataLength = BitConverter.ToInt32(buffer.SubArray(0 + 1 + 4 + 4, 4), 0);

                string filename = BamNet.Bits.ByteArrayToString(buffer.SubArray(0 + 1 + 4 + 4 + 4, filenameLength), 0);
                string message = BamNet.Bits.ByteArrayToString(buffer.SubArray(0 + 1 + 4 + 4 + 4 + filenameLength, messageLength), 0);
                byte[] filebytes = buffer.SubArray(0 + 1 + 4 + 4 + 4 + filenameLength + messageLength, filedataLength);

                Log(message);

                if (containsFile)
                {
                    if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\downloads")) Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\downloads");

                    using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "\\downloads\\" + filename))
                    {
                        BinaryWriter bw = new BinaryWriter(sw.BaseStream);
                        
                        for (int i = 0; i < filebytes.Length; i++)
                        {
                            bw.Write(filebytes[i]);
                        }
                        
                    }

                    LogDownload(filename);
                }
            }
        }

        private void formLANChat_FormClosing(object sender, FormClosingEventArgs e)
        {
            _listenThread.IsBackground = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string baseIp = "192.168.0.";
            List<string> ips = new List<string>();
            for (int i = 0; i < 20; i++)
            {
                ips.Add(baseIp + i.ToString());
            }

            // header
            // 1 bytes : containsFile
            // 4 bytes : messageLength
            // 4 bytes : filenameLength

            // message
            // filenameLength bytes : filename
            // messageLength bytes : name + message

            bool containsFile = (_filePath != null) ? true : false;
            string name = textBox1.Text;
            string message = richTextBox1.Text + "\n";
            string filename = "";
            if (_filePath != null)
            {
                string[] filepathParts = _filePath.Split('\\');
                filename = filepathParts[filepathParts.Length - 1];
            }

            // data
            byte[] filenameData = BamNet.Bits.StringToBytes(filename);
            byte[] messageData = BamNet.Bits.StringToBytes(name + ": " + message);
            List<byte> fileData = new List<byte>();
            if (_filePath != null)
            {
                fileData.AddRange(BamNet.Bits.FileToBytes(_filePath));
            }

            // header
            byte[] containsFileData = BitConverter.GetBytes(containsFile);
            if (BitConverter.IsLittleEndian) containsFileData.Reverse();

            byte[] messageLengthData = BitConverter.GetBytes(messageData.Length);
            if (BitConverter.IsLittleEndian) messageLengthData.Reverse();

            byte[] filenameLength = BitConverter.GetBytes(filenameData.Length);
            if (BitConverter.IsLittleEndian) filenameLength.Reverse();

            byte[] fileLength = BitConverter.GetBytes(fileData.Count);

            byte[] headerData = containsFileData.Concat(messageLengthData).Concat(filenameLength).Concat(fileLength).ToArray();
            byte[] bodyData = filenameData.Concat(messageData).Concat(fileData).ToArray();

            foreach (string ip in ips)
            {
                try
                {
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), 1337);
                    Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    int senv = server.SendTo(headerData.Concat(bodyData).ToArray(), SocketFlags.None, remoteEndPoint);
                }
                catch (System.Net.Sockets.SocketException exception)
                {
                    throw exception;
                    continue;
                }
            }


            textBox2.Text = "";
            _filePath = null;
            richTextBox1.Clear();
        }

        private void buttonAttachFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofg = new OpenFileDialog();
            
            if (ofg.ShowDialog() == DialogResult.OK)
            {
                _filePath = ofg.FileName;
                textBox2.Text = _filePath;
            }
        }


    }

    public static class Extensions
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
