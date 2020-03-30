using System;
using System.Data.Common;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using DataMgr;
using Npgsql;
using NpgsqlTypes;
using Org.BouncyCastle.Crypto.Tls;

namespace DataMgr
{
    public class DataMgr
    {
        // private MySqlConnection mysqlConn;
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
            string connStr = "Host=localhost;Port=5433;Username=unityServer;Password=123456;Database=game";
            PGConn = new NpgsqlConnection(connStr);
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
            string cmdStr = string.Format("select * from b_user where id='{0}';",id);
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
            string cmdStr = string.Format("insert into b_user (id,pw) values (@id,@pw);");
            NpgsqlCommand cmd = new NpgsqlCommand(cmdStr,PGConn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@pw", pw);
            try {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e) {
                Console.WriteLine("[DataMgr]Register : " + e.Message);
                return false;
            }
        }
        //创建角色
        // public bool CreatePlayer(string id)
        // {
        //     //防SQL注入
        //     if (!IsSafeStr(id))
        //         return false;
        //     //序列化
        //     IFormatter formatter = new BinaryFormatter();
        //     MemoryStream stream = new MemoryStream();
        //     PlayerData playerData = new PlayerData();
        //     try
        //     {
        //         formatter.Serialize(stream,playerData);
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine("[DataMgr]CreatePlayer 序列化 : "+e.Message);
        //         return false;
        //     }
        //     byte[] byteArr = stream.ToArray();
        //     //写入数据库
        //     string cmdStr = string.Format("insert into player set id='{0}',data=@data;",id);
        //     NpgsqlCommand cmd = new NpgsqlCommand(cmdStr,PGConn);
        //     cmd.Parameters.Add("@data", NpgsqlDbType.Boolean);
        //     cmd.Parameters[0].Value = byteArr;
        //     try
        //     {
        //         cmd.ExecuteNonQuery();
        //         return true;
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine("[DataMgr]Create 写入 : " + e.Message);
        //         return false;
        //     }
        // }
        //get玩家数据
        public PlayerData GetPlayerData(string id) {
            //防SQL注入
            if (!IsSafeStr(id))
                return null;
            // 查询
            string cmdStr = string.Format("select * from b_player where id='{0}';",id);
            NpgsqlCommand cmd = new NpgsqlCommand(cmdStr,PGConn);
            try
            {
                NpgsqlDataReader dataReader = cmd.ExecuteReader();
                bool hasRows = dataReader.HasRows;
                if (!hasRows) {
                    dataReader.Close();
                    return null;
                }

                PlayerData retData = new PlayerData();
                while (dataReader.Read()) {
                     retData = new PlayerData(dataReader["part_index"].ToString());
                }
                dataReader.Close();
                return retData;
                
                
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]GetPlayerData : " + e.Message);
                return null;
            }
        }

        //检查用户名和密码
        public bool CheckPassword(string id,string pw)
        {
            //防止sql注入
            if (!IsSafeStr(id) || !IsSafeStr(pw))
                return false;
            //查询
            string cmdStr = string.Format("select * from b_user where id='{0}' and pw='{1}';",id,pw);
            NpgsqlCommand cmd = new NpgsqlCommand(cmdStr,PGConn);
            try
            {
                NpgsqlDataReader dataReader = cmd.ExecuteReader();
                bool hasRows = dataReader.HasRows;
                dataReader.Close();
                return hasRows;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]CheckPassword : " + e.Message);
                return false;
            }
        }

        //todo:保存玩家数据
        public bool SavePlayerData(Player player) {
            return false;
        }

        public bool IsSafeStr(string str)    //判定安全字符
        {
            return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\{|\}|%|@|\*|!|\' ]");
        }

    }
}