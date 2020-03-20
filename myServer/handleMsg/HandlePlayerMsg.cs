using System;
using DataMgr;
using ServNet;
namespace handleMsg
{
    //处理角色协议
    public partial class HandlePlayerMsg
    {
        //测试用:获取分数
        //返回协议:int 分数
        public void MsgGetScore(Player player, ProtocolBase protocolBase) {
            ProtocolPbprotobuf protocolRet = new ProtocolPbprotobuf();
            protocolRet.SetName("GetScore");
            protocolRet.SetRespon(0, "", player.data.score);
            player.Send(protocolRet);
            Console.WriteLine("MsgGetScore : " + player.id + ":" + player.data.score);
        }

        //测试用:增加分数功能
        public void MsgAddScore(Player player,ProtocolBase protocolBase) {
            ProtocolPbprotobuf protocol = (ProtocolPbprotobuf) protocolBase;
            string protoName = protocol.GetName();
            //处理
            player.data.score += 1;
            Console.WriteLine("MsgAddScore : "+player.id+ " : " + player.data.score.ToString());
        }

    }
}