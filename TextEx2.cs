using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Display
{
    public partial class TextEx2 : Label
    {
        public int SetSpeed
        {
            get { return speed; }
            set { speed = value; Invalidate(); }
        }

        public int Max_Repeat_Time { get; set; }

        System.Threading.Timer tmrTick;
        int position, speed, _Parent_Width;
        int repeat_Count = 0;
        bool enableScrollText = false;

        public TextEx2()
        {
            InitializeComponent();

            //UseCompatibleTextRendering = true;
            AutoEllipsis = true;
            Max_Repeat_Time = 1;

            tmrTick = new System.Threading.Timer(new TimerCallback(TickTimer),null, 100, 20);
            //tmrTick.Tick += new EventHandler(tick); 
            //tmrTick.Interval = 20;
            //tmrTick.Start();
        }

        private void TickTimer(object state)
        {
            //Thread.Sleep(20);
            if (!enableScrollText) return;

            if (position < -Width)
            {
                //this.Size = new Size(width, Height);
                position = _Parent_Width;
                if(++repeat_Count >= Max_Repeat_Time)
                {
                    OnNotifyEndProcess_TextRun(repeat_Count);
                    Stop();
                }
            }

            position -= speed;
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            // Abort Thread
            if (tmrTick != null)
            {
                try
                {
                    tmrTick.Dispose();
                    tmrTick = null;
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

        public void Start(int Parent_Width)
        {
            enableScrollText = true;
            _Parent_Width = Parent_Width;
            repeat_Count = 0;
            //maxPosition = (int)this.CreateGraphics().MeasureString(this.Text, this.Font).Width;

            if (this.Width < Parent_Width)
            {
                SetSpeed = 0;
            }
            else
            {
                position = Parent_Width;
            }
        }
        public void Stop()
        {
            // Abort Thread
            if (tmrTick != null)
            {
                try
                {
                    tmrTick.Dispose();
                    tmrTick = null;
                }
                catch
                { }
            }
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
        public Color OutlineForeColor { get; set; }
        public float OutlineWidth { get; set; }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TranslateTransform((float)position, 0);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            e.Graphics.FillEllipse(new SolidBrush(BackColor), ClientRectangle);
            using (GraphicsPath gp = new GraphicsPath())
            using (Pen outline = new Pen(OutlineForeColor, OutlineWidth)
            { LineJoin = LineJoin.Round })
            using (StringFormat sf = new StringFormat())
            using (Brush foreBrush = new SolidBrush(ForeColor))
            {
                gp.AddString(Text, Font.FontFamily, (int)Font.Style,
                    Font.Size, ClientRectangle, sf);
                e.Graphics.ScaleTransform(1.3f, 1.35f);

                e.Graphics.DrawPath(outline, gp);
                e.Graphics.FillPath(foreBrush, gp);
            }
        }


        //protected override void OnTextChanged(EventArgs e)
        //{
        //    maxPosition = (int)this.CreateGraphics().MeasureString(this.Text, this.Font).Width;

        //    base.OnTextChanged(e);
        //}

        private void tick(object sender, EventArgs e)
        {
            if (!enableScrollText) return;

            if (position < -Width)
            {
                //this.Size = new Size(width, Height);
                position = _Parent_Width;
            }

            position -= speed;
            Invalidate();
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
