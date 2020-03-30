using System;
using DataMgr;
using Google.Protobuf.WellKnownTypes;
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
            protocolRet.SetResponse("GetScore",0, "", player.data.score);
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
        //获取玩家列表
        public void MsgGetList(Player player,ProtocolBase protocolBase) {
            Scene.Scene.instance.SendPlayerList(player);
        }
        //更新信息
        public void MsgUpdateInfo(Player player,ProtocolBase protocolBase) {
            ProtocolPbprotobuf protocol = new ProtocolPbprotobuf();
            string protoName = protocol.GetName();
            foreach (var value in protocol.buf.PlayerInfos.Values){
                Scene.Scene.instance.UpdateInfo(value.Id, value.Pos.X, value.Pos.Y, value.Pos.Z);
                //广播
                ProtocolPbprotobuf protocolRet = new ProtocolPbprotobuf();
                protocolRet.SetName(ProtocolPbprotobuf.QueryName.UpdateInfo.ToString());
                protocolRet.buf.PlayerInfos[value.Id] = new PlayerInfo()
                {
                    Id = value.Id,
                    NickName = "",
                    Pos = new Pos(){X = value.Pos.X,Y = value.Pos.Y,Z = value.Pos.Z}
                };
                ServNet.ServNet._instance.Broadcast(protocolRet);
            }
        }
    }
}