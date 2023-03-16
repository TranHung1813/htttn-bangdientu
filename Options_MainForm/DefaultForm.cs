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
            panelThongBao.Stop();
            panelVanBan.Stop();

            Task.Run(() =>
            {
                _mp.Stop();
            });
        }
        public void Set_Infomation(string ThongBao, string VanBan, string VideoURL)
        {
            txtThongBao.Text = ThongBao.Trim().ToUpper();
            txtVanBan.Text = VanBan;
            //txtThongBao.Text = JustifyParagraph(txtThongBao.Text, txtThongBao.Font, panelThongBao.Width - 10);
            txtVanBan.Text = JustifyParagraph(txtVanBan.Text, txtVanBan.Font, panelVanBan.Width - 10);
            pictureBox1.Visible = false;
            pictureBox2.Visible = false;

            ////string _Text = "HTTT nguồn cấp tỉnh là hệ thống dùng chung phục vụ hoạt động TTCS ở cả 3 cấp tỉnh, huyện và xã. Cán bộ làm công tác TTCS cấp tỉnh, cấp huyện và cấp xã được cấp tài khoản để sử dụng các chức năng trên HTTT nguồn cấp tỉnh thực hiện công tác TTCS.";
            //Font font = new Font(txtThongBao.Font.Name, txtThongBao.Font.Size);
            //pictureBox1.Width = panel1.Width;
            //pictureBox1.Height = (int)(this.CreateGraphics().MeasureString(txtThongBao.Text, font, panel1.Width).Height * 1.3);
            //pictureBox1.Image = ConvertTextToImage(txtThongBao.Text, font, panel1.BackColor, txtThongBao.ForeColor, pictureBox1.Width, pictureBox1.Height);
            ////pictureBox1.Image = ConvertTextToImage(txtThongBao);
            //txtThongBao.Visible = false;

            ////string _Text2 = "Triển khai thực hiện nhiệm vụ “Xây dựng hệ thống thông tin nguồn và thu thập, tổng hợp, phân tích, quản lý dữ liệu, đánh giá hiệu quả hoạt động thông tin cơ sở” tại Quyết định số 135/QĐ-TTg ngày 20/01/2020 của Thủ tướng Chính phủ phê duyệt Đề án nâng cao hiệu quả hoạt động thông tin cơ sở dựa trên ứng dụng công nghệ thông tin; Bộ Thông tin và Truyền thông ban hành Hướng dẫn về chức năng, tính năng kỹ thuật của Hệ thống thông tin nguồn trung ương, Hệ thống thông tin nguồn cấp tỉnh và kết nối các hệ thống thông tin - Phiên bản 1.0 (gửi kèm theo văn bản này).";
            //Font font2 = new Font(txtVanBan.Font.Name, txtVanBan.Font.Size);
            //pictureBox2.Width = panel2.Width;
            //pictureBox2.Height = (int)(this.CreateGraphics().MeasureString(txtVanBan.Text, font2, panel2.Width).Height * 1.3);
            //pictureBox2.Image = ConvertTextToImage(txtVanBan.Text, font2, panel2.BackColor, txtVanBan.ForeColor, pictureBox2.Width, pictureBox2.Height);
            ////pictureBox2.Image = ConvertTextToImage(txtVanBan);
            //txtVanBan.Visible = false;

            panelThongBao.SetSpeed = 1;
            int Text_Height1 = txtThongBao.Height;
            panelThongBao.Start(Text_Height1, 10000);

            panelVanBan.SetSpeed = 1;
            int Text_Height2 = txtVanBan.Height;
            panelVanBan.Start(Text_Height2, 10000);

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
        public string JustifyParagraph(string text, Font font, int ControlWidth)
        {
            string result = string.Empty;
            List<string> ParagraphsList = new List<string>();
            ParagraphsList.AddRange(text.Split(new[] { "\n" }, StringSplitOptions.None).ToList());

            foreach (string Paragraph in ParagraphsList)
            {
                string line = string.Empty;
                int ParagraphWidth = TextRenderer.MeasureText(Paragraph, font).Width;

                if (ParagraphWidth > ControlWidth)
                {
                    //Get all paragraph words, add a normal space and calculate when their sum exceeds the constraints
                    string[] Words = Paragraph.Split(' ');
                    line = Words[0] + (char)32;
                    for (int x = 1; x < Words.Length; x++)
                    {
                        string tmpLine = line + (Words[x] + (char)32);
                        if (TextRenderer.MeasureText(tmpLine, font).Width > ControlWidth)
                        {
                            //Max lenght reached. Justify the line and step back
                            result += Justify(line.TrimEnd(), font, ControlWidth) + "\r\n";
                            line = string.Empty;
                            --x;
                        }
                        else
                        {
                            //Some capacity still left
                            line += (Words[x] + (char)32);
                        }
                    }
                    //Adds the remainder if any
                    if (line.Length > 0)
                        result += line + "\r\n";
                }
                else
                {
                    result += Paragraph + "\r\n";
                }
            }
            return result.TrimEnd(new[] { '\r', '\n' });
        }
        private string Justify(string text, Font font, int width)
        {
            char SpaceChar = (char)0x200A;
            List<string> WordsList = text.Split((char)32).ToList();
            if (WordsList.Capacity < 2)
                return text;

            int NumberOfWords = WordsList.Capacity - 1;
            int WordsWidth = TextRenderer.MeasureText(text.Replace(" ", ""), font).Width;
            int SpaceCharWidth = TextRenderer.MeasureText(WordsList[0] + SpaceChar, font).Width
                               - TextRenderer.MeasureText(WordsList[0], font).Width;

            //Calculate the average spacing between each word minus the last one 
            int AverageSpace = ((width - WordsWidth) / NumberOfWords) / SpaceCharWidth;
            float AdjustSpace = (width - (WordsWidth + (AverageSpace * NumberOfWords * SpaceCharWidth)));

            //Add spaces to all words
            return ((Func<string>)(() => {
                string Spaces = "";
                string AdjustedWords = "";

                for (int h = 0; h < AverageSpace; h++)
                    Spaces += SpaceChar;

                foreach (string Word in WordsList)
                {
                    AdjustedWords += Word + Spaces;
                    //Adjust the spacing if there's a reminder
                    if (AdjustSpace > 0)
                    {
                        AdjustedWords += SpaceChar;
                        AdjustSpace -= SpaceCharWidth;
                    }
                }
                return AdjustedWords.TrimEnd();
            }))();
        }
        public void DefaultForm_FitToContainer(int Height, int Width)
        {
            Utility.FitUserControlToContainer(this, Height, Width);

            txtThongBao.MaximumSize = new Size(panelThongBao.Width, 0);
            txtVanBan.MaximumSize = new Size(panelVanBan.Width, 0);
            //txtVanBan.Text = JustifyParagraph(txtVanBan.Text, txtVanBan.Font, panelVanBan.Width - 10);
        }
    }
}
