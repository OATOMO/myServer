using System;
using DataMgr;
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
        //注册
        //@协议参数,用户名,密码 @返回协议:-1表示失败,0表示成功
        public void MsgRegister(Conn conn,ProtocolBase protocolBase) {
            ProtocolPbprotobuf protocol = (ProtocolPbprotobuf) protocolBase;
            string protoName = protocol.GetName();
            string protoType = protocol.GetTypeStr();
            string id = protocol.buf.Register.Id;
            string pw = protocol.buf.Register.Pw;
            string strFormat = "[收到注册协议] : " + conn.GetAdress();
            Console.WriteLine(strFormat + " 用户名:" + id + " 密码:" + pw);
            
            //构建返回协议
            ProtocolPbprotobuf protocolRet = new ProtocolPbprotobuf();
            if (DataMgr.DataMgr.instance.Register(id,pw)){
                protocolRet.SetResponse(ProtocolPbprotobuf.QueryName.Register.ToString(),
                                    0,"Register success");
            }
            else{
                protocolRet.SetResponse(ProtocolPbprotobuf.QueryName.Register.ToString(),
                                    -1,"Register fail");
            }
            //创建角色
            // DataMgr.DataMgr.instance.CreatePlayer(id);  考虑一下
            //返回协议给客户端
            conn.Send(protocolRet);
        }
        //登录
        //@协议参数id,pw; @返回协议:-1失败,0成功
        public void MsgLogin(Conn conn, ProtocolBase protocolBase) {
            ProtocolPbprotobuf protocol = (ProtocolPbprotobuf) protocolBase;
            string protoName = protocol.GetName();
            string id = protocol.buf.Login.Id;
            string pw = protocol.buf.Login.Pw;
            string strFormat = "[收到登录协议] : " + conn.GetAdress();
            Console.WriteLine(strFormat + " 用户名: " + id + " 密码: " + pw);
            
            //构建返回协议
            ProtocolPbprotobuf protocolRet = new ProtocolPbprotobuf();
            //验证
            if (!DataMgr.DataMgr.instance.CheckPassword(id,pw)){
                protocolRet.SetResponse(ProtocolPbprotobuf.QueryName.Login.ToString(),
                                            -1,"login fail");
                conn.Send(protocolRet);
                return;
            }
            //是否已登录
            ProtocolPbprotobuf protocolLogout = new ProtocolPbprotobuf();
            protocolLogout.SetName(ProtocolPbprotobuf.QueryName.Logout.ToString());
            if (!Player.KickOff(id,protocolLogout)){    //不查询,直接踢
                protocolRet.SetResponse(ProtocolPbprotobuf.QueryName.Login.ToString(),
                                        -1,"踢人失败 T_T");
                conn.Send(protocolRet);
            }
            //获取玩家数据
            PlayerData playerData = DataMgr.DataMgr.instance.GetPlayerData(id);
            if (playerData == null){
                protocolRet.SetResponse(ProtocolPbprotobuf.QueryName.GetPlayerData.ToString(),
                                                        -1,"get PlayerData fail T_T");
                conn.Send(protocolRet);
                return;
            }
            conn._player = new Player(id,conn);
            conn._player.data = playerData;
            //事件触发
            ServNet.ServNet._instance.HandlePlayerEvent.OnLogin(conn._player);
            //返回
            protocolRet.SetResponse(ProtocolPbprotobuf.QueryName.Login.ToString(),0,"login success");
            conn.Send(protocolRet);
            return;
        }
        
        //登出功能
        //返回协议:0正常下线
        public void MsgLogout(Conn conn,ProtocolBase protocolBase) {
            ProtocolPbprotobuf protocolRet = new ProtocolPbprotobuf();
            protocolRet.SetResponse(ProtocolPbprotobuf.QueryName.Logout.ToString(),code:0,msg:"Logout");
            if (conn._player == null){
                conn.Send(protocolRet);
                conn.Close();
            }
            else{
                conn.Send(protocolRet);
                conn._player.Logout();
            }
        }
    }
}