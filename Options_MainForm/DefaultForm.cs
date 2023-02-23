using LibVLCSharp.Shared;
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
    public partial class DefaultForm : UserControl
    {
        private LibVLC _libVLC;
        private MediaPlayer _mp;

        public DefaultForm()
        {
            InitializeComponent();

            Init_VLC_Library();
        }

        private void Init_VLC_Library()
        {
            Core.Initialize();

            _libVLC = new LibVLC();
            _mp = new MediaPlayer(_libVLC);
            videoView1.MediaPlayer = _mp;
        }

        public void ShowVideo(string url)
        {
            string[] @params = new string[] { "input-repeat=65535" };

            try
            {
                _mp.Play(new Media(_libVLC, new Uri(url), @params));
            }
            catch
            { }
        }

        public void Set_Infomation(string ThongBao, string VanBan, string VideoURL)
        {
            txtThongBao.Text = ThongBao;
            txtVanBan.Text = VanBan;

            Timer_DelayTextRun.Interval = 5000;
            Timer_DelayTextRun.Start();

            ShowVideo(VideoURL);
        }

        private void Timer_DelayTextRun_Tick(object sender, EventArgs e)
        {
            panelThongBao.SetSpeed = 1;
            int Text_Height1 = txtThongBao.Height;
            panelThongBao.Start(Text_Height1);

            panelVanBan.SetSpeed = 1;
            int Text_Height2 = txtVanBan.Height;
            panelVanBan.Start(Text_Height2);

            Timer_DelayTextRun.Stop();
        }

        public void DefaultForm_FitToContainer(int Height, int Width)
        {
            Utility.FitUserControlToContainer(this, Height, Width);
        }
    }
}
