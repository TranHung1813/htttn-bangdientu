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

        public void Schedule(Schedule message, int Priority)
        {
            if (message.IsActive != true)
            {
                DeleteMessage_by_Id(message.Id);
                OnNotify_Time2Delete(message.Id, true);

                return;
            }

            //replace if message ID is already existed
            int index = _schedule_msg_List.FindIndex(s => s.msg.Id == message.Id);
            if (index != -1)
            {
                DeleteMessage_by_Id(message.Id);
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
                Log.Error(ex, "ValidTime_Handle");
            }
        }

        private void MessageHandle(ScheduleMsg_Type message)
        {
            // Chuyển các mốc thời gian trong tuần về dạng giây (số giây trôi qua từ 0h00 T2)
            List<int> TimeList_perWeek = new List<int>();
            if(message.msg.IsDaily == true)
            {
                for(int CountDay = 0; CountDay < message.msg.Days.Count; CountDay++)
                {
                    TimeList_perWeek.AddRange(message.msg.Times.Select(x => x + 24 * 3600 * (message.msg.Days[CountDay] - 1)).ToList());
                }
                TimeList_Handle(TimeList_perWeek, ref message.TimeList, ref message.WeeklyTimeList);
            }
            else
            {
                TimeList_perWeek.AddRange(message.msg.Times);
                TimeList_Handle(TimeList_perWeek, ref message.TimeList);
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

        private void NotifyPlay(ScheduleMsg_Type message)
        {
            // Notify First Time to Play
            OnNotify_Time2Play(message.msg.Id, message.Priority, message.msg.ScheduleType, message.msg.TextContent, message.msg.Songs, message.msg.FullScreen,
                               message.msg.IdleTime, message.msg.Loops, message.msg.Duration, message.msg.ColorValue, message.msg.Title, message.msg.TextContent);
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

            if (message.msg.From <= CurrentTime)
            {
                // Nếu bản tin đã Valid => tạo Timer chạy đến toTime để xóa bản tin
                if(message.msg.To == 0)
                {

                }
                else if (CurrentTime < message.msg.To)
                {
                    message.ValidHandle_Timer = new Timer();
                    message.ValidHandle_Timer.Interval = (int)(message.msg.To - CurrentTime) * 1000 + 1000;
                    message.ValidHandle_Timer.Tick += delegate (object sender, EventArgs e)
                    {
                        DeleteMessage_by_Id(message.msg.Id);
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
                        DeleteMessage_by_Id(message.msg.Id);
                        OnNotify_Time2Delete(message.msg.Id, true);

                        message.ValidHandle_Timer.Stop();
                    }
                    else if (Time >= message.msg.From && Time < message.msg.To)
                    {
                        // Notify First Time to Play
                        NotifyPlay(message);
                        message.ValidHandle_Timer.Interval = (int)(message.msg.To - Time) * 1000 + 1000;
                        MessageHandle(message);
                    }
                    else if(Time >= message.msg.From && message.msg.To == 0)
                    {
                        // Notify First Time to Play
                        NotifyPlay(message);
                        MessageHandle(message);

                        message.ValidHandle_Timer.Stop();
                    }
                };
                message.ValidHandle_Timer.Start();
            }

            // Add message to Schedule List (replace if message ID is already existed)
            _schedule_msg_List.Add(message);
        }

        public void DeleteMessage_by_Id(string messageId)
        {
            Log.Information("DeleteMessage_by_Id: {A}", messageId);
            foreach(var schedule_msg in _schedule_msg_List)
            {
                if(schedule_msg.msg.Id == messageId)
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
                    }
                    catch { }
                }
            }
            _schedule_msg_List.RemoveAll(r => r.msg.Id == messageId);
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
                                                                        int IdleTime, int LoopNum, int Duration, string ColorValue, string Title, string TextContent)
        {
            if (_NotifyTime2Play != null)
            {
                _NotifyTime2Play(this, new NotifyTime2Play(ScheduleId, Priority, ScheduleType, Text, MediaUrl, FullScreen, IdleTime, LoopNum, Duration, ColorValue, Title, TextContent));
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
        public NotifyTime2Play(string scheduleId, int priority, DisplayScheduleType scheduleType, string text, List<string> mediaUrl, bool fullScreen, int idleTime, int loopNum, int duration, string colorValue, string title, string textContent)
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
