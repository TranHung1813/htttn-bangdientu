using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Display
{
    public partial class Page_Text : UserControl
    {
        //private string _txt = "- Theo điểm a khoản 1 Điều 12 Nghị định 117/2020/NĐ-CP ngày 28/9/2020 của Chính phủ Quy định xử phạt hành chính trong lĩnh vực y tế:\r\n\r\n" +
        //                      "- Phạt tiền từ 1.000.000 đồng đến 3.000.000 đồng đối với một trong các hành vi: Không thực hiện biện pháp bảo vệ cá nhân đối với người tham gia chống dịch và người có ngy cơ mắc bệnh dịch theo hướng dẫn của cơ quan y tế.";
        private const int MAXVALUE = 1000 * 1000 * 1000;

        System.Timers.Timer Duration_VanBan_Tmr;

        private bool _is_ThongBaoAvailable = false;
        private bool _is_VanBanAvailable = false;

        public Form_Text form_Text = new Form_Text();

        public Page_Text()
        {
            InitializeComponent();
            lb_Title.Text = "";
            lb_Content.Text = "";
            pictureBox1.Visible = false;

            Duration_VanBan_Tmr = new System.Timers.Timer();
            AutoHideScreen_Check();
            this.Dock = DockStyle.None;
        }

        private void Form_Text_NotifyEndProcess_TextRun(object sender, NotifyTextEndProcess e)
        {
            this.Visible = false;
            form_Text.NotifyEndProcess_TextRun -= Form_Text_NotifyEndProcess_TextRun;
        }

        public void ShowText(DisplayScheduleType ScheduleType, string Text, string ColorValue = "")
        {
            Log.Information("ShowText: {A}, Noi dung: {B}", ScheduleType, Text.Substring(0, Text.Length / 5));
            if (ScheduleType == DisplayScheduleType.BanTinThongBao)
            {
                try
                {
                    lb_Title.Text = Text.Trim().ToUpper() + "\n";
                    if (ColorValue != "") lb_Title.ForeColor = ColorTranslator.FromHtml(ColorValue);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "ShowText: {A}", ScheduleType);
                }

                panel_TextRun.SetSpeed = 1;
                int Text_Height = lb_Title.Height + lb_Content.Height;
                panel_TextRun.Start(Text_Height, 10000);

                _is_ThongBaoAvailable = true;
            }
            else if (ScheduleType == DisplayScheduleType.BanTinVanBan)
            {
                try
                {
                    lb_Content.Text = Text;
                    lb_Content.Text = JustifyParagraph(lb_Content.Text, lb_Content.Font, panel_TextRun.Width - 12);
                    if (ColorValue != "") lb_Content.ForeColor = ColorTranslator.FromHtml(ColorValue);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "ShowText: {A}", ScheduleType);
                }

                //pictureBox1.Width = lb_Content.Width;
                //pictureBox1.Height = lb_Content.Height;
                //pictureBox1.Location = lb_Content.Location;

                ////pictureBox1.Width = panel_TextRun.Width;
                ////pictureBox1.Height = panel_TextRun.Height;
                ////Bitmap BM_Title = ConvertTextToImage(lb_Title);
                ////Bitmap BM_Content = ConvertTextToImage(lb_Content);
                ////pictureBox1.Image = MergeImages(BM_Title, BM_Content);
                ////lb_Content.Visible = false;
                ////lb_Title.Visible = false;

                //Bitmap BM_Content = ConvertTextToImage(lb_Content.Text, lb_Content.Font, panel_TextRun.BackColor, lb_Content.ForeColor, lb_Content.Width, lb_Content.Height);
                //pictureBox1.Image = BM_Content;

                panel_TextRun.SetSpeed = 1;
                int Text_Height = lb_Title.Height + lb_Content.Height;
                panel_TextRun.Start(Text_Height, 10000);

                _is_VanBanAvailable = true;
            }
        }
        public void ShowText(string Title, string Content, string ScheduleId, int Priority = 0, int Duration = MAXVALUE)
        {
            if (form_Text != null)
            {
                try
                {
                    form_Text.CloseForm();
                    form_Text.Dispose();
                }
                catch { }
            }

            form_Text = new Form_Text();

            form_Text.PageText_FitToContainer(Height, Width);

            form_Text.NotifyEndProcess_TextRun += Form_Text_NotifyEndProcess_TextRun;
            form_Text.SetSpeed = 1;
            form_Text.ShowText(Title, Content, ScheduleId, Priority, Duration);
            this.Visible = true;
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
                            result += Justify(line.TrimEnd(), font, ControlWidth) + "\n";
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
                        result += line + "\n";
                }
                else
                {
                    result += Paragraph + "\n";
                }
            }
            return result.TrimEnd(new[] { '\n' });
        }
        public static String Justify(String s, Int32 count)
        {
            if (count <= 0)
                return s;

            Int32 middle = s.Length / 2;
            IDictionary<Int32, Int32> spaceOffsetsToParts = new Dictionary<Int32, Int32>();
            String[] parts = s.Split(' ');
            for (Int32 partIndex = 0, offset = 0; partIndex < parts.Length; partIndex++)
            {
                spaceOffsetsToParts.Add(offset, partIndex);
                offset += parts[partIndex].Length + 1; // +1 to count space that was removed by Split
            }
            foreach (var pair in spaceOffsetsToParts.OrderBy(entry => Math.Abs(middle - entry.Key)))
            {
                count--;
                if (count < 0)
                    break;
                parts[pair.Value] += ' ';
            }
            return String.Join(" ", parts);
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
            return ((Func<string>)(() =>
            {
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
        public void Close()
        {
            //lb_Title.Text = "";
            //lb_Content.Text = "";
            //panel_TextRun.Stop();
            if (form_Text != null)
            {
                try
                {
                    form_Text.CloseForm();
                    form_Text.Dispose();
                }
                catch { }
            }

            if (Duration_VanBan_Tmr != null)
            {
                Duration_VanBan_Tmr.Stop();
                Duration_VanBan_Tmr.Dispose();
            }

            _is_ThongBaoAvailable = false;
            _is_VanBanAvailable = false;
            AutoHideScreen_Check();
        }
        public void PageText_FitToContainer(int Height, int Width)
        {
            Utility.FitUserControlToContainer(this, Height, Width);
        }

        private void lb_Title_TextChanged(object sender, EventArgs e)
        {
            lb_Content.Location = new Point(lb_Content.Location.X, lb_Title.Height);
        }

        private void lb_Title_SizeChanged(object sender, EventArgs e)
        {
            lb_Content.Location = new Point(lb_Content.Location.X, lb_Title.Height);
        }

        public void Test()
        {
            //panel1.BackColor = Color.FromArgb(100, 0, 0, 0);
        }
        private void AutoHideScreen_Check()
        {
            if (_is_VanBanAvailable == false)
            {
                this.Visible = false;
            }
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
