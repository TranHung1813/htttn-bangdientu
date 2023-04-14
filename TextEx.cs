using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Display
{
    public partial class PanelEx : Panel
    {
        Thread trd_Handle_TextRun;
        System.Windows.Forms.Timer tmrTick;
        System.Windows.Forms.Timer tmrDelay_TextRun;
        int locationY, speed, height, maxPosition;
        bool enableScrollPanel = false;

        private bool _isStop_WaitTextRun = false;

        public PanelEx()
        {
            InitializeComponent();

            //trd_Handle_TextRun = new Thread(new ThreadStart(this.ThreadTask_Handle_TextRun));
            //trd_Handle_TextRun.IsBackground = true;
            //trd_Handle_TextRun.Start();

            //tmrTick = new System.Windows.Forms.Timer();
            //tmrTick.Interval = 50;
            //tmrTick.Tick += TmrTick_Tick;

            Control.CheckForIllegalCrossThreadCalls = false;
        }
        public int SetSpeed
        {
            get { return speed; }
            set { speed = value; Invalidate(); }
        }
        private const int RUNNING = 1;
        private const int STOPPED = 2;
        private int State = 0;
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

        public void Start(int Length_Text_Inside, int Delay_Text_Run)
        {
            if (State == RUNNING)
            {
                try
                {
                    Stop();
                }
                catch { }
            }
            locationY = this.Location.Y;
            height = this.Size.Height;
            enableScrollPanel = true;
            maxPosition = Length_Text_Inside;

            _isStop_WaitTextRun = false;

            if (maxPosition < height)
            {
                SetSpeed = 0;
                Thread_Stop();
                //Timer_Stop();
            }
            else
            {
                State = RUNNING;
                if (Delay_Text_Run > 100)
                {
                    tmrDelay_TextRun = new System.Windows.Forms.Timer();
                    tmrDelay_TextRun.Interval = Delay_Text_Run;
                    tmrDelay_TextRun.Tick += TmrDelay_TextRun_Tick;

                    tmrDelay_TextRun.Start();
                }
                else
                {
                    tmrDelay_TextRun = new System.Windows.Forms.Timer();
                    tmrDelay_TextRun.Interval = 100;
                    tmrDelay_TextRun.Tick += TmrDelay_TextRun_Tick;

                    tmrDelay_TextRun.Start();
                }
            }
        }
        public void Stop()
        {
            if (State == RUNNING)
            {
                //SetSpeed = 0;
                Thread_Stop();
                //Timer_Stop();

                this.Size = new Size(Width, height);
                this.Location = new Point(this.Location.X, locationY);

                if (tmrDelay_TextRun != null)
                {
                    try
                    {
                        tmrDelay_TextRun.Stop();
                        tmrDelay_TextRun = null;
                    }
                    catch { }
                }

                State = STOPPED;
            }
        }
        public void Stop_WaitTextRun()
        {
            if (State == RUNNING)
            {
                _isStop_WaitTextRun = true;
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

        private void Timer_Start()
        {
            if (tmrTick != null)
            {
                try
                {
                    tmrTick.Stop();
                }
                catch { }
            }
            tmrTick.Start();
        }
        private void Timer_Stop()
        {
            if (tmrTick != null)
            {
                try
                {
                    tmrTick.Stop();
                }
                catch { }
            }
        }

        private void ThreadTask_Handle_TextRun()
        {
            while (true)
            {
                Thread.Sleep(40);
                if (!enableScrollPanel) continue;

                this.Invoke((MethodInvoker)delegate
                {
                    // Running on the UI thread
                    if (this.Location.Y < -maxPosition)
                    {
                        if (_isStop_WaitTextRun == true)
                        {
                            Stop();
                            return;
                        }
                        this.Size = new Size(Width, height);
                        this.Location = new Point(this.Location.X, height);
                    }

                    this.Location = new Point(this.Location.X, this.Location.Y - speed);
                    Height += speed;
                    //position -= speed;
                    Invalidate();
                });
            }
        }


        private void TmrTick_Tick(object sender, EventArgs e)
        {
            if (!enableScrollPanel) return;

            // Running on the UI thread
            if (this.Location.Y < -maxPosition)
            {
                this.Size = new Size(Width, height);
                this.Location = new Point(this.Location.X, height);
            }

            this.Location = new Point(this.Location.X, this.Location.Y - speed);
            Height += speed;
            //position -= speed;
            Invalidate();
        }
        private void TmrDelay_TextRun_Tick(object sender, EventArgs e)
        {
            //Timer_Start();
            Thread_Start();

            tmrDelay_TextRun.Stop();
        }
    }
}
