using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Timers;
using Org.BouncyCastle.Asn1.Cms;
using ServNet;

namespace client
{
    //客户端网络连接
    public class Connection
    {
        //常量
        private const int BUFFER_SIZE = 1024;

        //Socket
        private Socket _socket;

        //BUFF
        private byte[] _readbuff = new byte[BUFFER_SIZE];

        private int _buffCount = 0;

        //黏包分包
        private Int32 _msgLength = 0;

        private byte[] _lenBytes = new byte[sizeof(Int32)];

        //协议
        public ProtocolBase _proto;

        //心跳
        public float _lastTackTime = 0;

        public float _heartBeatTime = 30; //发送心跳间隔

        //消息分发
        public MsgDistribution msgDist = new MsgDistribution();

        //状态
        public enum Status
        {
            None,
            Connected,
        }

        public Status _status = Status.None;

        //连接服务端
        public bool Connect(string host, int port) {
            try{
                //socket
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //Connect
                _socket.Connect(host, port);
                //BeginReceive
                _socket.BeginReceive(_readbuff, _buffCount, BUFFER_SIZE - _buffCount,
                    SocketFlags.None, ReceiveCb, _readbuff);
                Debug.Log("连接成功");
                //状态
                _status = Status.Connected;
                return true;
            }
            catch (Exception e){
                Console.WriteLine("连接失败" + e.Message);
                return false;
            }
        }

        //关闭连接
        public bool Close() {
            try{
                _socket.Close();
                return true;
            }
            catch (Exception e){
                Console.WriteLine("关闭失败" + e.Message );
                return false;
            }
        }
        //接受回调
        private void ReceiveCb(IAsyncResult ar) {
            try{
                int count = _socket.EndReceive(ar);
                _buffCount = _buffCount + count;
                ProcessData();
                _socket.BeginReceive(_readbuff, _buffCount, BUFFER_SIZE - _buffCount,
                    SocketFlags.None, ReceiveCb, _readbuff);
            }
            catch (Exception e){
                Console.WriteLine("ReceiveCb失败"+e.Message);
                _status = Status.None;
            }
        }
        //消息处理
        private void ProcessData() {
            //黏包分包处理
            if (_buffCount < sizeof(Int32))
                return;
            //包体长度
            Array.Copy(_readbuff,_lenBytes,sizeof(Int32));
            _msgLength = BitConverter.ToInt32(_lenBytes,0);
            if (_buffCount < sizeof(Int32) + _msgLength)
                return;
            //协议解码
            ProtocolBase protocol = _proto.Decode(_readbuff, sizeof(Int32), _msgLength);
            Debug.Log("收到消息" + protocol.GetDesc());
            lock (msgDist.msgList){
                msgDist.msgList.Add(protocol);
            }
            //清楚已处理的消息
            int count = _buffCount - _msgLength - sizeof(Int32);
            Array.Copy(_readbuff,sizeof(Int32)+_msgLength,
                _readbuff,0,count);
            _buffCount = count;
            if (_buffCount > 0){
                ProcessData();
            }
        }
        //send 及 重载
        public bool Send(ProtocolBase protocolBase) {
            if (_status != Status.Connected){
                Debug.LogError("[Connection]: 没连接就发送数据是不好的 !!!");
                return false;
            }

            byte[] b = protocolBase.Encode();
            byte[] length = BitConverter.GetBytes(b.Length);
            byte[] sendbuff = length.Concat(b).ToArray();
            _socket.Send(sendbuff);
            Debug.Log("发送消息 : " + protocolBase.GetDesc());
            return true;
        }

        public bool Send(ProtocolBase protocolBase,string cbName,MsgDistribution.Delegate cb) {
            if (_status != Status.Connected)
                return false;
            msgDist.AddOnceListener(cbName,cb);
            return Send(protocolBase);
        }

        public bool Send(ProtocolBase protocolBase, MsgDistribution.Delegate cb) {
            string cbName = protocolBase.GetName();
            return Send(protocolBase, cbName, cb);
        }
        // 心跳
        public void Update() {
            //带动消息分发
            msgDist.Update();
            //心跳
            if (_status == Status.Connected){
                if (Time.time - _lastTackTime > _heartBeatTime){
                    ProtocolBase protocol = NetMgr.GetHeatBeatProtocol();
                    Send(protocol);
                    _lastTackTime = Time.time; 
                    
                }
            }
        }
    }
}