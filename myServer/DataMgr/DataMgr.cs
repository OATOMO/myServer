using System;
using MySql.Data.MySqlClient;
using Npgsql;
using Org.BouncyCastle.Crypto.Tls;

namespace myServer.DataMgr
{
    public class DataMgr
    {
        private MySqlConnection sqlconn;
        //单例模式
        public static DataMgr instance;//构造函数实现单例和连接

        public DataMgr()
        {
            instance = this;
            ConnectMysql();
        }

        public void ConnectPG()
        {
            //数据库
            string connStr = "Host=localhost;Port=5432;Username=unityServer;Password=5432;Database=game";
            var sqlconn = new NpgsqlConnection(connStr);
            try
            {
                sqlconn.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]connect : "+e.Message);
                return;
            }
        }

        public void ConnectMysql()
        {
            //数据库
            string connStr = "Database=game;DataSource=127.0.0.1;";
            connStr += "User Id=root;Password=123456l;port=3306";
            sqlconn = new MySqlConnection(connStr);
            try
            {
                sqlconn.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]connect : "+e.Message);
                return;
            }
            
        }

    }
}