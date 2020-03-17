using System;

namespace myServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            DataMgr.DataMgr dataMgr = new DataMgr.DataMgr();
            bool ret = dataMgr.Register("atom","123456");
            if (ret)
            {
                Console.WriteLine("注册成功");
            }
            else
            {
                Console.WriteLine("注册失败");
            }

        }
    }
}