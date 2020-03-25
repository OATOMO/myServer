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
            response = 3;
        }
        */
        public MsgType GetType()
        {
            return buf.Type;
        }
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
                case 3: result = "Response";
                    break;
                default: result = "";
                    break;
            }
            return result;
        }
        //set login 登录协议
        public void SetLogin(string id, string pw) {
            buf.Query = QueryName.Login.ToString();
            buf.Type = MsgType.ConnMsg;
            buf.Login = new Login();
            buf.Login.Id = id;
            buf.Login.Pw = pw;
        }
        //set Register 
        public void SetRegister(string id,string pw,string phone,string email) {
            buf.Query = QueryName.Register.ToString();
            buf.Type = MsgType.ConnMsg;
            buf.Register = new Register();
            buf.Register.Id = id;
            buf.Register.Pw = pw;
            buf.Register.Phone = phone;
            buf.Register.Email = email;
        }

        //set respon
        public void SetResponse(string query, int code=0,string msg="",int value=0) {
            buf.Query = query;
            buf.Type = MsgType.Response;
            buf.Response = new Response();
            buf.Response.Code = code;
            buf.Response.Msg = msg;
            buf.Response.Value = value;
        }
        public enum QueryName
        {
            Login,
            Logout,
            Register,
            GetPlayerData,
            PlayerLeave,
            GetList,
            UpdateInfo,
        }
    }
}