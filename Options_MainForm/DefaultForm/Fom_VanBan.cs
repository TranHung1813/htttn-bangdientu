using Serilog;
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

        private Point Default_Location = new Point();

        public int SetSpeed
        {
            get { return speed; }
            set { speed = value; Invalidate(); }
        }
        public Form_VanBan()
        {
            InitializeComponent();

            Moving_Tmr = new System.Timers.Timer();
            Moving_Tmr.Interval = 10000;
            Moving_Tmr.Elapsed += Moving_Tmr_Elapsed;

            Duration_VanBan_Tmr = new System.Timers.Timer();

            txtVanBan.Text = "";
        }

        private void Moving_Tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Moving_Tmr.Interval != 40) Moving_Tmr.Interval = 40;
            if (this.Location.Y < (Default_Location.Y - MaxPosition))
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
                this.Location = new Point(this.Location.X, Default_Location.Y + ContainerHeight);
            }
            this.Location = new Point(this.Location.X, this.Location.Y - speed);
        }

        public void SetLocation_VanBan(Point TB_Location)
        {
            Default_Location = new Point(TB_Location.X + 3, TB_Location.Y);
        }

        public void ShowText(string Content, string ScheduleId, int Priority = 0, int Duration = MAXVALUE)
        {
            this.Location = Default_Location;
            Log.Information("ShowText: Văn bản: {A}", Content);
            if (_Priority_VanBan < Priority) return;
            //if (txtVanBan.Text == Content) return;
            try
            {
                txtVanBan.Text = Content;
                txtVanBan.Text = JustifyParagraph(txtVanBan.Text, txtVanBan.Font, panel_TextRun.Width - 6);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ShowText: Văn bản: {A}", Content);
            }

            this.Visible = true;
            MaxPosition = txtVanBan.Height;
            Bitmap bmTitle = ConvertTextToImage(txtVanBan);

            this.SelectBitmap(bmTitle);

            if (MaxPosition < ContainerHeight)
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
            });
            isValid = true;

            _is_VanBanAvailable = true;
            _Priority_VanBan = Priority;
            ScheduleID_VanBan = ScheduleId;
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

            this.Location = Default_Location;
        }

        public void PageText_FitToContainer(int Height, int Width)
        {
            ContainerHeight = Height;
            Utility.fitFormToContainer(this, Height, this.Width, Height, Width);

            if(ContainerHeight_OldValue != 0)
                txtVanBan.Font = new Font(txtVanBan.Font.FontFamily, (float)(txtVanBan.Font.Size * ((float)Height / (float)ContainerHeight_OldValue)), txtVanBan.Font.Style);

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
