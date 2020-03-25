using System.Collections.Generic;
using System.Linq;
using DataMgr;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using ServNet;

namespace Scene
{
    //代表场景,它带有ScenePlayer类型的列表,维护着场景中的角色信息
    public class Scene
    {
        //单例
        public static Scene instance;
        public Scene() {
            instance = this;
        }
        List<ScenePlayer> _list = new List<ScenePlayer>();
        //根据名字获取ScenePlayer
        private ScenePlayer GetScenePlayer(string id) {
            for (int i = 0;i < _list.Count;i++){
                if (_list[i].id == id){
                    return _list[i];
                }
            }
            return null;
        }
        //添加Scene角色
        public void AddPlayer(string id) {
            lock (_list){
                ScenePlayer p = new ScenePlayer();
                p.id = id;
                _list.Add(p);
            }
        }
        //删除Scene角色
        public void DelPlayer(string id) {
            lock (_list){
                ScenePlayer p = GetScenePlayer(id);
                if (p != null)
                    _list.Remove(p);
            }
            ProtocolPbprotobuf protocol = new ProtocolPbprotobuf();
            protocol.SetResponse(ProtocolPbprotobuf.QueryName.PlayerLeave.ToString(),0,id);
            ServNet.ServNet._instance.Broadcast(protocol);
        }
        //发送列表
        public void SendPlayerList(Player player) {
            int count = _list.Count;
            ProtocolPbprotobuf protocol = new ProtocolPbprotobuf();
            protocol.SetName(ProtocolPbprotobuf.QueryName.GetList.ToString());
            for (int i = 0; i < count; i++){
                ScenePlayer p = _list[i];
                protocol.buf.PlayerInfos[p.id] = new PlayerInfo()
                {
                    Id = p.id,
                    NickName = "",
                    Pos = new Pos(){X = p.x,Y = p.y,Z = p.z}
                };
                player.Send(protocol);
            }
        }
        //更新信息
        public void UpdateInfo(string id,float x,float y,float z) {
            int count = _list.Count;
            ProtocolPbprotobuf protocol = new ProtocolPbprotobuf();
            ScenePlayer p = GetScenePlayer(id);
            if (p == null)
                return;
            p.x = x;
            p.y = y;
            p.z = z;
        }
    }
}