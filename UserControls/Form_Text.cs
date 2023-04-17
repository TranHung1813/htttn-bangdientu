using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Display
{
    public partial class Form_Text : CSWinFormLayeredWindow.PerPixelAlphaForm
    {
        System.Timers.Timer Moving_Tmr;

        private const int MAXVALUE = 1000 * 1000 * 1000;

        System.Timers.Timer Duration_VanBan_Tmr;

        private bool _is_ThongBaoAvailable = false;
        public bool _is_VanBanAvailable = false;
        public int _Priority_VanBan = 1000;
        public string ScheduleID_VanBan = "";
        public bool isValid = false;
        int speed = 0;

        private int MaxPosition = 0;
        public int SetSpeed
        {
            get { return speed; }
            set { speed = value; Invalidate(); }
        }
        public Form_Text()
        {
            InitializeComponent();

            Moving_Tmr = new System.Timers.Timer();
            Moving_Tmr.Interval = 10000;
            Moving_Tmr.Elapsed += Moving_Tmr_Elapsed;

            Duration_VanBan_Tmr = new System.Timers.Timer();

            this.Activate();
        }

        private void Moving_Tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Moving_Tmr.Interval = 30;
            if (this.Location.Y < - MaxPosition)
            {
                if (isValid == false)
                {
                    _is_VanBanAvailable = false;
                    AutoHideScreen_Check();
                    _Priority_VanBan = 1000;
                    Log.Information("BanTinVanBan Stop!");
                    this.BackColor = Color.Black;
                    OnNotifyEndProcess_TextRun();

                    Moving_Tmr.Stop();
                    return;
                }
                this.Location = new Point(this.Location.X, (int)Screen.PrimaryScreen.Bounds.Size.Height);
            }
            this.Location = new Point(this.Location.X, this.Location.Y - speed);
        }

        public void ShowText(string Title, string Content, string ScheduleId, int Priority = 0, int Duration = MAXVALUE)
        {
            this.Location = new Point(0, 0);
            Log.Information("ShowText: Tiêu đề: {A}, Nội dung: {B}", Title, Content.Substring(0, Content.Length / 5));
            if (lb_Content.Text == Content && lb_Title.Text == Title) return;
            try
            {
                lb_Title.Text = Title.Trim().ToUpper() + "\n";
                lb_Content.Text = Content;
                lb_Content.Text = JustifyParagraph(lb_Content.Text, lb_Content.Font, panel_TextRun.Width - 10);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ShowText: Tiêu đề: {A}, Nội dung: {B}", Title, Content.Substring(0, Content.Length / 5));
            }

            this.Visible = true;
            //panel_TextRun.SetSpeed = 1;
            //int Text_Height = lb_Title.Height + lb_Content.Height;
            //panel_TextRun.Start(Text_Height, 10000);

            MaxPosition = lb_Title.Height + lb_Content.Height;
            Bitmap bmContent = ConvertTextToImage(lb_Content);
            Bitmap bmTitle = ConvertTextToImage(lb_Title);

            this.SelectBitmap(MergeImages(bmTitle, bmContent));
            //this.Location = new Point(this.Location.X + 3, this.Location.Y);
            if (MaxPosition < (int)Screen.PrimaryScreen.Bounds.Size.Height)
            {
                speed = 0;
            }
            else
            {
                Moving_Tmr.Stop();
                Moving_Tmr.Interval = 10000;
                Moving_Tmr.Start();
            }

            Duration_Handle(Duration_VanBan_Tmr, ref Duration_VanBan_Tmr, Duration, () =>
            {
                // Stop Media
                isValid = false;
                if (speed == 0)
                {
                    _is_VanBanAvailable = false;
                    AutoHideScreen_Check();
                    _Priority_VanBan = 1000;
                    Log.Information("BanTinVanBan Stop!");
                    OnNotifyEndProcess_TextRun();

                    Moving_Tmr.Stop();
                    return;
                }
                //this.Visible = false;
            });
            isValid = true;

            _is_ThongBaoAvailable = true;
            _is_VanBanAvailable = true;
            _Priority_VanBan = Priority;
            ScheduleID_VanBan = ScheduleId;

            this.Show();
            this.Activate();
            this.BringToFront();
            this.Focus();

        }
        private void Duration_Handle(System.Timers.Timer tmr, ref System.Timers.Timer return_tmr, int Duration, Action action)
        {
            try
            {
                tmr.Stop();
                tmr.Dispose();
            }
            catch { }
            tmr = new System.Timers.Timer();
            // Xu ly Duration cho text content
            tmr.Interval = Duration + 1000;
            tmr.Elapsed += (o, ev) =>
            {
                action();
                // Stop this Timer
                tmr.Stop();
                tmr.Dispose();
            };
            tmr.Start();

            return_tmr = tmr;
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
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

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
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                Rectangle rectF1 = new Rectangle(0, 0, width, Height);
                graphics.FillRectangle(new SolidBrush(bgcolor), 0, 0, bmp.Width, bmp.Height);
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
                g.DrawImage(image1, 0, 3);
                g.DrawImage(image2, 4, image1.Height);
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
        private string Justify(string text, Font font, int width)
        {
            char SpaceChar = (char)0x200A;
            List<string> WordsList = text.Split((char)32).ToList();
            if (WordsList.Capacity < 2)
                return text;

            // First space handle
            int NumberFirstSpace = text.TakeWhile(c => char.IsWhiteSpace(c)).Count();

            int NumberOfWords = WordsList.Capacity - 1 - NumberFirstSpace;
            int SpaceCharWidth = TextRenderer.MeasureText(WordsList[NumberFirstSpace] + SpaceChar, font).Width
                               - TextRenderer.MeasureText(WordsList[NumberFirstSpace], font).Width;
            int WordsWidth = TextRenderer.MeasureText(text.Replace(" ", ""), font).Width + NumberFirstSpace * SpaceCharWidth * 4;

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
                    if (Word == "")
                    {
                        AdjustedWords += Word + SpaceChar + SpaceChar + SpaceChar + SpaceChar;
                    }
                    else
                    {
                        AdjustedWords += Word + Spaces;
                        //Adjust the spacing if there's a reminder
                        if (AdjustSpace > 0)
                        {
                            AdjustedWords += SpaceChar;
                            AdjustSpace -= SpaceCharWidth;
                        }
                    }
                }
                return AdjustedWords.TrimEnd();
            }))();
        }
        public void CloseForm()
        {
            lb_Title.Text = "";
            lb_Content.Text = "";
            panel_TextRun.Stop();

            if (Duration_VanBan_Tmr != null)
            {
                Duration_VanBan_Tmr.Stop();
                Duration_VanBan_Tmr.Dispose();
            }
            if (Moving_Tmr != null)
            {
                Moving_Tmr.Stop();
            }
            isValid = false;
            _is_ThongBaoAvailable = false;
            _is_VanBanAvailable = false;
            AutoHideScreen_Check();
            _Priority_VanBan = 1000;

            this.Location = new Point(0, 0);
        }
        public void PageText_FitToContainer(int Height, int Width)
        {
            Utility.fitFormToContainer(this, this.Height, this.Width, Height, Width);
        }

        private void lb_Title_TextChanged(object sender, EventArgs e)
        {
            lb_Content.Location = new Point(lb_Content.Location.X, lb_Title.Height);
        }

        private void lb_Title_SizeChanged(object sender, EventArgs e)
        {
            lb_Content.Location = new Point(lb_Content.Location.X, lb_Title.Height);
        }

        private void AutoHideScreen_Check()
        {
            if (_is_VanBanAvailable == false)
            {
                this.Visible = false;
            }
        }
        private event EventHandler<NotifyTextEndProcess> _NotifyEndProcess_TextRun;
        public event EventHandler<NotifyTextEndProcess> NotifyEndProcess_TextRun
        {
            add
            {
                _NotifyEndProcess_TextRun += value;
            }
            remove
            {
                _NotifyEndProcess_TextRun -= value;
            }
        }
        protected virtual void OnNotifyEndProcess_TextRun()
        {
            if (_NotifyEndProcess_TextRun != null)
            {
                _NotifyEndProcess_TextRun(this, new NotifyTextEndProcess());
            }
        }
    }
    public class NotifyTextEndProcess : EventArgs
    {
        public NotifyTextEndProcess()
        {

        }
    }
}
