﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Display
{
    public partial class Frm_TextOverlay : Form
    {
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams baseParams = base.CreateParams;

                const int WS_EX_NOACTIVATE = 0x08000000;
                const int WS_EX_TOOLWINDOW = 0x00000080;
                baseParams.ExStyle |= (int)(WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW);

                return baseParams;
            }
        }
        private const int MAX_REPEAT_TIME = 1;
        private string _Text = "";
        public Frm_TextOverlay()
        {
            InitializeComponent();

            BackColor = Color.Gray;
            TransparencyKey = Color.Gray;

            txtOverlay.NotifyEndProcess_TextRun += (object o, NotifyEndProcess e) =>
            {
                OnNotify_TextRun_Finish();
            };
        }
        private event EventHandler<Notify_TextRun_Finish> _Notify_TextOverlay_Finish;
        public event EventHandler<Notify_TextRun_Finish> Notify_TextOverlay_Finish
        {
            add
            {
                _Notify_TextOverlay_Finish += value;
            }
            remove
            {
                _Notify_TextOverlay_Finish -= value;
            }
        }
        protected virtual void OnNotify_TextRun_Finish()
        {
            if (_Notify_TextOverlay_Finish != null)
            {
                _Notify_TextOverlay_Finish(this, new Notify_TextRun_Finish());
            }
        }

        public void ShowTextOverlay(string Txt)
        {
            _Text = Txt;
            _Text = "HTTT nguồn cấp tỉnh là hệ thống dùng chung phục vụ hoạt động TTCS ở cả 3 cấp tỉnh, huyện và xã. Cán bộ làm công tác TTCS cấp tỉnh, cấp huyện và cấp xã được cấp tài khoản để sử dụng các chức năng trên HTTT nguồn cấp tỉnh thực hiện công tác TTCS.";
            //_Text = "Hello world!!";

            txtOverlay.Visible = false;
            timer_DelayText.Start();
        }

        private void timer_DelayText_Tick(object sender, EventArgs e)
        {
            txtOverlay.Visible = true;
            txtOverlay.SetSpeed = 2;
            txtOverlay.Max_Repeat_Time = MAX_REPEAT_TIME;
            txtOverlay.Start(panel1.Width);

            timer_DelayText.Stop();
        }
    }
    public class Notify_TextRun_Finish : EventArgs
    {
        public Notify_TextRun_Finish()
        {

        }
    }

}
