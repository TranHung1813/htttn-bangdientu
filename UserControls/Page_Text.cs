﻿using System;
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
    public partial class Page_Text : UserControl
    {
        private string _txt = "* Theo điểm a khoản 1 Điều 12 Nghị định 117/2020/NĐ-CP ngày 28/9/2020 của Chính phủ Quy định xử phạt hành chính trong lĩnh vực y tế:\r\n\r\n" +
                              "* Phạt tiền từ 1.000.000 đồng đến 3.000.000 đồng đối với một trong các hành vi: Không thực hiện biện pháp bảo vệ cá nhân đối với người tham gia chống dịch và người có ngy cơ mắc bệnh dịch theo hướng dẫn của cơ quan y tế.\r\n";
        public Page_Text()
        {
            InitializeComponent();
        }

        public void ShowText(string txt_Title, string txt_Content)
        {
            lb_Title.Text += "\r\n";
            lb_Content.Text = _txt;// + _txt;

            timerDelayTextRun.Stop();
            timerDelayTextRun.Interval = 10000;
            timerDelayTextRun.Start();
        }
        public void Close()
        {
            panel_TextRun.Stop();
        }

        private void timerDelayTextRun_Tick(object sender, EventArgs e)
        {
            panel_TextRun.SetSpeed = 1;
            int Text_Height = lb_Title.Height + lb_Content.Height;
            panel_TextRun.Start(Text_Height);

            timerDelayTextRun.Stop();
        }
        public void PageText_FitToContainer(int Height, int Width)
        {
            Utility.FitUserControlToContainer(this, Height, Width);
        }
    }
    public class GrowLabel : Label
    {
        private bool mGrowing;
        public GrowLabel()
        {
            this.AutoSize = false;
        }
        private void resizeLabel()
        {
            if (mGrowing) return;
            try
            {
                mGrowing = true;
                Size sz = new Size(this.Width, Int32.MaxValue);
                sz = TextRenderer.MeasureText(this.Text, this.Font, sz, TextFormatFlags.WordBreak);
                this.Height = sz.Height;
            }
            finally
            {
                mGrowing = false;
            }
        }
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            resizeLabel();
        }
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            resizeLabel();
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            resizeLabel();
        }
    }
}
