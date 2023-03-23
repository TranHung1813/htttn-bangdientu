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
            List<int> abc = new List<int> { 30000, 30600, 30600, 29000, 31200 };
            int[] test1 = null;
            int[] test2 = null;
            TimeList_Handle(abc, ref test1, ref test2);
            //Schedule(null);
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

            ScheduleMsg_Type new_messsage = new ScheduleMsg_Type();
            new_messsage.msg = message;

            //int[] test = { 10, 15, 30 };
            TimeList_Handle(message.timeList, ref new_messsage.TimeList, ref new_messsage.WeeklyTimeList);

            // Timer Init
            new_messsage.timer = new Timer();
            new_messsage.timer.Interval = new_messsage.TimeList[0] * 1000;
            new_messsage.timer.Tick += delegate (object sender, EventArgs e)
            {
                OnNotify_Time2Play(new_messsage.msg.idleTime, new_messsage.msg.loopNum, new_messsage.msg.duration, new_messsage.msg.playList);
                Timer this_timer = (Timer)sender;
                if (new_messsage.CountTime < new_messsage.TimeList.Length)
                {
                    this_timer.Interval = new_messsage.TimeList[new_messsage.CountTime] * 1000;
                    new_messsage.CountTime++;
                }
                else
                {
                    this_timer.Stop();
                }
            };
            new_messsage.timer.Start();
            // Add message to List
            _schedule_msg_List.Add(new_messsage);
        }

        private void TimeList_Handle(List<int> TimeList, ref int[] NewTimeList, ref int[] WeeklyTimeList)
        {
            if (TimeList.Count <= 0) return;

            int CurrentSecond = (int)DateTime.Now.TimeOfDay.TotalSeconds;
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
                TimeList = TimeList.Select(x => x + TotalSecond_1Week).ToList();
            }
            else
            {
                TimeList.RemoveAll(x => x < 0);
            }

            NewTimeList = new int[TimeList.Count];

            NewTimeList[0] = TimeList[0];
            for (int CountValue = 1; CountValue < TimeList.Count; CountValue++)
            {
                NewTimeList[CountValue] = TimeList[CountValue] - TimeList[CountValue - 1];
            }
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
        public int CountTime { get { return countTime ?? 1; } set { countTime = value; } }
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
