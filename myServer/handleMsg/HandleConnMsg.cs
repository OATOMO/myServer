using System;
using ServNet;
namespace handleMsg
{
    //处理连接协议 
    public class HandleConnMsg
    {
        //心跳,参数:无
        public void MsgHeatBeat(Conn conn, ProtocolBase protocolBase) {
            conn._lastTickTime = Sys.GetTimeStamp();
            Console.WriteLine("[更新心跳时间] : " + conn.GetAdress());
        }
    }
}