using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Display
{
    public partial class PanelEx2 : Panel
    {
        Thread trd_Handle_TextRun;
        //Timer tmrTick;
        int repeat_Count = 0;
        int speed, width, maxPosition;
        bool enableScrollPanel = false;

        public PanelEx2()
        {
            InitializeComponent();

            trd_Handle_TextRun = new Thread(new ThreadStart(this.ThreadTask_Handle_TextRun));
            trd_Handle_TextRun.IsBackground = true;
            trd_Handle_TextRun.Start();
        }
        public int SetSpeed
        {
            get { return speed; }
            set { speed = value; Invalidate(); }
        }
        public int Max_Repeat_Time { get; set; }

        protected override void Dispose(bool disposing)
        {
            // Abort Thread
            if (trd_Handle_TextRun != null)
            {
                try
                {
                    trd_Handle_TextRun.Abort();
                    trd_Handle_TextRun = null;
                }
                catch
                { }
            }
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        public void Start(int Length_Text_Inside)
        {
            width = this.Size.Width;
            enableScrollPanel = true;
            maxPosition = Length_Text_Inside;

            if (maxPosition < width)
            {
                SetSpeed = 0;
                Thread_Stop();
            }
            else
            {
                this.Location = new Point(width, this.Location.Y);
                Thread_Start();
            }
        }
        public void Stop()
        {
            SetSpeed = 0;
            Thread_Stop();
        }
        private event EventHandler<NotifyEndProcess> _NotifyEndProcess_TextRun;
        public event EventHandler<NotifyEndProcess> NotifyEndProcess_TextRun
        {
            add
            {
                _NotifyEndProcess_TextRun += value;
            }
            remove
            {
                _NotifyEndProcess_TextRun -= value;
            }
        }
        protected virtual void OnNotifyEndProcess_TextRun(int Repeat_Count)
        {
            if (_NotifyEndProcess_TextRun != null)
            {
                _NotifyEndProcess_TextRun(this, new NotifyEndProcess(Repeat_Count));
            }
        }
        private void Thread_Start()
        {
            if (trd_Handle_TextRun != null)
            {
                try
                {
                    trd_Handle_TextRun.Abort();
                    trd_Handle_TextRun = null;
                }
                catch { }
            }
            trd_Handle_TextRun = new Thread(new ThreadStart(this.ThreadTask_Handle_TextRun));
            trd_Handle_TextRun.IsBackground = true;
            trd_Handle_TextRun.Start();
        }
        private void Thread_Stop()
        {
            if (trd_Handle_TextRun != null)
            {
                try
                {
                    trd_Handle_TextRun.Abort();
                    trd_Handle_TextRun = null;
                }
                catch { }
            }
        }

        private void ThreadTask_Handle_TextRun()
        {
            while (true)
            {
                Thread.Sleep(20);
                if (!enableScrollPanel) continue;

                this.Invoke((MethodInvoker)delegate
                {
                    // Running on the UI thread
                    if (this.Location.X < -maxPosition)
                    {
                        if (++repeat_Count >= Max_Repeat_Time)
                        {
                            OnNotifyEndProcess_TextRun(repeat_Count);
                            Stop();
                        }
                        this.Size = new Size(width, Height);
                        this.Location = new Point(width, this.Location.Y);
                    }

                    this.Location = new Point(this.Location.X - speed, this.Location.Y);
                    Width += speed;
                    //position -= speed;
                    Invalidate();
                });
            }
        }
    }
    public class NotifyEndProcess : EventArgs
    {
        public int Repeat_count = 0;
        public NotifyEndProcess(int repeat_count)
        {
            Repeat_count = repeat_count;
        }
    }
}
