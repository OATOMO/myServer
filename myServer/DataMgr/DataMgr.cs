using System;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using Npgsql;
using Org.BouncyCastle.Crypto.Tls;

namespace myServer.DataMgr
{
    public class DataMgr
    {
        private MySqlConnection mysqlConn;
        private NpgsqlConnection PGConn;
        //单例模式
        public static DataMgr instance;//构造函数实现单例和连接

        public DataMgr()
        {
            instance = this;
            ConnectPG();
        }

        public void ConnectPG()
        {
            //数据库
            string connStr = "Host=localhost;Port=5432;Username=unityServer;Password=5432;Database=game";
            var PGConn = new NpgsqlConnection(connStr);
            try
            {
                PGConn.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]connect : "+e.Message);
                return;
            }
        }
        
        
        //是否存在用户
        private bool CanRegister(string id)
        {
            //防sql注入
            if (!IsSafeStr(id))
                return false;
            //查询id是否存在
            string cmdStr = string.Format("select * from user where id='{0}';",id);
            NpgsqlCommand cmd = new NpgsqlCommand(cmdStr,PGConn);
            try
            {
                NpgsqlDataReader dataReader = cmd.ExecuteReader();
                bool hasRows = dataReader.HasRows;
                dataReader.Close();
                return !hasRows;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]CanRegister fail : " + e.Message);
                return false;
            }
        }

        //注册
        public bool Register(string id, string pw)
        {
            //防sql注入
            if (!IsSafeStr(id) || !IsSafeStr(pw))
            {
                Console.WriteLine("[DataMgr]Register: 使用非法字符");
                return false;
            }
            //能否注册
            if (!CanRegister(id)) {
                Console.WriteLine("[DataMgr]Register!CanRegister");
                return false;
            }
            //写入数据库User表
            string cmdStr = string.Format("insert into user set id = '{0}' , pw='{1}';",id,pw);
            NpgsqlCommand cmd = new NpgsqlCommand(cmdStr);
            try {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e) {
                Console.WriteLine("[DataMgr]Register" + e.Message);
                return false;
            }
        }
        //创建角色
        public bool CreatePlayer(string id)
        {
            //防SQL注入
            if (!IsSafeStr(id))
                return false;
        }

        public bool IsSafeStr(string str)    //判定安全字符
        {
            return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\{|\}|%|@|\*|!|\' ]");
        }

    }
}