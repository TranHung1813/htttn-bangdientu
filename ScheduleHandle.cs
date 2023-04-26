using LibVLCSharp.Shared;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Display
{
    public class ScheduleHandle
    {
        private string PathFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Files");
        private string _FileName = "";

        List<ScheduleMsg_Type> _schedule_msg_List = new List<ScheduleMsg_Type>();
        public ScheduleHandle()
        {

        }

        public void Load_ScheduleMessageInfo()
        {
            List<DataUser_ScheduleMessage> messages = SqLiteDataAccess.Load_ScheduleMessage_Info();
            if (messages != null)
            {
                foreach (var message in messages)
                {
                    Schedule msg = JsonConvert.DeserializeObject<Schedule>(message.JsonData);
                    Schedule(msg, message.Priority);
                }
            }
            else
            {
                // Null Handle
            }
        }

        private void Delete_ScheduleMessage_inDB(string MsgId)
        {
            List<DataUser_ScheduleMessage> messages = SqLiteDataAccess.Load_ScheduleMessage_Info();
            if (messages != null)
            {
                int index = messages.FindIndex(s => (s.ScheduleId == MsgId));
                if (index != -1)
                {
                    // Save lai
                    SqLiteDataAccess.DeleteAll_ScheduleMessage();
                    if (messages.Count > 1)
                    {
                        messages.RemoveAt(index);
                        int Id = 1;
                        foreach (var msg in messages)
                        {
                            DataUser_ScheduleMessage info_Save = new DataUser_ScheduleMessage();

                            info_Save.Id = Id++;
                            info_Save.ScheduleId = msg.ScheduleId;
                            info_Save.JsonData = msg.JsonData;
                            info_Save.Priority = msg.Priority;

                            SqLiteDataAccess.SaveInfo_ScheduleMessage(info_Save);
                        }
                    }
                }
            }
            else
            {
                // Null Handle
            }
        }

        public void Schedule(Schedule message, int Priority)
        {
            if (message.IsActive != true)
            {
                DeleteMessage_by_Id(message.Id, "isActive = False");
                OnNotify_Time2Delete(message.Id, true);

                return;
            }

            //replace if message ID is already existed
            int index = _schedule_msg_List.FindIndex(s => s.msg.Id == message.Id);
            if (index != -1)
            {
                DeleteMessage_by_Id(message.Id, "Trùng ScheduleId với bản tin trong DB");
                OnNotify_Time2Delete(message.Id, false);
            }
            ScheduleMsg_Type new_messsage = new ScheduleMsg_Type();
            new_messsage.msg = message;
            new_messsage.Priority = Priority;

            try
            {
                ValidTime_Handle(new_messsage);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "ValidTime_Handle");
            }
        }

        private void MessageHandle(ScheduleMsg_Type message)
        {
            // Tinh lai Times theo từng loại bản tin (Times của Server chưa chuẩn)
            List<int> NewTimes = Caculate_TimeList(message.msg);
            // Chuyển các mốc thời gian trong tuần về dạng giây (số giây trôi qua từ 0h00 T2)
            int NearestTime = 0;
            List<int> TimeList_perWeek = new List<int>();
            if (message.msg.IsDaily == true)
            {
                // Daily
                for (int CountDay = 0; CountDay < message.msg.Days.Count; CountDay++)
                {
                    TimeList_perWeek.AddRange(NewTimes.Select(x => x + 24 * 3600 * (message.msg.Days[CountDay] - 1)).ToList());
                }
                TimeList_Handle(TimeList_perWeek, ref message.TimeList, ref message.WeeklyTimeList, ref NearestTime);
            }
            else
            {
                // Today
                TimeList_perWeek.AddRange(NewTimes);
                TimeList_Handle(TimeList_perWeek, ref message.TimeList, ref NearestTime);

                // Check xem Current Time có chung ngày với FromTime không?
                long CurrentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                DateTime FromTime = UnixTimeStampToDateTime(message.msg.From);
                if (DateTime.Now.DayOfWeek != FromTime.DayOfWeek) return;
            }

            // Còn Time Valid && Đang chạy dở => Chạy tiếp
            if (message.msg.ScheduleType == DisplayScheduleType.BanTinVideo)
            {
                if ((-NearestTime) < message.msg.Duration && NearestTime <= 0)
                {
                    // Notify First Time to Play (StartPosition = Điểm bắt đầu chạy tiếp)
                    int StartPosition = (int)(-NearestTime);
                    if (StartPosition <= 3) StartPosition = 0;
                    int CurrentSecond = (int)DateTime.UtcNow.TimeOfDay.TotalSeconds;
                    int Duration = (int)((message.msg.Duration + message.msg.IdleTime) * message.msg.Loops - (CurrentSecond - NewTimes[0]) - message.msg.IdleTime);
                    OnNotify_Time2Play(message.msg.Id, message.Priority, message.msg.ScheduleType, message.msg.TextContent, message.msg.Songs, message.msg.FullScreen,
                                       message.msg.IdleTime, message.msg.Loops, Duration, message.msg.ColorValue, message.msg.Title, message.msg.TextContent, StartPosition);
                }
            }
            else if (message.msg.ScheduleType == DisplayScheduleType.BanTinThongBao ||
                     message.msg.ScheduleType == DisplayScheduleType.BanTinVanBan ||
                     message.msg.ScheduleType == DisplayScheduleType.BanTinHinhAnh)
            {
                if ((-NearestTime) < message.msg.Duration && NearestTime <= 0)
                {
                    // Notify First Time to Play (StartPosition = Điểm bắt đầu chạy tiếp)
                    int Duration = message.msg.Duration - (int)(-NearestTime);
                    OnNotify_Time2Play(message.msg.Id, message.Priority, message.msg.ScheduleType, message.msg.TextContent, message.msg.Songs, message.msg.FullScreen,
                                       message.msg.IdleTime, message.msg.Loops, Duration, message.msg.ColorValue, message.msg.Title, message.msg.TextContent, 0);
                }
            }

            // Init Timer
            if (message.TimeList.Length <= 0) return;

            message.Schedule_Timer = new Timer();
            message.Schedule_Timer.Interval = message.TimeList[0] * 1000;
            message.Schedule_Timer.Tick += delegate (object sender, EventArgs e)
            {
                NotifyPlay(message);
                Timer this_timer = (Timer)sender;
                if (++message.CountTime >= message.TimeList.Length)
                {
                    if (message.msg.IsDaily == true)
                    {
                        message.CountTime = 0;
                        message.TimeList = message.WeeklyTimeList;
                    }
                    else
                    {
                        this_timer.Stop();
                        return;
                    }
                }
                // Set Interval to run to next Time in TimeList
                this_timer.Interval = message.TimeList[message.CountTime] * 1000;
            };
            message.Schedule_Timer.Start();
            // Add message to Schedule List (replace if message ID is already existed)
            //replace if message ID is already existed
            int index = _schedule_msg_List.FindIndex(s => s.msg.Id == message.msg.Id);
            if (index != -1)
            {
                _schedule_msg_List[index] = message;
            }
            else
            {
                _schedule_msg_List.Add(message);
            }
        }
        private List<int> Caculate_TimeList(Schedule message)
        {
            List<int> TimesList_Return = new List<int>();
            if (message.ScheduleType == DisplayScheduleType.BanTinThongBao || message.ScheduleType == DisplayScheduleType.BanTinVanBan)
            {
                TimesList_Return.Add(message.Times[0]);
            }
            else if (message.ScheduleType == DisplayScheduleType.BanTinHinhAnh)
            {
                TimesList_Return = message.Times;
            }
            else if (message.ScheduleType == DisplayScheduleType.BanTinVideo)
            {
                if (message.Duration == 0)
                {
                    TimesList_Return.Add(message.Times[0]);
                }
                else
                {
                    for (int CountTimeLoop = 0; CountTimeLoop < message.Loops; CountTimeLoop++)
                    {
                        int Value = message.Times[0] + (message.Duration + message.IdleTime) * CountTimeLoop;
                        TimesList_Return.Add(Value);
                    }
                }
            }
            return TimesList_Return;
        }
        private async Task<Task<int>> Duration_Calculate(string url)
        {
            int Duration = 0;
            Core.Initialize();
            var tcs = new TaskCompletionSource<int>();

            LibVLC _libVLC = new LibVLC();
            MediaPlayer _mp = new MediaPlayer(_libVLC);

            try
            {
                _mp.Play(new Media(_libVLC, new Uri(url)));
                _mp.Playing += (o, e) =>
                {
                    Duration = (int)(_mp.Length / 1000) + 1;
                    tcs.TrySetResult(Duration);
                    _mp.Stop();
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "Duration_Calculate: {A}", url);
            }

            return tcs.Task;
        }
        private void NotifyPlay(ScheduleMsg_Type message)
        {
            // Notify First Time to Play
            OnNotify_Time2Play(message.msg.Id, message.Priority, message.msg.ScheduleType, message.msg.TextContent, message.msg.Songs, message.msg.FullScreen,
                               message.msg.IdleTime, message.msg.Loops, message.msg.Duration, message.msg.ColorValue, message.msg.Title, message.msg.TextContent, 0);
        }

        private void TimeList_Handle(List<int> TimeList, ref int[] NewTimeList, ref int[] WeeklyTimeList, ref int NearestValue)
        {
            if (TimeList.Count <= 0) return;

            int d = (int)DateTime.Now.DayOfWeek;
            int CurrentSecond = (int)DateTime.UtcNow.TimeOfDay.TotalSeconds + 24 * 3600 * (d - 1);
            int TotalSecond_1Week = 7 * 24 * 3600;

            TimeList = TimeList.Distinct().ToList();
            TimeList.Sort();

            // Every week Handle

            WeeklyTimeList = new int[TimeList.Count];

            WeeklyTimeList[0] = TotalSecond_1Week + TimeList[0] - TimeList[TimeList.Count - 1];
            for (int CountValue = 1; CountValue < TimeList.Count; CountValue++)
            {
                WeeklyTimeList[CountValue] = TimeList[CountValue] - TimeList[CountValue - 1];
            }

            // This week Time List Handle
            TimeList = TimeList.Select(x => x - CurrentSecond).ToList();

            // Lấy giá trị gần giá trị hiện tại nhất
            int index = TimeList.FindLastIndex(i => i <= 0);
            if (index == -1) NearestValue = 1;
            else NearestValue = TimeList[index];

            if (TimeList[TimeList.Count - 1] < 0)
            {
                // Nếu toàn bộ mốc thời gian đã quá hạn => phát vào tuần sau
                TimeList = TimeList.Select(x => x + TotalSecond_1Week).ToList();
            }
            else
            {
                // Xoa het gia tri <= 0 tương đương với lịch đã quá hạn
                TimeList.RemoveAll(x => x <= 0);
            }

            // Tính mốc thời gian cho Timer
            NewTimeList = new int[TimeList.Count];

            NewTimeList[0] = TimeList[0];
            for (int CountValue = 1; CountValue < TimeList.Count; CountValue++)
            {
                NewTimeList[CountValue] = TimeList[CountValue] - TimeList[CountValue - 1];
            }

            NewTimeList = NewTimeList.Where(x => x > 0).ToArray();
        }

        private void TimeList_Handle(List<int> TimeList, ref int[] NewTimeList, ref int NearestValue)
        {
            if (TimeList.Count <= 0) return;

            int CurrentSecond = (int)DateTime.UtcNow.TimeOfDay.TotalSeconds;

            TimeList = TimeList.Distinct().ToList();
            TimeList.Sort();

            // This week Time List Handle
            TimeList = TimeList.Select(x => x - CurrentSecond).ToList();

            // Lấy giá trị gần giá trị hiện tại nhất
            int index = TimeList.FindLastIndex(i => i <= 0);
            if (index == -1) NearestValue = 1;
            else NearestValue = TimeList[index];
            // Xoa het gia tri < 0 tương đương với lịch đã quá hạn
            TimeList.RemoveAll(x => x <= 0);

            NewTimeList = new int[TimeList.Count];

            // Nếu toàn bộ mốc thời gian đã quá hạn => không xử lý nữa
            if (TimeList.Count <= 0) return;

            // Tính mốc thời gian cho Timer
            NewTimeList[0] = TimeList[0];
            for (int CountValue = 1; CountValue < TimeList.Count; CountValue++)
            {
                NewTimeList[CountValue] = TimeList[CountValue] - TimeList[CountValue - 1];
            }

            NewTimeList = NewTimeList.Where(x => x > 0).ToArray();
        }

        private async void ValidTime_Handle(ScheduleMsg_Type message)
        {
            long CurrentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Tính Duration cho video nếu server chưa có
            if (message.msg.ScheduleType == DisplayScheduleType.BanTinVideo && message.msg.Duration == 0)
            {
                int timeout = 5000;
                Task<int> task = Duration_Calculate(message.msg.Songs[0]).Result;
                if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
                {
                    // task completed within timeout
                    message.msg.Duration = task.Result;
                }
                else
                {
                    // timeout logic
                    Log.Error("Get Video Duration Timeout: {A} ms", timeout);
                }
            }

            if (message.msg.From <= CurrentTime)
            {
                // Hết Time valid => Không xử lý
                if ((CurrentTime >= message.msg.To) && (message.msg.To != 0))
                {
                    DeleteMessage_by_Id(message.msg.Id, "Hết thời gian Valid");
                    OnNotify_Time2Delete(message.msg.Id, true);
                    return;
                }
                else if (message.msg.To == 0)
                {
                    // Nếu toTime = 0 => Không cần tạo Timer chạy đến toTime
                }
                else if (CurrentTime < message.msg.To)
                {
                    // Nếu bản tin đã Valid => tạo Timer chạy đến toTime để xóa bản tin
                    message.ValidHandle_Timer = new Timer();
                    message.ValidHandle_Timer.Interval = (int)(message.msg.To - CurrentTime) * 1000 + 1000;
                    message.ValidHandle_Timer.Tick += delegate (object sender, EventArgs e)
                    {
                        DeleteMessage_by_Id(message.msg.Id, "Hết thời gian Valid");
                        OnNotify_Time2Delete(message.msg.Id, true);

                        message.ValidHandle_Timer.Stop();
                    };
                    message.ValidHandle_Timer.Start();
                }
                MessageHandle(message);
            }
            else if (message.msg.From > CurrentTime)
            {
                // Nếu bản tin chưa Valid => tạo Timer chạy đến fromTime, xử lý bản tin => đổi Interval để cạy đến toTime để xóa bản tin
                message.ValidHandle_Timer = new Timer();
                message.ValidHandle_Timer.Interval = (int)(message.msg.From - CurrentTime) * 1000 + 1000;
                message.ValidHandle_Timer.Tick += delegate (object sender, EventArgs e)
                {
                    long Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    if (Time >= message.msg.To && message.msg.To != 0)
                    {
                        DeleteMessage_by_Id(message.msg.Id, "Hết thời gian Valid");
                        OnNotify_Time2Delete(message.msg.Id, true);

                        message.ValidHandle_Timer.Stop();
                    }
                    else if (Time >= message.msg.From && Time < message.msg.To)
                    {
                        //Notify First Time to Play
                        //NotifyPlay(message);
                        message.ValidHandle_Timer.Interval = (int)(message.msg.To - Time) * 1000 + 1000;
                        MessageHandle(message);
                    }
                    else if (Time >= message.msg.From && message.msg.To == 0)
                    {
                        //Notify First Time to Play
                        //NotifyPlay(message);
                        MessageHandle(message);

                        message.ValidHandle_Timer.Stop();
                    }
                };
                message.ValidHandle_Timer.Start();
            }

            // Download Hinh anh hoac Video
            if (message.msg.ScheduleType == DisplayScheduleType.BanTinVideo || message.msg.ScheduleType == DisplayScheduleType.BanTinHinhAnh)
            {
                List<DataUser_SavedFiles> SavedFiles = SqLiteDataAccess.Load_SavedFiles_Info();

                if (SavedFiles != null)
                {
                    // Kiem tra xem File da download chua, neu roi thi khong can Download
                    int index = SavedFiles.FindIndex(s => (s.ScheduleId == message.msg.Id) && (s.Link == message.msg.Songs[0]));
                    if (index != -1)
                    {
                        // Da Download
                        if (File.Exists(SavedFiles[index].PathLocation))
                        {

                        }
                        else
                        {
                            // Neu chua download thi Download
                            message = DownloadAsync(message);
                        }
                    }
                    else
                    {
                        // Neu chua download thi Download
                        message = DownloadAsync(message);
                    }
                }
                else
                {
                    // Neu chua download thi Download
                    message = DownloadAsync(message);
                }
            }

            // Add message to Schedule List (replace if message ID is already existed)
            List<DataUser_ScheduleMessage> Messages = SqLiteDataAccess.Load_ScheduleMessage_Info();
            DataUser_ScheduleMessage info_Save = new DataUser_ScheduleMessage();
            if (Messages != null)
            {
                int index = Messages.FindIndex(s => s.ScheduleId == message.msg.Id);
                if (index == -1)
                {
                    info_Save.Id = Messages.Count + 1;
                }
                else
                {
                    info_Save.Id = index + 1;
                }
            }
            else
            {
                info_Save.Id = 1;
            }
            info_Save.ScheduleId = message.msg.Id;
            info_Save.JsonData = new JavaScriptSerializer().Serialize(message.msg);
            info_Save.Priority = message.Priority;

            SqLiteDataAccess.SaveInfo_ScheduleMessage(info_Save);
            _schedule_msg_List.Add(message);
        }

        public void DeleteMessage_by_Id(string messageId, string Reason = "")
        {
            Log.Information("DeleteMessage_by_Id: {A}, reason: {B}", messageId, Reason);
            foreach (var schedule_msg in _schedule_msg_List)
            {
                if (schedule_msg.msg.Id == messageId)
                {
                    try
                    {
                        if (schedule_msg.Schedule_Timer != null)
                        {
                            schedule_msg.Schedule_Timer.Stop();
                            schedule_msg.Schedule_Timer.Dispose();
                        }
                        if (schedule_msg.ValidHandle_Timer != null)
                        {
                            schedule_msg.ValidHandle_Timer.Stop();
                            schedule_msg.ValidHandle_Timer.Dispose();
                        }

                        if (schedule_msg.dlf != null) schedule_msg.dlf.StopDownLoad();
                    }
                    catch { }
                }
            }
            _schedule_msg_List.RemoveAll(r => r.msg.Id == messageId);
            Delete_ScheduleMessage_inDB(messageId);
        }

        private ScheduleMsg_Type DownloadAsync(ScheduleMsg_Type message)
        {
            if (message.msg.Duration == 0 && message.msg.ScheduleType == DisplayScheduleType.BanTinVideo) return message;
            string Url = message.msg.Songs[0];
            string ScheduleId = message.msg.Id;
            string fileExtension = "";
            Uri uri = new Uri(Url);
            try
            {
                fileExtension = Path.GetExtension(uri.LocalPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "Path_GetExtension: {Url}", Url);
            }
            _FileName = Path.Combine(PathFile, "SaveVideo-" + ScheduleId + fileExtension);

            if (message.dlf != null) message.dlf.StopDownLoad();
            message.dlf = new DownloadFile(Url, _FileName, ScheduleId);
            message.dlf.DownloadAsync();

            return message;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        private event EventHandler<NotifyTime2Play> _NotifyTime2Play;
        public event EventHandler<NotifyTime2Play> NotifyTime2Play
        {
            add
            {
                _NotifyTime2Play += value;
            }
            remove
            {
                _NotifyTime2Play -= value;
            }
        }
        private event EventHandler<NotifyTime2Delete> _NotifyTime2Delete;
        public event EventHandler<NotifyTime2Delete> NotifyTime2Delete
        {
            add
            {
                _NotifyTime2Delete += value;
            }
            remove
            {
                _NotifyTime2Delete -= value;
            }
        }
        protected virtual void OnNotify_Time2Play(string ScheduleId, int Priority, DisplayScheduleType ScheduleType, string Text, List<string> MediaUrl, bool FullScreen,
                                                                        int IdleTime, int LoopNum, int Duration, string ColorValue, string Title, string TextContent, int StartPosition)
        {
            if (_NotifyTime2Play != null)
            {
                _NotifyTime2Play(this, new NotifyTime2Play(ScheduleId, Priority, ScheduleType, Text, MediaUrl, FullScreen, IdleTime, LoopNum, Duration, ColorValue, Title, TextContent, StartPosition));
            }
        }
        protected virtual void OnNotify_Time2Delete(string ScheduleId, bool DeleteSavedFile)
        {
            if (_NotifyTime2Delete != null)
            {
                _NotifyTime2Delete(this, new NotifyTime2Delete(ScheduleId, DeleteSavedFile));
            }
        }
    }
    public struct ScheduleMsg_Type
    {
        public Schedule msg;
        public int Priority;
        public int[] TimeList;
        public int[] WeeklyTimeList;
        public Timer Schedule_Timer;
        public Timer ValidHandle_Timer;
        int? countTime;
        public DownloadFile dlf;
        public int CountTime { get { return countTime ?? 0; } set { countTime = value; } }
    }
    public class NotifyTime2Play : EventArgs
    {
        public string ScheduleId;
        public int Priority;
        public DisplayScheduleType ScheduleType;
        public string Text;
        public List<string> MediaUrl;
        public int IdleTime;
        public int LoopNum;
        public int Duration;
        public string ColorValue;
        public bool FullScreen;
        public string Title;
        public string TextContent;

        public int StartPosition = 0;
        public NotifyTime2Play(string scheduleId, int priority, DisplayScheduleType scheduleType, string text, List<string> mediaUrl, bool fullScreen, int idleTime, int loopNum, int duration, string colorValue, string title, string textContent, int startPosition)
        {
            ScheduleId = scheduleId;
            Priority = priority;
            ScheduleType = scheduleType;
            Text = text;
            MediaUrl = mediaUrl;

            IdleTime = idleTime;
            LoopNum = loopNum;
            Duration = duration;
            ColorValue = colorValue;
            FullScreen = fullScreen;
            Title = title;
            TextContent = textContent;

            StartPosition = startPosition;
        }
    }

    public class NotifyTime2Delete : EventArgs
    {
        public string ScheduleId;
        public bool DeleteSavedFile;
        public NotifyTime2Delete(string scheduleId, bool deleteSavedFile)
        {
            ScheduleId = scheduleId;
            DeleteSavedFile = deleteSavedFile;
        }
    }
}
