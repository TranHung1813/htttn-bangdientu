using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Display
{
    class SqLiteDataAccess
    {
        // Connection String Config
        private static string DatabasePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"BangDienTu.db");

        //*****************************************************************************************************************
        //****************************************** Access to Groups Infomation *******************************************
        public static List<DataUser_Groups_Info> Load_Groups_Info()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<DataUser_Groups_Info>("select * from Groups_Info", new DynamicParameters());
                return output.ToList();
            }
        }

        public static void AddInfo_Groups(DataUser_Groups_Info info)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                try
                {
                    cnn.Execute("insert into Groups_Info ( GroupId, Priority, Name, MasterType) values ( @GroupId, @Priority, @Name, @MasterType)", info);
                }
                catch
                { }
            }
        }
        public static void SaveInfo_Groups(DataUser_Groups_Info info)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                int id = cnn.Query<int>("select Id from Groups_Info where Id like @Id", new { Id = info.Id }).FirstOrDefault();

                if (id == info.Id)
                {
                    cnn.Execute("update Groups_Info set GroupId= @GroupId, Priority = @Priority, Name = @Name, MasterType = @MasterType where Id = @Id", info);
                }
                else
                {
                    cnn.Execute("insert into Groups_Info ( GroupId, Priority, Name, MasterType) values ( @GroupId, @Priority, @Name, @MasterType)", info);
                }
            }
        }
        //*****************************************************************************************************************
        //*****************************************************************************************************************
        private static string LoadConnectionString(string id = "Default")
        {
            return "Data Source=" + DatabasePath + ";Version=3;";
        }
    }
}
