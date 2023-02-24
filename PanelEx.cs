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
    public partial class PanelEx : Panel
    {
        Thread trd_Handle_TextRun;
        //Timer tmrTick;
        int locationY, speed, height, maxPosition;
        bool enableScrollPanel = false;

        public PanelEx()
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
        public const int RUNNING = 1;
        public const int STOPPED = 2;
        public int State { get; set; }
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
            locationY = this.Location.Y;
            height = this.Size.Height;
            enableScrollPanel = true;
            maxPosition = Length_Text_Inside;

            if (maxPosition < height)
            {
                SetSpeed = 0;
                Thread_Stop();
            }
            else
            {
                Thread_Start();
                State = RUNNING;
            }
        }
        public void Stop()
        {
            SetSpeed = 0;
            Thread_Stop();

            this.Size = new Size(Width, height);
            this.Location = new Point(this.Location.X, locationY);

            State = STOPPED;
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
                Thread.Sleep(50);
                if (!enableScrollPanel) continue;

                this.Invoke((MethodInvoker)delegate
                {
                    // Running on the UI thread
                    if (this.Location.Y < - maxPosition)
                    {
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
    }
}
