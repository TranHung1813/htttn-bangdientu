using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Display
{
    public partial class Form_VanBan : CSWinFormLayeredWindow.PerPixelAlphaForm
    {
        private const int MAXVALUE = 1000 * 1000 * 1000;
        private int MaxPosition = 0;

        System.Timers.Timer Duration_VanBan_Tmr;
        System.Timers.Timer Moving_Tmr;

        public bool _is_VanBanAvailable = false;
        public int _Priority_VanBan = 1000;
        public string ScheduleID_VanBan = "";
        public bool isValid = false;
        int speed = 0;
        private int ContainerHeight = 0;
        public int ContainerHeight_OldValue = 0;

        List<Bitmap> BM_Content_List = new List<Bitmap>();
        private const int MAXSIZE_IMAGE = Int16.MaxValue - 1000;
        private int CountImage = 0;

        private Point Default_Location = new Point();

        private int ScaleValue = 20;

        public int SetSpeed
        {
            get { return speed; }
            set { speed = value; Invalidate(); }
        }
        public Form_VanBan()
        {
            InitializeComponent();

            Moving_Tmr = new System.Timers.Timer();
            Moving_Tmr.Interval = 15000;
            Moving_Tmr.Elapsed += Moving_Tmr_Elapsed;

            Duration_VanBan_Tmr = new System.Timers.Timer();

            txtVanBan.Text = "";
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
            if (Moving_Tmr.Interval != 40) Moving_Tmr.Interval = 40;
            if (this.Location.Y < (Default_Location.Y - MaxPosition))
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
                        this.BackColor = Color.Black;
                        Dispose_AllImage(BM_Content_List);
                        OnNotifyEndProcess_TextRun();

                        Moving_Tmr.Stop();
                        return;
                    }
                    if (BM_Content_List.Count > 0)
                    {
                        MaxPosition = BM_Content_List[0].Height - ContainerHeight;
                        this.SelectBitmap(BM_Content_List[0]);
                    }
                    this.Location = new Point(this.Location.X, Default_Location.Y + ContainerHeight);
                }
                else
                {
                    if (CountImage < BM_Content_List.Count - 1)
                    {
                        MaxPosition = BM_Content_List[CountImage].Height - ContainerHeight;
                    }
                    else MaxPosition = BM_Content_List[CountImage].Height;
                    this.SelectBitmap(BM_Content_List[CountImage]);
                    this.Location = new Point(this.Location.X, Default_Location.Y);
                }
            }
            this.Location = new Point(this.Location.X, this.Location.Y - speed);
        }

        public void SetLocation_VanBan(Point TB_Location)
        {
            Default_Location = new Point(TB_Location.X + 2, TB_Location.Y);
        }

        public void ShowText(string Content, string ScheduleId, int Priority = 0, int Duration = MAXVALUE)
        {
            this.Location = Default_Location;
            Log.Information("ShowText: Văn bản: {A}", Content.Substring(0, Content.Length / 5));
            if (_Priority_VanBan < Priority) return;
            //if (txtVanBan.Text == Content) return;
            try
            {
                txtVanBan.Text = Content;
                txtVanBan.Text = JustifyParagraph(txtVanBan.Text, txtVanBan.Font, panel_TextRun.Width);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "ShowText: Văn bản: {A}", Content.Substring(0, Content.Length / 5));
            }

            this.Visible = true;

            // Xử lý Bitmap
            Bitmap bmContent = ConvertTextToImage(txtVanBan);
            if (bmContent.Height > MAXSIZE_IMAGE)
            {
                if(BM_Content_List.Count > 0) foreach (var img in BM_Content_List) img.Dispose();
                int NumImage = bmContent.Height / MAXSIZE_IMAGE + 1;
                for (int CountImage = 0; CountImage < (NumImage - 1); CountImage++)
                {
                    try
                    {
                        BM_Content_List.Add(CropImage(bmContent, new Rectangle(new Point(0, CountImage * MAXSIZE_IMAGE),
                                                                 new Size(bmContent.Width, MAXSIZE_IMAGE + ContainerHeight))));
                    }
                    catch { }
                }
                MaxPosition = MAXSIZE_IMAGE;
                BM_Content_List.Add(CropImage(bmContent, new Rectangle(new Point(0, (NumImage - 1) * MAXSIZE_IMAGE),
                                              new Size(bmContent.Width, bmContent.Height - (NumImage - 1) * MAXSIZE_IMAGE))));
                this.SelectBitmap(BM_Content_List[0]);
            }
            else
            {
                MaxPosition = bmContent.Height;
                this.SelectBitmap(bmContent);
            }

            if (MaxPosition < ContainerHeight)
            {
                speed = 0;
            }
            else
            {
                Moving_Tmr.Stop();
                Moving_Tmr.Interval = 15000;
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
                    CountImage = 0;
                    Log.Information("BanTinVanBan Stop!");
                    Dispose_AllImage(BM_Content_List);
                    OnNotifyEndProcess_TextRun();

                    Moving_Tmr.Stop();
                    return;
                }
            });
            isValid = true;

            _is_VanBanAvailable = true;
            _Priority_VanBan = Priority;
            ScheduleID_VanBan = ScheduleId;
            CountImage = 0;
            bmContent.Dispose();
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
                        AdjustedWords += Word + SpaceChar + SpaceChar;// + SpaceChar;
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

        private void AutoHideScreen_Check()
        {
            if (_is_VanBanAvailable == false)
            {
                this.Visible = false;
            }
        }

        public void CloseForm()
        {
            speed = 0;
            txtVanBan.Text = "";
            this.Visible = false;
            //panel_TextRun.Stop();

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
            _is_VanBanAvailable = false;
            AutoHideScreen_Check();
            _Priority_VanBan = 1000;
            Dispose_AllImage(BM_Content_List);
            CountImage = 0;

            this.Location = Default_Location;
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

        public void PageText_FitToContainer(int Height, int Width)
        {
            ContainerHeight = Height;
            Utility.fitFormToContainer(this, Height, this.Width, Height, Width);

            if (ContainerHeight_OldValue != 0)
                txtVanBan.Font = new Font(txtVanBan.Font.FontFamily, (float)(txtVanBan.Font.Size * ((float)Height / (float)ContainerHeight_OldValue)), txtVanBan.Font.Style);
            ScaleValue = (int)(ScaleValue * ((float)(int)Screen.PrimaryScreen.Bounds.Size.Height / (float)768));
            txtVanBan.MaximumSize = new Size(panel_TextRun.Width, 0);
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
}
