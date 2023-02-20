﻿using LibVLCSharp.Shared;
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

            txtThongBao.SetSpeed = 1;
            txtVanBan.SetSpeed = 1;

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
            txtThongBao.Size = panelThongBao.Size;
            txtVanBan.Text = VanBan;
            txtVanBan.Size = panelVanBan.Size;
            Timer_DelayTextRun.Start();

            ShowVideo(VideoURL);
        }

        private void Timer_DelayTextRun_Tick(object sender, EventArgs e)
        {
            txtThongBao.Start();
            txtVanBan.Start();
        }
    }
}
