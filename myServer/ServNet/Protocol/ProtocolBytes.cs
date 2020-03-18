using System;
using System.Linq;
using System.Text;

namespace myServer.ServNet{
    //字节流协议类型
    public class ProtocolBytes: ProtocolBase{
        //传输的字节流
        public byte[] bytes;
        //解码器
        public override ProtocolBase Decode(byte[] readbuff, int start, int length) {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.bytes = new byte[length];
            Array.Copy(readbuff,start,protocol.bytes,0,length);
            return protocol;
        }
        //编码器
        public override byte[] Encode() {
            return bytes;
        }
        //协议名称
        public override string GetName() {
            return GetString(0);
        }
        //描述
        public override string GetDesc() {
            string str = "";
            if (bytes == null) {
                return str;
            }

            for (int i = 0; i < bytes.Length; i++) {
                int b = (int) bytes[i];
                str += b.ToString() + " ";
            }

            return str;
        }
        //添加字符串
        public void AddString(string str) {
            Int32 len = str.Length;
            byte[] lenBytes = BitConverter.GetBytes(len);
            byte[] strBytes = Encoding.UTF8.GetBytes(str);
            if (bytes == null) {
                bytes = lenBytes.Concat(strBytes).ToArray();
            }
            else {
                bytes = bytes.Concat(lenBytes).Concat(strBytes).ToArray();
            }
        }
        //从字节数组的start处开始读取字符串
        public string GetString(int start, ref int end) {
            if (bytes == null)
                return "";
            if (bytes.Length < start + sizeof(Int32))
                return "";
            Int32 strLen = BitConverter.ToInt32(bytes, start);
            if (bytes.Length < start + sizeof(Int32) + strLen)
                return "";
            string str = Encoding.UTF8.GetString(bytes, start + sizeof(Int32), strLen);
            end = start + sizeof(Int32) + strLen;
            return str;
        }

        public string GetString(int start) {
            int end = 0;
            return GetString(start, ref end);
        }
    }
}