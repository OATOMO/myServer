using System;

namespace DataMgr
{
    [Serializable] //序列化数据
    public class PlayerData{
        public string partIndex;
        public int score = 0;
        public PlayerData(string _partIndex = "") {
            score = 100;
            partIndex = _partIndex;
        }
        

    }
}