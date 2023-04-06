using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Display
{
    public class DataUser_Groups_Info
    {
        public int Id { get; set; }
        public string GroupId { get; set; }
        public int Priority { get; set; }
        public string Name { get; set; }
        public int MasterType { get; set; }
    }

    public class DataUser_SavedFiles
    {
        public int Id { get; set; }
        public string ScheduleId { get; set; }
        public string PathLocation { get; set; } /* Path File Downloaded */
        public string Link { get; set; }
    }

    public class DataUser_DeviceInfo
    {
        public int Id { get; set; }
        public string NodeId { get; set; }
        public string NodeName { get; set; }
    }
}
