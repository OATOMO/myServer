using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using Enum = Google.Protobuf.WellKnownTypes.Enum;

namespace ServNet{
    public class ProtocolPbprotobuf: ProtocolBase
    {
        //传输的protobuf
        public Msg buf = new Msg();
        //解码器
        public override ProtocolBase Decode(byte[] readbuff, int start, int length) {
            ProtocolPbprotobuf protocol = new ProtocolPbprotobuf();
            byte[] bytes = new byte[length];
            Array.Copy(readbuff,start,bytes,0,length);
            protocol.buf = Msg.Parser.ParseFrom(bytes);
            return (ProtocolBase) protocol;
        }
        //编码器
        public override byte[] Encode() {
            return buf.ToByteArray();
        }
        //协议名称
        public override string GetName() {
            return buf.Query;
        }
        public void SetName(string name) {
            buf.Query = name;
        }

        public override string GetDesc() {
            return GetTypeStr() + GetName();
        }

        //协议类型
        /*
         enum msgType{
            connMsg = 0;
            playerEvent = 1;
            playerMsg = 2;
        }
        */
        public int GetTypeIndex()
        {
            return (int)buf.Type;
        }
        public string GetTypeStr() {
            string result = "";
            switch ((int)buf.Type){
                case 0: result = "HandleConnMsg";
                    break;
                case 1: result = "HandlePlayerEvent";
                    break;
                case 2: result = "HandlePlayerMsg";
                    break;
                default: result = "";
                    break;
            }
            return result;
        }
        
        //set respon
        public void SetRespon(int code=0,string msg="",int value=0) {
            buf.Respon.Code = code;
            buf.Respon.Msg = msg;
            buf.Respon.Value = value;
        }
    }
}