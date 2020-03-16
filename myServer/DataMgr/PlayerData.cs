using System;

namespace myServer.DataMgr
{
    [Serializable] //序列化数据
    public class PlayerData
    {
        public int score = 0;
        public PlayerData() {
            score = 100;
        }
        

    }
}