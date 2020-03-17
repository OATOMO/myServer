using System;
using System.Net.Sockets;
using myServer.DataMgr;
using MySqlX.Protocol;

namespace myServer.ServNet
{
    public class Conn
    {
        //常量
        public const int BUFFER_SIZE = 1024;
        //Socket
        public Socket socket;
        //是否使用
        public bool IsUse = false;
        //Buff
        public byte[] readBuffer = new byte[BUFFER_SIZE];
        public int bufferCount = 0;
        //黏包分包
        public byte[] LenBytes = new byte[sizeof(UInt32)];
        public Int32 msgLenth = 0;
        //心跳时间
        public long lastTickTime = long.MaxValue;
        //对应的player
        public Player player;
        //构造函数
        public conn()
        {
            readBuffer = new byte[BUFFER_SIZE];
        }
        //初始化
        public void Init(Socket socket)
        {
            this.socket = socket;
            IsUse = true;
            bufferCount = 0;
            //心跳处理
            lastTickTime = Sys.GetTimeStamp();
        }
        //剩余的buffer
        public int BufferRemain()
        {
            return BUFFER_SIZE - bufferCount;
        }
        //获取客户端的地址
        public string GetAdress()
        {
            if (!IsUse)
                return "无法获取地址";
            return socket.RemoteEndPoint.ToString();
        }
        //关闭
        public void Close()
        {
            if (!IsUse)
                return;
            if (player != null)
            {
                //玩家退出处理,稍后实现
                //player.Logout();
                return;
            }
            Console.WriteLine("[断开连接]"+GetAdress());
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            IsUse = false;
        }
        //发送协议,稍后实现
        public void Send(ProtocolBase protocol)
        {
            ServNet.instance.Send(this,protocol);
        }
    }
}