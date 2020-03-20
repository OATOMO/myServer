using System;
using System.Net.Sockets;
using DataMgr;

namespace ServNet
{
    public class Conn
    {
        //常量
        public const int BUFFER_SIZE = 1024;
        //Socket
        public Socket _socket;
        //是否使用
        public bool IsUse = false;
        //Buff
        public byte[] _readBuffer = new byte[BUFFER_SIZE];
        public int _bufferCount = 0;
        //黏包分包
        public byte[] _LenBytes = new byte[sizeof(UInt32)];
        public Int32 _msgLenth = 0;
        //心跳时间
        public long _lastTickTime = long.MaxValue;
        //对应的player
        public Player _player;
        //构造函数
        public Conn()
        {
            _readBuffer = new byte[BUFFER_SIZE];
        }
        //初始化
        public void Init(Socket socket)
        {
            this._socket = socket;
            IsUse = true;
            _bufferCount = 0;
            //心跳处理
            _lastTickTime = Sys.GetTimeStamp();
        }
        //剩余的buffer
        public int BufferRemain()
        {
            return BUFFER_SIZE - _bufferCount;
        }
        //获取客户端的地址
        public string GetAdress()
        {
            if (!IsUse)
                return "无法获取地址";
            return _socket.RemoteEndPoint.ToString();
        }
        //关闭
        public void Close()
        {
            if (!IsUse)
                return;
            if (_player != null)
            {
                //玩家退出处理,稍后实现
                //player.Logout();
                return;
            }
            Console.WriteLine("[断开连接]"+GetAdress());
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            IsUse = false;
        }
        //发送协议,稍后实现
        public void Send(ProtocolBase protocol)
        {
            ServNet._instance.Send(this,protocol);
        }
    }
}