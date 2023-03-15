using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Display
{
    public partial class Page_Text : UserControl
    {
        private string _txt = "- Theo điểm a khoản 1 Điều 12 Nghị định 117/2020/NĐ-CP ngày 28/9/2020 của Chính phủ Quy định xử phạt hành chính trong lĩnh vực y tế:\r\n\r\n" +
                              "- Phạt tiền từ 1.000.000 đồng đến 3.000.000 đồng đối với một trong các hành vi: Không thực hiện biện pháp bảo vệ cá nhân đối với người tham gia chống dịch và người có ngy cơ mắc bệnh dịch theo hướng dẫn của cơ quan y tế.";
        public Page_Text()
        {
            InitializeComponent();
        }

        public void ShowText(string txt_Title, string txt_Content)
        {
            lb_Title.Text += "\r\n";
            lb_Content.Text = _txt;

            pictureBox1.Width = lb_Content.Width;
            pictureBox1.Height = lb_Content.Height;
            pictureBox1.Location = lb_Content.Location;

            //pictureBox1.Width = panel_TextRun.Width;
            //pictureBox1.Height = panel_TextRun.Height;
            //Bitmap BM_Title = ConvertTextToImage(lb_Title);
            //Bitmap BM_Content = ConvertTextToImage(lb_Content);
            //pictureBox1.Image = MergeImages(BM_Title, BM_Content);
            //lb_Content.Visible = false;
            //lb_Title.Visible = false;

            Bitmap BM_Content = ConvertTextToImage(lb_Content.Text, lb_Content.Font, panel_TextRun.BackColor, lb_Content.ForeColor, lb_Content.Width, lb_Content.Height);
            pictureBox1.Image = BM_Content;
            lb_Content.Visible = false;

            panel_TextRun.SetSpeed = 1;
            int Text_Height = lb_Title.Height + lb_Content.Height;
            panel_TextRun.Start(Text_Height, 10000);
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
        public Bitmap ConvertTextToImage(string txt, Font font, Color bgcolor, Color fcolor, int width, int Height, StringFormat sf)
        {
            Bitmap bmp = new Bitmap(width, Height);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                Rectangle rectF1 = new Rectangle(0, 0, width, Height);
                graphics.FillRectangle(new SolidBrush(Color.AliceBlue), 0, 0, bmp.Width, bmp.Height);
                graphics.DrawString(txt, font, new SolidBrush(fcolor), rectF1, sf);
                graphics.Flush();
                font.Dispose();
                graphics.Dispose();


            }
            return bmp;
        }
        private Bitmap MergeImages(Image image1, Image image2)
        {
            Bitmap bitmap = new Bitmap(Math.Max(image1.Width, image2.Width), image1.Height + image2.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(image1, 0, 0);
                g.DrawImage(image2, 0, image1.Height);
            }

            return bitmap;
        }
        public void Close()
        {
            panel_TextRun.Stop();
            try
            {
                //timerDelayTextRun.Stop();
            }
            catch { }
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
