//测试用的异步socket客户端

using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Google.Protobuf.WellKnownTypes;

namespace myServer.test_client
{
    public class client
    {
        public Socket _socket;
        public const int BUFFER_SIZE = 1024;
        public byte[] readBuff = new byte[BUFFER_SIZE]; 
        public void Connection()
        {
            //socket
            _socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            //Connect
            string host = "127.0.0.1";
            int port = 6666;
            _socket.Connect(host,port);
            // _socket.BeginReceive(readBuff,0,BUFFER_SIZE,SocketFlags.None,ReceiveCb,null);
        }

        private void ReceiveCb(IAsyncResult ar)
        {
            try
            {
                //count
                int count = _socket.EndReceive(ar);
                //数据处理
                string str = Encoding.UTF8.GetString(readBuff, 0, count);
                Console.WriteLine(str);
                _socket.BeginReceive(readBuff,0,BUFFER_SIZE,SocketFlags.None,ReceiveCb,null);
                
            }
            catch (Exception e)
            {
                Console.WriteLine("client 连接断开 :" + e.Message);
                
            }
        }

        public void Send(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            byte[] length = BitConverter.GetBytes(bytes.Length);
            byte[] sendBuff = length.Concat(bytes).ToArray();
            _socket.Send(sendBuff);
            Console.WriteLine("send ok!");
        }

    }
}