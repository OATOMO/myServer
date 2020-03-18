using System;
using myServer.test_client;

namespace myServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            test_servNet_1();
        }

        public static void test_servNet_1()
        {
            ServNet.ServNet servNet = new ServNet.ServNet();
            servNet.Start("127.0.0.1",6666);
            Console.ReadLine();
        }

        public static void test_client_1()
        {
            myServer.test_client.client client = new client();
            client.Connection();
            Console.ReadKey();
            client.Send("atom");
            Console.ReadKey();
        }

        public static void test_dataMgr_1()
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