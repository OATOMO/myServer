using ServNet;

namespace DataMgr
{
    public class Player
    {
        public string id;
        //todo: 
        public Conn conn;
        //数据
        public PlayerData data;
        //临时数据
        public PlayerTempData tempData;
        //构造函数
        public Player(string id, Conn conn) {
            this.id = id;
            this.conn = conn;
            tempData = new PlayerTempData();
        }
        //发送
        public void Send(ProtocolBase protocol) {
            if (conn == null)
                return;
            ServNet.ServNet._instance.Send(conn,protocol);
        }
        //踢下线
        public static bool KickOff(string id,ProtocolBase protocol) {
            Conn[] conns = ServNet.ServNet._instance._conns;
            for (int i = 0; i < conns.Length; i++){
                if (conns[i] == null) continue;
                if (!conns[i].IsUse) continue;
                if (conns[i]._player == null) continue;
                if (conns[i]._player.id == id){
                    lock (conns[i]._player){
                        if (protocol != null){
                            conns[i]._player.Send(protocol);
                        }
                        return conns[i]._player.Logout(); //下线并保存数据
                    }
                }
            }
            return true;
        }

        public bool Logout() {
            //todo:事件处理
            // ServNet.ServNet._instance.handlePlayerEvent.OnLogout(this);
            //保存数据
            
            if (!DataMgr.instance.SavePlayerData(this))
                return false;
            //下线
            conn._player = null;
            conn.Close();
            return true;
        }
    }
}