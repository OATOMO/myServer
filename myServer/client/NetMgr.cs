using ServNet;

namespace client{
    public class NetMgr{
        public static Connection srvConn = new Connection();
        // public static Connection platformConn = new Connection();
        public static void Update() {
            srvConn.Update();
            //platformConn.Update();
        }
        //心跳
        public static ProtocolBase GetHeatBeatProtocol() {
            ProtocolPbprotobuf protocol = new ProtocolPbprotobuf();
            protocol.SetName("HeatBeat");
            return protocol;
        }
    }
}