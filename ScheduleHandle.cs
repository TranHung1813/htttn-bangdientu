using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Display
{
    public class ScheduleHandle
    {
        List<ScheduleMsg_Type> _schedule_msg_List = new List<ScheduleMsg_Type>();
        //Thread ScheduleHandle_trd;
        public ScheduleHandle()
        {
            //ScheduleHandle_trd = new Thread(new ThreadStart(this.ScheduleHandle_Thread));
            //ScheduleHandle_trd.IsBackground = true;
            //ScheduleHandle_trd.Start();
        }

        public void Schedule(Schedule message)
        {
            if (message.isActive != true)
            {
                DeleteMessage_by_Id(message.id);
                return;
            }

            //replace if message ID is already existed
            int index = _schedule_msg_List.FindIndex(s => s.msg.id == message.id);
            if (index != -1)
            {
                DeleteMessage_by_Id(message.id);
            }
            ScheduleMsg_Type new_messsage = new ScheduleMsg_Type();
            new_messsage.msg = message;

            try
            {
                ValidTime_Handle(new_messsage);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ValidTime_Handle");
            }
        }

        private void MessageHandle(ScheduleMsg_Type message)
        {
            // Chuyển các mốc thời gian trong tuần về dạng giây (số giây trôi qua từ 0h00 T2)
            List<int> TimeList_perWeek = new List<int>();
            if(message.msg.isDaily == true)
            {
                for(int CountDay = 0; CountDay < message.msg.days.Count; CountDay++)
                {
                    TimeList_perWeek.AddRange(message.msg.times.Select(x => x + 24 * 3600 * (message.msg.days[CountDay] - 1)).ToList());
                }
                TimeList_Handle(TimeList_perWeek, ref message.TimeList, ref message.WeeklyTimeList);
            }
            else
            {
                TimeList_perWeek.AddRange(message.msg.times);
                TimeList_Handle(TimeList_perWeek, ref message.TimeList);
            }

            // Init Timer
            if (message.TimeList.Length <= 0) return;
            message.Schedule_Timer = new Timer();
            message.Schedule_Timer.Interval = message.TimeList[0] * 1000;
            message.Schedule_Timer.Tick += delegate (object sender, EventArgs e)
            {
                OnNotify_Time2Play(message.msg.ScheduleType, message.msg.TextContent, message.msg.MediaContent, message.msg.FullScreen,
                                   message.msg.idleTime, message.msg.loops, message.msg.duration, message.msg.ColorValue);
                Timer this_timer = (Timer)sender;
                if (++message.CountTime >= message.TimeList.Length)
                {
                    if (message.msg.isDaily == true)
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
            _schedule_msg_List.Add(message);
        }

        private void TimeList_Handle(List<int> TimeList, ref int[] NewTimeList, ref int[] WeeklyTimeList)
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

        private void TimeList_Handle(List<int> TimeList, ref int[] NewTimeList)
        {
            if (TimeList.Count <= 0) return;

            int CurrentSecond = (int)DateTime.UtcNow.TimeOfDay.TotalSeconds;

            TimeList = TimeList.Distinct().ToList();
            TimeList.Sort();

            // This week Time List Handle
            TimeList = TimeList.Select(x => x - CurrentSecond).ToList();
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

        private void ValidTime_Handle(ScheduleMsg_Type message)
        {
            long CurrentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (message.msg.from <= CurrentTime && CurrentTime < message.msg.to)
            {
                // Nếu bản tin đã Valid => tạo Timer chạy đến toTime để xóa bản tin

                message.ValidHandle_Timer = new Timer();
                message.ValidHandle_Timer.Interval = (int)(message.msg.to - CurrentTime) * 1000 + 1000;
                message.ValidHandle_Timer.Tick += delegate (object sender, EventArgs e)
                {
                    DeleteMessage_by_Id(message.msg.id);

                    Timer this_timer = (Timer)sender;
                    this_timer.Stop();
                };
                message.ValidHandle_Timer.Start();

                MessageHandle(message);
            }
            else if (message.msg.from > CurrentTime)
            {
                // Nếu bản tin chưa Valid => tạo Timer chạy đến fromTime, xử lý bản tin => đổi Interval để cạy đến toTime để xóa bản tin
                message.ValidHandle_Timer = new Timer();
                message.ValidHandle_Timer.Interval = (int)(message.msg.from - CurrentTime) * 1000 + 1000;
                message.ValidHandle_Timer.Tick += delegate (object sender, EventArgs e)
                {
                    long Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    if (Time >= message.msg.to)
                    {
                        DeleteMessage_by_Id(message.msg.id);

                        Timer this_timer = (Timer)sender;
                        this_timer.Stop();
                    }
                    else if (Time >= message.msg.from && Time < message.msg.to)
                    {
                        message.ValidHandle_Timer.Interval = (int)(message.msg.to - Time) * 1000 + 1000;
                        MessageHandle(message);
                    }
                };
                message.ValidHandle_Timer.Start();
            }
        }

        public void DeleteMessage_by_Id(string messageId)
        {
            foreach(var schedule_msg in _schedule_msg_List)
            {
                if(schedule_msg.msg.id == messageId)
                {
                    try
                    {
                        schedule_msg.Schedule_Timer.Stop();
                        schedule_msg.Schedule_Timer.Dispose();
                        schedule_msg.ValidHandle_Timer.Stop();
                        schedule_msg.ValidHandle_Timer.Dispose();
                    }
                    catch { }
                }
            }
            _schedule_msg_List.RemoveAll(r => r.msg.id == messageId);
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
        protected virtual void OnNotify_Time2Play(DisplayScheduleType ScheduleType, string Text, string MediaUrl, bool FullScreen,
                                                                        int IdleTime, int LoopNum, int Duration, string ColorValue)
        {
            if (_NotifyTime2Play != null)
            {
                _NotifyTime2Play(this, new NotifyTime2Play(ScheduleType, Text, MediaUrl, FullScreen, IdleTime, LoopNum, Duration, ColorValue));
            }
        }
    }
    public struct ScheduleMsg_Type
    {
        public Schedule msg;
        public int[] TimeList;
        public int[] WeeklyTimeList;
        public Timer Schedule_Timer;
        public Timer ValidHandle_Timer;
        int? countTime;
        public int CountTime { get { return countTime ?? 0; } set { countTime = value; } }
    }
    public class NotifyTime2Play : EventArgs
    {
        public DisplayScheduleType ScheduleType;
        public string Text;
        public string MediaUrl;
        public int IdleTime;
        public int LoopNum;
        public int Duration;
        public string ColorValue;
        public bool FullScreen;
        public NotifyTime2Play(DisplayScheduleType scheduleType, string text, string mediaUrl, bool fullScreen, int idleTime, int loopNum, int duration, string colorValue)
        {
            ScheduleType = scheduleType;
            Text = text;
            MediaUrl = mediaUrl;

            IdleTime = idleTime;
            LoopNum = loopNum;
            Duration = duration;
            ColorValue = colorValue;
            FullScreen = fullScreen;
        }
    }
}
