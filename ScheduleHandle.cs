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
            MessageHandle(message);
        }

        private void MessageHandle(Schedule message)
        {
            if (message.isActive != true) return;

            // Chuyển các mốc thời gian trong tuần về dạng giây (số giây trôi qua từ 0h00 T2)
            List<int> TimeList_perWeek = new List<int>();
            if(message.isDaily == true)
            {
                for(int CountDay = 0; CountDay < message.dayList.Count; CountDay++)
                {
                    TimeList_perWeek.AddRange(message.timeList.Select(x => x + 24 * 3600 * (message.dayList[CountDay] - 1)).ToList());
                }
            }
            else
            {
                TimeList_perWeek.AddRange(message.timeList);
            }
            ScheduleMsg_Type new_messsage = new ScheduleMsg_Type();
            new_messsage.msg = message;

            // Xử lý các mốc thời gian để cho ra khoảng thời gian cài đặt cho Timer
            if (message.isDaily == true)
            {
                TimeList_Handle(TimeList_perWeek, ref new_messsage.TimeList, ref new_messsage.WeeklyTimeList);
            }
            else
            {
                TimeList_Handle(TimeList_perWeek, ref new_messsage.TimeList);
            }

            // Init Timer
            if (new_messsage.TimeList.Length <= 0) return;
            new_messsage.timer = new Timer();
            new_messsage.timer.Interval = new_messsage.TimeList[0] * 1000;
            new_messsage.timer.Tick += delegate (object sender, EventArgs e)
            {
                OnNotify_Time2Play(new_messsage.msg.idleTime, new_messsage.msg.loopNum, new_messsage.msg.duration, new_messsage.msg.playList);
                Timer this_timer = (Timer)sender;
                if (++new_messsage.CountTime >= new_messsage.TimeList.Length)
                {
                    if (new_messsage.msg.isDaily == true)
                    {
                        new_messsage.CountTime = 0;
                        new_messsage.TimeList = new_messsage.WeeklyTimeList;
                    }
                    else
                    {
                        this_timer.Stop();
                        return;
                    }
                }
                // Set Interval to run to next Time in TimeList
                this_timer.Interval = new_messsage.TimeList[new_messsage.CountTime] * 1000;
            };
            new_messsage.timer.Start();
            // Add message to Schedule List
            _schedule_msg_List.Add(new_messsage);
        }

        private void TimeList_Handle(List<int> TimeList, ref int[] NewTimeList, ref int[] WeeklyTimeList)
        {
            if (TimeList.Count <= 0) return;

            int d = (int)DateTime.Now.DayOfWeek;
            int CurrentSecond = (int)DateTime.Now.TimeOfDay.TotalSeconds + 24 * 3600 * (d - 1);
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

            int CurrentSecond = (int)DateTime.Now.TimeOfDay.TotalSeconds;

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

        public void DeleteMessage_by_Id(string messageId)
        {
            foreach(var schedule_msg in _schedule_msg_List)
            {
                if(schedule_msg.msg.id == messageId)
                {
                    schedule_msg.timer.Stop();
                    schedule_msg.timer.Dispose();
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
        protected virtual void OnNotify_Time2Play(int IdleTime, int LoopNum, int Duration, List<string> PlayList)
        {
            if (_NotifyTime2Play != null)
            {
                _NotifyTime2Play(this, new NotifyTime2Play(IdleTime, LoopNum, Duration, PlayList));
            }
        }
    }
    public struct ScheduleMsg_Type
    {
        public Schedule msg;
        public int[] TimeList;
        public int[] WeeklyTimeList;
        public Timer timer;
        int? countTime;
        public int CountTime { get { return countTime ?? 0; } set { countTime = value; } }
    }
    public class NotifyTime2Play : EventArgs
    {
        public int IdleTime;
        public int LoopNum;
        public int Duration;
        public List<string> playList;
        public NotifyTime2Play(int idleTime, int loopNum, int duration, List<string> PlayList)
        {
            IdleTime = idleTime;
            LoopNum = loopNum;
            Duration = duration;
            playList = PlayList;
        }
    }
}
