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

        private int ContainerHeight = 0;
        List<Bitmap> BM_Content_List = new List<Bitmap>();
        private const int MAXSIZE_IMAGE = Int16.MaxValue - 1000;
        private int CountImage = 0;

        private int MaxPosition = 0;
        private int ScaleValue = 18;
        public int SetSpeed
        {
            get { return speed; }
            set { speed = value; Invalidate(); }
        }
        public Form_Text()
        {
            InitializeComponent();

            Moving_Tmr = new System.Timers.Timer();
            Moving_Tmr.Interval = 20000;
            Moving_Tmr.Elapsed += Moving_Tmr_Elapsed;

            Duration_VanBan_Tmr = new System.Timers.Timer();

            this.Activate();
        }
        private void Dispose_AllImage(List<Bitmap> list_imgs)
        {
            if (list_imgs == null) return;
            foreach (Bitmap img in list_imgs)
            {
                try
                {
                    img.Dispose();
                }
                catch { }
            }
            list_imgs.Clear();
        }
        private void Moving_Tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Moving_Tmr.Interval = 30;
            if (this.Location.Y < -MaxPosition)
            {
                if (++CountImage >= BM_Content_List.Count)
                {
                    CountImage = 0;
                    if (isValid == false)
                    {
                        _is_VanBanAvailable = false;
                        AutoHideScreen_Check();
                        _Priority_VanBan = 1000;
                        Log.Information("BanTinVanBan Stop!");
                        Dispose_AllImage(BM_Content_List);
                        this.BackColor = Color.Black;
                        OnNotifyEndProcess_TextRun();

                        Moving_Tmr.Stop();
                        return;
                    }
                    if (BM_Content_List.Count > 0)
                    {
                        MaxPosition = BM_Content_List[0].Height - ContainerHeight;
                        this.SelectBitmap(BM_Content_List[0]);
                    }
                    this.Location = new Point(this.Location.X, ContainerHeight);
                }
                else
                {
                    if (CountImage < BM_Content_List.Count - 1)
                    {
                        MaxPosition = BM_Content_List[CountImage].Height - ContainerHeight;
                    }
                    else MaxPosition = BM_Content_List[CountImage].Height;
                    this.SelectBitmap(BM_Content_List[CountImage]);
                    this.Location = new Point(this.Location.X, 0);
                }
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
                lb_Content.Text = JustifyParagraph(lb_Content.Text, lb_Content.Font, panel_TextRun.Width - 6);
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
            Bitmap bmMerge = MergeImages(bmTitle, bmContent);

            if (bmMerge.Height > MAXSIZE_IMAGE)
            {
                if (BM_Content_List.Count > 0) foreach (var img in BM_Content_List) img.Dispose();
                int NumImage = bmMerge.Height / MAXSIZE_IMAGE + 1;
                for (int CountImage = 0; CountImage < (NumImage - 1); CountImage++)
                {
                    try
                    {
                        BM_Content_List.Add(CropImage(bmMerge, new Rectangle(new Point(0, CountImage * MAXSIZE_IMAGE),
                                                                 new Size(bmMerge.Width, MAXSIZE_IMAGE + ContainerHeight))));
                    }
                    catch { }
                }
                MaxPosition = MAXSIZE_IMAGE;
                BM_Content_List.Add(CropImage(bmMerge, new Rectangle(new Point(0, (NumImage - 1) * MAXSIZE_IMAGE),
                                              new Size(bmMerge.Width, bmMerge.Height - (NumImage - 1) * MAXSIZE_IMAGE))));

                this.SelectBitmap(BM_Content_List[0]);
            }
            else
            {
                MaxPosition = bmMerge.Height;
                this.SelectBitmap(bmMerge);
            }

            //this.Location = new Point(this.Location.X + 3, this.Location.Y);
            if (MaxPosition < ContainerHeight + 5)
            {
                speed = 0;
            }
            else
            {
                Moving_Tmr.Stop();
                Moving_Tmr.Interval = 25000;
                Moving_Tmr.Start();
            }

            Duration_Handle(ref Duration_VanBan_Tmr, Duration, () =>
            {
                // Stop Media
                isValid = false;
                if (speed == 0)
                {
                    _is_VanBanAvailable = false;
                    AutoHideScreen_Check();
                    _Priority_VanBan = 1000;
                    Log.Information("BanTinVanBan Stop!");
                    Dispose_AllImage(BM_Content_List);
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
            bmContent.Dispose();
            bmTitle.Dispose();
            bmMerge.Dispose();

            this.Show();
            this.Activate();
            this.BringToFront();
            this.Focus();

        }
        private void Duration_Handle(ref System.Timers.Timer tmr, int Duration, Action action)
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
                System.Timers.Timer thisTimer = (System.Timers.Timer)o;
                thisTimer.Stop();
                thisTimer.Dispose();
            };
            tmr.Start();
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
                g.DrawImage(image1, 0, 5);
                g.DrawImage(image2, 5, image1.Height);
            }

            return bitmap;
        }
        public Bitmap CropImage(Bitmap source, Rectangle section)
        {
            var bitmap = new Bitmap(section.Width, section.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
                return bitmap;
            }
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
                    int LineNumber = 0;
                    for (int x = 1; x < Words.Length; x++)
                    {
                        string tmpLine = line + (Words[x] + (char)32);
                        if (TextRenderer.MeasureText(tmpLine, font).Width > ControlWidth)
                        {
                            //Max lenght reached. Justify the line and step back
                            result += Justify(line.TrimEnd(), font, ControlWidth, LineNumber) + "\n";
                            LineNumber++;
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
        private string Justify(string text, Font font, int width, int LineNum)
        {
            char SpaceChar = (char)0x200A;
            List<string> WordsList = text.Split((char)32).ToList();
            if (WordsList.Capacity < 2)
                return text;

            int LengthFirstSpace = 0;
            string FirstSpace = "";
            if (LineNum == 0)
            {
                // First space handle
                int NumberFirstSpace = text.TakeWhile(c => !char.IsLetter(c)).Count();
                if (NumberFirstSpace > 0)
                {
                    FirstSpace = text.Substring(0, NumberFirstSpace);
                    text = text.Substring(NumberFirstSpace);
                    LengthFirstSpace = TextRenderer.MeasureText(FirstSpace, font).Width;
                    WordsList = text.Split((char)32).ToList();
                    width -= LengthFirstSpace - ScaleValue;
                }
            }

            // Usual handle
            int NumberOfWords = WordsList.Count - 1;
            int SpaceCharWidth = TextRenderer.MeasureText(WordsList[0] + SpaceChar, font).Width
                               - TextRenderer.MeasureText(WordsList[0], font).Width;
            int WordsWidth = TextRenderer.MeasureText(text.Replace(" ", ""), font).Width;


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
                        AdjustedWords += Word + SpaceChar;// + SpaceChar;// + SpaceChar;
                    }
                    else
                    {
                        AdjustedWords += Word + Spaces;
                        //Adjust the spacing if there's a reminder
                        if (AdjustSpace > SpaceCharWidth)
                        {
                            AdjustedWords += SpaceChar;
                            AdjustSpace -= SpaceCharWidth;
                        }
                    }
                }
                return FirstSpace + AdjustedWords.TrimEnd();
            }))();
        }
        public void CloseForm()
        {
            lb_Title.Text = "";
            lb_Content.Text = "";
            panel_TextRun.Stop();

            Duration_VanBan_Tmr?.Stop();
            Duration_VanBan_Tmr?.Dispose();

            Moving_Tmr?.Stop();

            isValid = false;
            _is_ThongBaoAvailable = false;
            _is_VanBanAvailable = false;
            AutoHideScreen_Check();
            _Priority_VanBan = 1000;

            Dispose_AllImage(BM_Content_List);

            this.Location = new Point(0, 0);
        }
        public void PageText_FitToContainer(int Height, int Width)
        {
            ContainerHeight = Height;
            ScaleValue = (int)(ScaleValue * ((float)Height / (float)768));
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
