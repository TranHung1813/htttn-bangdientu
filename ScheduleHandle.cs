using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Display
{
    public class ScheduleHandle
    {
        List<ScheduleMsg_Type> _schedule_msg_List = new List<ScheduleMsg_Type>();
        //Thread ScheduleHandle_trd;
        public ScheduleHandle()
        {
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
            new_messsage.TimeList = TimeList_Handle(message.timeList.ToArray());

            // Timer Init
            new_messsage.timer = new Timer();
            new_messsage.timer.Interval = new_messsage.TimeList[0] * 1000;
            new_messsage.timer.Tick += delegate (object sender, EventArgs e)
            {
                OnNotify_Time2Play(new_messsage.msg.idleTime, new_messsage.msg.playList);
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

        private int[] TimeList_Handle(int[] TimeList)
        {
            if (TimeList.Length <= 0) return null;

            Array.Sort(TimeList);
            int[] NewTimeList = new int[TimeList.Length];

            NewTimeList[0] = TimeList[0];
            for (int CountValue = 1; CountValue < TimeList.Length; CountValue++)
            {
                NewTimeList[CountValue] = TimeList[CountValue] - TimeList[CountValue - 1];
            }

            return NewTimeList;
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
        protected virtual void OnNotify_Time2Play(int Test, List<string> PlayList)
        {
            if (_NotifyTime2Play != null)
            {
                _NotifyTime2Play(this, new NotifyTime2Play(Test, PlayList));
            }
        }
    }
    public struct ScheduleMsg_Type
    {
        public Schedule msg;
        public int[] TimeList;
        public Timer timer;
        int? countTime;
        public int CountTime { get { return countTime ?? 1; } set { countTime = value; } }
    }
    public class NotifyTime2Play : EventArgs
    {
        public int test;
        public List<string> playList;
        public NotifyTime2Play(int Test, List<string> PlayList)
        {
            test = Test;
            playList = PlayList;
        }
    }
}
