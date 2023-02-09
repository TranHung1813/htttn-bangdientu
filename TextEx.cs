using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Display
{
    public partial class TextEx : Label
    {
        public int SetSpeed
        {
            get { return speed; }
            set { speed = value; Invalidate(); }
        }

        Timer tmrTick;
        int position, speed, height, maxPosition;
        bool enableScrollText = false;
        

        public TextEx()
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
            height = this.Size.Height;
            maxPosition = (int)this.CreateGraphics().MeasureString(this.Text, this.Font, this.Width).Height;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            maxPosition = (int)this.CreateGraphics().MeasureString(this.Text, this.Font, this.Width).Height;

            base.OnTextChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(0, (float)position);

            base.OnPaint(e);
        }

        private void tick(object sender, EventArgs e)
        {
            if (!enableScrollText) return;

            if (position < -maxPosition)
            {
                this.Size = new Size(Width, height);
                position = height;                
            }

            position -= speed;
            Height += speed;
            Invalidate();
        }
    }
}
