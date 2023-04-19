using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Display
{
    public class Schedule
    {
        public string Id { get; set; }     //Schedule ID, định danh các schedule khác nhau
        public long CreatedTime { get; set; }        //Thời điểm tạo lịch (UTC second)
        public int Duration { get; set; }       //Thời lượng phát (s) -> bỏ qua nếu có set loopNum
        public int Loops { get; set; }        //Số lần phát
        public int IdleTime { get; set; }       //Thời gian nghỉ giữa 2 lần phát (s)
        public bool IsDaily { get; set; }    //Phát lặp lại hàng ngày
        public bool IsBroadcast { get; set; }    //Phát quảng bá cho cả room cùng nghe
        public long From { get; set; }          //Có hiệu lực từ thời điểm (UTC second)
        public long To { get; set; }            //Có hiệu lực đến thời điểm (UTC second)
        public bool IsActive { get; set; }       //Còn hiệu lực hay không
        public List<int> Times { get; set; } //Thời điểm phát trong ngày (số giây trôi qua từ 0h)
        public List<int> Days { get; set; }  //Ngày phát trong tuần (T2 -> CN) nếu isDaily = true
        public List<string> Songs { get; set; }  //Danh sách nội dung
        public string Path { get; set; }          //Remote folder on MinIO server
        public string Title { get; set; }

        public DisplayScheduleType ScheduleType { get; set; } // Loại bản tin
        public string TextContent { get; set; }
        public string MediaContent { get; set; }
        public bool FullScreen { get; set; }
        public string ColorValue { get; set; }
    }

    public class Schedule_TxMessage
    {
        public int id;          /* Id bản tin - xem danh sách defined */

        public long time;       /* Thời gian bản tin in millisecond UTC */

        public string sender;      /* Imei/ID của đối tượng gửi tin */

        public ScheduleMessage message;    /* Bản tin chi tiết */
    }
    public class Ping_TxMessage
    {
        public int Id;          /* Id bản tin - xem danh sách defined */

        public long Time;       /* Thời gian bản tin in millisecond UTC */

        public string Sender;      /* Imei/ID của đối tượng gửi tin */

        public PingMessage Message;    /* Bản tin chi tiết */
    }

    public class ScheduleMessage
    {
        public bool isLeaderOnly;
        public Schedule schedule;
    }
    public enum DisplayScheduleType
    {
        BanTinThongBao,
        BanTinVanBan,
        BanTinVideo,
        BanTinHinhAnh,
    }
    public class PingMessage
    {
        public string Sn;              //serial number
        public int Type;                   //Loại bộ thu (0)/phát (1)
        public string Ip;                  //IP của node
        public string Id;              //deviceId
        public string Name;            //deviceName
        public string Mid;      //master config id
        public string Room;                //Tên room đang join, nếu đang ko join thì empty
        public bool Join;             //Trạng thái join room meeting
        public string Stmt;     //Imei của master đang stream, ko stream thì empty
        public string Stlk;       //Link đang streaming, nếu đang ko stream thì empty
        public int Stst;            //Trạng thái streaming
        public string Scid;  //SheduleID đang phát offline, nếu đang ko phát thì empty
        public string Scf;    //File đang play theo lịch
        public int Scst;      //Trạng thái play theo lịch offline
        public bool Mic;            //Trạng thái Mic On/Off
        public bool Spk;            //Trạng thái Loa meeting On/Off
        public bool Cam;            //Trạng thái Cam meeting On/Off
        public string Ver;          //Phiên bản app
        public int Vl1;                //Âm lượng loa thoại (%)
        public int Vl2;               //Âm lượng loa phát nhạc (%)
        public int Hw;               //Phiên bản phần cứng
    }
    public class DeviceConfigMessage
    {
        public string Id;
        public int DeviceType; /* 0: Thiết bị thu, 1: Thiết bị phát */
        public string Name;
        public int Priority;
        public List<Group> Groups;
    }

    public class SetConfigMessage
    {
        public string Cmd;  //nội dung bản tin config dạng string (SET,x,(abc)...)
        public List<ParamConfig> Obj;
    }
    public class ParamConfig
    {
        public int C;
        public string V;
    }


    public class Group
    {
        public string Id;
        public int MasterType; /* 0: Nhóm, 1: Thiết bị master, 2: User */
        public int Priority;
        public string Name;
    }
    public class StreamCommandMessage
    {
        public string MasterImei;      //imei master ra lệnh
        public commandCode CmdCode;            //Mã lệnh điều khiển

        public List<string> Serial = new List<string>(); //Danh sách imei thực hiện stream
        public bool IsAll;          //Lệnh cho tất cả cùng stream
        public StreamReqInfo StreamInfo;    //Thông tin stream
        public int Volume;
        public int DelayTurnOnRelayOnPrepair;   //Thời gian delay bật relay 2
    }
    public enum commandCode     //Các mã lệnh điều khiển từ master cmdCode:
    {
        CMD_STREAM_PREPAIR = 1,
        CMD_STREAM_START,
        CMD_STREAM_RUNNING,
        CMD_STREAM_STOP,
    }
    public class StreamReqInfo /*implements Parcelable*/
    {
        public string Name;
        public string Uri;      /* Link stream bắt buộc phải có */
        public string Extension;
        public string Drm_scheme;
        public string Drm_license_uri;
        public bool Drm_force_default_license_uri;
        public bool Drm_session_for_clear_content;
        public bool Drm_multi_session;
        public string Subtitle_uri;
        public string Subtitle_mime_type;
        public string Subtitle_language;
        public long Clip_start_position_ms;
        public long Clip_end_position_ms;
    }
}
