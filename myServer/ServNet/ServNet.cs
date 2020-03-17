using System;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace myServer.ServNet
{
    public class ServNet
    {
        //监听套接字
        public Socket listenfd;

        //客户端连接
        public Conn[] conns;

        //最大连接数
        public int maxConn = 50;

        //单例 (只是为了方便调用)
        public static ServNet instance;

        public ServNet()
        {
            instance = this;
        }

        //获取连接池索引,负数表示获取失败, 参考6章
        public int NewIndex()
        {

        }

        //开启服务器
        public void Start(string host, int port)
        {

        }

        //Accept回调
        private void AcceptCb(IAsyncResult ar)
        {

        }

        //关闭
        public void Close()
        {
            for (int i = 0; i < conns.Length; i++)
            {
                Conn conn = conns[i];
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
                    int count = conn.socket.EndReceive(ar);
                    //关闭信号
                    if (count <= 0)
                    {
                    }

                    conn.bufferCount += count;
                    ProcessData(conn);
                    conn.socket.BeginReceive();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        //
        private void ProcessData(Conn conn)
        {
            //小于长度字节
            if (conn.bufferCount < sizeof(Int32))
            {
                return;
            }
            //消息长度
            Array.Copy(conn.readBuffer,conn.LenBytes,sizeof(Int32));
            conn.msgLenth = BitConverter.ToInt32(conn.LenBytes, 0);
            if (conn.bufferCount < conn.msgLenth + sizeof(UInt32))
            {
                return;
            }
            //处理消息
            string str = System.Text.Encoding.UTF8.GetString(
                                conn.readBuffer, sizeof(Int32), conn.msgLenth);
            Console.WriteLine("收到消息 : ["+ conn.GetAdress() + "]" + str);
            Send(conn, str);
            //清除已处理的消息
            int count = conn.bufferCount - conn.msgLenth - sizeof(Int32);
            Array.Copy(conn.readBuffer,sizeof(Int32)+conn.msgLenth,conn.readBuffer,0,count);
            if (conn.bufferCount > 0 )
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
                conn.socket.BeginSend(sendbuff,0,sendbuff.Length,SocketFlags.None,null,null);
            }
            catch (Exception e)
            {
                Console.WriteLine("[发送消息] " + conn.GetAdress() + ":" + e.Message);
            }
        }

    }
}