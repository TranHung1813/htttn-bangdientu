using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
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

        Timer tmrTick;
        int position, speed, _Parent_Width;
        bool enableScrollText = false;

        public TextEx2()
        {
            InitializeComponent();

            //UseCompatibleTextRendering = true;
            AutoEllipsis = true;

            tmrTick = new Timer();
            tmrTick.Tick += tick;
            tmrTick.Interval = 20;
            //tmrTick.Start();
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
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TranslateTransform((float)position, 0);

            e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
            using (GraphicsPath gp = new GraphicsPath())
            using (Pen outline = new Pen(OutlineForeColor, OutlineWidth)
            { LineJoin = LineJoin.Round })
            using (StringFormat sf = new StringFormat())
            using (Brush foreBrush = new SolidBrush(ForeColor))
            {
                gp.AddString(Text, Font.FontFamily, (int)Font.Style,
                    Font.Size, ClientRectangle, sf);
                e.Graphics.ScaleTransform(1.3f, 1.35f);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
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

            if (position < - Width)
            {
                //this.Size = new Size(width, Height);
                position = _Parent_Width;
            }

            position -= speed;
            Invalidate();
        }
    }
}
