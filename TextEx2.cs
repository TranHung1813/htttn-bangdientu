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

        //Timer tmrTick;
        int position, speed, _Parent_Width;
        bool enableScrollText = false;
        Thread trd_Handle_TextRun;

        public TextEx2()
        {
            InitializeComponent();

            //UseCompatibleTextRendering = true;
            AutoEllipsis = true;

            trd_Handle_TextRun = new Thread(new ThreadStart(this.ThreadTask_Handle_TextRun));
            trd_Handle_TextRun.IsBackground = true;
            trd_Handle_TextRun.Start();
            //tmrTick = new Timer();
            //tmrTick.Tick += tick;
            //tmrTick.Interval = 20;
            //tmrTick.Start();
        }

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

        public void Start(int Parent_Width)
        {
            enableScrollText = true;
            _Parent_Width = Parent_Width;
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
        public Color OutlineForeColor { get; set; }
        public float OutlineWidth { get; set; }
        protected override async void OnPaint(PaintEventArgs e)
        {
            await PaintAsync(e);
        }
        private async Task PaintAsync(PaintEventArgs e)
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

        //private void tick(object sender, EventArgs e)
        //{
        //    if (!enableScrollText) return;

        //    if (position < - Width)
        //    {
        //        //this.Size = new Size(width, Height);
        //        position = _Parent_Width;
        //    }

        //    position -= speed;
        //    Invalidate();
        //}
        private void ThreadTask_Handle_TextRun()
        {
            while (true)
            {
                if (!enableScrollText) continue;

                if (position < -Width)
                {
                    //this.Size = new Size(width, Height);
                    position = _Parent_Width;
                }

                position -= speed;
                Invalidate();

                Thread.Sleep(20);
            }
        }
    }
}
