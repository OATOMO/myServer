using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Timers;
using handleMsg;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Net;
using IPAddress = System.Net.IPAddress;
using ServNet;

namespace ServNet
{
    public class ServNet
    {
        //监听套接字
        public Socket _listenfd;

        //客户端连接
        public Conn[] _conns;

        //最大连接数
        public int _maxConn = 50;
        
        //主定时器
        System.Timers.Timer _timer = new Timer(1000);
        //心跳时间
        public long heartBeatTime = 180;
        //使用的协议
        public ProtocolBase proto;
        //消息分发
        public HandleConnMsg HandleConnMsg = new HandleConnMsg();            //处理连接
        public HandlePlayerMsg HandlePlayerMsg = new HandlePlayerMsg();      //处理角色
        public HandlePlayerEvent HandlePlayerEvent = new HandlePlayerEvent();//处理玩家
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
            //定时器
            _timer.Elapsed += new ElapsedEventHandler(HandleMainTimer);
            _timer.AutoReset = false;
            _timer.Enabled = true;
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

        //主定时器
        public void HandleMainTimer(object sender,System.Timers.ElapsedEventArgs e) {
            //处理心跳
            HeartBeat();
            _timer.Start();
        }
        //心跳
        public void HeartBeat() {
            Console.WriteLine("[主定时器执行]");
            long timeNow = Sys.GetTimeStamp();
            for (int i = 0; i < _conns.Length; i++) {
                Conn conn = _conns[i];
                if (conn == null) continue;
                if (!conn.IsUse) continue;
                if (conn._lastTickTime < timeNow - heartBeatTime) {
                    Console.WriteLine("[心跳引起断开连接]" + conn.GetAdress());
                    lock (conn) {
                        conn.Close();    
                    }
                }
            }
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
            // string str = System.Text.Encoding.UTF8.GetString(
            //                     conn._readBuffer, sizeof(Int32), conn._msgLenth);
            // if (str == "atom")
            //     conn._lastTickTime = Sys.GetTimeStamp();
            // Console.WriteLine("收到消息 : ["+ conn.GetAdress() + "]" + str);
            // Send(conn, str);
            ProtocolBase protocol = proto.Decode(conn._readBuffer, sizeof(Int32), conn._msgLenth);
            HandleMsg(conn,protocol);
            //清除已处理的消息
            int count = conn._bufferCount - conn._msgLenth - sizeof(Int32);
            Array.Copy(conn._readBuffer,sizeof(Int32)+conn._msgLenth,conn._readBuffer,0,count);
            conn._bufferCount = count;
            if (conn._bufferCount > 0 )
            {
                ProcessData(conn);
            }
        }
        //
        private void HandleMsg(Conn conn,ProtocolBase protocolBase) {
            string name = protocolBase.GetName();
            string methodName = "Msg" + name;
            // Console.WriteLine("[收到协议] : " + name);
            //连接协议分发
            if (conn._player == null || name == "HeatBeat" || name == "Logout" || name == "Login"){
                MethodInfo mm = HandleConnMsg.GetType().GetMethod(methodName);
                if (mm == null){
                    Console.WriteLine("[warning]HandleConnMsg 没有处理连接方法: " + methodName);
                    return;
                }
                Object[] obj = new object[]{conn,protocolBase};
                Console.WriteLine("[处理连接消息]"+conn.GetAdress()+":"+name);
                mm.Invoke(HandleConnMsg,obj);
            }
            //角色协议分发
            else{
                MethodInfo mm = HandlePlayerMsg.GetType().GetMethod(methodName);
                if (mm == null){
                    Console.WriteLine("[warning]HandlePlayerMsg 没有处理连接方法: " + methodName);
                    return;
                }
                Object[] obj = new object[]{conn._player,protocolBase};
                Console.WriteLine("[处理玩家消息]"+conn._player.id+":"+name);
                mm.Invoke(HandlePlayerMsg,obj);
            }

            // //处理心跳
            // if (name == "HeatBeat"){
            //     Console.WriteLine("[更新心跳时间] : " + conn.GetAdress());
            //     conn._lastTickTime = Sys.GetTimeStamp();
            // }
            // //回射
            // Send(conn,protocolBase);
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
        public void Send(Conn conn,ProtocolBase protocolBase) {
            byte[] bytes = protocolBase.Encode();
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
        //广播
        public void Broadcast(ProtocolBase protocolBase) {
            for (int i = 0; i < _conns.Length; i++){
                if (!_conns[i].IsUse) continue;
                if (_conns[i]._player == null) continue;
                Send(_conns[i],protocolBase);
            }
        }
        //输出服务端信息
        public void Print() {
            Console.WriteLine("==========服务器登录信息==========");
            for (int i = 0; i < _conns.Length; i++){
                if (_conns[i] == null) continue;
                if (!_conns[i].IsUse) continue;
                string str = "连接 [" + _conns[i].GetAdress()+"]";
                if (_conns[i]._player != null)
                    Console.WriteLine(str + "play Id:" + _conns[i]._player.id);
            }
        }

    }
}