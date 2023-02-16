using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
        int position, speed, width, maxPosition;
        bool enableScrollText = false;

        public TextEx2()
        {
            InitializeComponent();

            UseCompatibleTextRendering = true;

            tmrTick = new Timer();
            tmrTick.Tick += tick;
            tmrTick.Interval = 20;
            tmrTick.Start();
        }

        public void Start()
        {
            enableScrollText = true;
            width = this.Size.Width;
            maxPosition = (int)this.CreateGraphics().MeasureString(this.Text, this.Font, this.Width).Width;

            if (maxPosition + 100 < width)
            {
                SetSpeed = 0;
            }
        }

        public void Stop()
        {
            enableScrollText = false;
            position = 0;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            maxPosition = (int)this.CreateGraphics().MeasureString(this.Text, this.Font, this.Width).Width;

            base.OnTextChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TranslateTransform((float)position, 0);

            base.OnPaint(e);
        }

        private void tick(object sender, EventArgs e)
        {
            if (!enableScrollText) return;

            if (position < -maxPosition)
            {
                this.Size = new Size(width, Height);
                position = width;
            }

            position -= speed;
            Width += speed;
            Invalidate();
        }
    }
}
