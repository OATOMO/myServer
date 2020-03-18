using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using Org.BouncyCastle.Utilities.Net;
using IPAddress = System.Net.IPAddress;

namespace myServer.ServNet
{
    public class ServNet
    {
        //监听套接字
        public Socket _listenfd;

        //客户端连接
        public Conn[] _conns;

        //最大连接数
        public int _maxConn = 50;

        //单例 (只是为了方便调用)
        public static ServNet _instance;

        public ServNet()
        {
            _instance = this;
        }

        //获取连接池索引,负数表示获取失败, 参考6章
        public int NewIndex()
        {
            if (_conns == null)
                return -1;
            for (int i = 0; i < _conns.Length; i++)
            {
                if (_conns[i] == null)
                {
                    _conns[i] = new Conn();
                    return i;
                }
                else if (_conns[i].IsUse == false)
                {
                    return i;
                }
            }
            return -1;
        }

        //开启服务器
        public void Start(string host, int port)
        {
            //连接池
            _conns = new Conn[_maxConn];
            for (int i = 0; i < _maxConn; i++)
            {
                _conns[i] = new Conn();
            }
            //Socket
            _listenfd = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            //Bind
            IPAddress ipAdr = IPAddress.Parse(host);
            IPEndPoint ipEp = new IPEndPoint(ipAdr,port);
            _listenfd.Bind(ipEp);
            //Listen
            _listenfd.Listen(_maxConn);
            //Accpet
            _listenfd.BeginAccept(AcceptCb,null);
            Console.WriteLine("[服务器]启动成功");


        }

        //Accept回调
        private void AcceptCb(IAsyncResult ar)
        {
            try
            {
                Socket socket = _listenfd.EndAccept(ar);
                int index = NewIndex();
                if (index < 0)
                {
                    socket.Close();
                    Console.WriteLine("[警告,连接已满]");
                }
                else
                {
                    Conn conn = _conns[index];
                    conn.Init(socket);
                    string adr = conn.GetAdress();
                    Console.WriteLine("客户端连接[" + adr + "]Conn池Id:"+index);
                    conn._socket.BeginReceive(conn._readBuffer, conn._bufferCount,
                        conn.BufferRemain(), SocketFlags.None,
                        ReceiveCb, conn);
                }
                _listenfd.BeginAccept(AcceptCb,null);
            }
            catch (Exception e)
            {
                Console.WriteLine("[]Acceptb 失败 : " + e.Message);
            }
        }

        //关闭
        public void Close()
        {
            for (int i = 0; i < _conns.Length; i++)
            {
                Conn conn = _conns[i];
                if (conn == null) continue;
                if (!conn.IsUse) continue;
                lock (conn) //避免线程竞争
                {
                    conn.Close();
                }
            }
        }

        //接受消息回调
        private void ReceiveCb(IAsyncResult ar)
        {
            Conn conn = (Conn) ar.AsyncState;
            lock (conn)
            {
                try
                {
                    int count = conn._socket.EndReceive(ar);
                    //关闭信号
                    if (count <= 0)
                    {
                        Console.WriteLine("收到 ["+ conn.GetAdress() +"] 断开连接");
                        conn.Close();
                        return;
                    }
                    
                    conn._bufferCount += count;
                    ProcessData(conn);
                    conn._socket.BeginReceive(conn._readBuffer, conn._bufferCount,
                        conn.BufferRemain(), SocketFlags.None,
                        ReceiveCb, conn);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[ServNet]ReceiveCb : " + e.Message);
                }
            }
        }

        //
        private void ProcessData(Conn conn)
        {
            //小于长度字节
            if (conn._bufferCount < sizeof(Int32))
            {
                return;
            }
            //消息长度
            Array.Copy(conn._readBuffer,conn._LenBytes,sizeof(Int32));
            conn._msgLenth = BitConverter.ToInt32(conn._LenBytes, 0);
            if (conn._bufferCount < conn._msgLenth + sizeof(UInt32))
            {
                return;
            }
            //处理消息
            string str = System.Text.Encoding.UTF8.GetString(
                                conn._readBuffer, sizeof(Int32), conn._msgLenth);
            Console.WriteLine("收到消息 : ["+ conn.GetAdress() + "]" + str);
            Send(conn, str);
            //清除已处理的消息
            int count = conn._bufferCount - conn._msgLenth - sizeof(Int32);
            Array.Copy(conn._readBuffer,sizeof(Int32)+conn._msgLenth,conn._readBuffer,0,count);
            conn._bufferCount = count;
            if (conn._bufferCount > 0 )
            {
                ProcessData(conn);
            }
        }
        //发送
        public void Send(Conn conn,string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            byte[] length = BitConverter.GetBytes(bytes.Length);
            byte[] sendbuff = length.Concat(bytes).ToArray();
            try
            {
                conn._socket.BeginSend(sendbuff,0,sendbuff.Length,SocketFlags.None,null,null);
            }
            catch (Exception e)
            {
                Console.WriteLine("[发送消息] " + conn.GetAdress() + ":" + e.Message);
            }
        }

    }
}