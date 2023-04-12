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
        //****************************************** Access to Schedule Message Infomation *******************************************
        public static List<DataUser_ScheduleMessage> Load_ScheduleMessage_Info()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<DataUser_ScheduleMessage>("select * from ScheduleMessage", new DynamicParameters());
                return output.ToList();
            }
        }

        public static void AddInfo_ScheduleMessage(DataUser_ScheduleMessage info)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                try
                {
                    cnn.Execute("insert into ScheduleMessage ( JsonData, Priority, ScheduleId) values ( @JsonData, @Priority, @ScheduleId)", info);
                }
                catch
                { }
            }
        }
        public static void DeleteInfo_ScheduleMessage(DataUser_ScheduleMessage info)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                try
                {
                    cnn.Execute("delete from ScheduleMessage where Id like @Id", new { Id = info.Id });
                }
                catch (Exception ex)
                { }
            }
        }
        public static void SaveInfo_ScheduleMessage(DataUser_ScheduleMessage info)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                int id = cnn.Query<int>("select Id from ScheduleMessage where Id like @Id", new { Id = info.Id }).FirstOrDefault();

                if (id == info.Id)
                {
                    cnn.Execute("update ScheduleMessage set JsonData= @JsonData, Priority = @Priority, ScheduleId = @ScheduleId where Id = @Id", info);
                }
                else
                {
                    cnn.Execute("insert into ScheduleMessage ( JsonData, Priority, ScheduleId) values ( @JsonData, @Priority, @ScheduleId)", info);
                }
            }
        }
        //*****************************************************************************************************************
        //****************************************** Access to Device Infomation *******************************************
        public static DataUser_DeviceInfo Load_Device_Info()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                try
                {
                    DataUser_DeviceInfo output = cnn.Query<DataUser_DeviceInfo>("select * from Device_Info", new DynamicParameters()).FirstOrDefault();
                    return output;
                }
                catch
                {

                }

                return null;
            }
        }

        public static void SaveInfo_Device(DataUser_DeviceInfo info)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                int id = cnn.Query<int>("select Id from Device_Info where Id like @Id", new { Id = 1 }).FirstOrDefault();

                if (id != 0)
                {
                    cnn.Execute("update Device_Info set  NodeId = @NodeId, NodeName = @NodeName where Id = 1", info);
                }
                else
                {
                    cnn.Execute("insert into Device_Info ( NodeId, NodeName) values ( @NodeId, @NodeName)", info);
                }
            }
        }
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
        //****************************************** Access to Saved File Infomation *******************************************
        public static List<DataUser_SavedFiles> Load_SavedFiles_Info()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<DataUser_SavedFiles>("select * from SavedFiles", new DynamicParameters());
                return output.ToList();
            }
        }

        public static void AddInfo_SavedFiles(DataUser_SavedFiles info)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                try
                {
                    cnn.Execute("insert into SavedFiles ( ScheduleId, PathLocation, Link) values ( @ScheduleId, @PathLocation, @Link)", info);
                }
                catch
                { }
            }
        }
        public static void SaveInfo_SavedFiles(DataUser_SavedFiles info)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                int id = cnn.Query<int>("select Id from SavedFiles where Id like @Id", new { Id = info.Id }).FirstOrDefault();

                if (id == info.Id)
                {
                    cnn.Execute("update SavedFiles set ScheduleId= @ScheduleId, PathLocation = @PathLocation, Link = @Link where Id = @Id", info);
                }
                else
                {
                    cnn.Execute("insert into SavedFiles ( ScheduleId, PathLocation, Link) values ( @ScheduleId, @PathLocation, @Link)", info);
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
