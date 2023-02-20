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
    public partial class PanelEx : Panel
    {
        Timer tmrTick;
        int position, speed, height, maxPosition;
        bool enableScrollPanel = false;

        public PanelEx()
        {
            InitializeComponent();

            tmrTick = new Timer();
            tmrTick.Tick += tick;
            tmrTick.Interval = 40;
            tmrTick.Start();
        }
        public int SetSpeed
        {
            get { return speed; }
            set { speed = value; Invalidate(); }
        }

        public void Start(int Length_Text_Inside)
        {
            position = 0;
            height = this.Size.Height;
            enableScrollPanel = true;
            maxPosition = Length_Text_Inside;

            if (maxPosition < height)
            {
                SetSpeed = 0;
            }
        }

        private void tick(object sender, EventArgs e)
        {
            if (!enableScrollPanel) return;

            if (this.Location.Y < -maxPosition)
            {
                this.Size = new Size(Width, height);
                this.Location = new Point(this.Location.X, height);
            }

            this.Location = new Point(this.Location.X, this.Location.Y - speed);
            //position -= speed;
            Height += speed;
            Invalidate();
        }
    }
}
