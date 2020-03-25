using DataMgr;

namespace handleMsg
{
    //玩家事件类 ,上下线
    public partial class HandlePlayerEvent
    {
        //上线
        public void OnLogin(Player player) {
            Scene.Scene.instance.AddPlayer(player.id);
        }
        //下线
        public void OnLogout(Player player) {
            Scene.Scene.instance.DelPlayer(player.id);
        }
    }
}