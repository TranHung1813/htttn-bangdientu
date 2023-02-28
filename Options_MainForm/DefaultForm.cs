using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        public void Close()
        {
            if (panelThongBao.State == PanelEx.RUNNING)
            {
                panelThongBao.Stop();
            }
            if (panelVanBan.State == PanelEx.RUNNING)
            {
                panelVanBan.Stop();
            }

            try
            {
                Timer_DelayTextRun.Stop();
            }
            catch { }

            Task.Run(() =>
            {
                _mp.Stop();
            });
        }

        public void Set_Infomation(string ThongBao, string VanBan, string VideoURL)
        {
            //txtThongBao.Text = ThongBao;
            //txtVanBan.Text = VanBan;

            string _Text = "HTTT nguồn cấp tỉnh là hệ thống dùng chung phục vụ hoạt động TTCS ở cả 3 cấp tỉnh, huyện và xã. Cán bộ làm công tác TTCS cấp tỉnh, cấp huyện và cấp xã được cấp tài khoản để sử dụng các chức năng trên HTTT nguồn cấp tỉnh thực hiện công tác TTCS.";
            Font font = new Font("Microsoft Sans Serif", 35);
            pictureBox1.Width = panel1.Width;
            pictureBox1.Height = (int)(this.CreateGraphics().MeasureString(_Text, font, panel1.Width).Height * 1.3);
            pictureBox1.Image = ConvertTextToImage(_Text, font, panel1.BackColor, txtThongBao.ForeColor, pictureBox1.Width, pictureBox1.Height);
            //pictureBox1.Image = ConvertTextToImage(txtThongBao);
            txtThongBao.Visible = false;

            string _Text2 = "Triển khai thực hiện nhiệm vụ “Xây dựng hệ thống thông tin nguồn và thu thập, tổng hợp, phân tích, quản lý dữ liệu, đánh giá hiệu quả hoạt động thông tin cơ sở” tại Quyết định số 135/QĐ-TTg ngày 20/01/2020 của Thủ tướng Chính phủ phê duyệt Đề án nâng cao hiệu quả hoạt động thông tin cơ sở dựa trên ứng dụng công nghệ thông tin; Bộ Thông tin và Truyền thông ban hành Hướng dẫn về chức năng, tính năng kỹ thuật của Hệ thống thông tin nguồn trung ương, Hệ thống thông tin nguồn cấp tỉnh và kết nối các hệ thống thông tin - Phiên bản 1.0 (gửi kèm theo văn bản này).";
            Font font2 = new Font("Microsoft Sans Serif", 35);
            pictureBox2.Width = panel2.Width;
            pictureBox2.Height = (int)(this.CreateGraphics().MeasureString(_Text2, font2, panel2.Width).Height * 1.3);
            pictureBox2.Image = ConvertTextToImage(_Text2, font2, panel2.BackColor, txtVanBan.ForeColor, pictureBox2.Width, pictureBox2.Height);
            //pictureBox2.Image = ConvertTextToImage(txtVanBan);
            txtVanBan.Visible = false;

            Validate();

            Timer_DelayTextRun.Interval = 10000;
            Timer_DelayTextRun.Start();

            ShowVideo(VideoURL);
        }
        public Bitmap ConvertTextToImage(Control control)
        {
            var bitmap = new Bitmap(control.Width, control.Height);
            control.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));

            return bitmap;
        }

        public Bitmap ConvertTextToImage(string txt, Font font, Color bgcolor, Color fcolor, int width, int Height)
        {
            Bitmap bmp = new Bitmap(width, Height);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                Rectangle rectF1 = new Rectangle(0, 0, width, Height);
                graphics.FillRectangle(new SolidBrush(bgcolor), 0, 0, bmp.Width, bmp.Height);
                graphics.DrawString(txt, font, new SolidBrush(fcolor), rectF1);
                graphics.Flush();
                font.Dispose();
                graphics.Dispose();


            }
            return bmp;
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
